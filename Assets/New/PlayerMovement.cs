using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    /* TODO
     *  3. Walk on planets
     */
    Rigidbody rb;
    [SerializeField]float speed = 1f;
    Vector3 target;
    [SerializeField]GameObject cam;
    [SerializeField] float rotationAcc = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    private void Update()
    {
        Vector3 localTarget = -transform.InverseTransformPoint(cam.transform.position);

        float angle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;

        Vector3 eulerAngleVelocity = new Vector3(0, angle, 0);
        Quaternion deltaRotation = Quaternion.Euler(eulerAngleVelocity * Time.deltaTime * rotationAcc);
        rb.MoveRotation(rb.rotation * deltaRotation);

    }
    private void FixedUpdate()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        rb.velocity += transform.right * horizontal * speed;
        rb.velocity += transform.forward * vertical * speed;

    }
}
