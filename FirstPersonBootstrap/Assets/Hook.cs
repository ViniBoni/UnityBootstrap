using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hook : MonoBehaviour
{

    public Camera viewCamera;

    public GameObject line;
    private LineRenderer lineRenderer;
    public GameObject hook;
    public Transform hookSpawn;
    private Transform currentHook;

    public Rigidbody rb;

    private string hookState = "inactive";

    public float hookMovingSpeed = 1.3f;

    public float hookPullForce = 20f;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        

        if(Input.GetMouseButtonDown(0))
        {


            //Get where the player is looking
            RaycastHit viewHit;
            Physics.Raycast(viewCamera.transform.position, viewCamera.transform.forward, out viewHit, Mathf.Infinity, LayerMask.GetMask("Ground"));



            //Destroy other hooks
            Destroy(GameObject.FindGameObjectWithTag("Hook"));

            //Spawn in new hook
            currentHook = Instantiate(hook, hookSpawn.position, Quaternion.identity).transform;

            //Subtract the destination from the origin
            currentHook.up = viewHit.point - currentHook.position;


            //Change hook state
            hookState = "moving";


        }

    }


    void FixedUpdate()
    {

        if(hookState == "hooked")
        {

            //Get direction
            Vector3 hookDirection = currentHook.position - transform.position;

            //Apply speed
            rb.velocity = hookDirection.normalized * hookPullForce;

        }

        if(hookState == "moving")
        {

            //Get previous position
            Vector3 previousPosition = currentHook.position;

            //Move hook
            currentHook.position += currentHook.up * hookMovingSpeed;

            //Cast a ray between old position and new position
            RaycastHit hookHit;
            bool hit = Physics.Raycast(previousPosition, currentHook.up, out hookHit, hookMovingSpeed, LayerMask.GetMask("Ground"));

            //If that ray hits
            if(hit)
            {

                //Put the hook where the ray hit
                currentHook.position = hookHit.point;

                //Make the hook point towards the surface
                currentHook.up = -hookHit.normal;

                //Change state to hooked
                hookState = "hooked";

            }

        }




    }
}
