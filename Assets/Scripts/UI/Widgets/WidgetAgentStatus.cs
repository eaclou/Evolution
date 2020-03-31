using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WidgetAgentStatus : MonoBehaviour {

    public Image imageHealth;
    public Image imageEnergy;
    public Image imageFood;
    public Text textValHealth;
    public Text textValEnergy;
    public Text textValFood;

    public Text textBiomass;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void UpdateBars(float health, float energy, float food, float mass) {
        imageHealth.gameObject.transform.localScale = new Vector3(1f, health, 1f);
        textValHealth.text = (health * 100f).ToString("F0");
        imageEnergy.gameObject.transform.localScale = new Vector3(1f, energy, 1f);
        textValEnergy.text = (energy * 100f).ToString("F0");
        imageFood.gameObject.transform.localScale = new Vector3(1f, food, 1f);
        textValFood.text = (food * 100f).ToString("F0");

        textBiomass.text = "Biomass: " + mass.ToString("F3");
    }
}
