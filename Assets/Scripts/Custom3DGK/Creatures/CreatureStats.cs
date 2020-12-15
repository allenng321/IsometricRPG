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
        public float MaxSpeed { get { return _MaxSpeed; } }
        
        [SerializeField]
        private float _JumpSpeed = 40;
        public float JumpSpeed { get { return _JumpSpeed; } }
        
        [SerializeField]
        private float _JumpAbortSpeed = 2;
        public float JumpAbortSpeed { get { return _JumpAbortSpeed; } }

        [SerializeField]
        private float _Acceleration = 5;
        public float Acceleration { get { return _Acceleration; } }

        [SerializeField]
        private float _Deceleration = 5;
        public float Deceleration { get { return _Deceleration; } }

        [SerializeField]
        private float _MinTurnSpeed = 400;
        public float MinTurnSpeed { get { return _MinTurnSpeed; } }

        [SerializeField]
        private float _MaxTurnSpeed = 1200;
        public float MaxTurnSpeed { get { return _MaxTurnSpeed; } }
        
        /************************************************************************************************************************/
    }
}