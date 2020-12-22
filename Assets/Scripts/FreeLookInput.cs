using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CinemachineFreeLook))]
public class FreeLookInput : MonoBehaviour
{
        public CinemachineFreeLook fl;
        
        private Mouse _Mouse;

        private void OnEnable()
        {
                _Mouse = Mouse.current;
        }

        private void Update()
        {
                var d = _Mouse.delta;
                fl.m_XAxis.m_InputAxisValue = d.x.ReadValue();
                fl.m_YAxis.m_InputAxisValue = d.y.ReadValue();
        }
}