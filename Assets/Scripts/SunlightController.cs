using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunlightController : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The decay strength of this sunshaft")]
    private float decayStrength = 1f;

    [SerializeField]
    [Tooltip("The decay rate of this sunshaft")]
    private float decayRate = 0.5f;

    public float GetDecayStrength()
    {
        return decayStrength;
    }

    public void SetDecayStrength(float newStrength)
    {
        decayStrength = newStrength;
    }

    public float GetDecayRate()
    {
        return decayStrength;
    }

    public void SetDecayRate(float newRate)
    {
        decayRate = newRate;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.transform.root.gameObject.GetComponent<BaseController>())
        {

            Debug.Log("Decaying");
            BaseController controller = other.transform.root.gameObject.GetComponent<BaseController>();

            if (controller.GetDecayTimer() >= decayRate)
            {
                controller.Decay(decayStrength);
                controller.SetDecayTimer(0f);
            }
            else
            {
                float newTimer = controller.GetDecayTimer();
                newTimer += Time.deltaTime;
                controller.SetDecayTimer(newTimer);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.root.gameObject.GetComponent<BaseController>())
        {
            BaseController controller = other.transform.root.gameObject.GetComponent<BaseController>();
            controller.SetDecayTimer(0f);
        }
    }
}
