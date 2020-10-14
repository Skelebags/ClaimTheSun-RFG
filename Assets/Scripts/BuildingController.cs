using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BuildingController : MonoBehaviour
{
    [SerializeField]
    [Tooltip("How long this building takes to build in seconds")]
    private float buildTime = 5f;

    [SerializeField]
    [Tooltip("The maximum health of the unit")]
    private float maxHealth = 10f;
    private float currentHealth;

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
    [Tooltip("The Colour to show the building is placeable")]
    private Color placeableColor;

    [SerializeField]
    [Tooltip("The Colour to show the building is unplaceable")]
    private Color unplaceableColor;

    [SerializeField]
    [Tooltip("The Colour to show the building is in construction")]
    private Color constructionColor;

    [SerializeField]
    [Tooltip("This building's rallypoint")]
    private GameObject rallyPoint;

    private enum State { placing, building, ready};
    private State state;

    private Dictionary<KeyCode, GameObject> keyObjDict;

    private List<GameObject> buildQueue;

    private float buildTimer;
    private float unitTimer;

    private MeshRenderer[] meshRenderers;
    private Color[] baseColors;

    private Collider[] colliders;

    private bool canPlace;
    private bool isIntersecting;

    private LayerMask groundMask;

    // Start is called before the first frame update
    void Awake()
    {
        state = State.placing;
        buildQueue = new List<GameObject>();
        buildTimer = 0f;
        unitTimer = 0f;
        keyObjDict = new Dictionary<KeyCode, GameObject>();
        for (int i = 0; i < hotKeys.Length; i++)
        {
            keyObjDict.Add(hotKeys[i], unitPrefabs[i]);
        }

        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        baseColors = new Color[meshRenderers.Length];
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            baseColors[i] = meshRenderers[i].material.color;
        }

        colliders = GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            collider.isTrigger = true;
        }
        GetComponent<NavMeshObstacle>().enabled = false;

        isIntersecting = false;
        canPlace = true;
        groundMask = LayerMask.GetMask("Ground");
    }

    // Update is called once per frame
    void Update()
    {
        switch(state)
        {
            case State.ready:
                GetComponent<Rigidbody>().isKinematic = true;
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
                break;
            case State.building:
                if (buildTimer >= buildTime)
                {
                    for(int i = 0; i < meshRenderers.Length; i++)
                    {
                        meshRenderers[i].material.color = baseColors[i];
                    }
                    state = State.ready;
                    buildTimer = 0;
                }
                else
                {
                    transform.position = new Vector3(transform.position.x, Mathf.Lerp(transform.position.y, GetComponentInChildren<Collider>().bounds.extents.y, Time.deltaTime), transform.position.z);
                    foreach (MeshRenderer meshRenderer in meshRenderers)
                    {
                        meshRenderer.material.color = constructionColor;
                    }
                    buildTimer += Time.deltaTime;
                }
                GetComponent<Rigidbody>().isKinematic = true;
                break;

            case State.placing:
                if(canPlace && !isIntersecting)
                {
                    foreach(MeshRenderer meshRenderer in meshRenderers)
                    {
                        meshRenderer.material.color = placeableColor;
                    }
                }
                else
                {
                    foreach (MeshRenderer meshRenderer in meshRenderers)
                    {
                        meshRenderer.material.color = unplaceableColor;
                    }
                }
                break;
        }
    }

    public void AddToQueue(KeyCode key)
    {
        if(buildQueue.Count < QUEUE_MAX)
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
        newUnit.GetComponent<UnitController>().MoveOrder(rallyPoint.transform.position);
    }

    public KeyCode[] GetHotKeys()
    {
        return hotKeys;
    }

    public float GetBuildTime()
    {
        return buildTime;
    }

    public void SetPlaceable(bool placeable)
    {
        canPlace = placeable;
    }

    public bool GetIntersecting()
    {
        return isIntersecting;
    }

    public void Place()
    {
        transform.Translate(-Vector3.up * GetComponentInChildren<Collider>().bounds.size.y);
        foreach (Collider collider in colliders)
        {
            collider.isTrigger = false;
        }
        GetComponent<NavMeshObstacle>().enabled = true;

        state = State.building;
    }

    public void RallyPointVisible(bool state)
    {
        rallyPoint.GetComponentInChildren<MeshRenderer>().enabled = state;
    }

    public void SetRallyPointPosition(Vector3 position)
    {
        rallyPoint.transform.position = position;
    }

    public void Damage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Kill();
        }
    }

    public void Kill()
    {
        Destroy(transform.root.gameObject);
    }

    private void OnTriggerEnter(Collider collision)
    {
        if(collision.CompareTag("Building"))
        {
            isIntersecting = true;
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.CompareTag("Building"))
        {
            isIntersecting = false;
        }
    }
}
