#pragma warning disable CS0618 // Type or member is obsolete (for NormalizedEndTime in Animancer Lite).
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using System;
using Animancer;
using UnityEngine;
using LateExe;

namespace Custom3DGK.States
{
    public sealed class WallState : CreatureState
    {
        [SerializeField] private ClipState.Transition _WallClingAnimation;

        public bool CanTryWallHang { get; set; }
        
        public bool IsWallHanging { get; set; }
        
        private Executor _executor;
        private InvokeId _wallHangCoroutine;
        
        private void Awake()
        {
            Action onEnd = () => Creature.Animancer.Play(_WallClingAnimation);
            _WallClingAnimation.Events.OnEnd = onEnd;
            _executor = new Executor(this);
        }
        
        private void OnEnable()
        {
            Creature.Animancer.Play(_WallClingAnimation);
            IsWallHanging = true;
        }
        
        public override Vector3 RootMotion
        {
            get
            {
                return Vector3.zero;
            }
        }
    }
}