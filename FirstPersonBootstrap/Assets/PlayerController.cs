using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public Camera viewCamera;
    Rigidbody rb;



    [Space(15)]
    public float walkSpeed = 5;
    public float runSpeed = 10;
    public KeyCode runKey = KeyCode.LeftShift;



    [Space(15)]
    public float jumpStrength = 5;
    public KeyCode jumpKey = KeyCode.Space;
    Collider col;
    bool jumpButton, canJump;
    public float downGravity;



    [Space(15)]
    public float hookSpeed;
    public float hookPull;
    string hookState = "inactive";

    public GameObject hook, line;
    Transform currentHook;

    LineRenderer lineRenderer;
    public Transform hookSpawnPos;
    int everythingButPlayer;


    
    void Start()
    {

        //Get rigidbody
        rb = GetComponent<Rigidbody>();

        //Get collider
        col = GetComponent<Collider>();
        
        
        //Make a layermask of everything but the player layer
        int player = 1 << 3;
        everythingButPlayer = ~player;
    }

    RaycastHit hookHit;
    void Update()
    {



        //Hook stuff
        {
            //If the player clicks
            if(Input.GetButtonDown("Fire1"))
            {



                //Check if raycast hit anything, using layermask
                bool hit = Physics.Raycast(viewCamera.transform.position, viewCamera.transform.forward, out hookHit, Mathf.Infinity, everythingButPlayer);

                //Delete other hook (if any)
                Destroy(GameObject.FindGameObjectWithTag("Hook"));
                Destroy(GameObject.FindGameObjectWithTag("Line"));

                //Summon hook
                currentHook = Instantiate(hook, hookSpawnPos.position, Quaternion.identity).transform;
                lineRenderer = Instantiate(line, hookSpawnPos.position, Quaternion.identity).GetComponent<LineRenderer>();
                lineRenderer.SetPosition(0, Vector3.up * 2000);
                lineRenderer.SetPosition(1, Vector3.up * 2001);

                //Point it to hookHit point
                //If raycast didn't hit, point forwards
                currentHook.up = hit ? hookHit.point - currentHook.transform.position : viewCamera.transform.forward;
                

                //change state to moving
                hookState = "moving";


            }

            if(Input.GetButtonDown("Fire2"))
            {
                DeleteHook();
            }
        }
        

        //Jump stuff
        {
            //If player presses jump, turn jumpButton to true, but have a little delay before turning it back to false
            //this helps a lot with game feel, the player shouldn't miss a jump because of a few frames.
            if(Input.GetKeyDown(jumpKey)) StartCoroutine(JumpButtonLong());

            //Jump coroutine
            IEnumerator JumpButtonLong()
            {
                
                jumpButton = true;

                //Wait 12 frames
                for(int i = 0; i < 12; i++)
                {
                    yield return new WaitForFixedUpdate();
                }
                
                jumpButton = false;
            }
        }
    
    }

    public void DeleteHook()
    {
        hookState = "inactive";
        Destroy(GameObject.FindGameObjectWithTag("Hook"));
        Destroy(GameObject.FindGameObjectWithTag("Line"));
    }

    RaycastHit throughHit;
    public Vector3 hookDir;
    void FixedUpdate()
    {
        //Ground Movement
        //If pressing WASD or grounded
        if(Mathf.Abs(Input.GetAxis("Horizontal")) > 0 || Mathf.Abs(Input.GetAxis("Vertical")) > 0 || Grounded())
        {

            {
                //Define if the player should run or walk
                float speed = Input.GetKey(runKey) ? runSpeed : walkSpeed;

                //Get the relative forward direction of the camera, ignoring y
                Vector3 forward = viewCamera.transform.forward.normalized;

                //Get the relative right direction of the camera, ignoring y
                Vector3 right = viewCamera.transform.right.normalized;

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
       
        //Hook stuff
        {
            //If the hook is moving
            if(hookState == "moving")
            {
                canJump = true;
                Vector3 previousPos = currentHook.position;
                //Move it forward by x ammount
                currentHook.position += currentHook.up * hookSpeed;

                //Change line points
                lineRenderer.SetPosition(0, hookSpawnPos.position);
                lineRenderer.SetPosition(1, currentHook.position);


                //Cast a ray from the previous position to the next position
                //This detects if the hook went through somehting
                bool throughSomething = Physics.Raycast(previousPos, currentHook.up, out throughHit, hookSpeed, everythingButPlayer);


                //If the hook hit or went through something
                if(throughSomething)
                {
                        //Vertical correction, move the hook a little up/down, depending on the angle of the hit 
                        //Without this the hook ends up being a little bit inside whatever it touched
                        Vector3 verticalCorrection = new Vector3(0, throughHit.normal.y * .12f, 0);


                        //Place the hook in the surface it hit, considering the vertical correction
                        currentHook.position = throughHit.point + verticalCorrection;

                        //Make the hook face whatever it hit
                        currentHook.up = -throughHit.normal;

                        //change state to hooked
                        hookState = "hooked";


                    
                }



            }

            //If hook is hooked
            else if(hookState == "hooked")
            {

                //Change line points
                lineRenderer.SetPosition(0, hookSpawnPos.position);
                lineRenderer.SetPosition(1, currentHook.position);

                //Get the direction between the player and the hook
                hookDir = currentHook.position - transform.position;


                //Move the player in the direction of the hook, using rigidbody velocity
                rb.velocity = Vector3.zero;
                rb.AddForce(hookDir.normalized * hookPull, ForceMode.Impulse);

                //If the player jumps
                if(jumpButton)
                {
                    //delete hook parts
                    Destroy(GameObject.FindGameObjectWithTag("Hook"));
                    Destroy(GameObject.FindGameObjectWithTag("Line"));
                    
                    //change state to inactive
                    hookState = "inactive";
                }

        

            }

        }
        
        //Jump Stuff
        {
            if(Grounded()) canJump = true;
            //If player pressed jump and is grounded
            if (canJump && jumpButton)
            {

                canJump = false;
                jumpButton = false;

                //Set y velocity to 0
                Vector3 spd = rb.velocity;
                spd.y = 0;
                rb.velocity = spd;

                rb.AddForce(rb.transform.up * jumpStrength, ForceMode.Impulse);
            }

            if(rb.velocity.y < 0) rb.velocity -= Vector3.up * downGravity;

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
}