// Animancer // Copyright 2020 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using UnityEngine;

namespace Custom3DGK.Creatures
{
    /// <summary>
    /// Takes the root motion from the <see cref="Animator"/> attached to the same <see cref="GameObject"/> and applies
    /// it to a <see cref="Creature"/> on a different object.
    /// </summary>
    public sealed class RootMotionRedirect : MonoBehaviour
    {
        /************************************************************************************************************************/

        [SerializeField]
        private Creature _Creature;

        /************************************************************************************************************************/

        private void OnAnimatorMove()
        {
            _Creature.OnAnimatorMove();
        }

        /************************************************************************************************************************/

        // Ignore these Animation Events because the attack animations will only start when we tell them to, so it
        // would be stupid to use additional events for something we already directly caused.
#pragma warning disable IDE0060 // Remove unused parameter.
        private void MeleeAttackStart(int throwing = 0) { }
#pragma warning restore IDE0060 // Remove unused parameter.
        private void MeleeAttackEnd() { }

        /************************************************************************************************************************/
    }
}
