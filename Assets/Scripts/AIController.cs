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

    [SerializeField]
    private GameObject closestSunShaft;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        state = State.build;

        //closestSunShaft = FindClosestSunShaft();
        currentGenerators = 0;
        currentInfantryUnit = 0;
        currentVehicleUnit = 0;
        currentAirUnit = 0;
    }

    // Update is called once per frame
    void Update()
    {
        closestSunShaft = FindClosestSunShaft();

        // if any selected objects have been destroyed, remove them from selected objects list
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

        // Check what the AI should be doing this frame
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

        // Have we lost?
        if (hqBuilding == null)
        {
            state = State.lose;
        }

        // Act based on state?
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

    // If we lose
    private void Lose()
    {
        // Destroy every AI controlled building and unit
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

    // If in the attack state
    private void Attack()
    {
        GameObject attackTarget = null;

       
        foreach (GameObject unit in GameObject.FindGameObjectsWithTag("Unit"))
        {
            if (unit.GetComponent<UnitController>())
            {
                // Select every AI controlled unit
                if (unit.GetComponent<UnitController>().GetTeam() == team)
                {
                    selectedObjects.Add(unit);
                }
                // Priority one is attacking enemy units
                else if(attackTarget == null)
                {
                    attackTarget = unit;
                }
            }
        }

        // Priority two is attacking enemy buildings 
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

        // send the attack order to every selected friendly unit
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

    // if we are in the recruit state
    private void Recruit()
    {
        // Add a unit to the queue of every friendly factory
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

    // Check if we need to recruit
    private bool NeedsToRecruit()
    {
        currentInfantryUnit = 0;
        currentVehicleUnit = 0;
        currentAirUnit = 0;

        // For every friendly unit type, check how many exist against how many we want
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

        // If we have enough, we don't need to recruit
        if (currentInfantryUnit >= desiredUnits && currentVehicleUnit >= desiredUnits && currentAirUnit >= desiredUnits)
        {
            return false;
        }
        // if not, then we recruit
        else
        {
            return true;
        }
    }

    // if in the build state
    private void Build()
    {
        if (gc != null)
        {
            // check which buildings we need more of, and build them
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

    // check if we need to build
    private bool NeedsToBuild()
    {
        currentGenerators = 0;
        currentInfantryFact = 0;
        currentVehicleFact = 0;
        currentAirFact = 0;

        // Count each type of friendly building
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

    // Place buildings at a given position
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
            // Loop through each sunshaft in the level
            foreach (GameObject sunShaft in GameObject.FindGameObjectsWithTag("Sunlight"))
            {
                // compare the closest point of each sunshaft to find the closest
                if (tempSunShaft == null || (sunShaft.GetComponent<Collider>().ClosestPointOnBounds(hqBuilding.transform.position) - hqBuilding.transform.position).magnitude < (tempSunShaft.GetComponent<Collider>().ClosestPointOnBounds(hqBuilding.transform.position) - hqBuilding.transform.position).magnitude)
                {
                    tempSunShaft = sunShaft;
                }
            }
        }

        return tempSunShaft;
    }

    // Get a random position within the bounds of a given collider
    private Vector3 GetRandomPointInBounds(Collider collider)
    {
        Vector3 point =  new Vector3(
            Random.Range(collider.bounds.min.x, collider.bounds.max.x),
            0f,
            Random.Range(collider.bounds.min.z, collider.bounds.max.z));

        if(point != collider.ClosestPoint(point))
        {
            point = GetRandomPointInBounds(collider);
        }

        return point;
    }

    // Gets a random position between two transforsm
    private Vector3 GetRandomPointInRange(Transform minTransform, Transform maxTransform)
    {
        return new Vector3(Random.Range(minTransform.position.x, maxTransform.position.x), 0f, Random.Range(minTransform.position.z, maxTransform.position.z));
    }
}
