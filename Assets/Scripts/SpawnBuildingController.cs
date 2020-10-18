using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnBuildingController : BuildingController
{
    [SerializeField]
    [Tooltip("The unit prefab that this building can spawn")]
    private GameObject[] unitPrefabs;

    [SerializeField]
    [Tooltip("The IDs for each unit")]
    private List<string> unitIDs;

    [SerializeField]
    [Tooltip("The Maximum size of the build queue")]
    private const int QUEUE_MAX = 5;

    [SerializeField]
    [Tooltip("The distance from the building that it spawns units")]
    private float spawnDist = 1f;

    [SerializeField]
    [Tooltip("This building's rallypoint")]
    private GameObject rallyPoint;
    
    private Dictionary<string, GameObject> unitDict;

    private List<GameObject> buildQueue;
    
    private float unitTimer;

    protected override void Awake()
    {
        base.Awake();
        buildQueue = new List<GameObject>();
        unitTimer = 0f;
        unitDict = new Dictionary<string, GameObject>();
        unitIDs = new List<string>();

        foreach(GameObject unit in unitPrefabs)
        {
            unitIDs.Add(unit.GetComponent<BaseController>().GetID());
            unitDict.Add(unit.GetComponent<BaseController>().GetID(), unit);
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

    public void AddToQueue(string id)
    {
        if(buildQueue.Count < QUEUE_MAX)
        {
            buildQueue.Add(unitDict[id]);
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

    public List<string> GetUnitIDs()
    {
        return unitIDs;
    }

    public float GetUnitCost(string id)
    {
        return unitDict[id].GetComponent<UnitController>().GetBuildCost();
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

    public int GetQueueSize()
    {
        return buildQueue.Count;
    }
}
