using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class Jump : MonoBehaviour
{
    public float jumpStrength = 5;
    public KeyCode jumpKey = KeyCode.Space;

    private Rigidbody rb;
    public Collider col;

    bool jumpButton;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {

        if(Input.GetKeyDown(jumpKey)) StartCoroutine(JumpButtonLong());



    }

    IEnumerator JumpButtonLong()
    {
        jumpButton = true;
        for(int i = 0; i < 12; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        jumpButton = false;
    }

    void FixedUpdate()
    {
        if (jumpButton && Grounded())
        {
            jumpButton = false;

            Vector3 spd = rb.velocity;
            spd.y = 0;
            rb.velocity = spd;

            rb.AddForce(rb.transform.up * jumpStrength, ForceMode.Impulse);
        }
    }


    bool Grounded()
    {
        //Create a new bool, default to false
        //This will be set to true if we touch the ground
        bool grounded = false;

        //Get the bit mask of the ground layer
        //This is straight up black magic, only wizards understand how this works
        int groundMask = 1 << 6;


        //Get the position of the bottom of the player's hitbox
        Vector3 origin = new Vector3(col.bounds.center.x, col.bounds.min.y + .48f, col.bounds.center.z);


        //Cast a sphere on the bottom of the player
        Collider[] hits = Physics.OverlapSphere(origin, .5f, groundMask);

        //Loop through everything the sphere is touching
        for(int i = 0; i < hits.Length; i++)
        {
            //If it hits ground, boom, grounded
            if(hits[i].gameObject.layer == 6) grounded = true;
        }


        return grounded;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Vector3 origin = new Vector3(col.bounds.center.x, col.bounds.min.y + .48f, col.bounds.center.z);
        Gizmos.DrawWireSphere(origin, .5f);
    }

}
