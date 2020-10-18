using UnityEngine;

public class AIController : MouseManager
{
    private enum State { build, recruit, attack, lose};
    [SerializeField]
    private State state;

    [SerializeField]
    [Tooltip("The hq of the ai")]
    private GameObject hqBuilding;

    [SerializeField]
    [Tooltip("The maximum point of the build zone")]
    private Transform maxBuildZone;

    [SerializeField]
    [Tooltip("The minimum point of the build zone")]
    private Transform minBuildZone;

    [SerializeField]
    [Tooltip("How much energy generators the AI wants to have built")]
    private int desiredGenerators = 4;
    private int currentGenerators;

    [SerializeField]
    [Tooltip("How many of each factory does the AI want to have built")]
    private int desiredFactories = 1;
    private int currentInfantryFact;
    private int currentVehicleFact;
    private int currentAirFact;

    [SerializeField]
    [Tooltip("How many of each unit does the AI want to have built")]
    private int desiredUnits = 2;
    private int currentInfantryUnit;
    private int currentVehicleUnit;
    private int currentAirUnit;

    private GameObject closestSunShaft;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        state = State.build;

        closestSunShaft = FindClosestSunShaft();
        currentGenerators = 0;
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
        }


        if (NeedsToBuild())
        {
            state = State.build;
        } else if(NeedsToRecruit())
        {
            state = State.recruit;
        } else
        {
            state = State.attack;
        }


        if (hqBuilding == null)
        {
            state = State.lose;
        }

        switch (state)
        {
            case State.build:
                Build();
                break;

            case State.recruit:
                Recruit();
                break;

            case State.attack:
                Attack();
                break;

            case State.lose:
                Lose();
                break;
        }

                
    }

    private void Lose()
    {
        foreach (GameObject unit in GameObject.FindGameObjectsWithTag("Unit"))
        {
            if (unit.GetComponent<UnitController>())
            {
                if (unit.GetComponent<UnitController>().GetTeam() == team)
                {
                    Destroy(unit);
                }
            }
        }

        foreach (GameObject building in GameObject.FindGameObjectsWithTag("Building"))
        {
            if (building.GetComponent<BuildingController>())
            {
                if (building.GetComponent<BuildingController >().GetTeam() == team)
                {
                    Destroy(building);
                }
            }
        }
    }

    private void Attack()
    {
        GameObject attackTarget = null;

        foreach (GameObject unit in GameObject.FindGameObjectsWithTag("Unit"))
        {
            if (unit.GetComponent<UnitController>())
            {
                if (unit.GetComponent<UnitController>().GetTeam() == team)
                {
                    selectedObjects.Add(unit);
                }
                else if(attackTarget == null)
                {
                    attackTarget = unit;
                }
            }
        }

        if (attackTarget == null)
        {
            foreach (GameObject building in GameObject.FindGameObjectsWithTag("Building"))
            {
                if (building.GetComponent<BuildingController>() && building.GetComponent<BuildingController>().GetTeam() != team)
                {
                    attackTarget = building;
                }
            }
        }

        if(attackTarget != null)
        {
            foreach(GameObject unit in selectedObjects)
            {
                if(unit.GetComponent<UnitController>() && unit.GetComponent<UnitController>().state != UnitController.State.attacking)
                {
                    unit.GetComponent<UnitController>().AttackOrder(attackTarget);
                }
            }
        }
    }

    private void Recruit()
    {
        foreach (GameObject building in GameObject.FindGameObjectsWithTag("Building"))
        {
            if (building.GetComponent<SpawnBuildingController>() && building.GetComponent<SpawnBuildingController>().GetTeam() == team)
            {
                if(building.GetComponent<SpawnBuildingController>().GetID() == "InfantryFactory" && currentInfantryUnit + building.GetComponent<SpawnBuildingController>().GetQueueSize() < desiredUnits)
                {
                    building.GetComponent<SpawnBuildingController>().AddToQueue("InfantryUnit");
                }
                if (building.GetComponent<SpawnBuildingController>().GetID() == "VehicleFactory" && currentVehicleUnit + building.GetComponent<SpawnBuildingController>().GetQueueSize() < desiredUnits)
                {
                    building.GetComponent<SpawnBuildingController>().AddToQueue("VehicleUnit");
                }
                if (building.GetComponent<SpawnBuildingController>().GetID() == "AirFactory" && currentAirUnit + building.GetComponent<SpawnBuildingController>().GetQueueSize() < desiredUnits)
                {
                    building.GetComponent<SpawnBuildingController>().AddToQueue("AirUnit");
                }
            }
        }
    }

    private bool NeedsToRecruit()
    {
        currentInfantryUnit = 0;
        currentVehicleUnit = 0;
        currentAirUnit = 0;

        foreach (GameObject unit in GameObject.FindGameObjectsWithTag("Unit"))
        {
            if (unit.GetComponent<UnitController>() && unit.GetComponent<UnitController>().GetTeam() == team)
            {
                if (unit.GetComponent<UnitController>().GetID() == "InfantryUnit")
                {
                    currentInfantryUnit++;
                }

                if (unit.GetComponent<UnitController>().GetID() == "VehicleUnit")
                {
                    currentVehicleUnit++;
                }

                if (unit.GetComponent<UnitController>().GetID() == "AirUnit")
                {
                    currentAirUnit++;
                }
            }
        }

        if (currentInfantryUnit >= desiredUnits && currentVehicleUnit >= desiredUnits && currentAirUnit >= desiredUnits)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private void Build()
    {
        if (gc != null)
        {
            if (currentGenerators < desiredGenerators)
            {

                if (gc.CanAfford(buildingDict["Generator"].GetComponent<BaseController>().GetBuildCost()))
                {
                    Debug.Log("Place generator");
                    PlaceBuilding("Generator", GetRandomPointInBounds(closestSunShaft.GetComponent<Collider>()));
                }

            }
            else if (currentInfantryFact < desiredFactories)
            {

                if (gc.CanAfford(buildingDict["InfantryFactory"].GetComponent<BaseController>().GetBuildCost()))
                {
                    Debug.Log("Place infantry factory");
                    PlaceBuilding("InfantryFactory", GetRandomPointInRange(minBuildZone, maxBuildZone));
                }

            }
            else if (currentVehicleFact < desiredFactories)
            {

                if (gc.CanAfford(buildingDict["VehicleFactory"].GetComponent<BaseController>().GetBuildCost()))
                {
                    Debug.Log("Place vehicle factory");
                    PlaceBuilding("VehicleFactory", GetRandomPointInRange(minBuildZone, maxBuildZone));
                }

            }
            else if (currentAirFact < desiredFactories)
            {
                if (gc.CanAfford(buildingDict["AirFactory"].GetComponent<BaseController>().GetBuildCost()))
                {
                    Debug.Log("Place air factory");
                    PlaceBuilding("AirFactory", GetRandomPointInRange(minBuildZone, maxBuildZone));
                }

            }
        }
    }

    private bool NeedsToBuild()
    {
        currentGenerators = 0;
        currentInfantryFact = 0;
        currentVehicleFact = 0;
        currentAirFact = 0;

        foreach (GameObject building in GameObject.FindGameObjectsWithTag("Building"))
        {
            if (building.GetComponent<BuildingController>() && building.GetComponent<BuildingController>().GetTeam() == team)
            {
                if (building.GetComponent<BuildingController>().GetID() == "Generator")
                {
                    currentGenerators++;
                }

                if (building.GetComponent<BuildingController>().GetID() == "InfantryFactory")
                {
                    currentInfantryFact++;
                }

                if (building.GetComponent<BuildingController>().GetID() == "VehicleFactory")
                {
                    currentVehicleFact++;
                }

                if (building.GetComponent<BuildingController>().GetID() == "AirFactory")
                {
                    currentAirFact++;
                }
            }
        }

        if(currentGenerators >= desiredGenerators && currentInfantryFact >= desiredFactories && currentVehicleFact >= desiredFactories && currentAirFact >= desiredFactories)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private void PlaceBuilding(string id, Vector3 position)
    {
        GameObject currentPlaceableObject = Instantiate(buildingDict[id], position, Quaternion.identity);
        if (!currentPlaceableObject.GetComponent<BuildingController>().GetIntersecting())
        {
            currentPlaceableObject.GetComponent<BaseController>().SetTeam(team);
            currentPlaceableObject.GetComponent<BuildingController>().Place();
            gc.SpendEnergy(currentPlaceableObject.GetComponent<BuildingController>().GetBuildCost());
        }
        else
        {
            Destroy(currentPlaceableObject);
        }
    }

    private GameObject FindClosestSunShaft()
    {
        GameObject tempSunShaft = null;

        if (GameObject.FindGameObjectsWithTag("Sunlight") != null)
        {
            foreach (GameObject sunShaft in GameObject.FindGameObjectsWithTag("Sunlight"))
            {
                if (tempSunShaft == null || (sunShaft.GetComponentInChildren<Collider>().ClosestPointOnBounds(hqBuilding.transform.position) - hqBuilding.transform.position).magnitude < (tempSunShaft.GetComponentInChildren<Collider>().ClosestPointOnBounds(hqBuilding.transform.position) - hqBuilding.transform.position).magnitude)
                {
                    tempSunShaft = sunShaft;
                }
            }
        }

        return tempSunShaft;
    }

    private Vector3 GetRandomPointInBounds(Collider collider)
    {
        Vector3 point =  new Vector3(
            Random.Range(collider.bounds.min.x, collider.bounds.max.x),
            0f,
            Random.Range(collider.bounds.min.y, collider.bounds.max.y));

        //if(point != collider.ClosestPoint(point))
        //{
        //    point = GetRandomPointInBounds(collider);
        //}

        return point;
    }

    private Vector3 GetRandomPointInRange(Transform minTransform, Transform maxTransform)
    {
        return new Vector3(Random.Range(minTransform.position.x, maxTransform.position.x), 0f, Random.Range(minTransform.position.z, maxTransform.position.z));
    }
}
