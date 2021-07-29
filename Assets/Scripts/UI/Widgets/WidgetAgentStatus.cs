using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WidgetAgentStatus : MonoBehaviour {
        
    public Text textValHealth;
    public Text textValEnergy;
    public Text textValFood;
    public Text textValAge;
    public Text textCurBehavior;
    public Text textValBiomass;
    	
    public void UpdateBars(Agent agent) {
        
        textValHealth.text = (agent.coreModule.health * 100f).ToString("F0") + "%";      
        textValEnergy.text = agent.coreModule.energy.ToString("F0");        
        textValFood.text = (agent.coreModule.stomachContentsPercent * 100f).ToString("F0");
        textValBiomass.text = "Biomass: " + agent.currentBiomass.ToString("F3");
        string lifeStage = agent.curLifeStage.ToString();
        if(agent.curLifeStage == Agent.AgentLifeStage.Mature) {
            if(agent.isSexuallyMature) {
                lifeStage = "Mature";
            }
            else {
                lifeStage = "Young";
            }
        }
        textValAge.text = lifeStage + ", Age: " + agent.ageCounter;
        textCurBehavior.text = agent.curActionState.ToString();
        
    }
    public void UpdateBars(float health, float energy, float food, float mass, float stamina) {

        //imageHealth.gameObject.transform.localScale = new Vector3(1f, health, 1f);
        textValHealth.text = (health * 100f).ToString("F0") + "%";
        
        float energyCapped = Mathf.Clamp01(energy * 0.1f); //***EC refactor how energy works
        //imageEnergy.gameObject.transform.localScale = new Vector3(1f, energyCapped, 1f);
        textValEnergy.text = energy.ToString("F0");
        
        //imageFood.gameObject.transform.localScale = new Vector3(1f, food, 1f);
        textValFood.text = (food * 100f).ToString("F0");

        textValBiomass.text = "Biomass: " + mass.ToString("F3");

        textValAge.text = "Age: ";

        //imageStamina.gameObject.transform.localScale = new Vector3(1f, stamina, 1f);
        //textValStamina.text = (stamina * 100f).ToString("F0");
        
    } //*** OLD
}
