using System;
using UnityEngine;
using UnityEngine.UI;

public class SensorsPanel : MonoBehaviour
{
    AgentGenome genome => UIManager.instance.focusedCandidate.candidateGenome;

    [SerializeField] Image internals;
    [SerializeField] Image contact;
    
    [SerializeField] Sensor plantFood;
    [SerializeField] Sensor eggsFood;
    [SerializeField] Sensor meatFood;
    [SerializeField] Sensor corpseFood;
    [SerializeField] Sensor microbeFood;
    [SerializeField] Sensor friendSensor;
    [SerializeField] Sensor foeSensor;
    [SerializeField] Sensor wallSensor;
    [SerializeField] Sensor waterSensor;
    [SerializeField] Sensor commSensor;
    
    BodyGenome body;
    CritterModuleFoodSensorsGenome food;
    CritterModuleFriendSensorsGenome friend;
    CritterModuleThreatSensorsGenome threat;
    CritterModuleEnvironmentSensorsGenome environment;
    CritterModuleCommunicationGenome communication;

    public void Refresh() 
    {
        if (genome.bodyGenome.foodGenome == null) return;
        
        body = genome.bodyGenome;
        food = body.foodGenome;
        friend = body.friendGenome;
        threat = body.threatGenome;
        environment = body.environmentalGenome;
        communication = body.communicationGenome;
                
        plantFood.SetSensorEnabled(food.useNutrients || food.useStats);
        microbeFood.SetSensorEnabled(food.usePos);
        eggsFood.SetSensorEnabled(food.useEggs);
        meatFood.SetSensorEnabled(food.useVel);
        corpseFood.SetSensorEnabled(food.useCorpse);
        friendSensor.SetSensorEnabled(friend.usePos || friend.useVel || friend.useDir);
        foeSensor.SetSensorEnabled(threat.usePos || threat.useVel || threat.useDir);
        waterSensor.SetSensorEnabled(environment.useWaterStats);
        wallSensor.SetSensorEnabled(environment.useCardinals || environment.useDiagonals);
        commSensor.SetSensorEnabled(communication.useComms);

        internals.color = Color.white;        
        contact.color = Color.white;         
    }
    
    [Serializable] 
    public class Sensor
    {
        [SerializeField] Image image;
        [SerializeField] TooltipUI tooltip;
        
        // * Move to central location (lookup?)
        [SerializeField] Color enabledColor = Color.white;
        [SerializeField] Color disabledColor = Color.gray * 0.75f;
        
        public void SetSensorEnabled(bool value) 
        { 
            //tooltip.isSensorEnabled = value; //***EAC re-implement this later
            image.color = value ? enabledColor : disabledColor;     
        }
    }
}
