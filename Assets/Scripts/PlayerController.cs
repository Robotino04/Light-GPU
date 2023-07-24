using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    public float turnSpeed = 1000.0f;
    public float flySpeed = 5;
    private Vector3 translation = Vector3.zero;
    private Vector3 rotation = Vector3.zero;
    private Vector3 cameraRotation = Vector3.zero;
    public Transform playerCamera;
    private bool CursorCatch = false;

    // Start is called before the first frame update
    void Start()
    {
        if (CursorCatch)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    // Update is called once per frame
    void Update()
    {
        translation.z = Input.GetAxis("Vertical") * moveSpeed;
        translation.x = Input.GetAxis("Horizontal") * moveSpeed;
        translation.y = Input.GetAxis("Fly") * flySpeed;

        rotation.y = Input.GetAxis("Mouse X") * turnSpeed;
        cameraRotation.x = -Input.GetAxis("Mouse Y") * turnSpeed;

        if (Input.GetButtonDown("Cursor Mode"))
        {
            CursorCatch = !CursorCatch;
        }
        if (CursorCatch)
        {
            Cursor.lockState = CursorLockMode.Locked;
            transform.Rotate(rotation * Time.deltaTime);
            playerCamera.Rotate(cameraRotation * Time.deltaTime);
            transform.Translate(translation * Time.deltaTime);
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }



    }
}
