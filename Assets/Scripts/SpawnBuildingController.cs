using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnBuildingController : BuildingController
{
    [SerializeField]
    [Tooltip("The unit prefab that this building can spawn")]
    private GameObject[] unitPrefabs;

    [SerializeField]
    [Tooltip("The Hotkey to place build each unit")]
    private KeyCode[] hotKeys = { KeyCode.Alpha1 };

    [SerializeField]
    [Tooltip("The Maximum size of the build queue")]
    private const int QUEUE_MAX = 5;

    [SerializeField]
    [Tooltip("The distance from the building that it spawns units")]
    private float spawnDist = 1f;

    [SerializeField]
    [Tooltip("This building's rallypoint")]
    private GameObject rallyPoint;

    private Dictionary<KeyCode, GameObject> keyObjDict;

    private List<GameObject> buildQueue;
    
    private float unitTimer;

    protected override void Awake()
    {
        base.Awake();
        buildQueue = new List<GameObject>();
        unitTimer = 0f;
        keyObjDict = new Dictionary<KeyCode, GameObject>();
        for (int i = 0; i < hotKeys.Length; i++)
        {
            keyObjDict.Add(hotKeys[i], unitPrefabs[i]);
        }
    }

    protected override void Update()
    {
        base.Update();

        if(state == State.ready)
        {
            if (buildQueue.Count > 0)
            {
                unitTimer += Time.deltaTime;
                if (unitTimer >= buildQueue[0].GetComponent<UnitController>().GetBuildTime())
                {
                    SpawnUnit(buildQueue[0]);
                    buildQueue.RemoveAt(0);
                    unitTimer = 0f;
                }
            }
        }
        else
        {
            RallyPointVisible(false);
        }


    }

    public void AddToQueue(KeyCode key)
    {
        if (buildQueue.Count < QUEUE_MAX)
        {
            buildQueue.Add(keyObjDict[key]);
        }
    }

    private void SpawnUnit(GameObject unit)
    {
        Vector3 spawnPos = new Vector3(transform.position.x, transform.position.y - GetComponentInChildren<Collider>().bounds.extents.y + unit.GetComponentInChildren<Collider>().bounds.extents.y, transform.position.z);
        spawnPos = spawnPos + transform.forward * spawnDist;
        GameObject newUnit = Instantiate(unit);
        newUnit.transform.position = spawnPos;
        newUnit.GetComponent<UnitController>().SetTeam(team);
        newUnit.GetComponent<UnitController>().MoveOrder(rallyPoint.transform.position);
    }

    public KeyCode[] GetHotKeys()
    {
        return hotKeys;
    }

    public float GetUnitCost(KeyCode key)
    {
        return keyObjDict[key].GetComponent<UnitController>().GetBuildCost();
    }

    public void RallyPointVisible(bool state)
    {
        if (rallyPoint != null)
        {
            rallyPoint.GetComponentInChildren<MeshRenderer>().enabled = state;
        }
    }

    public void SetRallyPointPosition(Vector3 position)
    {
        if (rallyPoint != null)
        {
            rallyPoint.transform.position = position;
        }
    }
}
