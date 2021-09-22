using System;
using UnityEngine;
using UnityEngine.UI;

public class SensorsPanel : MonoBehaviour
{
    SelectionManager selectionManager => SelectionManager.instance;
    AgentGenome genome => selectionManager.focusedCandidate.candidateGenome;
    Agent agent => CameraManager.instance.targetAgent;

    [SerializeField] Sensor internals;
    [SerializeField] Sensor contact;    
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
        meatFood.SetSensorEnabled(food.useVel); //***EAC FIX THESE!!!!
        corpseFood.SetSensorEnabled(food.useCorpse);
        friendSensor.SetSensorEnabled(friend.usePos || friend.useVel || friend.useDir);
        foeSensor.SetSensorEnabled(threat.usePos || threat.useVel || threat.useDir);
        waterSensor.SetSensorEnabled(environment.useWaterStats);
        wallSensor.SetSensorEnabled(false); // environment.useCardinals || environment.useDiagonals);
        commSensor.SetSensorEnabled(communication.useComms);

        internals.SetSensorEnabled(true);        
        contact.SetSensorEnabled(true);


        //***EAC AWFUL AWFUL code:::
        if (!agent) return;
        string txt = "Nutrients (disabled)";
        if(agent.foodModule.nutrientGradX != null) {
            txt = "nutrientDir [" + agent.foodModule.nutrientGradX[0].ToString("F2") + "," + agent.foodModule.nutrientGradY[0].ToString("F2");
        }
        plantFood.SetSensorTooltip(txt);
        
        microbeFood.SetSensorTooltip("Microbes: " + agent.foodModule.nearestAnimalParticlePos);

        if(!food.useEggs) {
            txt = "Eggs (disabled)";
        }
        else {
            txt = "Eggs [" + agent.foodModule.foodEggDirX[0].ToString("F2") + "," + agent.foodModule.foodEggDirY[0].ToString("F2") + "] d: " + agent.foodModule.foodEggDistance[0].ToString("F2");
        }
        eggsFood.SetSensorTooltip(txt);

        txt = "Animals (disabled)";
        //if()
        //if (agent.foodModule.foodAnimalDirX != null) {
        //    txt = "Animals: " + agent.foodModule.foodAnimalDirX[0] + "," + agent.foodModule.foodAnimalDirY[0] + "]";
        //}
        if (agent.foodModule.foodAnimalVelX != null) {
            txt = "Animal vel [" + agent.foodModule.foodAnimalVelX[0].ToString("F2") + "," + agent.foodModule.foodAnimalVelY[0].ToString("F2") + "]";
        }
        meatFood.SetSensorTooltip(txt);
                
        if(!food.useCorpse) {
            txt = "Carrion (disabled)";
        }
        else {
            txt = "Carrion [" + agent.foodModule.foodCorpseDirX[0].ToString("F2") + "," + agent.foodModule.foodCorpseDirY[0].ToString("F2") + "] d: " + agent.foodModule.foodCorpseDistance[0].ToString("F2");
        }
        corpseFood.SetSensorTooltip(txt);

        txt = "Friend (disabled)";
        if (agent.friendModule.friendDirX != null) {
            txt = "Friend [" + agent.friendModule.friendDirX[0].ToString("F2") + "," + agent.friendModule.friendDirY[0].ToString("F2") + "] vel [" + agent.friendModule.friendVelX[0].ToString("F2") + "," + agent.friendModule.friendVelY[0].ToString("F2") + "]";
        }
        friendSensor.SetSensorTooltip(txt);

        txt = "Foe (disabled)";
        if (agent.threatsModule.enemyDirX != null) {
            txt = "Foe: " + agent.threatsModule.enemyDirX[0].ToString("F2") + "," + agent.threatsModule.enemyDirY[0].ToString("F2");
        }
        foeSensor.SetSensorTooltip(txt);

        txt = "Water (disabled)";
        if (agent.environmentModule.waterDepth != null) {
            txt = "Water: " + agent.environmentModule.waterDepth[0].ToString("F2") + ", vel [" + agent.environmentModule.waterVelX[0].ToString("F2") + "," + agent.environmentModule.waterVelY[0].ToString("F2") + "]";
        }
        waterSensor.SetSensorTooltip(txt);

        wallSensor.SetSensorTooltip("Walls (disabled)");

        txt = "CommsIn (disabled)";
        if(communication.useComms) {
            txt = "CommsIn " + agent.communicationModule.inComm0[0].ToString("F2") + ", " + agent.communicationModule.inComm1[0].ToString("F2") + ", " + agent.communicationModule.inComm2[0].ToString("F2") + ", " + agent.communicationModule.inComm3[0].ToString("F2");
        }
        commSensor.SetSensorTooltip(txt);

        internals.SetSensorTooltip("Health: " + agent.coreModule.hitPoints[0].ToString("F2") + ", Energy: " + agent.coreModule.energyStored[0].ToString("F2"));
        contact.SetSensorTooltip("Contact Force [" + agent.coreModule.contactForceX[0].ToString("F2") + "," + agent.coreModule.contactForceY[0].ToString("F2") + "]");
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
        public void SetSensorTooltip(string txt) {
            tooltip.tooltipString = txt;
        }
    }
}
