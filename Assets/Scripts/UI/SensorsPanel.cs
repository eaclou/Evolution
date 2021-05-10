using System;
using UnityEngine;
using UnityEngine.UI;

public class SensorsPanel : MonoBehaviour
{
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

    public void Refresh(AgentGenome genome) {
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
        
        /*
        imageSensorComms.GetComponent<GenomeButtonTooltipSource>().isSensorEnabled = genome.bodyGenome.communicationGenome.useComms;
        imageSensorWater.GetComponent<GenomeButtonTooltipSource>().isSensorEnabled = genome.bodyGenome.environmentalGenome.useWaterStats;
        imageSensorWalls.GetComponent<GenomeButtonTooltipSource>().isSensorEnabled = genome.bodyGenome.environmentalGenome.useCardinals || genome.bodyGenome.environmentalGenome.useDiagonals;
        imageSensorFoodPlant.GetComponent<GenomeButtonTooltipSource>().isSensorEnabled = foodGenome.useNutrients || foodGenome.useStats;
        imageSensorFoodMicrobe.GetComponent<GenomeButtonTooltipSource>().isSensorEnabled = genome.bodyGenome.foodGenome.usePos;  // ********* THESE ARE WRONG ^ ^ ^
        imageSensorFoodMeat.GetComponent<GenomeButtonTooltipSource>().isSensorEnabled = genome.bodyGenome.foodGenome.useVel;
        imageSensorFoodEggs.GetComponent<GenomeButtonTooltipSource>().isSensorEnabled = genome.bodyGenome.foodGenome.useEggs;
        imageSensorFoodCorpse.GetComponent<GenomeButtonTooltipSource>().isSensorEnabled = genome.bodyGenome.foodGenome.useCorpse;
        imageSensorFriend.GetComponent<GenomeButtonTooltipSource>().isSensorEnabled = genome.bodyGenome.friendGenome.usePos || genome.bodyGenome.friendGenome.useVel || genome.bodyGenome.friendGenome.useDir;
        imageSensorFoe.GetComponent<GenomeButtonTooltipSource>().isSensorEnabled = genome.bodyGenome.threatGenome.usePos || genome.bodyGenome.threatGenome.useVel || genome.bodyGenome.threatGenome.useDir;
        imageSensorInternals.GetComponent<GenomeButtonTooltipSource>().isSensorEnabled = true;
        imageSensorContact.GetComponent<GenomeButtonTooltipSource>().isSensorEnabled = true;

        if (genome.bodyGenome.communicationGenome.useComms) {
            imageSensorComms.color = Color.white;
        }
        else {
            imageSensorComms.color = Color.gray * 0.75f;
        }
        if (genome.bodyGenome.environmentalGenome.useWaterStats) {
            imageSensorWater.color = Color.white;
        }
        else {
            imageSensorWater.color = Color.gray * 0.75f;
        }
        if (genome.bodyGenome.environmentalGenome.useCardinals || genome.bodyGenome.environmentalGenome.useDiagonals) {
            imageSensorWalls.color = Color.white;
        }
        else {
            imageSensorWalls.color = Color.gray * 0.75f;
        }
                        
        if (foodGenome.useNutrients || foodGenome.useStats) {
            imageSensorFoodPlant.color = Color.white;
        }
        else {
            imageSensorFoodPlant.color = Color.gray * 0.75f;
        }

        if (foodGenome.usePos) {
            imageSensorFoodMicrobe.color = Color.white;
        }
        else {
            imageSensorFoodMicrobe.color = Color.gray * 0.75f;
        }

        if (foodGenome.useVel) {
            imageSensorFoodMeat.color = Color.white;
        }
        else {
            imageSensorFoodMeat.color = Color.gray * 0.75f;
        }

        if (foodGenome.useEggs) {
            imageSensorFoodEggs.color = Color.white;
        }
        else {
            imageSensorFoodEggs.color = Color.gray * 0.75f;
        }

        if (foodGenome.useCorpse) {
            imageSensorFoodCorpse.color = Color.white;
        }
        else {
            imageSensorFoodCorpse.color = Color.gray * 0.75f;
        }

        if (genome.bodyGenome.friendGenome.usePos || genome.bodyGenome.friendGenome.useVel || genome.bodyGenome.friendGenome.useDir) {
            imageSensorFriend.color = Color.white;
        }
        else {
            imageSensorFriend.color = Color.gray * 0.75f;
        }
        if (genome.bodyGenome.threatGenome.usePos || genome.bodyGenome.threatGenome.useVel || genome.bodyGenome.threatGenome.useDir) {
            imageSensorFoe.color = Color.white;
        }
        else {
            imageSensorFoe.color = Color.gray * 0.75f;
        }
        */

        internals.color = Color.white;        
        contact.color = Color.white;         
    }
    
    [Serializable] 
    public class Sensor
    {
        [SerializeField] Image image;
        [SerializeField] GenomeButtonTooltipSource tooltip;
        
        public void SetSensorEnabled(bool value) 
        { 
            tooltip.isSensorEnabled = value;
            
            // * Expose values in central location (lookup?)
            image.color = value ? Color.white : Color.gray * 0.75f;     
        }
    }
}
