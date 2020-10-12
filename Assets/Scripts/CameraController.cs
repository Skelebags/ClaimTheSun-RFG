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

    private Vector3 startMousePos = Vector3.zero;

    // Update is called once per frame
    void Update()
    {
        // Move camera based on WASD movement with Q to move down and E to move up
        if(Input.GetKey(KeyCode.A))
        {
            transform.Translate(-Vector3.right * cameraSpeed, Space.World);
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

        // Pan Camera on hold left click
        if(Input.GetMouseButtonDown(0))
        {
            startMousePos = Input.mousePosition;
        }
        if(Input.GetMouseButton(0))
        {
            transform.Translate(-(Input.mousePosition - startMousePos) * panSpeed);
            startMousePos = Input.mousePosition;
        }
        if(Input.GetMouseButtonUp(0))
        {
            startMousePos = Vector3.zero;
        }
        // Rotate camera on hold middle click
        if (Input.GetMouseButtonDown(2))
        {
            startMousePos = Input.mousePosition;
        }
        if (Input.GetMouseButton(2))
        {
            transform.Rotate((Input.mousePosition.y - startMousePos.y) * rotSpeed, (Input.mousePosition.x - startMousePos.x) * rotSpeed, 0 , Space.World);
            // Clamp rotations
            transform.eulerAngles = new Vector3(Mathf.Clamp(transform.eulerAngles.x, 10, 90), transform.eulerAngles.y, Mathf.Clamp(transform.eulerAngles.z, 0f, 0f));
            startMousePos = Input.mousePosition;
        }
        if (Input.GetMouseButtonUp(2))
        {
            startMousePos = Vector3.zero;
        }
    }
}
