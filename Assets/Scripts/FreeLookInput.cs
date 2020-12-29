using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CinemachineFreeLook))]
public class FreeLookInput : MonoBehaviour
{
    public CinemachineFreeLook fl;

    private static Mouse Mouse => Mouse.current;
    private static Gamepad Gamepad => Gamepad.current;

    private void Update()
    {
        float x, y;
        x = y = 0;

        // First by default check for movement of mouse
        if (!(Mouse is null) && Mouse.delta.IsActuated(.05f))
        {
            var d = Mouse.delta;
            x = d.x.ReadValue();
            y = d.y.ReadValue();
        }

        // Then if GamePad is connected check if the right stick is moved then use that value and not mouse if present
        if (!(Gamepad is null) && Gamepad.rightStick.IsActuated(.08f))
        {
            var v = Gamepad.rightStick.ReadValue();
            x = v.x;
            y = v.y;
        }

        fl.m_XAxis.m_InputAxisValue = x;
        fl.m_YAxis.m_InputAxisValue = y;
    }
}