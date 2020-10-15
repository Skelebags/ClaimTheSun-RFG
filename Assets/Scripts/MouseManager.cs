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

    [SerializeField]
    [Tooltip("The selection indicator prefab")]
    private GameObject selectionIndicator;

    public enum MouseState { idle, placing};
    public MouseState mouseState;

    public List<GameObject> selectedObjects;

    private Dictionary<KeyCode, GameObject> keyObjDict;

    private GameObject currentPlaceableObject;

    private float mouseWheelRotation;

    private bool canPlace;

    private GameController gc;

    private LayerMask groundMask;

    void Start()
    {
        selectedObjects = new List<GameObject>();
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
        if (selectedObjects != null && selectedObjects.Count != 0)
        {
            //switch (selectedObjects[0].tag)
            //{
            //    case "Building":
            //        HandleBuildingControls();
            //        break;
            //    case "Unit":
            //        HandleUnitControls();
            //        break;
            //}
            HandleBuildingControls();
            HandleUnitControls();
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

                            if(!Input.GetKey(KeyCode.LeftShift))
                            {
                                ClearSelection();
                            }

                            if (hitObject.GetComponent<BaseController>().GetTeam() == team)
                            {
                                SelectObject(hitObject);

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
        if (selectedObjects != null && selectedObjects.Count != 0)
        {
            if (selectedObjects[0].CompareTag("Building"))
            {
                if(selectedObjects[0].GetComponent<SpawnBuildingController>())
                {
                    selectedObjects[0].GetComponent<SpawnBuildingController>().RallyPointVisible(false);
                }
            }

            selectedObjects[0].GetComponent<BaseController>().ClearUI();
        }
        foreach(GameObject gameObject in GameObject.FindGameObjectsWithTag("Selection_Indicator"))
        {
            Destroy(gameObject);
        }
        selectedObjects.Clear();
    }

    private void SelectObject(GameObject obj)
    {
        if (selectedObjects != null)
        {
            selectedObjects.Add(obj);
            GameObject indicator = Instantiate(selectionIndicator, selectedObjects[0].transform.position, selectedObjects[0].transform.rotation);
            indicator.GetComponent<SelectionIndicator>().Attach(obj);
            if(selectedObjects.Count == 1)
            {
                if (selectedObjects[0].CompareTag("Building"))
                {
                    if (selectedObjects[0].GetComponent<SpawnBuildingController>())
                    {
                        selectedObjects[0].GetComponent<SpawnBuildingController>().RallyPointVisible(true);
                    }
                }

                selectedObjects[0].GetComponent<BaseController>().GenerateUI();

                for (int i = 0; i < selectedObjects[0].GetComponent<BaseController>().GetButtons().Length; i++)
                {
                    int index = i;
                    selectedObjects[0].GetComponent<BaseController>().GetButtons()[i].onClick.AddListener(() => BuildingButtonControl(index));
                }
            }
            else
            {
                foreach(GameObject gameObject in selectedObjects)
                {


                    gameObject.GetComponent<BaseController>().ClearUI();
                }
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
        if (selectedObjects != null && selectedObjects.Count == 1)
        {
            if (selectedObjects[0].GetComponent<SpawnBuildingController>())
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
                            selectedObjects[0].GetComponent<SpawnBuildingController>().SetRallyPointPosition(hitInfo.point);
                        }
                    }
                }

                foreach (KeyCode hotKey in selectedObjects[0].GetComponent<SpawnBuildingController>().GetHotKeys())
                {
                    if (Input.GetKeyDown(hotKey))
                    {
                        if (gc.CanAfford(selectedObjects[0].GetComponent<SpawnBuildingController>().GetUnitCost(hotKey)))
                        {
                            gc.SpendEnergy(selectedObjects[0].GetComponent<SpawnBuildingController>().GetUnitCost(hotKey));
                            selectedObjects[0].GetComponent<SpawnBuildingController>().AddToQueue(hotKey);
                        }
                        else
                        {
                            Debug.Log("Not Enough Energy");
                        }
                    }
                }
            }
        } else 
        if(selectedObjects.Count > 1)
        {
            foreach(GameObject gameObject in selectedObjects)
            {
                if(gameObject.GetComponent<SpawnBuildingController>())
                {
                    gameObject.GetComponent<SpawnBuildingController>().RallyPointVisible(false);
                }
            }
        }
    }

    private void BuildingButtonControl(int index)
    {
        if (selectedObjects[0].GetComponent<SpawnBuildingController>())
        {
            KeyCode hotKey = selectedObjects[0].GetComponent<SpawnBuildingController>().GetHotKeys()[index];
            if (gc.CanAfford(selectedObjects[0].GetComponent<SpawnBuildingController>().GetUnitCost(hotKey)))
            {
                gc.SpendEnergy(selectedObjects[0].GetComponent<SpawnBuildingController>().GetUnitCost(hotKey));
                selectedObjects[0].GetComponent<SpawnBuildingController>().AddToQueue(hotKey);
            }
            else
            {
                Debug.Log("Not Enough Energy");
            }
        }
    }

    private void HandleUnitControls()
    {
        List<UnitController> unitControllers = new List<UnitController>();
        foreach(GameObject gameObject in selectedObjects)
        {
            if(gameObject.CompareTag("Unit"))
            {
                unitControllers.Add(gameObject.GetComponent<UnitController>());
            }
        }
        if(unitControllers.Count == 1)
        {
            unitControllers[0].UpdateUI();
        }

        if (selectedObjects != null && selectedObjects.Count != 0)
        {
            // Move the selected unit with the right mouse button
            if(Input.GetMouseButtonDown(1))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                RaycastHit hitInfo;

                if (Physics.Raycast(ray, out hitInfo))
                {
                    GameObject hitObject = hitInfo.transform.root.gameObject;
                    foreach(UnitController controller in unitControllers)
                    {
                        if (hitObject.CompareTag("Ground"))
                        {
                            controller.MoveOrder(hitInfo.point);
                        }
                        if (hitObject.CompareTag("Building") || hitObject.CompareTag("Unit"))
                        {
                            if (hitObject.GetComponent<BaseController>().GetTeam() != team)
                            {
                                controller.AttackOrder(hitObject);
                            }
                            else
                            {
                                controller.MoveOrder(hitObject.transform.position);
                            }
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
