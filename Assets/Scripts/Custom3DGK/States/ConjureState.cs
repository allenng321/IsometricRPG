// Animancer // Copyright 2020 Kybernetik //

#pragma warning disable CS0618 // Type or member is obsolete (for NormalizedEndTime in Animancer Lite).
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using System.Collections;
using Animancer;
using UnityEngine;

namespace Custom3DGK.States
{
    public sealed class ConjureState : CreatureState
    {
        [SerializeField] private ClipState.Transition _Conjure;
        [SerializeField] private ConjureData[] conjuresAvailable;

        private void Awake()
        {
            _Conjure.Events.OnEnd = Creature.ForceEnterIdleState;
        }
        
        public override bool CanEnterState(CreatureState previousState)
        {
            return !Creature.IsGrounded();
        }
        
        private void OnEnable()
        {
            Creature.Animancer.Play(_Conjure);
            Creature.ForwardSpeed = 0;
            var pooledPrefab = conjuresAvailable[Creature.ConjureType].conjureModel;
            pooledPrefab.SetPoolParent(Creature.Planet.transform);
            var instance = pooledPrefab.Get<PooledMonoBehaviour>(transform.position, transform.rotation);
            StartCoroutine(DissipateAfterDelay(instance));
        }
        
        
        private IEnumerator DissipateAfterDelay(PooledMonoBehaviour instance)
        {
            int idx = Creature.ConjureType;
            yield return new WaitForSeconds(conjuresAvailable[idx].dissipateDelay);
            conjuresAvailable[idx].Timeout(instance);
        }
        
        public override bool CanExitState(CreatureState nextState)
        {
            return _Conjure.State.NormalizedTime >= _Conjure.State.Events.NormalizedEndTime;
        }
    }
}
