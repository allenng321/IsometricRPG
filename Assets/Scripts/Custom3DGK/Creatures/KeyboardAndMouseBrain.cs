// Animancer // Copyright 2020 Kybernetik //

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

using Animancer.FSM;
using Custom3DGK.States;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Custom3DGK.Creatures
{
    /// <summary>
    /// A <see cref="CreatureBrain"/> which controls the creature using keyboard input.
    /// </summary>
    /// <remarks>
    /// Equivalent to <see cref="PlayerInput"/> from the 3D Game Kit.
    /// </remarks>
    public sealed class KeyboardAndMouseBrain : CreatureBrain
    {
        /************************************************************************************************************************/

        [SerializeField] private CreatureState _Attack;
        [SerializeField] private CreatureState _Conjure;
        [SerializeField] private float _AttackInputTimeOut = 0.5f;
        [SerializeField] private float _ConjureInputTimeOut = 0.5f;
        [SerializeField] private GravityVirtualCameraController _CameraController;

        [HideInInspector] public Vector2 Input = Vector2.zero;
        
        private StateMachine<CreatureState>.InputBuffer _InputBuffer;

        private Gamepad _Gamepad; 
        private Keyboard _Keyboard; 
        private Mouse _Mouse;

        private bool _jumpWasPressed = false;
        private bool _jumpIsHeld = false;
        private bool _hangWasPressed = false;
        private bool _hangIsHeld = false;
        private bool _conjureWasPressed = false;
        private bool _attackWasPressed = false;
        private Vector2 _input = Vector2.zero;

        // TESTING VARIABLES        
        private Vector2 _testInput = Vector2.zero;
        private bool _testingToggle = true;

        /************************************************************************************************************************/

        private void Start()
        {
            // TEST CASES
            // InvokeRepeating("AbruptBackAndForth", 5f, 5f);
            // InvokeRepeating("BasicJump", 0f, 5f);
        }
        
        private void Awake()
        {
            _InputBuffer = new StateMachine<CreatureState>.InputBuffer(Creature.StateMachine);
            TargetRotation = transform.rotation;
            ForwardDirection = Vector3.zero;
            _Gamepad = Gamepad.current;
            _Keyboard = Keyboard.current;
            _Mouse = Mouse.current;
            var moveAction = new InputAction("Move");
            moveAction.AddCompositeBinding("Axis")
                .With("Positive", "<Keyboard>/dKey")
                .With("Negative", "<Keyboard>/aKey");

        }

        /************************************************************************************************************************/

        private void Update()
        {
            UpdateActions();
            
            if (_testInput != Vector2.zero)
            {
                // Debug.Log("USING TEST INPUT");
                // _input = _testInput;
                // _testInput = Vector2.zero;  // only a touch of the button, not constant
                // Debug.Log(_input);

            }
            else if (_Gamepad != null)
            {
                if (_Gamepad.leftStick.IsActuated(0.2f))
                    _input= _Gamepad.leftStick.ReadValue();
            }
            else
            {
                if (_Keyboard.aKey.isPressed) _input.x -= 1;
                if (_Keyboard.dKey.isPressed) _input.x += 1;
                if (_Keyboard.wKey.isPressed) _input.y += 1;
                if (_Keyboard.sKey.isPressed) _input.y -= 1;
                _input.Normalize();
            }

            if (_input == Vector2.zero)
            {
                ForwardDirection = Vector3.zero;
                return;
            }
            
            Vector3 groundNormal = Creature.Motor.TransientPosition - Creature.Planet.transform.position;
            Vector3 cameraForward = -Vector3.Cross(groundNormal, _CameraController.transform.right).normalized;
            Quaternion baseRotation = Quaternion.LookRotation(cameraForward, groundNormal);
            float angle = Mathf.Atan2(_input.x,_input.y) * Mathf.Rad2Deg;
            TargetRotation = baseRotation * Quaternion.AngleAxis(angle, Vector3.up);
            // ForwardDirection is both a direction based on TargetRotation and a magnitude used for movement velocity
            ForwardDirection = TargetRotation * Vector3.forward * Vector2.ClampMagnitude(_input, 1).magnitude;
            _input = Vector2.zero;
        }

        /************************************************************************************************************************/

        private void UpdateActions()
        {
            // Collect Input
            if (_Gamepad != null)
            {   // TODO is there a dictionary or array with entries of which was pressed this frame?
                _jumpWasPressed = _Gamepad.buttonSouth.isPressed;
                _hangWasPressed = _Gamepad.leftShoulder.isPressed;
                _conjureWasPressed = _Gamepad.buttonWest.wasPressedThisFrame;
                _attackWasPressed = _Gamepad.buttonEast.wasPressedThisFrame;
                if (_Gamepad.dpad.up.wasPressedThisFrame) Creature.ConjureType = 0;
                if (_Gamepad.dpad.right.wasPressedThisFrame) Creature.ConjureType = 1;
                if (_Gamepad.dpad.down.wasPressedThisFrame) Creature.ConjureType = 2;
                if (_Gamepad.dpad.left.wasPressedThisFrame) Creature.ConjureType = 3;
            }
            else
            {
                _jumpWasPressed = _Keyboard.spaceKey.wasPressedThisFrame;
                _hangWasPressed = _Keyboard.leftCommandKey.wasPressedThisFrame || _Keyboard.rightCommandKey.wasPressedThisFrame;
                _conjureWasPressed = _Mouse.leftButton.wasPressedThisFrame;
                _attackWasPressed = _Keyboard.shiftKey.wasPressedThisFrame;
                if (_Keyboard.upArrowKey.wasPressedThisFrame) Creature.ConjureType = 0;
                if (_Keyboard.rightArrowKey.wasPressedThisFrame) Creature.ConjureType = 1;
                if (_Keyboard.downArrowKey.wasPressedThisFrame) Creature.ConjureType = 2;
                if (_Keyboard.leftArrowKey.wasPressedThisFrame) Creature.ConjureType = 3;
            }

                        
            // JUMP NOTES: Jump gets priority for better platforming.
            // - should have option to implement double jump
            // - decided not to "charge" jumps, instead we are able to hold jump to increase air time
            if (_jumpWasPressed)
            {
                if (!_jumpIsHeld)
                {
                    _jumpIsHeld = true;
                    Creature.Airborne.TryJump();
                }
            }
            else if (_jumpIsHeld)
            {
                _jumpIsHeld = false;
                Creature.Airborne.CancelJump();
            }

            if (_hangWasPressed)
            {
                _hangIsHeld = true;
                Creature.Airborne.TryWallHang();
            }
            else if (_hangIsHeld)
            {
                _hangIsHeld = false;
                Creature.Airborne.CancelWallHang();
            }
            
            if (_conjureWasPressed)
                _InputBuffer.TrySetState(_Conjure, _ConjureInputTimeOut);

            if (_attackWasPressed)
                _InputBuffer.TrySetState(_Attack, _AttackInputTimeOut);
            else
            {
                _InputBuffer.Update();
            }
        }

        /************************************************************************************************************************/
        /* TESTING */
        /************************************************************************************************************************/

        private void AbruptBackAndForth()
        {
            Debug.Log("CHANGED DIRECTION");
            if (_testingToggle)
            {
                // Debug.Log("TESTING DOWN INPUT");
                _testInput = Vector2.down;
                // _testInput = Vector2.right;
                // _testInput = new Vector2(0.7f, 0.7f);
                // _testInput = new Vector2(1f, 1f);
            }
            else
            {
                // Debug.Log("TESTING UP INPUT");
                _testInput = Vector2.up;
                // _testInput = _testInput = new Vector2(1f, 0f);
            }
            _testingToggle = !_testingToggle;

        }
        private void BasicJump()
        {
            if (_testingToggle)
            {
            }
            else
            {
            }
            _testingToggle = !_testingToggle;

        }
    }
}
