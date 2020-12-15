// Animancer // Copyright 2020 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using Animancer.FSM;
using Custom3DGK.Creatures;
using UnityEngine;

namespace Custom3DGK.States
{
    /// <summary>
    /// Base class for the various states a <see cref="Brains.Creature"/> can be in and actions they can perform.
    /// </summary>
    public abstract class CreatureState : StateBehaviour<CreatureState>,
        IOwnedState<CreatureState>
    {
        /************************************************************************************************************************/

        [SerializeField]
        private Creature _Creature;

        /// <summary>The <see cref="Brains.Creature"/> that owns this state.</summary>
        public Creature Creature
        {
            get { return _Creature; }
        }

#if UNITY_EDITOR
        protected void Reset()
        {
            _Creature = Animancer.Editor.AnimancerEditorUtilities.GetComponentInHierarchy<Creature>(gameObject);
        }
#endif

        /************************************************************************************************************************/

        public StateMachine<CreatureState> OwnerStateMachine { get { return _Creature.StateMachine; } }

        /************************************************************************************************************************/

        /// <summary>
        /// Some states (such as <see cref="AirborneState"/>) will want to apply their own source of root motion, but
        /// most will just use the root motion from the animations.
        /// </summary>
        public virtual Vector3 RootMotion { get { return _Creature.Animancer.Animator.deltaPosition; } }
        public virtual Quaternion RootRotation { get { return _Creature.Animancer.Animator.deltaRotation; } }

        /// <summary>
        /// Indicates whether the root motion applied each frame while this state is active should be constrained to
        /// only move in the specified <see cref="CreatureBrain.ForwardDirection"/>. Otherwise the root motion can
        /// move the <see cref="Creature"/> in any direction. Default is true.
        /// </summary>
        public virtual bool FullMovementControl { get { return true; } }
        public virtual bool FullRotationControl { get { return true; } }

        /************************************************************************************************************************/
    }
}
