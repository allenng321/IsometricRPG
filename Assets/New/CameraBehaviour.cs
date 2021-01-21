using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    //TODO
    /*
     * 4. Make it work with a controller
     */
    Transform target;
    float mousePosX;
    float mousePosY;
    [SerializeField] float sensitivity;
    void Start()
    {
        target = GameObject.Find("FollowTarget").transform;
    }
    void Update()
    {
        mousePosY = sensitivity * Input.GetAxis("CameraVertical");
        mousePosX = sensitivity * Input.GetAxis("CameraHorizontal");
        target.rotation *= Quaternion.AngleAxis(mousePosX, transform.up);
        
    }
}
