// Animancer // Copyright 2020 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using System;
using Animancer;
using Animancer.FSM;
using Custom3DGK.States;
using UnityEngine;
using KinematicCharacterController;

namespace Custom3DGK.Creatures
{
    /// <summary>
    /// A centralised group of references to the common parts of a creature and a state machine for their actions.
    /// </summary>
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Rigidbody))]
    [DefaultExecutionOrder(-5000)]// Initialise the State Machine early.
    public sealed class Creature : MonoBehaviour, ICharacterController
    {
        /************************************************************************************************************************/
        private Vector3 _gravityUp;
        private Quaternion _currentRotation;
        private Vector3 _rootMotionPositionDelta;
        private Quaternion _rootMotionRotationDelta;
        private float _maxDegreesDelta = 0f; 
        
        public KinematicCharacterMotor Motor;
        public SensorToolkit.Sensor WallHangSensor;
        
        [SerializeField]
        private AnimancerComponent _Animancer;
        public AnimancerComponent Animancer { get { return _Animancer; } }

        [SerializeField]
        private CreatureBrain _Brain;
        public CreatureBrain Brain
        {
            get { return _Brain; }
            set
            {
                if (_Brain == value)
                    return;

                var oldBrain = _Brain;
                _Brain = value;

                // Make sure the old brain doesn't still reference this creature.
                if (oldBrain != null)
                    oldBrain.Creature = null;

                // Give the new brain a reference to this creature.
                if (value != null)
                    value.Creature = this;
            }
        }
        
        /// <summary>Inspector toggle so you can easily compare raw root motion with controlled motion.</summary>
        [SerializeField]
        private bool _FullMovementControl = true;
        
        [SerializeField]
        private Planet _Planet;
        public Planet Planet { get { return _Planet; } }
        
        [SerializeField]
        private CreatureStats _Stats;
        public CreatureStats Stats { get { return _Stats; } }

        /************************************************************************************************************************/

        [Header("States")]
        [SerializeField]
        private CreatureState _Respawn;
        public CreatureState Respawn { get { return _Respawn; } }

        [SerializeField]
        private CreatureState _Idle;
        public CreatureState Idle { get { return _Idle; } }

        [SerializeField]
        private LocomotionState _Locomotion;
        public LocomotionState Locomotion { get { return _Locomotion; } }

        [SerializeField]
        private AirborneState _Airborne;
        public AirborneState Airborne { get { return _Airborne; } }
        
        [SerializeField]
        private WallState _Wall;
        public WallState Wall { get { return _Wall; } }

        /************************************************************************************************************************/

        public StateMachine<CreatureState> StateMachine { get; private set; }

        /// <summary>
        /// Forces the <see cref="StateMachine"/> to return to the <see cref="Idle"/> state.
        /// </summary>
        public Action ForceEnterIdleState { get; private set; }

        public float ForwardSpeed { get; set; }
        public float DesiredForwardSpeed { get; set; }
        public float VerticalSpeed { get; set; }
        [HideInInspector] public int ConjureType { get; set; }
        public Material GroundMaterial { get; private set; }
        // public Transform ft;

        
        /************************************************************************************************************************/

        private void Awake()
        {
            // Note that this class has a [DefaultExecutionOrder] attribute to ensure that this method runs before any
            // other components that might want to access it.
            ForceEnterIdleState = () => StateMachine.ForceSetState(_Idle);
            StateMachine = new StateMachine<CreatureState>(_Respawn);
            
            var planets = (Planet[]) FindObjectsOfType(typeof(Planet));
            foreach(Planet planet in planets)
            {
                planet.OnPlanetChange += newPlanet => _Planet = newPlanet;
            }
        }

        private void Start()
        {
            Motor.CharacterController = this;
            _gravityUp = _Planet.GetGravityUpForPosition(Motor.Transform.position);
        }

        /************************************************************************************************************************/
        #region Motion
        /************************************************************************************************************************/

        /// <summary>
        /// Check if this <see cref="Creature"/> should enter the Idle, Locomotion, or Airborne states depending on
        /// whether it is grounded and the movement input from the <see cref="Brain"/>.
        /// </summary>
        /// <remarks>
        /// We could add some null checks to this method to support creatures that don't have all the standard states,
        /// such as a creature that can't move or a flying creature that never lands.
        /// </remarks>
        public bool IsOtherMotionState()
        {
            CreatureState state;
            if (IsGrounded() && !_Airborne.InAir)
            {
                state = _Brain.ForwardDirection == Vector3.zero ? _Idle : _Locomotion;
            }
            else
            {
                state = _Airborne;
            }
            return
                state != StateMachine.CurrentState &&
                StateMachine.TryResetState(state);
        }

        /************************************************************************************************************************/

        public void UpdateSpeedControl()
        {
            Vector3 movement = Vector3.ClampMagnitude(_Brain.ForwardDirection, 1);
            DesiredForwardSpeed = movement.magnitude * _Stats.MaxSpeed;  // zero if magnitude zero
            
            float deltaSpeed = movement != Vector3.zero ? _Stats.Acceleration : _Stats.Deceleration;
            
            ForwardSpeed = Mathf.MoveTowards(
                ForwardSpeed,
                DesiredForwardSpeed,
                deltaSpeed * Time.deltaTime
            );
        }

        /************************************************************************************************************************/

        public float CurrentTurnSpeed
        {
            get
            {
                // Interpolates between the range 'a' & 'b', in this case, the turn speed 
                return Mathf.Lerp(
                    _Stats.MaxTurnSpeed,
                    _Stats.MinTurnSpeed,
                    ForwardSpeed / DesiredForwardSpeed);
            }
        }

        /************************************************************************************************************************/

        public float GetDeltaAngle()
        {
            float angle;
            Vector3 angleAxis = Vector3.zero;
            (Brain.TargetRotation * Quaternion.Inverse(_currentRotation)).ToAngleAxis(out angle, out angleAxis);
            if (Vector3.Angle(_gravityUp, angleAxis) > 90f) {
                angle = -angle;
            }
            return Mathf.DeltaAngle(0f, angle);
        }
        
        public void TurnTowards(float deltaAngle)
        {
            // // Since we do not have quick turn animations like the LocomotionState, we just increase the turn speed
            // // when the direction we want to go is further away from the direction we are currently facing.
            // var turnSpeed = Vector3.Angle(Creature.transform.forward, input) * (1f / 180) *
            //                 _TurnSpeedProportion *
            //                 Creature.CurrentTurnSpeed;

            var _AirborneTurnSpeedProportion = 5.4f;
            var workingTurnSpeed = _Airborne.InAir
                ? Mathf.Abs(deltaAngle / 180) * _AirborneTurnSpeedProportion * CurrentTurnSpeed
                : CurrentTurnSpeed;
            // NOTE: deltaAngle must be positive for `Quaternion.RotateTowards` to move toward destination
            _maxDegreesDelta += Math.Abs(deltaAngle) * workingTurnSpeed * Time.deltaTime;
        }

        public void TurnTowards()
        {
            TurnTowards(GetDeltaAngle());
        }

        /************************************************************************************************************************/

        public void OnAnimatorMove()
        {
            // CheckGround();  // TODO bring this back at some point

            // Accumulate rootMotion deltas between character updates 
            _rootMotionPositionDelta += StateMachine.CurrentState.RootMotion;
            _rootMotionRotationDelta = StateMachine.CurrentState.RootRotation;
        }

        public bool IsGrounded()
        {
            return Motor.GroundingStatus.FoundAnyGround;
        }

        /************************************************************************************************************************/

        // TODO reimplement this as part of KCC workflow
        // private void CheckGround()
        // {
        //     if (!IsGrounded())
        //         return;
        //
        //     const float GroundedRayDistance = 1f;
        //
        //     RaycastHit hit;
        //     var ray = new Ray(transform.position + Vector3.up * GroundedRayDistance * 0.5f, -Vector3.up);
        //     if (Physics.Raycast(ray, out hit, GroundedRayDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
        //     {
        //         // Store the current walking surface so the correct audio is played.
        //         var groundRenderer = hit.collider.GetComponentInChildren<Renderer>();
        //         GroundMaterial = groundRenderer ? groundRenderer.sharedMaterial : null;
        //     }
        //     else
        //     {
        //         GroundMaterial = null;
        //     }
        // }

        void Update()
        {
            // For some reason smoothing gravity makes it weird
            // _gravityUp = Vector3.Slerp(
                // Motor.CharacterUp, 
                // _Planet.GetGravityUpForPosition(Motor.Transform.position), 
                // 1 - Mathf.Exp(Time.deltaTime)
            // );
            _gravityUp = _Planet.GetGravityUpForPosition(Motor.Transform.position);
            // if(ft) ft.position = transform.position;
        }
        
        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            if (StateMachine.CurrentState.FullRotationControl)
            {
                // Debug.Log("SCRIPTED ROTATION");
                if (_maxDegreesDelta > 0) // necessary to avoid NaN camera positions
                    currentRotation = Quaternion.RotateTowards(
                        currentRotation,
                        Brain.TargetRotation,
                        _maxDegreesDelta);
            }
            else
            {
                // Debug.Log("ROOT ROTATION");
                // currentRotation = _rootMotionRotationDelta * currentRotation;
                _rootMotionRotationDelta.ToAngleAxis(out var angle, out var axis);

                if (Vector3.Angle(axis, Vector3.up) < 1) axis = _gravityUp;
                else if (Vector3.Angle(axis, Vector3.down) < 1) axis = -_gravityUp;
                else axis = Motor.Transform.InverseTransformDirection(axis);

                var rot = Quaternion.AngleAxis(angle, axis);
                currentRotation = rot * currentRotation;
            }

            // PLANET ALIGNMENT SECTION
            // Align body's up axis with the center of planet
            currentRotation = 
                _currentRotation =
                    Quaternion.FromToRotation(currentRotation * Vector3.up, _gravityUp) * currentRotation;
        }

        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            
            // TODO: implement planet velocity, may need the following again
            // Vector3 down = (transform.position - _Planet.transform.position).normalized;
            // // Rotating the planet with child player inhabitant causes sideways force, so we 
            // //   filter out any force which is no longer directly towards the center of the planet
            // float downSpeed = Vector3.Dot(_rigidbody.velocity, down);
            // _rigidbody.velocity =  down * downSpeed;
            // // Apply new force
            // _rigidbody.AddForce(down * VerticalSpeed);
            //////////////////////////////
            
            
            // Vertical speed
            // If player is in air and not on ground then use the airborne gravity multiplier to adjust the time to reground player
            var s = _Airborne.InAir? _Planet.Gravity * _Airborne.gravityMultiplier: _Planet.Gravity;
            if (VerticalSpeed <= 0)
                VerticalSpeed = -s;
            else
                VerticalSpeed -= s;
            
            // if (_Wall.IsWallHanging)
            // {
            //     VerticalSpeed = 0;
            //     return;
            // }
            
            // Override the vertical speed in duration of jump
            if (_Airborne.Jumping)
            {
                // Set the jumping var to false so that it acts as one time force
                _Airborne.Jumping = false;
                VerticalSpeed = Stats.JumpForce;
            }
            if (_Airborne.InAir && _Airborne.CancelJumping != AirborneState.JumpingCancel.Disable)
            {
                // Just in case if the player is in air and we want to cancel jump
                _Airborne.Jumping = false;
                
                // Acts as a breaking force to stop going up
                if (VerticalSpeed > 0)
                    VerticalSpeed -= _Airborne.CancelJumping == AirborneState.JumpingCancel.Immediate
                        ? VerticalSpeed + 2.5f
                        : Stats.JumpAbortSpeed;

                // Set the bool to false once going up is halted
                _Airborne.CancelJumping =
                    VerticalSpeed <= 0 ? AirborneState.JumpingCancel.Disable : _Airborne.CancelJumping;
            }

            Vector3 verticalVelocity = Motor.CharacterUp * (VerticalSpeed * deltaTime);

            // Horizontal velocity
            Vector3 horizontalVelocity = Vector3.zero;
            Vector3 horizontalMovement = Vector3.zero;

            // Special case for processing movement when in air either falling off a ledge or jump
            if (_Airborne.InAir)
            {
                var pDelta = Motor.GetVelocityFromMovement(_rootMotionPositionDelta, deltaTime);
                var forward = Motor.CharacterForward;

                horizontalVelocity = Vector3.Dot(Motor.Transform.InverseTransformDirection(pDelta), forward) * forward;
                horizontalVelocity *= Stats.AirborneHorizontalVelocityMultiplier;
            }
            else if (!_FullMovementControl || // If FullMovementControl is disabled in the Inspector.
                !StateMachine.CurrentState.FullMovementControl) // Or the current state does not want it.
            {
                // Use only the raw RootMotion.
                horizontalVelocity = Motor.GetVelocityFromMovement(_rootMotionPositionDelta, deltaTime); 
            }
            else
            {
                // ForwardDirection is based on how character is currently oriented, which may not be 
                //  the same as the Brain's `TargetRotation`. Also, it is responsible for reporting the 
                //  magnitude of the direction.
                Vector3 direction = _currentRotation * _Brain.ForwardDirection;
                
                // Debug.DrawRay(Motor.Transform.position + Vector3.up, direction, Color.magenta);
                // Debug.DrawRay(Motor.Transform.position + (2 * Vector3.up), Motor.CharacterForward, Color.cyan);
                
                // Calculate the Root Motion only in the specified direction.
                direction.Normalize();
                float magnitude = Vector3.Dot(direction, _rootMotionPositionDelta);
                horizontalMovement = direction * (magnitude * 100);
                horizontalVelocity = horizontalMovement * (ForwardSpeed * deltaTime);
            }
            
            // Put them together
            currentVelocity = horizontalVelocity + verticalVelocity;
        }

        public void BeforeCharacterUpdate(float deltaTime)
        {
            
        }

        public void PostGroundingUpdate(float deltaTime)
        {
            
        }

        public void AfterCharacterUpdate(float deltaTime)
        {
            // Reset deltas
            _maxDegreesDelta = 0;
            _rootMotionPositionDelta = Vector3.zero;
            _rootMotionRotationDelta = Quaternion.identity;
        }

        public bool IsColliderValidForCollisions(Collider coll)
        {
            return true;
        }

        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
            
        }

        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint,
            ref HitStabilityReport hitStabilityReport)
        {
            
        }

        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition,
            Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
        {
            
        }

        public void OnDiscreteCollisionDetected(Collider hitCollider)
        {
            
        }
        
        #endregion
    }
}
