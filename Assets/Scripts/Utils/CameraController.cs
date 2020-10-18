using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private float cameraSpeed = 1f;

    [SerializeField]
    private float panSpeed = 0.1f;

    [SerializeField]
    private float rotSpeed = 0.1f;

    [SerializeField]
    private float minMoveBuffer = 0.5f;

    [SerializeField]
    private MouseManager mm;

    private Vector3 startMousePos = Vector3.zero;
    private Vector3 startPanPos = Vector3.zero;

    private LayerMask groundMask;

    private void Start()
    {
        groundMask = LayerMask.GetMask("Ground");
    }

    // Update is called once per frame
    void Update()
    {
        // Move camera based on WASD movement with Q to move down and E to move up
        if(Input.GetKey(KeyCode.A))
        {
            transform.Translate(-Vector3.right * cameraSpeed);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.Translate(Vector3.right * cameraSpeed);
        }
        if (Input.GetKey(KeyCode.W))
        {
            transform.Translate(Vector3.forward * cameraSpeed);
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.Translate(-Vector3.forward * cameraSpeed);
        }
        if (Input.GetKey(KeyCode.Q))
        {
            transform.Translate(-Vector3.up * cameraSpeed);
        }
        if (Input.GetKey(KeyCode.E))
        {
            transform.Translate(Vector3.up * cameraSpeed);
        }

        if(!mm.HasAnySelected())
        {
            // Pan Camera on hold middle click
            if (Input.GetMouseButtonDown(2))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                RaycastHit hitInfo;
                if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, groundMask))
                {
                    startMousePos = Input.mousePosition;
                    startPanPos = hitInfo.point;
                }
                //startMousePos = Input.mousePosition;
            }
            if (Input.GetMouseButton(2))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                RaycastHit hitInfo;
                if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, groundMask))
                {
                    if ((Input.mousePosition - startMousePos).magnitude >= minMoveBuffer)
                    {
                        transform.Translate((hitInfo.point - startPanPos) * -panSpeed, Space.World);

                        startPanPos = hitInfo.point;
                        startMousePos = Input.mousePosition;
                    }
                }
                //transform.Translate((Input.mousePosition - startMousePos) * -panSpeed);
                //startMousePos = Input.mousePosition;
            }
            if (Input.GetMouseButtonUp(2))
            {
                startMousePos = Vector3.zero;
            }
        }
        
        // Rotate camera on hold right click
        if (Input.GetMouseButtonDown(1))
        {
            startMousePos = Input.mousePosition;
        }
        if (Input.GetMouseButton(1))
        {
            transform.Rotate((Input.mousePosition.y - startMousePos.y) * rotSpeed, (Input.mousePosition.x - startMousePos.x) * rotSpeed, 0 , Space.World);
            // Clamp rotations
            transform.eulerAngles = new Vector3(Mathf.Clamp(transform.eulerAngles.x, 10, 90), transform.eulerAngles.y, Mathf.Clamp(transform.eulerAngles.z, 0f, 0f));
            startMousePos = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(1))
        {
            startMousePos = Vector3.zero;
        }

        // Zoom out with scroll wheel
        if(mm.mouseState == MouseManager.MouseState.idle)
        {
            transform.Translate(Vector3.forward * Input.mouseScrollDelta.y);
        }
    }
}
