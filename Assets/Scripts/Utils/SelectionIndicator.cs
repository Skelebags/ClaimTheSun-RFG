using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionIndicator : MonoBehaviour
{
    [Tooltip("How much larger than the selected object should the indicator be")]
    public float sizeMultiplier = 1.25f;

    private GameObject attachedObject;

    void Awake()
    {
        MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer mr in meshRenderers)
        {
            mr.enabled = false;
        }
    }

    void Update()
    {
        if(attachedObject != null)
        {
            Bounds bigBounds =attachedObject.GetComponentInChildren<Renderer>().bounds;

            transform.position = new Vector3(bigBounds.center.x, 0, bigBounds.center.z);
            transform.localScale = new Vector3(bigBounds.size.x * sizeMultiplier, bigBounds.size.y * sizeMultiplier, bigBounds.size.z * sizeMultiplier);

            MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();

            foreach(MeshRenderer mr in meshRenderers)
            {
                mr.enabled = true;
            }
        }
        else
        {
            MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();

            foreach (MeshRenderer mr in meshRenderers)
            {
                mr.enabled = false;
            }
        }
    }

    public void Attach(GameObject targetObject)
    {
        attachedObject = targetObject;

    }
}
