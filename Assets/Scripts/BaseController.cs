using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseController : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Time taken in seconds to build this unit")]
    protected float buildTime = 5f;

    [SerializeField]
    [Tooltip("How much energy this costs to build")]
    protected float buildCost = 5f;

    [SerializeField]
    [Tooltip("The maximum health of the unit")]
    protected float maxHealth = 10f;
    protected float currentHealth { get; set; }

    [SerializeField]
    [Tooltip("The armour value of the unit")]
    protected float armour = 0f;

    [SerializeField]
    [Tooltip("How much this unit resists decay")]
    protected float decayResistance = 0f;

    [SerializeField]
    [Tooltip("Which team this entity is on")] [Range(0, 9)]
    protected int team = 0;

    [SerializeField]
    [Tooltip("This building's UI element prefab")]
    protected GameObject uiPrefab;
    protected GameObject uiPanel;
    protected Button[] buttons;

    protected static float MAX_ARMOUR_REDUCTION = 10f;

    private float decaytimer;

    private GameObject canvas;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
        decaytimer = 0f;
    }


    public float GetBuildTime()
    {
        return buildTime;
    }

    public float GetBuildCost()
    {
        return buildCost;
    }

    public float GetTeam()
    {
        return team;
    }

    public float GetDecayTimer()
    {
        return decaytimer;
    }

    public void SetDecayTimer(float timer)
    {
        decaytimer = timer;
    }

    public void SetTeam(int newTeam)
    {
        team = newTeam;
    }

    public void Damage(float damage, float penetration)
    {
        float trueDamage = damage / Mathf.Clamp((armour + 1/ penetration + 1), 1f, MAX_ARMOUR_REDUCTION);

        currentHealth -= trueDamage;
        if (currentHealth <= 0)
        {
            Kill();
        }
    }

    public void Decay(float decayStrength)
    {
        float trueDecay = Mathf.Clamp(decayStrength - decayResistance, 0f, decayStrength);

        currentHealth -= trueDecay;
        if(currentHealth <= 0)
        {
            Kill();
        }
    }

    public void Kill()
    {
        ClearUI();
        Destroy(transform.root.gameObject);
    }

    public void GenerateUI()
    {
        canvas = GameObject.Find("Canvas");
        uiPanel = Instantiate(uiPrefab, canvas.transform);
        buttons = uiPanel.GetComponentsInChildren<Button>();
    }

    public GameObject GetUiPanel()
    {
        return uiPanel;
    }

    public Button[] GetButtons()
    {
        return buttons;
    }

    public void ClearUI()
    {
        canvas = null;
        if(buttons != null)
        {
            foreach (Button button in buttons)
            {
                button.onClick.RemoveAllListeners();
            }
        }
        if(uiPanel)
        {
            Destroy(uiPanel);
        }
    }


}
