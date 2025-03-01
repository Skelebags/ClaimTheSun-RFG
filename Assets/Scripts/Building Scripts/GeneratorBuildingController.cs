﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GeneratorBuildingController : BuildingController
{
    [SerializeField]
    [Tooltip("The extra energy generated by this building")]
    private float energyGeneration = 2f;

    [SerializeField]
    [Tooltip("The rate in seconds that this building generates energy")]
    private float energyRate = 1f;
    private float energyTimer = 0f;

    [SerializeField]
    private GameController teamController;

    protected override void Awake()
    {
        base.Awake();

        foreach(GameObject manager in GameObject.FindGameObjectsWithTag("Game_Manager"))
        {
            if (manager.GetComponent<MouseManager>().GetTeam() == team)
            {
                teamController = manager.GetComponent<GameController>();
            }
        }
    }

    protected override void Update()
    {
        base.Update();
        if (state == State.building)
        {
            foreach (GameObject manager in GameObject.FindGameObjectsWithTag("Game_Manager"))
            {
                if (manager.GetComponent<MouseManager>().GetTeam() == team)
                {
                    teamController = manager.GetComponent<GameController>();
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (state == State.ready)
        {
            energyTimer += Time.deltaTime;
            if (energyTimer >= energyRate)
            {
                teamController.AddEnergy(energyGeneration);
                energyTimer = 0f;
            }
        }
    }

    public void UpdateUI()
    {
        if (uiPanel)
        {
            uiPanel.transform.Find("CURRENT_HEALTH").GetComponent<Text>().text = currentHealth.ToString("#.#") + " / " + maxHealth.ToString();
        }
    }
}
