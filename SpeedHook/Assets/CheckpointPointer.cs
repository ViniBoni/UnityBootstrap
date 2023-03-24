using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Mathf;

public class CheckpointPointer : MonoBehaviour
{

    public Camera cam;
    public Transform follow;

    public float z = .35f;


    Vector3 dir;
    Vector3 forward;
    float angle;

    



    // Update is called once per frame
    void Update()
    {

        dir = (follow.position - transform.position).normalized;
        forward = cam.transform.forward;

        angle = Vector3.Angle(dir, forward);


        Vector3 pos = Camera.main.WorldToViewportPoint(follow.position);
        if(angle < 90)
        {
            pos.x = Clamp01(pos.x);
            pos.y = Clamp01(pos.y);
        }
        else
        {
            pos.x = Clamp01(-pos.x);
            pos.y = Clamp01(-pos.y);

            bool inHorizontalEdge = pos.x == 0 || pos.x == 1;
            bool inVerticalEdge = pos.y == 0 || pos.y == 1;

            bool inEdge = inHorizontalEdge || inVerticalEdge;

            if(!inEdge)
            {
                bool closestToRight = Abs(pos.x - 1) < pos.x;
                bool closestToUp = Abs(pos.y - 1) < pos.y;

                float distanceHorizontal = closestToRight ? Abs(pos.x - 1) : pos.x;
                float distanceVertical = closestToUp ? Abs(pos.y - 1) : pos.y;

                bool closestHorizontal = distanceHorizontal < distanceVertical;

                if(closestHorizontal)
                    pos.x = closestToRight ? 1 : 0;        
                else
                    pos.y = closestToUp ? 1 : 0;
                
            }
        }
        pos.z = z;
        transform.position = Camera.main.ViewportToWorldPoint(pos);




    }
}
