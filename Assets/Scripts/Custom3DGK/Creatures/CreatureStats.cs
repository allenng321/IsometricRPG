// Animancer // Copyright 2020 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using System;
using UnityEngine;

namespace Custom3DGK.Creatures
{
    /// <summary>The numerical details of a <see cref="Creature"/>.</summary>
    [Serializable]
    public sealed class CreatureStats
    {
        /************************************************************************************************************************/

        [SerializeField]
        private float _MaxSpeed = 10;
        public float MaxSpeed => _MaxSpeed;

        [SerializeField]
        private float _JumpForce = 1200;
        public float JumpForce => _JumpForce;

        [SerializeField]
        private float _JumpAbortSpeed = 250;
        public float JumpAbortSpeed => _JumpAbortSpeed;

        [SerializeField]
        private float _AirborneHorizontalVelocityMultiplier = .75f;
        public float AirborneHorizontalVelocityMultiplier => _AirborneHorizontalVelocityMultiplier;

        [SerializeField]
        private float _Acceleration = 5;
        public float Acceleration => _Acceleration;

        [SerializeField]
        private float _Deceleration = 5;
        public float Deceleration => _Deceleration;

        [SerializeField]
        private float _MinTurnSpeed = 400;
        public float MinTurnSpeed => _MinTurnSpeed;

        [SerializeField]
        private float _MaxTurnSpeed = 1200;
        public float MaxTurnSpeed => _MaxTurnSpeed;

        /************************************************************************************************************************/
    }
}