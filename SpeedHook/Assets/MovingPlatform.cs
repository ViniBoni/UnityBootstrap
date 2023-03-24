using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovingPlatform : MonoBehaviour
{

    public List<Transform> points = new List<Transform>();
    public float speed = 10;
    int index;
    Rigidbody rb;
    Transform oldWaypoint;
    public float smoothing = 2;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = 99999;
        rb.angularDrag = 0;
        rb.useGravity = false;
        transform.position = points[0].position;
        index = 0;
    }

    //Accelerate speed based on distance from point A, then point B

    void FixedUpdate()
    {


        if(Vector3.Distance(transform.position, points[index].position) < .05f)
        {
            oldWaypoint = points[index];
            index++;
            if(index == points.Count)
                index = 0;
        }

        float oldDistance = Vector3.Distance(oldWaypoint.position, transform.position);
        float nextDistance = Vector3.Distance(points[index].position, transform.position);

        float closestDistance = oldDistance < nextDistance ? oldDistance : nextDistance;

        closestDistance = Mathf.Clamp(closestDistance / smoothing, .01f, 1f);

        //destination - origin
        Vector3 newVelocity = (points[index].position - transform.position).normalized * speed * closestDistance;

        rb.velocity = newVelocity;

    }



    void OnDrawGizmos()
    {
        
        for (int i = 0; i < points.Count; i++)
        {
            
            Gizmos.color = Color.blue;
            if(points[i] != null) Gizmos.DrawSphere(points[i].position, .1f);
            int previousPoint = ( i == 0 ) ? points.Count - 1 : i - 1;
            if(points[i] != null && points[previousPoint] != null) Gizmos.DrawLine(points[previousPoint].position, points[i].position);
        }
        
    }
}
