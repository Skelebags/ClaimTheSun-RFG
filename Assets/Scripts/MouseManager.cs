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
    private MeshRenderer[] currentMeshRenderers;

    private float mouseWheelRotation;

    private bool canPlace;

    private Color[] baseColors;

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
        HandleNewObjectHotkey();

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
                    currentMeshRenderers = null;
                    baseColors = null;
                }
                else
                {
                    currentPlaceableObject = Instantiate(keyObjDict[hotKey]);
                    currentMeshRenderers = currentPlaceableObject.GetComponentsInChildren<MeshRenderer>();
                    baseColors = new Color[currentMeshRenderers.Length];
                    for(int i = 0; i < currentMeshRenderers.Length; i++)
                    {
                        baseColors[i] = currentMeshRenderers[i].material.color;
                    }
                    
                    mouseState = MouseState.placing;
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
            if(hitInfo.collider.gameObject.CompareTag("Ground"))
            {
                currentPlaceableObject.SetActive(true);
                canPlace = true;
                currentPlaceableObject.transform.position = hitInfo.point;
                currentPlaceableObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
                foreach(MeshRenderer meshRenderer in currentMeshRenderers)
                {
                    meshRenderer.material.color = new Color(0f, 0f, 1f, 0.25f);
                }
            }
        }
        else
        {
            foreach (MeshRenderer meshRenderer in currentMeshRenderers)
            {
                meshRenderer.material.color = new Color(1f, 0f, 0f, 0.25f);
            }
            canPlace = false;
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
            for(int i = 0; i < currentMeshRenderers.Length; i++)
            {
                currentMeshRenderers[i].material.color = baseColors[i];
            }
            currentPlaceableObject = null;
            currentMeshRenderers = null;
            baseColors = null;
            mouseState = MouseState.idle;
        }
    }
}
