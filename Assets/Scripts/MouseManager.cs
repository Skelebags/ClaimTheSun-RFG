using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MouseManager : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The Building prefab that this instance will spawn")]
    private GameObject[] placeableObjectPrefabs;

    [SerializeField]
    [Tooltip("The Hotkey to place this building")]
    private KeyCode[] hotKeys = { KeyCode.Alpha1 };

    [SerializeField]
    [Tooltip("Which team this entity is on")]
    [Range(0, 9)]
    protected int team = 0;


    public enum MouseState { idle, placing};
    public MouseState mouseState;

    public GameObject selectedObject;

    private Dictionary<KeyCode, GameObject> keyObjDict;

    private GameObject currentPlaceableObject;

    private float mouseWheelRotation;

    private bool canPlace;

    private GameController gc;

    private LayerMask groundMask;

    void Start()
    {
        groundMask = LayerMask.GetMask("Ground");
        keyObjDict = new Dictionary<KeyCode, GameObject>();
        for(int i = 0; i < hotKeys.Length; i++)
        {
            keyObjDict.Add(hotKeys[i], placeableObjectPrefabs[i]);
        }
        gc = GetComponent<GameController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (selectedObject != null)
        {
            switch (selectedObject.tag)
            {
                case "Building":
                    HandleBuildingControls();
                    break;
                case "Unit":
                    HandleUnitControls();
                    break;
            }
        }
        else
        {
            HandleNewObjectHotkey();
        }

        switch(mouseState)
        {
            case MouseState.idle:
                if (Input.GetMouseButtonDown(0))
                {
                    if (!IsPointerOverUIElement())
                    {
                        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                        RaycastHit hitInfo;
                        if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, ~groundMask))
                        {
                            GameObject hitObject = hitInfo.transform.root.gameObject;
                            Debug.Log(hitObject.name);

                            if (selectedObject != null)
                            {
                                ClearSelection();
                            }

                            if (hitObject.GetComponent<BaseController>().GetTeam() == team)
                            {
                                SelectObject(hitObject);
                                hitObject.GetComponent<BaseController>().GenerateUI();

                                
                                for (int i = 0; i < selectedObject.GetComponent<BaseController>().GetButtons().Length; i++)
                                {
                                    Debug.Log(i);
                                    int index = i;
                                    selectedObject.GetComponent<BaseController>().GetButtons()[i].onClick.AddListener(() => BuildingButtonControl(index));
                                }
                            }
                        }
                        else
                        {
                            ClearSelection();
                        }
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
        if(selectedObject != null)
        {
            if (selectedObject.CompareTag("Building"))
            {
                if(selectedObject.GetComponent<SpawnBuildingController>())
                {
                    selectedObject.GetComponent<SpawnBuildingController>().RallyPointVisible(false);
                }
            }

            selectedObject.GetComponent<BaseController>().ClearUI();
        }

        selectedObject = null;
    }

    private void SelectObject(GameObject obj)
    {
        selectedObject = obj;
        if (selectedObject.CompareTag("Building"))
        {
            if (selectedObject.GetComponent<SpawnBuildingController>())
            {
                selectedObject.GetComponent<SpawnBuildingController>().RallyPointVisible(true);
            }
        }
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
                    mouseState = MouseState.idle;
                }
                else
                {
                    currentPlaceableObject = Instantiate(keyObjDict[hotKey]);
                    currentPlaceableObject.GetComponent<BaseController>().SetTeam(team);
                    mouseState = MouseState.placing;
                }
            }
        }
    }

    private void HandleBuildingControls()
    {
        if(selectedObject != null)
        {
            if (selectedObject.GetComponent<SpawnBuildingController>())
            {

                if (Input.GetMouseButtonDown(1))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                    RaycastHit hitInfo;
                    if (Physics.Raycast(ray, out hitInfo))
                    {
                        GameObject hitObject = hitInfo.transform.root.gameObject;
                        if (hitObject.CompareTag("Ground"))
                        {
                            selectedObject.GetComponent<SpawnBuildingController>().SetRallyPointPosition(hitInfo.point);
                        }
                    }
                }

                foreach (KeyCode hotKey in selectedObject.GetComponent<SpawnBuildingController>().GetHotKeys())
                {
                    if (Input.GetKeyDown(hotKey))
                    {
                        if (gc.CanAfford(selectedObject.GetComponent<SpawnBuildingController>().GetUnitCost(hotKey)))
                        {
                            gc.SpendEnergy(selectedObject.GetComponent<SpawnBuildingController>().GetUnitCost(hotKey));
                            selectedObject.GetComponent<SpawnBuildingController>().AddToQueue(hotKey);
                        }
                        else
                        {
                            Debug.Log("Not Enough Energy");
                        }
                    }
                }
            }
        }
    }

    private void BuildingButtonControl(int index)
    {
        if (selectedObject.GetComponent<SpawnBuildingController>())
        {
            KeyCode hotKey = selectedObject.GetComponent<SpawnBuildingController>().GetHotKeys()[index];
            if (gc.CanAfford(selectedObject.GetComponent<SpawnBuildingController>().GetUnitCost(hotKey)))
            {
                gc.SpendEnergy(selectedObject.GetComponent<SpawnBuildingController>().GetUnitCost(hotKey));
                selectedObject.GetComponent<SpawnBuildingController>().AddToQueue(hotKey);
            }
            else
            {
                Debug.Log("Not Enough Energy");
            }
        }
    }

    private void HandleUnitControls()
    {
        UnitController unitController = selectedObject.GetComponent<UnitController>();
        unitController.UpdateUI();

        if (selectedObject != null)
        {
            // Move the selected unit with the right mouse button
            if(Input.GetMouseButtonDown(1))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                RaycastHit hitInfo;

                if (Physics.Raycast(ray, out hitInfo))
                {
                    GameObject hitObject = hitInfo.transform.root.gameObject;
                    if (hitObject.CompareTag("Ground"))
                    {
                        unitController.MoveOrder(hitInfo.point);
                    }
                    if(hitObject.CompareTag("Building") || hitObject.CompareTag("Unit"))
                    {
                        if(hitObject.GetComponent<BaseController>().GetTeam() != team)
                        {
                            unitController.AttackOrder(hitObject);
                        }
                        else
                        {
                            unitController.MoveOrder(hitObject.transform.position);
                        }
                    }
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
            currentPlaceableObject.transform.position = new Vector3(hitInfo.point.x, hitInfo.point.y + currentPlaceableObject.GetComponentInChildren<Collider>().bounds.extents.y, hitInfo.point.z);
            currentPlaceableObject.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
        }
        else
        {
            canPlace = false;
            mouseWheelRotation = 0;
        }
        if(!gc.CanAfford(currentPlaceableObject.GetComponent<BuildingController>().GetBuildCost()))
        {
            canPlace = false;
        }
        currentPlaceableObject.GetComponent<BuildingController>().SetPlaceable(canPlace);
    }

    private void RotateFromMouseWheel()
    {
        mouseWheelRotation += Input.mouseScrollDelta.y;
        currentPlaceableObject.transform.Rotate(Vector3.up, mouseWheelRotation * 10f);
    }

    private void ReleaseIfClicked()
    {
        if(Input.GetMouseButtonDown(0) && canPlace && !currentPlaceableObject.GetComponent<BuildingController>().GetIntersecting())
        {
            currentPlaceableObject.GetComponent<BuildingController>().Place();
            gc.SpendEnergy(currentPlaceableObject.GetComponent<BuildingController>().GetBuildCost());
            currentPlaceableObject = null;
            mouseState = MouseState.idle;
            mouseWheelRotation = 0f;
        }
    }

    public int GetTeam()
    {
        return team;
    }

    ///Returns 'true' if we touched or hovering on Unity UI element.
    public static bool IsPointerOverUIElement()
    {
        return IsPointerOverUIElement(GetEventSystemRaycastResults());
    }
    ///Returns 'true' if we touched or hovering on Unity UI element.
    public static bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaysastResults)
    {
        for (int index = 0; index < eventSystemRaysastResults.Count; index++)
        {
            RaycastResult curRaysastResult = eventSystemRaysastResults[index];
            if (curRaysastResult.gameObject.layer == LayerMask.NameToLayer("UI"))
                return true;
        }
        return false;
    }
    ///Gets all event systen raycast results of current mouse or touch position.
    static List<RaycastResult> GetEventSystemRaycastResults()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, raysastResults);
        return raysastResults;
    }
}
