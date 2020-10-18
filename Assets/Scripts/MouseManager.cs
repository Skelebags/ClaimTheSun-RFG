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
    protected GameObject[] placeableObjectPrefabs;

    [SerializeField]
    [Tooltip("The Hotkey to place this building")]
    //private KeyCode[] hotKeys = { KeyCode.Alpha1 };
    //private string[] buildingIDs;
    private List<string> buildingIDs;

    [SerializeField]
    [Tooltip("Which team this entity is on")]
    [Range(0, 9)]
    protected int team = 0;

    [SerializeField]
    [Tooltip("The selection indicator prefab")]
    private GameObject selectionIndicator;

    [SerializeField]
    [Tooltip("The selection box")]
    private RectTransform selectionBox;

    [SerializeField]
    [Tooltip("The minimum change in position before the selection box will be drawn")]
    private float boxSelectBuffer = 0.5f;
    private Vector2 boxStartPos;

    public enum MouseState { idle, placing};
    public MouseState mouseState;

    public List<GameObject> selectedObjects;
    
    protected Dictionary<string, GameObject> buildingDict;

    private GameObject currentPlaceableObject;

    private float mouseWheelRotation;

    private bool canPlace;

    protected GameController gc;

    private LayerMask groundMask;
    private LayerMask sunMask;

    protected virtual void Start()
    {
        buildingIDs = new List<string>();
        selectedObjects = new List<GameObject>();
        groundMask = LayerMask.GetMask("Ground");
        sunMask = LayerMask.GetMask("Sunlight");
        buildingDict = new Dictionary<string, GameObject>();
 

        foreach(GameObject building in placeableObjectPrefabs)
        {
            buildingIDs.Add(building.GetComponent<BaseController>().GetID());
            buildingDict.Add(building.GetComponent<BaseController>().GetID(), building);
        }
        gc = GetComponent<GameController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (selectedObjects != null && selectedObjects.Count != 0)
        {
            foreach (GameObject gameObject in selectedObjects)
            {
                if (gameObject == null)
                {
                    selectedObjects.Remove(gameObject);
                }
            }

            HandleBuildingControls();
            HandleUnitControls();
        }
        else
        {
            //HandleNewObjectHotkey();
        }

        switch(mouseState)
        {
            case MouseState.idle:
                if (Input.GetMouseButtonDown(0))
                {
                    boxStartPos = Input.mousePosition;
                    if (!IsPointerOverUIElement())
                    {

                        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                        RaycastHit hitInfo;
                        if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, ~(groundMask | sunMask)))
                        {
                            GameObject hitObject = hitInfo.transform.root.gameObject;
                            Debug.Log(hitObject.name);

                            if (!Input.GetKey(KeyCode.LeftShift))
                            {
                                ClearSelection();
                            }

                            if (hitObject.GetComponent<BaseController>() && hitObject.GetComponent<BaseController>().GetTeam() == team)
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
                if (Input.GetMouseButton(0) && (boxStartPos - (Vector2)Input.mousePosition).magnitude > boxSelectBuffer)
                {
                    UpdateSelectionBox(Input.mousePosition);
                }
                if (Input.GetMouseButtonUp(0))
                {
                    ReleaseSelectionBox();
                }
                break;
            case MouseState.placing:
                MoveCurrentObjectToMouse();
                RotateFromMouseWheel();
                ReleaseIfClicked();
                break;

        }
        if(selectedObjects.Count > 1)
        {
            foreach (GameObject gameObject in selectedObjects)
            {
                gameObject.GetComponent<BaseController>().ClearUI();
            }
        }
    }

    private void ClearSelection()
    {
        foreach (GameObject gameObject in selectedObjects)
        {
            if(gameObject.CompareTag("Building"))
            {
                if(gameObject.GetComponent<SpawnBuildingController>())
                {
                    gameObject.GetComponent<SpawnBuildingController>().RallyPointVisible(false);
                }
            }

            gameObject.GetComponent<BaseController>().ClearUI();
        }

        foreach (GameObject gameObject in GameObject.FindGameObjectsWithTag("Selection_Indicator"))
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

                selectedObjects[0].GetComponent<BaseController>().GenerateUI();

                if (selectedObjects[0].CompareTag("Building"))
                {
                    if (selectedObjects[0].GetComponent<SpawnBuildingController>())
                    {
                        selectedObjects[0].GetComponent<SpawnBuildingController>().RallyPointVisible(true);
                        for (int i = 0; i < selectedObjects[0].GetComponent<SpawnBuildingController>().GetButtons().Length; i++)
                        {
                            int index = i;
                            selectedObjects[0].GetComponent<SpawnBuildingController>().GetButtons()[i].onClick.AddListener(() => BuildingButtonControl(index));
                            selectedObjects[0].GetComponent<SpawnBuildingController>().GetButtons()[i].GetComponentInChildren<Text>().text = selectedObjects[0].GetComponent<SpawnBuildingController>().GetUnitIDs()[index];
                        }
                    }
                    if(selectedObjects[0].GetComponent<HQBuildingController>())
                    {
                        for (int i = 0; i < selectedObjects[0].GetComponent<HQBuildingController>().GetButtons().Length; i++)
                        {
                            int index = i;
                            selectedObjects[0].GetComponent<HQBuildingController>().GetButtons()[i].onClick.AddListener(() => PlaceBuilding(buildingIDs[index]));
                            selectedObjects[0].GetComponent<HQBuildingController>().GetButtons()[i].GetComponentInChildren<Text>().text = buildingIDs[index];
                        }
                    }
                }


            }
        }
    }

    private void UpdateSelectionBox(Vector2 currentMousePos)
    {
        if(!selectionBox.gameObject.activeInHierarchy)
        {
            selectionBox.gameObject.SetActive(true);
        }

        float width = currentMousePos.x - boxStartPos.x;
        float height = currentMousePos.y - boxStartPos.y;

        selectionBox.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));
        selectionBox.anchoredPosition = boxStartPos + new Vector2(width / 2, height / 2);
    }

    private void ReleaseSelectionBox()
    {
        if(selectionBox.gameObject.activeInHierarchy)
        {
            selectionBox.gameObject.SetActive(false);

            Vector2 min = selectionBox.anchoredPosition - (selectionBox.sizeDelta / 2);
            Vector2 max = selectionBox.anchoredPosition + (selectionBox.sizeDelta / 2);

            if(GameObject.FindGameObjectsWithTag("Unit") != null)
            {
                foreach (GameObject unit in GameObject.FindGameObjectsWithTag("Unit"))
                {
                    if (unit != null)
                    {
                        if (unit.GetComponent<UnitController>())
                        {
                            if (unit.GetComponent<UnitController>().GetTeam() == team)
                            {
                                Vector3 screenMin = Camera.main.WorldToScreenPoint(unit.GetComponent<Collider>().bounds.min);
                                Vector3 screenMax = Camera.main.WorldToScreenPoint(unit.GetComponent<Collider>().bounds.max);
                                if (screenMax.x > min.x && screenMin.x < max.x && screenMax.y > min.y && screenMin.y < max.y)
                                {
                                    SelectObject(unit);
                                }
                            }
                        }
                    }
                }
            }
            
        }
    }

    //private void HandleNewObjectHotkey()
    //{
        //foreach (KeyCode hotKey in hotKeys)
        //{
        //    if (Input.GetKeyDown(hotKey))
        //    {
        //        if (currentPlaceableObject != null)
        //        {
        //            Destroy(currentPlaceableObject);
        //            mouseState = MouseState.idle;
        //        }
        //        else
        //        {
        //            currentPlaceableObject = Instantiate(keyObjDict[hotKey]);
        //            currentPlaceableObject.GetComponent<BaseController>().SetTeam(team);
        //            mouseState = MouseState.placing;
        //        }
        //    }
        //}
    //}

    private void PlaceBuilding(string id)
    {
        if (currentPlaceableObject != null)
        {
            Destroy(currentPlaceableObject);
            mouseState = MouseState.idle;
        }
        else
        {
            currentPlaceableObject = Instantiate(buildingDict[id]);
            currentPlaceableObject.GetComponent<BaseController>().SetTeam(team);
            mouseState = MouseState.placing;
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
                    if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, ~sunMask))
                    {
                        GameObject hitObject = hitInfo.transform.root.gameObject;
                        if (hitObject.CompareTag("Ground"))
                        {
                            selectedObjects[0].GetComponent<SpawnBuildingController>().SetRallyPointPosition(hitInfo.point);
                        }
                    }
                }
            }
            if(selectedObjects[0].GetComponent<GeneratorBuildingController>())
            {
                selectedObjects[0].GetComponent<GeneratorBuildingController>().UpdateUI();
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
            string id = selectedObjects[0].GetComponent<SpawnBuildingController>().GetUnitIDs()[index];
            if (gc.CanAfford(selectedObjects[0].GetComponent<SpawnBuildingController>().GetUnitCost(id)))
            {
                gc.SpendEnergy(selectedObjects[0].GetComponent<SpawnBuildingController>().GetUnitCost(id));
                selectedObjects[0].GetComponent<SpawnBuildingController>().AddToQueue(id);
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

                if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, ~sunMask))
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
        if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, groundMask) && hitInfo.normal == Vector3.up)
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

        if (Input.GetKeyDown(KeyCode.Escape) && currentPlaceableObject != null)
        {
            Destroy(currentPlaceableObject);
            mouseState = MouseState.idle;
        }
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
