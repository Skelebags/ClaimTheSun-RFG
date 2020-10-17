using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class BuildingController : BaseController
{
    [SerializeField]
    [Tooltip("The Colour to show the building is placeable")]
    private Color placeableColor;

    [SerializeField]
    [Tooltip("The Colour to show the building is unplaceable")]
    private Color unplaceableColor;

    [SerializeField]
    [Tooltip("The Colour to show the building is in construction")]
    private Color constructionColor;

    protected enum State { placing, building, ready};
    protected State state;

    private float buildTimer;

    private MeshRenderer[] meshRenderers;
    private Color[] baseColors;

    private Collider[] colliders;

    private bool canPlace;
    private bool isIntersecting;

    // Awake is called when the object is first instantiated
    protected new virtual void Awake()
    {
        base.Awake();
        state = State.placing;

        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        baseColors = new Color[meshRenderers.Length];
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            if (meshRenderers[i].material.HasProperty("_Color"))
            {
                baseColors[i] = meshRenderers[i].material.color;
            }
        }

        colliders = GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            collider.isTrigger = true;
        }
        GetComponent<NavMeshObstacle>().enabled = false;

        isIntersecting = false;
        canPlace = true;
        
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        switch(state)
        {
            case State.ready:

                break;
            case State.building:
                if (buildTimer >= buildTime)
                {
                    for(int i = 0; i < meshRenderers.Length; i++)
                    {
                        meshRenderers[i].material.color = baseColors[i];
                    }
                    transform.position = new Vector3(transform.position.x, GetComponentInChildren<Collider>().bounds.extents.y, transform.position.z);
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

    private void OnTriggerEnter(Collider collision)
    {
        if(collision.CompareTag("Building"))
        {
            isIntersecting = true;
        }
    }

    private void OnTriggerStay(Collider collision)
    {
        if (collision.CompareTag("Building"))
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
