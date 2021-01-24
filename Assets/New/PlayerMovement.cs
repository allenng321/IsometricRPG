using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    Rigidbody rb;
    [SerializeField]float speed = 1000f;
    [SerializeField]float jumpPower = 5f;
    Vector3 target;
    [SerializeField]GameObject cam;
    [SerializeField] float rotationAcc = 5f;

    GameObject raycaster;
    [SerializeField]float rayDistance = 0.05f;

    void Debugging()
    {
        Debug.DrawRay(raycaster.transform.position, -transform.up * rayDistance);
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        raycaster = transform.GetChild(0).gameObject;
    }
    private void Update()
    {
        Vector3 localTarget = -transform.InverseTransformPoint(cam.transform.position);

        float angle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;

        Vector3 eulerAngleVelocity = new Vector3(0, angle, 0);
        Quaternion deltaRotation = Quaternion.Euler(eulerAngleVelocity * Time.deltaTime * rotationAcc);
        rb.MoveRotation(rb.rotation * deltaRotation);

        Debug.Log(Grounded());

        Debugging(); /// always in the end!
    }
    private void FixedUpdate()
    {
        if (Input.GetAxisRaw("Horizontal") > 0)
        {
            rb.AddForce(transform.right * speed * Time.deltaTime);
        }
        else if (Input.GetAxisRaw("Horizontal") < 0)
        {
            rb.AddForce(-transform.right * speed * Time.deltaTime);
        }

        if (Input.GetAxisRaw("Vertical") > 0)
        {
            rb.AddForce(transform.forward * speed * Time.deltaTime);
        }
        else if (Input.GetAxisRaw("Horizontal") < 0)
        {
            rb.AddForce(-transform.forward * speed * Time.deltaTime);
        }
        if(Input.GetAxisRaw("Jump") > 0)
        {
            Jump();
        }

    }
    void Jump()
    {
        if(Grounded())
        rb.AddForce(transform.up * jumpPower, ForceMode.Impulse);
    }
    bool Grounded()
    {
        
        RaycastHit hit;
        bool hitt = Physics.Raycast(raycaster.transform.position, -transform.up, out hit, rayDistance);
        if (hitt)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
