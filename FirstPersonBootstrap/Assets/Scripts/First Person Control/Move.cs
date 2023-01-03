using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider),typeof(Rigidbody))]
public class Move : MonoBehaviour
{
    public float walkSpeed = 5;
    public float runSpeed = 10;
    public KeyCode runKey = KeyCode.LeftShift;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        float speed = Input.GetKey(runKey) ? runSpeed : walkSpeed;

        Vector3 newSpeed = Vector3.zero;

        float inputX = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
        float inputZ = Input.GetAxis("Vertical") * speed * Time.deltaTime;

        rb.transform.Translate(inputX, 0, inputZ);
    }


    //You only start decelerating once you let go

}
