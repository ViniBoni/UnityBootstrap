using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Look : MonoBehaviour
{
    public float sensitivity = 1;
    public float smoothing = 2;


    private Transform charCamera;
    private Vector2 currentMouseLook;

    void Start()
    {
        //Lock the cursor so it doesn't move off-screen
        Cursor.lockState = CursorLockMode.Locked;

        //Make the cursor invisible
        Cursor.visible = false;

        //Get the camera
        charCamera = Camera.main.transform;
        
    }

    void Update()
    {
        Vector2 newMouseDelta = Vector2.Scale(new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")), Vector2.one * sensitivity);
        currentMouseLook += newMouseDelta;
        currentMouseLook.y = Mathf.Clamp(currentMouseLook.y, -90, 90);
        charCamera.localRotation = Quaternion.AngleAxis(-currentMouseLook.y, Vector3.right);
        transform.localRotation = Quaternion.AngleAxis(currentMouseLook.x, Vector3.up);
    
    }
}
