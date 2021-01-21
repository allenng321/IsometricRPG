using Cinemachine;
using UnityEngine;
///using UnityEngine.InputSystem;
using KinematicCharacterController;

public class GravityVirtualCameraController : MonoBehaviour
{
    public GameObject player;
    public CinemachineVirtualCamera gravityCamera;

    [HideInInspector] public Quaternion baseRotation;

    private float xRotationSpeed = 100;
    private float yRotationSpeed = 100;
    private float defaultVAngle = 35;
    private float maxVAngle = 160f;
    private float minVAngle = 1f;   // Overhead top-down views
    private Planet _planet;
   /// private Gamepad _gamepad;
   /// private Mouse _mouse;
    private Transform _follow;
    private KinematicCharacterMotor _followMotor;
    private Transform _lookAt;
    private Transform _baseCameraTransform;
    private Vector3 _offsetDirection;
    private float _horizontalAngle;
    private float _offset;
    private float offsetMax = 25f;
    private float offsetMin = -5f;
    private float _rescaleOffsetA;
    private float _rescaleOffsetB;
    private float _groundSet;
    private float _groundSetA = 0;
    private float _groundSetB = 5f;
    private float _groundSetMin = 70;
    
    private void Awake()
    {
        _planet = player.GetComponent<PlanetInhabitant>().CurrentPlanet;
        _followMotor = player.GetComponent<KinematicCharacterMotor>();
        _follow = _followMotor.transform;
        _lookAt = _planet.transform;
        gravityCamera.Follow = _follow;
        gravityCamera.LookAt = _lookAt;
        _baseCameraTransform = gravityCamera.transform;

        _rescaleOffsetA = (offsetMin - offsetMax) / (maxVAngle - minVAngle);
        _rescaleOffsetB = offsetMin - _rescaleOffsetA * maxVAngle;

        //_gamepad = Gamepad.current;
        //_mouse = Mouse.current;
        
        var planets = (Planet[]) FindObjectsOfType(typeof(Planet));
        foreach(Planet planet in planets)
        {
            planet.OnPlanetChange += newPlanet => _lookAt = newPlanet.transform;
        }
    }

    private float Rescale(float domain0, float domain1, float range0, float range1, float x)
    {
        // Uninterpolate
        float b = domain1 - domain0 != 0 ? domain1 - domain0 : 1 / domain1;
        float uninterpolatedX = (x - domain0) / b;
        // Interpolate and return
        return range0 * (1 - uninterpolatedX) + range1 * uninterpolatedX;
    }
    
    void Update()
    {
        Vector2 inputValue = Vector2.zero;

        //if (_gamepad != null)
        //{
        //    if (_gamepad.rightStick.IsActuated(0.09f))
        //    {
        //        inputValue.x = _gamepad.rightStick.x.ReadValue();
        //        inputValue.y = _gamepad.rightStick.y.ReadValue();
        //    }
        //}
        //else
       // {
       //     inputValue.x = _mouse.delta.x.ReadValue();
       //     inputValue.y = _mouse.delta.y.ReadValue();
       // }
        
        // TESTING BEGIN
        // inputValue = Vector2.down;
        // TESTING END
        
        _horizontalAngle = inputValue.x * -xRotationSpeed * Time.deltaTime;
        defaultVAngle += inputValue.y * yRotationSpeed * Time.deltaTime;

        if (defaultVAngle > maxVAngle) defaultVAngle = maxVAngle;
        if (defaultVAngle < minVAngle) defaultVAngle = minVAngle;
        
        _offset = _rescaleOffsetA * defaultVAngle + _rescaleOffsetB;
        _offsetDirection = _followMotor.CharacterUp;
        _baseCameraTransform.position = _follow.position + _offsetDirection * _offset;
        
        Vector3 groundNormal = _baseCameraTransform.position - _lookAt.transform.position;
        Vector3 forwardsVector = -Vector3.Cross(groundNormal, _baseCameraTransform.right).normalized;

        baseRotation = Quaternion.LookRotation(forwardsVector, groundNormal);
        // TODO find why sometimes the camera rotates of it's own accord when clicking in / out editor 

        gravityCamera.transform.rotation = Quaternion.Euler(baseRotation.eulerAngles) * Quaternion.AngleAxis(90, Vector3.right);
        _baseCameraTransform = gravityCamera.transform; // preserve for the next run through
        // The following line raises the camera above ground when it the angle begins to turn "up"
        if (defaultVAngle > _groundSetMin)
        {
            _groundSet = Rescale(_groundSetMin, maxVAngle, _groundSetA, _groundSetB, defaultVAngle);
            gravityCamera.transform.position += forwardsVector * _groundSet;
        }
        // Rotate the camera locally only for this update frame
        // Note: Vertical first is important, otherwise horizontal rotation jitters
        gravityCamera.transform.RotateAround(_follow.position, baseRotation * -Vector3.right, defaultVAngle);
        gravityCamera.transform.RotateAround(_follow.position, baseRotation * -Vector3.up, _horizontalAngle);
    }
}