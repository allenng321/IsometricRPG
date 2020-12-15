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

        public bool IsJumping { get; private set; }

        private Executor _executor;
        private InvokeId _isJumpingCoroutine;
        private bool _isJumpInitializing = true;
        
        /************************************************************************************************************************/

        private void Awake()
        {
            _executor = new Executor(this);
        }

        private void OnEnable()
        {
            Creature.Animancer.Play(_Animations);
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

        private void Update()
        {
            // When you jump, do not start checking if you have landed until you stop going up.
            if (IsJumping)
            {
                if (Creature.IsGrounded() && Creature.VerticalSpeed <= 0 && !_isJumpInitializing)
                {
                    IsJumping = false;
                }
            }
            else
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
            if (Creature.IsGrounded() || _WallState.IsWallHanging)
            {
                // Entering this state would have called OnEnable.
                IsJumping = true;
                Creature.Motor.ForceUnground();
                _isJumpInitializing = true;
                _PlayAudio.Invoke(); 
                
                if (_isJumpingCoroutine != null)
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
                    });
                _executor.DelayExecute(
                    0.25f,
                    x =>
                    {
                        _isJumpInitializing = false;
                    });                
            }
        }
        
        public void CancelJump()
        {
            IsJumping = false;
            if (_isJumpingCoroutine != null)
            {
                // In case we manage to jump again before existing coroutine fires
                _executor.StopExecute(_isJumpingCoroutine);
            }
        }

        /************************************************************************************************************************/

        public bool TryWallHang()
        {
            if (!Creature.IsGrounded() && !_isJumpInitializing && Creature.WallHangSensor.GetNearest() != null)
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
