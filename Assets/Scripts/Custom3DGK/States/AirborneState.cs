// Animancer // Copyright 2020 Kybernetik //

#pragma warning disable CS0618 // Type or member is obsolete (for Mixers in Animancer Lite).
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using System.Reflection;
using Animancer;
using UnityEngine;
using UnityEngine.Events;
using LateExe;

namespace Custom3DGK.States
{
    public sealed class AirborneState : CreatureState
    {
        /************************************************************************************************************************/

        [SerializeField] private LinearMixerState.Transition _Animations;
        [SerializeField] private LandingState _LandingState;
        [SerializeField] private WallState _WallState;
        [SerializeField] private UnityEvent _PlayAudio;// See the Read Me.

        [Range(.1f, 10)] public float gravityMultiplier = 2f;

        public bool InAir { get; private set; }
        public bool Jumping { get; set; }
        
        public enum JumpingCancel { Disable, Delayed, Immediate }
        public JumpingCancel CancelJumping { get; set; }

        private Executor _executor;
        private InvokeId _isJumpingCoroutine;
        public bool IsJumpInitializing { get; private set; }
        
        /************************************************************************************************************************/

        private void Awake()
        {
            _executor = new Executor(this);
        }

        private void OnEnable()
        {
            Creature.Animancer.Play(_Animations);
            
            // set in air true it does not even if we fell off a ledge as long as we are not on ground
            InAir = true;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// The airborne animations do not have root motion, so we just let the brain determine which way to go.
        /// </summary>
        public override Vector3 RootMotion
        {
            get
            {
                Vector3 fwdDirection = Creature.Brain.TargetRotation * Creature.Brain.ForwardDirection;  
                return fwdDirection * (Creature.ForwardSpeed * Time.deltaTime);
            }
        }

        /************************************************************************************************************************/

        private void CheckGroundStatus()
        {
            if (!InAir) return;

            if (Creature.IsGrounded() && Creature.VerticalSpeed <= 0 && !IsJumpInitializing)
            {
                InAir = false;
            }
        }

        private void Update()
        {
            // When you jump, do not start checking if you can switch to land state until you reach ground.
            CheckGroundStatus();
            
            if (!InAir)
            {
                // If we have a landing state, try to enter it.
                if (_LandingState != null)
                {
                    if (Creature.StateMachine.TrySetState(_LandingState))
                        return;
                }
                else // Otherwise check the default transitions to Idle or Locomotion.
                {
                    if (Creature.IsOtherMotionState())
                        return;
                }
            }

            _Animations.State.Parameter = Creature.VerticalSpeed;

            Creature.UpdateSpeedControl();

            var input = Creature.Brain.ForwardDirection;
            if (input == Vector3.zero)  // no turning unless input
                return; 
            
            Creature.TurnTowards();
        }

        /************************************************************************************************************************/
        public void ClearLog()
        {
            var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
            var type = assembly.GetType("UnityEditor.LogEntries");
            var method = type.GetMethod("Clear");
            method.Invoke(new object(), null);
        }
        
        public void TryJump()
        {
            // ClearLog();

            // We did not override CanEnterState to check if the Creature is grounded because this state is also used
            // if you walk off a ledge, so instead we check that condition here when specifically attempting to jump.

            // first deny if the current is landing or conjure state
            var cs = Creature.StateMachine.CurrentState;
            if (cs is LandingState || cs is ConjureState) return;

            // next check the conditions required to jump
            if (!Creature.IsGrounded() && !_WallState.IsWallHanging) return;
            
            // Entering this state would have called OnEnable.
            InAir = true;
            Creature.Motor.ForceUnground();
            IsJumpInitializing = true;
            _PlayAudio.Invoke();
            Jumping = true; 

            /*if (_isJumpingCoroutine != null)
                {
                    // In case we manage to jump again before existing coroutine fires
                    _executor.StopExecute(_isJumpingCoroutine);
                }
                
                _isJumpingCoroutine = _executor.DelayExecute(
                    1f,
                    x =>
                    {
                        _isJumpingCoroutine = null; // So we do not cancel finished routines
                        IsJumping = false;
                    });*/

            _executor.DelayExecute(
                0.25f,
                x => { IsJumpInitializing = false; });
        }
        
        // exists in case if needed on a key press in future
        public void CancelJump(bool conjure = false)
        {
            IsJumpInitializing = false;
            Jumping = false;
            CancelJumping = conjure ? JumpingCancel.Immediate : JumpingCancel.Delayed;

            // check ground with a delay so that player has enough time to stand on the conjure and then disable InAir 
            _executor.DelayExecute(
                0.05f,
                x => { CheckGroundStatus(); });

            /*if (_isJumpingCoroutine != null)
            {
                // In case we manage to jump again before existing coroutine fires
                _executor.StopExecute(_isJumpingCoroutine);
            }*/
        }

        /************************************************************************************************************************/

        public bool TryWallHang()
        {
            if (!Creature.IsGrounded() && !IsJumpInitializing && Creature.WallHangSensor.GetNearest() != null)
            {
                Creature.StateMachine.TryResetState(_WallState);
                return true;
            } 
            _WallState.IsWallHanging = false;
            return false;
        }
        
        public void CancelWallHang()
        {
            _WallState.IsWallHanging = false;
            if (!Creature.IsGrounded())
            {
                Creature.StateMachine.TryResetState(this);
            }
        }

        /************************************************************************************************************************/
    }
}
