using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The base maximum energy for the player")]
    private float maxEnergy = 100f;
    private float currentEnergy;

    [SerializeField]
    [Tooltip("The base rate of energy production")]
    private float baseEnergyGain = 1f;

    [SerializeField]
    [Tooltip("The time in seconds between energy gain ticks")]
    private float energyGainRate = 1f;
    private float energyTimer;


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
    }

    public bool CanAfford(float energyCost)
    {
        return energyCost <= currentEnergy;
    }

    public void SpendEnergy(float energyCost)
    {
        currentEnergy -= energyCost;
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
