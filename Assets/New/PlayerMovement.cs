using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    /* TODO
     *  1. Face only camera
     *  2. Walk in facing direction
     *  3. Walk on planets
     */
    Rigidbody rb;
    float speed = 10f;
    Vector3 target;
    GameObject cam;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cam = GameObject.FindGameObjectWithTag("MainCamera");
    }
    private void Update()
    {
        target = cam.transform.position;
        Vector3 direction = (transform.position - target).normalized;
        transform.LookAt(new Vector3(direction.x, 0, direction.z));
    }
    private void FixedUpdate()
    {



        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        rb.velocity += new Vector3(horizontal * speed * Time.fixedDeltaTime, 0, vertical * speed * Time.fixedDeltaTime);

    }
}
