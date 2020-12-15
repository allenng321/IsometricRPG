// Animancer // Copyright 2020 Kybernetik //

#pragma warning disable CS0618 // Type or member is obsolete (for ControllerStates in Animancer Lite).
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using System;
using Animancer;
using UnityEngine;
using UnityEngine.Events;

namespace Custom3DGK.States
{
    /// <summary>
    /// A <see cref="CreatureState"/> which moves the creature.
    /// </summary>
    public sealed class LocomotionState : CreatureState
    {
        /************************************************************************************************************************/

        [SerializeField] private Float2ControllerState.Transition _LocomotionBlendTree;
        [SerializeField] private LinearMixerState.Transition _QuickTurnMixer;
        // [SerializeField] private ClipState.Transition _QuickTurnLeft;
        // [SerializeField] private ClipState.Transition _QuickTurnRight;
        [SerializeField] private ClipState.Transition _WalkTurnLeft;
        [SerializeField] private ClipState.Transition _WalkTurnRight;
        [SerializeField] private ClipState.Transition _IdleTurnLeft;
        [SerializeField] private ClipState.Transition _IdleTurnRight;
        [SerializeField] private ClipState.Transition _IdleSmallTurnLeft;
        [SerializeField] private ClipState.Transition _IdleSmallTurnRight;
        [SerializeField] private float _MoveSpeedThresholdForQuickTurn = 2;
        
        private float _QuickTurnDegreeThreshold = 145f;
        private float _IdleTurnDegreeThreshold = 45f;
        private float _MinimumTurnDelta = 4f;
        private Quaternion _startingRotation = Quaternion.identity;
        private AnimancerState _currentState;

        /************************************************************************************************************************/

        private void Awake()
        {
            _QuickTurnMixer.Events.OnEnd = () =>
            {
                // Debug.Log("END QUICK TURN");
                Creature.Animancer.Play(_LocomotionBlendTree);
            };
            
            _IdleTurnLeft.Events.OnEnd =
            _IdleTurnRight.Events.OnEnd =
            _IdleSmallTurnLeft.Events.OnEnd =
            _IdleSmallTurnRight.Events.OnEnd =
                () =>
                {
                    Debug.Log("END TURN");
                    _startingRotation = Quaternion.identity;
                    Creature.Animancer.Play(_LocomotionBlendTree);
                };
            Creature.Animancer.States.GetOrCreate(_QuickTurnMixer);
        }

        /************************************************************************************************************************/

        public override bool CanEnterState(CreatureState previousState)
        {
            return Creature.IsGrounded();
        }
        
        /************************************************************************************************************************/

        private void OnEnable()
        {
            // _QuickTurnMixer.CreateState();  // Needed to initialize the Mixer
            _currentState = Creature.Animancer.Play(_LocomotionBlendTree);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// The locomotion turn animations need to blend the target direction with the root motion rotation.
        /// </summary>
        public override Quaternion RootRotation
        {
            get
            {
                if (_LocomotionBlendTree.State.IsActive) return Quaternion.identity;
                
                // if (_currentState == _QuickTurnLeft.State || _currentState == _QuickTurnLeft.State)
                    // return Creature.Animancer.Animator.deltaRotation;
               
                Quaternion proposedRootRotation = Creature.Motor.Transform.rotation * Creature.Animancer.Animator.deltaRotation;
                float _proposedRootDeltaAngle = Quaternion.Angle(
                    Creature.Motor.Transform.rotation,
                    proposedRootRotation
                );
               
                // Convert the delta angle to delta rotation in next two statements
                Quaternion interpolatedRootTarget = Quaternion.RotateTowards(
                        Creature.Motor.Transform.rotation,
                        Creature.Brain.TargetRotation,
                        _proposedRootDeltaAngle
                    );
               return Quaternion.Inverse(Creature.Motor.Transform.rotation) * interpolatedRootTarget;
                
               
               // //////// PROPOSAL FOR INTERPOLATED ROOT ROTATION /////////////////////////
               // float targetDeltaAngleFromStart = Quaternion.Angle(
               //     _startingRotation,
               //     Creature.Brain.TargetRotation
               // );
               // Debug.Log("targetDeltaAngleFromStart");
               // Debug.Log(targetDeltaAngleFromStart);
               // Quaternion totalRootRotation = Creature.Motor.Transform.rotation * Creature.Animancer.Animator.deltaRotation;
               // float deltaAngleFromAnimStart = Quaternion.Angle(
               //     _startingRotation,
               //     Creature.Animancer.Animator.deltaRotation
               // );
               // Debug.Log("deltaAngleFromAnimStart");
               // Debug.Log(deltaAngleFromAnimStart);
               // float interpolatedDeltaAngle = Mathf.Lerp(targetDeltaAngleFromStart, 0,targetDeltaAngleFromStart / deltaAngleFromAnimStart);
               // Debug.Log("interpolatedDeltaAngle");
               // Debug.Log(interpolatedDeltaAngle);
               // float currentAngleFromAnimStart = Quaternion.Angle(
               //     _startingRotation,
               //     Creature.Motor.Transform.rotation
               // );
               // Debug.Log("currentAngleFromAnimStart");
               // Debug.Log(currentAngleFromAnimStart);
               // float thisIsTheDesiredDeltaAngle = interpolatedDeltaAngle - currentAngleFromAnimStart;
               // Debug.Log("thisIsTheDesiredDeltaAngle");
               // Debug.Log(thisIsTheDesiredDeltaAngle);
               // Quaternion interpolatedRootTarget = Quaternion.RotateTowards(
               //     Creature.Motor.Transform.rotation,
               //     Creature.Brain.TargetRotation,
               //     thisIsTheDesiredDeltaAngle
               // );
               // return Quaternion.Inverse(Creature.Motor.Transform.rotation) * interpolatedRootTarget;
               // //////// END PROPOSAL /////////////////////////////////////////
               
                // // OLD - USED NORMALIZED TIME AND ANIMATOR'S TARGET ROTATION
                // // NOTE: _currentState.RemainingDuration -- returns NaN for some reason
                // float remainingNormalizedTime = _currentState.NormalizedEndTime - _currentState.NormalizedTime;
                // float remainingDuration = _currentState.Duration * remainingNormalizedTime;
                // float currentAnimAngle = Quaternion.Angle(
                //     Creature.Brain.TargetRotation,
                //     Creature.Animancer.Animator.targetRotation
                // );
                // float deltaAngle = Mathf.Lerp(0, _startingTargetAngle, _startingTargetAngle / currentAnimAngle);
                // float maxDegreesDelta = deltaAngle * Time.deltaTime / remainingDuration;
                // return Quaternion.RotateTowards(
                //         Creature.Motor.Transform.rotation,
                //         Creature.Brain.TargetRotation,
                //         maxDegreesDelta
                //     );    
                ////////// END OLD //////////////////////
            }
        }
        
        /************************************************************************************************************************/

        public override bool FullMovementControl { get { return false; } }
        public override bool FullRotationControl
        {
            get { return !UseRootRotation(); }
        }
        
        /************************************************************************************************************************/
        
        private void Update()
        {
            // If we are in another state, or if we're playing a turn animation
            if (Creature.IsOtherMotionState() || !_LocomotionBlendTree.State.IsActive)
                return;

            Creature.UpdateSpeedControl();
            // ParameterX is the Speed parameter which we set to control the Blend Tree.
            float fwdSpeed = _LocomotionBlendTree.State.ParameterX = Creature.ForwardSpeed;

            // If root rotation is used we don't want any scripted rotation.
            if (UseRootRotation()) return;

            float deltaAngle = Creature.GetDeltaAngle();
            _QuickTurnMixer.State.Parameter = deltaAngle + 180;

            if (
                Mathf.Abs(deltaAngle) > _MinimumTurnDelta && 
                Creature.Brain.ForwardDirection != Vector3.zero
            ) {
                if (fwdSpeed < 0.1)
                {
                    // Large slow turn
                    if (Mathf.Abs(deltaAngle) > _IdleTurnDegreeThreshold)
                    {
                        PlayTurnAnimation(deltaAngle < 0 ? _IdleTurnLeft : _IdleTurnRight);
                        return;
                    }
                    else // Small slow turn
                    {
                        PlayTurnAnimation(deltaAngle < 0 ? _IdleSmallTurnLeft : _IdleSmallTurnRight);
                        return;
                    }
                }

                if (fwdSpeed > _MoveSpeedThresholdForQuickTurn)
                {
                    // And turning sharp enough.
                    if (Mathf.Abs(deltaAngle) > _QuickTurnDegreeThreshold)
                    {
                        Creature.Animancer.Play(_QuickTurnMixer);
                        // PlayTurnAnimation(deltaAngle < 0 ? _QuickTurnLeft : _QuickTurnRight);
                        return;
                    }
                }
            }
            
            if (fwdSpeed > 0)
            {
                Creature.TurnTowards(deltaAngle);
            }

            // UpdateAudio();
        }

        /************************************************************************************************************************/

        private void PlayTurnAnimation(ClipState.Transition turn)
        {
            _startingRotation = Creature.Motor.Transform.rotation;

            // Make sure the desired turn is not already active so we don't keep using it repeatedly.
            if (turn.State == null || turn.State.Weight == 0)
            {            
                Debug.Log(turn.Name);
                _currentState = Creature.Animancer.Play(turn);
            }
        }
        
        /************************************************************************************************************************/

        [SerializeField] private UnityEvent _PlayFootstepAudio; // See the Read Me.
        private bool _CanPlayAudio;
        private bool _IsPlayingAudio;

        /// <remarks>
        /// This is the same logic used for locomotion audio in the original PlayerController.
        /// </remarks>
        private void UpdateAudio()
        {
            // ParameterY is FootFall parameter which gets automatically updated by a curve built into each of the
            // animations (and blended by the Blend Tree) so that we can read from it to determine when
            // to play the footstep sounds.
            var footFallCurve = _LocomotionBlendTree.State.ParameterY;

            if (footFallCurve > 0.01f && !_IsPlayingAudio && _CanPlayAudio)
            {
                _IsPlayingAudio = true;
                _CanPlayAudio = false;

                // The full 3D Game Kit has different footstep sounds depending on the ground material and your speed
                // so it calls RandomAudioPlayer.PlayRandomClip with those parameters:
                //_FootstepAudio.PlayRandomClip(Creature.GroundMaterial, Creature.ForwardSpeed < 4 ? 0 : 1);

                // Unfortunately UnityEvents cannot call methods with multiple parameters (UltEvents can), but it does
                // not realy matter because the 3D Game Kit Lite only has one set of footstep sounds anyway.

                _PlayFootstepAudio.Invoke();
            }
            else if (_IsPlayingAudio)
            {
                _IsPlayingAudio = false;
            }
            else if (footFallCurve < 0.01f && !_CanPlayAudio)
            {
                _CanPlayAudio = true;
            }
        }

        /************************************************************************************************************************/
        
        public bool UseRootRotation()
        {
            // If the default locomotion state is not active we must be performing an animated turn.
            return !_LocomotionBlendTree.State.IsActive;
        }
    }
}
