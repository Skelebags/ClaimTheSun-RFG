using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManagerScript : MonoBehaviour
{
    [SerializeField]
    private Slider energyBar;

    [SerializeField]
    private Text energyText;

    private GameController gc;

    void Start()
    {
        gc = GetComponent<GameController>();
    }

    // Update is called once per frame
    void Update()
    {
        energyText.text = gc.GetCurrentEnergy().ToString("#.#") + "/" + gc.GetMaxEnergy();
        energyBar.value = gc.GetEnergyPercentage();
    }
}
