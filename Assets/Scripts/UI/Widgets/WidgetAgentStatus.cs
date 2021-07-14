using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WidgetAgentStatus : MonoBehaviour {

    //public Image imageHealth;
    //public Image imageEnergy;
    //public Image imageFood;
    //public Image imageStamina;
    public Text textValHealth;
    public Text textValEnergy;
    public Text textValFood;
    //public Text textValStamina;

    public Text textBiomass;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void UpdateBars(float health, float energy, float food, float mass, float stamina) {

        //imageHealth.gameObject.transform.localScale = new Vector3(1f, health, 1f);
        textValHealth.text = (health * 100f).ToString("F0");
        
        float energyCapped = Mathf.Clamp01(energy * 0.1f); //***EC refactor how energy works
        //imageEnergy.gameObject.transform.localScale = new Vector3(1f, energyCapped, 1f);
        textValEnergy.text = energy.ToString("F0");
        
        //imageFood.gameObject.transform.localScale = new Vector3(1f, food, 1f);
        textValFood.text = (food * 100f).ToString("F0");

        textBiomass.text = "Biomass: " + mass.ToString("F3");

        //imageStamina.gameObject.transform.localScale = new Vector3(1f, stamina, 1f);
        //textValStamina.text = (stamina * 100f).ToString("F0");
        
    }
}
