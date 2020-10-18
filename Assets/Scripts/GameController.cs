using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The base maximum energy for the player")]
    private float maxEnergy = 100f;

    [SerializeField]
    private float currentEnergy;

    [SerializeField]
    [Tooltip("The base rate of energy production")]
    private float baseEnergyGain = 1f;

    [SerializeField]
    [Tooltip("The time in seconds between energy gain ticks")]
    private float energyGainRate = 1f;
    private float energyTimer;

    [SerializeField]
    [Tooltip("The player HQ building")]
    private GameObject playerHQ;

    [SerializeField]
    [Tooltip("The enemy HQ building")]
    private GameObject enemyHQ;

    [SerializeField]
    [Tooltip("The victory UI panel")]
    private GameObject winPanel;

    [SerializeField]
    [Tooltip("The defeat UI panel")]
    private GameObject losePanel;

    void Start()
    {
        currentEnergy = maxEnergy;
        energyTimer = 0;
    }

    void Update()
    {
        if(currentEnergy > maxEnergy)
        {
            currentEnergy = maxEnergy;
        }

        if(energyTimer >= energyGainRate && currentEnergy < maxEnergy)
        {
            currentEnergy += baseEnergyGain;
            energyTimer = 0;
        }
        else
        {
            energyTimer += Time.deltaTime;
        }

        if(enemyHQ == null && playerHQ != null)
        {
            winPanel.SetActive(true);
        }
        if (enemyHQ != null && playerHQ == null)
        {
            losePanel.SetActive(true);
        }
    }

    public bool CanAfford(float energyCost)
    {
        return energyCost <= currentEnergy;
    }

    public void SpendEnergy(float energyCost)
    {
        currentEnergy -= energyCost;
    }

    public void AddEnergy(float energy)
    {
        currentEnergy += energy;
    }

    public float GetEnergyPercentage()
    {
        return currentEnergy / maxEnergy;
    }

    public float GetCurrentEnergy()
    {
        return currentEnergy;
    }

    public float GetMaxEnergy()
    {
        return maxEnergy;
    }
}
