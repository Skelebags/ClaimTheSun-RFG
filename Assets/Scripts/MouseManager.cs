using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The Building prefab that this instance will spawn")]
    private GameObject[] placeableObjectPrefabs;

    [SerializeField]
    [Tooltip("The Hotkey to place this building")]
    private KeyCode[] hotKeys = { KeyCode.Alpha1 };


    public enum MouseState { idle, placing};
    public MouseState mouseState;

    public GameObject selectedObject;

    private Dictionary<KeyCode, GameObject> keyObjDict;

    private GameObject currentPlaceableObject;

    private float mouseWheelRotation;

    private bool canPlace;

    private LayerMask groundMask;


    void Start()
    {
        groundMask = 1 << LayerMask.NameToLayer("Ground");
        keyObjDict = new Dictionary<KeyCode, GameObject>();
        for(int i = 0; i < hotKeys.Length; i++)
        {
            keyObjDict.Add(hotKeys[i], placeableObjectPrefabs[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (selectedObject == null)
        {
            HandleNewObjectHotkey();
        }
        else
        {
            HandleBuildingHotkey();
        }

        switch(mouseState)
        {
            case MouseState.idle:
                if (Input.GetMouseButtonDown(0))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                    RaycastHit hitInfo;
                    if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, ~groundMask))
                    {
                        GameObject hitObject = hitInfo.transform.root.gameObject;

                        SelectObject(hitObject);
                    }
                    else
                    {
                        ClearSelection();
                    }
                }
                break;
            case MouseState.placing:
                MoveCurrentObjectToMouse();
                RotateFromMouseWheel();
                ReleaseIfClicked();
                break;
        }
    }

    private void ClearSelection()
    {
        selectedObject = null;
    }

    private void SelectObject(GameObject obj)
    {
        selectedObject = obj;
    }

    private void HandleNewObjectHotkey()
    {
        foreach (KeyCode hotKey in hotKeys)
        {
            if (Input.GetKeyDown(hotKey))
            {
                if (currentPlaceableObject != null)
                {
                    Destroy(currentPlaceableObject);
                }
                else
                {
                    currentPlaceableObject = Instantiate(keyObjDict[hotKey]);
                    mouseState = MouseState.placing;
                }
            }
        }
    }

    private void HandleBuildingHotkey()
    {
        if(selectedObject != null)
        {
            foreach (KeyCode hotKey in selectedObject.GetComponent<BuildingController>().GetHotKeys())
            {
                if (Input.GetKeyDown(hotKey))
                {
                    selectedObject.GetComponent<BuildingController>().AddToQueue(hotKey);
                }
            }
        }
    }

    private void MoveCurrentObjectToMouse()
    {

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, groundMask))
        {
            canPlace = true;
            currentPlaceableObject.GetComponent<BuildingController>().SetPlaceable(canPlace);
            currentPlaceableObject.transform.position = new Vector3(hitInfo.point.x, hitInfo.point.y + currentPlaceableObject.GetComponentInChildren<Collider>().bounds.extents.y, hitInfo.point.z);
            currentPlaceableObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
        }
        else
        {
            canPlace = false;
            currentPlaceableObject.GetComponent<BuildingController>().SetPlaceable(canPlace);
        }
    }

    private void RotateFromMouseWheel()
    {
        Debug.Log(Input.mouseScrollDelta);
        mouseWheelRotation += Input.mouseScrollDelta.y;
        currentPlaceableObject.transform.Rotate(Vector3.up, mouseWheelRotation * 10f);
    }

    private void ReleaseIfClicked()
    {
        if(Input.GetMouseButtonDown(0) && canPlace)
        {
            currentPlaceableObject.GetComponent<BuildingController>().Place();
            currentPlaceableObject = null;
            mouseState = MouseState.idle;
        }
    }
}
