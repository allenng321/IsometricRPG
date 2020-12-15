// Animancer // Copyright 2020 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using UnityEngine;

namespace Custom3DGK.Creatures
{
    /// <summary>
    /// Base class for controlling the actions of a <see cref="Brains.Creature"/>.
    /// </summary>
    public abstract class CreatureBrain : MonoBehaviour
    {
        /************************************************************************************************************************/

        [SerializeField]
        private Creature _Creature;
        public Creature Creature
        {
            get { return _Creature; }
            set
            {
                if (_Creature == value)
                    return;

                var oldCreature = _Creature;
                _Creature = value;

                // Make sure the old creature doesn't still reference this brain.
                if (oldCreature != null)
                    oldCreature.Brain = null;

                // Give the new creature a reference to this brain.
                if (value != null)
                    value.Brain = this;
            }
        }

        /************************************************************************************************************************/

        public Vector3 ForwardDirection { get; protected set; }
        public Quaternion TargetRotation { get; protected set; }

        /************************************************************************************************************************/
    }
}