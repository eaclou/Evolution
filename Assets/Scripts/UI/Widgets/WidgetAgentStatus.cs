using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WidgetAgentStatus : MonoBehaviour {

    public Image imageHealth;
    public Image imageEnergy;
    public Image imageFood;
    public Image imageStamina;
    public Text textValHealth;
    public Text textValEnergy;
    public Text textValFood;
    public Text textValStamina;

    public Text textBiomass;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void UpdateBars(float health, float energy, float food, float mass, float stamina) {
        imageHealth.gameObject.transform.localScale = new Vector3(1f, health, 1f);
        textValHealth.text = (health * 100f).ToString("F0");
        /*if(health < 0.33f) {
            imageHealth.color = Color.red;
        }
        else if(health > 0.66f) {
            imageHealth.color = Color.green;
        }
        else {
            imageHealth.color = Color.yellow;
        }
        */
        float energyCapped = Mathf.Clamp01(energy * 0.167f);
        imageEnergy.gameObject.transform.localScale = new Vector3(1f, energyCapped, 1f);
        textValEnergy.text = (energy * 100f).ToString("F0");
        /*if(energyCapped < 0.33f) {
            imageEnergy.color = Color.red;
        }
        else if(energyCapped > 0.66f) {
            imageEnergy.color = Color.green;
        }
        else {
            imageEnergy.color = Color.yellow;
        }*/
        imageFood.gameObject.transform.localScale = new Vector3(1f, food, 1f);
        textValFood.text = (food * 100f).ToString("F0");

        textBiomass.text = "Biomass: " + mass.ToString("F3");

        imageStamina.gameObject.transform.localScale = new Vector3(1f, stamina, 1f);
        textValStamina.text = (stamina * 100f).ToString("F0");
        /*if(stamina < 0.33f) {
            imageStamina.color = Color.red;
        }
        else if(stamina > 0.66f) {
            imageStamina.color = Color.green;
        }
        else {
            imageStamina.color = Color.yellow;
        }*/
    }
}
