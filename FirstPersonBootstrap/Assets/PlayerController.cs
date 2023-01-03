using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public float walkSpeed = 5;
    public float runSpeed = 10;
    public KeyCode runKey = KeyCode.LeftShift;


    Rigidbody rb;
    public Camera viewCamera;
    public Transform hitPosition;

    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    RaycastHit hookHit;
    void Update()
    {
        //Get the position where the hook will hit.
        {
            //Make a layermask of everything but the player layer
            int player = 1 << 3;
            int everythingButPlayer = ~player;

            //Cast a ray based on the camera's position and direction to see where the hook will hit
            Physics.Raycast(viewCamera.transform.position, viewCamera.transform.forward, out hookHit, 50, everythingButPlayer);

        }
    }


    void FixedUpdate()
    {

        //Movement
        {
            //Define if the player should run or walk
            float speed = Input.GetKey(runKey) ? runSpeed : walkSpeed;

            //Get the relative forward direction of the camera, ignoring y
            Vector3 forward = viewCamera.transform.forward;

            //Get the relative right direction of the camera, ignoring y
            Vector3 right = viewCamera.transform.right;

            //Create a temporary vector to store the new velocity
            //This is done because we can't directly modify the x/y/z values of rb.velocity
            Vector3 newVelocity;

            //Get the speed the player should move relative to where they're looking
            Vector3 relativeX = Input.GetAxis("Horizontal") * speed * right;
            Vector3 relativeZ = Input.GetAxis("Vertical") * speed * forward;

            //Add the relative x and z values
            newVelocity = relativeX + relativeZ;

            //Set the y to the current y velocity of the rigidbody
            //If we don't do this the player will just float in mid air
            newVelocity.y = rb.velocity.y;

            //Apply the new velocity
            rb.velocity = newVelocity;

        }

       
        

    }

}
