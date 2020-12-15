// Animancer // Copyright 2020 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using Animancer;
using Custom3DGK.Creatures;
using UnityEngine;
using UnityEngine.Events;

namespace Custom3DGK.States
{
    /// <summary>
    /// A <see cref="CreatureState"/> which teleports back to the starting position, plays an animation then returns
    /// to the <see cref="Creature.Idle"/> state.
    /// </summary>
    public sealed class RespawnState : CreatureState
    {
        /************************************************************************************************************************/

        [SerializeField] private ClipState.Transition _Animation;
        [SerializeField] private UnityEvent _OnEnterState;// See the Read Me.
        [SerializeField] private UnityEvent _OnExitState;// See the Read Me.

        private Vector3 _StartingPosition;

        /************************************************************************************************************************/

        private void Awake()
        {
            _Animation.Events.OnEnd = Creature.ForceEnterIdleState;
            _StartingPosition = transform.position;
        }

        /************************************************************************************************************************/

        private void OnEnable()
        {
            Creature.Animancer.Play(_Animation);
            Creature.transform.position = _StartingPosition;
            _OnEnterState.Invoke();
        }

        /************************************************************************************************************************/

        private void OnDisable()
        {
            _OnExitState.Invoke();
        }

        /************************************************************************************************************************/

        public override bool CanExitState(CreatureState nextState)
        {
            return false;
        }

        /************************************************************************************************************************/
    }
}
