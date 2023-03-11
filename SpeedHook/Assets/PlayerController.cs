using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Mathf;

public class PlayerController : MonoBehaviour
{


    #region Ground Movement


    [Header("Ground Movement")]
    public string state = "default";
    public float speed = 10;
    public int acceleration = 8;
    public int deceleration = 16;
    public int slowDeceleration = 80;
    public Slider speedSlider;

    public float maxSlopeAngle;
    Vector3 groundNormal;

    int stoppedFrames;
    Vector3 stoppedPosition;

    #endregion

    #region Jump


    [Space(15)][Header("Jump")]
    public float jumpStrength = 5;
    public KeyCode jumpKey = KeyCode.Space;
    Collider col;
    bool jumpButton;
    public float downGravity;


    #endregion

    #region Hooking

    [Space(15)][Header("Hooking")]
    public float hookSpeed;
    public float hookPull;

    public float maxFlyingSpeed = 20;
    float currentMaxFlyingSpeed;

    public GameObject hook, line;
    Transform currentHook;
    bool justUnhooked;
    RaycastHit hookHit;
    LineRenderer lineRenderer;
    public Transform hookSpawnPos;
    int everythingButPlayer, ground;

    #endregion

    #region Misc

    [Space(15)][Header("Misc")]
    public List<AudioClip> clips = new List<AudioClip>();
    public float sensitivity = 3;
    AudioSource a;
    private Vector2 currentMouseLook;
    Vector3 prevForward;
    Vector3 prevRight;
    Transform hookDirTransform;
    Vector3 hookDir;
    int rightFrames = 0;
    public Camera viewCamera;
    public MANAGER manager;
    Rigidbody rb;

    #endregion

    
    void Start()
    {
        currentMaxFlyingSpeed = maxFlyingSpeed;

        //Lock mouse
        Cursor.lockState = CursorLockMode.Locked;

        //Make the cursor invisible
        Cursor.visible = false;

        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        a = GetComponent<AudioSource>();
        state = "default";
        
        //Make a layermask of everything but the player layer
        int player = 1 << 3;
        everythingButPlayer = ~player;

        ground = 1 << 6;
    }

    void Update()
    {


        //Camera view movement 
        Vector2 newMouseDelta = Vector2.Scale(new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")), Vector2.one * sensitivity);
        currentMouseLook += newMouseDelta;
        currentMouseLook.y = Clamp(currentMouseLook.y, -89, 89);
    
        //Hook stuff
        if(!manager.ended)
        {
            //If the player clicks, spawn hook
            if(Input.GetButtonDown("Fire1"))
            {
              
                //Delete any other hook and line
                DeleteHook(false);

                //Play hook shoot audio
                a.PlayOneShot(clips[0]);

                //Check if view raycast hit anything, using layermask
                bool hit = Physics.Raycast(viewCamera.transform.position, viewCamera.transform.forward, out hookHit, Infinity, ground);
  
                //Summon hook
                currentHook = Instantiate(hook, hookSpawnPos.position, Quaternion.identity).transform;
                lineRenderer = Instantiate(line, hookSpawnPos.position, Quaternion.identity).GetComponent<LineRenderer>();
                lineRenderer.SetPosition(0, Vector3.up * 2000);
                lineRenderer.SetPosition(1, Vector3.up * 2001);
  
                //Point it to hookHit point
                //If raycast didn't hit, point forwards
                currentHook.up = hit ? hookHit.point - currentHook.transform.position : viewCamera.transform.forward;
                
                state = Grounded() ? "default" : rb.velocity.magnitude > 1f ? "flying" : "default";
  
            }

            //If the player right-clicks, despawn hook
            if(Input.GetButtonDown("Fire2")) DeleteHook(state == "hooked");
            
        }
        
        //Jump button stuff
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

    void FixedUpdate()
    {

        bool WASD = Abs(Input.GetAxisRaw("Horizontal")) > .1f || Abs(Input.GetAxisRaw("Vertical")) > .1f;

        //Get the relative forward direction of the camera, ignoring y
        Vector3 forward = viewCamera.transform.forward;
        forward.y = 0;
        forward = forward.normalized;
        
        //Get the relative right direction of the camera, ignoring y
        Vector3 right = viewCamera.transform.right;
        right.y = 0;
        right = right.normalized;

        Vector3 xzVel = rb.velocity;
        xzVel.y = 0;

        Vector3 zeroVelocity = new Vector3(0, Grounded() ? 0 : rb.velocity.y, 0);

        switch(state)
        {
            
            case "default":
            {

                currentMaxFlyingSpeed = maxFlyingSpeed;
                moveView();
                move();

                if (Grounded() && jumpButton) jump();
            
                if(currentHook != null) moveHook();

                break;
            }

            case "hooked":
            {

                currentMaxFlyingSpeed = maxFlyingSpeed;
                moveView();

                if(lineRenderer == null) 
                {
                    Debug.LogWarning("Weird hook error");
                    DeleteHook(false);
                    state = "default";
                    break;
                }

                lineRenderer.SetPosition(0, hookSpawnPos.position);
                lineRenderer.SetPosition(1, currentHook.position);


                //Get the direction between the player and the hook
                hookDir = currentHook.position - transform.position;


                //If not moving...
                if(rb.velocity.magnitude <= 1f) 
                {

                    //...stop sound after a frame, 
                    //to account for the first frame where you're not moving yet
                    StartCoroutine(waitAFrameThenStop()); IEnumerator waitAFrameThenStop()
                    {
                        yield return new WaitForFixedUpdate();

                        if(rb.velocity.magnitude <= 1f) 
                        {
                            a.Stop();
                        }
                    }
                    
                }


                //Move the player in the direction of the hook, using rigidbody velocity
                rb.velocity = hookDir.normalized * hookPull;

                if(jumpButton) jump();



                break;
            }

            case "flying":
            {

                if(justUnhooked)
                {
                    justUnhooked = false;
                    goto MoveView;
                }

                prevForward = forward;
                prevRight = right;

                MoveView:
                moveView();


                if(Grounded()) 
                {
                    StartCoroutine(groundedFrames());
                    if(jumpButton) jump();
                }
                IEnumerator groundedFrames()
                {
                    for (int i = 0; i < 25; i++) yield return new WaitForFixedUpdate();
                    if(Grounded() && state == "flying") state = "default";

                }
                
                if(currentHook != null) moveHook();


                //Get every force from old forward and apply it to new forward
                float fSpeed = Vector3.Dot(rb.velocity, prevForward);
                Vector3 newSpeed = forward * fSpeed;
                newSpeed.y = rb.velocity.y;
                rb.velocity = newSpeed;
                

                if(Vector3.Dot(rb.velocity, forward) > maxFlyingSpeed && Input.GetAxisRaw("Vertical") == 1) //if moving faster than max speed and pressing W
                {
                    rb.velocity -= forward * (speed/slowDeceleration);
                }
                else if(Input.GetAxisRaw("Vertical") == 1) //if pressing w and below maxFlyingSpeed
                {
                    newSpeed = rb.velocity;
                    newSpeed += forward * (speed/acceleration/3f);
                    newSpeed.y = 0;

                    float s = Clamp(currentMaxFlyingSpeed, speed, 9999f);
                    if(Abs(Vector3.Dot(newSpeed, forward)) >= s)
                        newSpeed = forward * s;

                    newSpeed.y = rb.velocity.y;
                    rb.velocity = newSpeed;

                }
                else if (Input.GetAxisRaw("Vertical") == 0) //if not pressing W
                {
                    Vector3 oldVel = rb.velocity;
                    rb.velocity -= forward * (speed/deceleration);
                    if(rb.velocity.magnitude > oldVel.magnitude) rb.velocity = oldVel;
                }
                else if (Input.GetAxisRaw("Vertical") == -1) //if pressing S
                {
                    rb.velocity -= forward * (speed/acceleration/3f);
                    if(Vector3.Dot(rb.velocity, forward) <= -speed)
                        rb.velocity = -forward * speed;
                }
                
                //In the future, make this more friendly towards the player, maybe only decrease current max flying speed after a couple of frames
                if(Vector3.Dot(rb.velocity, forward) < currentMaxFlyingSpeed) currentMaxFlyingSpeed = Vector3.Dot(rb.velocity, forward);

                
                float maxStrafeSpeed = Clamp(currentMaxFlyingSpeed / 2f, speed / 2f, 9999f);
                int sign = RoundToInt(Sign((float)rightFrames)); //lol
                if(Input.GetAxisRaw("Horizontal") != 0)
                {
                    int h = RoundToInt(Input.GetAxisRaw("Horizontal"));

                    if(Abs(rightFrames) < acceleration || sign != h)
                        rightFrames += h;

                    rb.velocity += right * maxStrafeSpeed / acceleration * rightFrames;
                    
                }
                else
                {
                    if(rightFrames != 0)
                        rightFrames -= sign;

                    rb.velocity += right * maxStrafeSpeed / acceleration * rightFrames;

                }

                break;
            }

        }



        void move()
        {
                if(WASD)
                {

                    stoppedFrames = 0;
                    //Get forward and right direction
                    forward *= Input.GetAxisRaw("Vertical");
                    right *= Input.GetAxisRaw("Horizontal");

                    //if on slope
                    if(groundNormal != Vector3.up)
                    {
                        forward = Vector3.ProjectOnPlane(forward, groundNormal).normalized;
                        right = Vector3.ProjectOnPlane(right, groundNormal).normalized;
                    }


                    //Get the sum of directions
                    Vector3 dir = forward + right;
                    dir = dir.normalized;


                    //Get force to add
                    Vector3 add = dir * (speed/acceleration);


                    //If about to surpass max speed, make it equal to max speed
                    if((xzVel + add).magnitude >= speed)
                    {
                        Vector3 newSpeed = dir * speed;
                        if(!Grounded()) 
                            newSpeed.y = rb.velocity.y;
                        rb.velocity = newSpeed;
                    }
                    //If not, add speed
                    else  rb.velocity += add;
                    
                }
                else 
                {
                    Vector3 oldVelocity = rb.velocity;
                    //Get direction of movement ignoring y
                    xzVel = rb.velocity;
                    if(!Grounded()) 
                        xzVel.y = 0;
                    xzVel = xzVel.normalized;

                    //Get diretion of positive movement
                    Vector3 add = xzVel * (speed / deceleration);

                    rb.velocity -= add;


                    if(rb.velocity.magnitude > oldVelocity.magnitude) 
                    {
                        rb.velocity = zeroVelocity;
                        stoppedFrames++;
                    }

                    if(stoppedFrames == 2)
                    {
                        stoppedPosition = transform.position;
                    }
                    else if(stoppedFrames > 2 && Grounded())
                    {
                        transform.position = stoppedPosition;
                    }
                }
        }

        void jump()
        {
            stoppedFrames = 0;

            DeleteHook(state == "hooked");

            jumpButton = false;

            //Set y velocity to 0 if falling down
            if(rb.velocity.y <= 0)
            {
                Vector3 spd = rb.velocity;
                spd.y = 0;
                rb.velocity = spd;
            }

            rb.AddForce(rb.transform.up * jumpStrength, ForceMode.Impulse);

            
        }

        void moveHook()
        {
            RaycastHit hookHit;

            Vector3 oldHookPos = currentHook.position;


            currentHook.position += currentHook.up * hookSpeed;

            //Change line points
            lineRenderer.SetPosition(0, hookSpawnPos.position);
            lineRenderer.SetPosition(1, currentHook.position);


            //If hook hit something
            if(Physics.Raycast(oldHookPos, currentHook.up, out hookHit, hookSpeed, ground))
            {
                    //Vertical correction, move the hook a little up/down, depending on the angle of the hit, without this the hook ends up being a little bit inside whatever it touched
                    Vector3 verticalCorrection = new Vector3(0, hookHit.normal.y * .12f, 0);

                    currentHook.position = hookHit.point + verticalCorrection;

                    //Make the hook face whatever it hit
                    currentHook.up = -hookHit.normal;

                    //change state to hooked
                    state = "hooked";

                    a.PlayOneShot(clips[1]);
                    a.PlayOneShot(clips[2]);

            }

            if(Vector3.Distance(currentHook.position, transform.position) > 40) DeleteHook(false);
        }

        void moveView()
        {
            if(!manager.ended)
            {
                viewCamera.transform.localRotation = Quaternion.AngleAxis(-currentMouseLook.y, Vector3.right);
                transform.localRotation = Quaternion.AngleAxis(currentMouseLook.x, Vector3.up);
    
                forward = viewCamera.transform.forward;
                forward.y = 0;
                forward = forward.normalized;
    
                right = viewCamera.transform.right;
                right.y = 0;
                right = right.normalized;
            }
        }

        if(!Grounded()) groundNormal = Vector3.up;

        rb.velocity -= rb.velocity.y < 0 ? Vector3.up * downGravity * 2 : Vector3.up * downGravity;

        xzVel = rb.velocity;
        xzVel.y = 0;
        speedSlider.value = xzVel.magnitude;


        if(manager.ended)
            rb.velocity = Vector3.zero;
        

    }

    void OnCollisionStay(Collision col)
    {
        foreach (ContactPoint c in col.contacts)
        {
            if(c.normal != Vector3.up && c.point.y < transform.position.y)
            {
                groundNormal = c.normal;
                break;
            }
        }

    }


    public void DeleteHook(bool changeState)
    {
        if(changeState) 
            state = "flying";
        

        if(state == "hooked")
        {
            if(hookDirTransform == null) hookDirTransform = new GameObject().transform;
            justUnhooked = true;

            hookDir.y = 0;
            hookDirTransform.forward = hookDir.normalized;

            prevForward = hookDirTransform.forward;
            prevRight = hookDirTransform.right;
        }

        Destroy(GameObject.FindGameObjectWithTag("Hook"));
        Destroy(GameObject.FindGameObjectWithTag("Line"));
        a.Stop();                
    }

    bool Grounded()
    {
  
        bool grounded = false;

        //Get the bit mask of the ground layer
        int groundMask = 1 << 6;


       

        //Get the position of the bottom of the player's hitbox
        Vector3 origin = new Vector3(col.bounds.center.x, col.bounds.min.y + .48f, col.bounds.center.z);


        //Cast a sphere on the bottom of the player
        Collider[] hits = Physics.OverlapSphere(origin, .5f, groundMask);

        //Loop through everything the sphere is touching
        if(hits.Length > 0) grounded = true;

        return grounded;
    }
}
