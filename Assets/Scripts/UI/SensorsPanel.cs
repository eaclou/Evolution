using System;
using UnityEngine;
using UnityEngine.UI;

public enum SensorID
{
    Internals,
    Contact,
    Plants,
    Eggs,
    Meat,
    Corpse,
    Microbes,
    Friend,
    Foe,
    Wall,
    Water,
    Communication,      
}

public class SensorsPanel : MonoBehaviour
{
    SelectionManager selectionManager => SelectionManager.instance;
    AgentGenome genome => selectionManager.focusedCandidate.candidateGenome;
    Agent agent => CameraManager.instance.targetAgent;

    [SerializeField] Sensor[] sensors;

    BodyGenome body;
    CritterModuleFoodSensorsGenome food;
    //CritterModuleFriendSensorsGenome friend;
    //CritterModuleThreatSensorsGenome threat;
    CritterModuleEnvironmentSensorsGenome environment;
    //CritterModuleCommunicationGenome communication;

    public void Refresh() 
    {
        if (genome?.bodyGenome?.foodGenome == null) return;
        
        body = genome.bodyGenome;
        food = body.foodGenome;
        //friend = body.friendGenome;
        //threat = body.threatGenome;
        environment = body.environmentalGenome;
        //communication = body.communicationGenome;
        
        foreach (var sensor in sensors)
            sensor.SetSensorEnabled(IsSensorEnabled(sensor.id));

        if (!agent) return;
        
        foreach (var sensor in sensors)
            sensor.SetSensorTooltip(GetSensorTooltipText(sensor));
    }
    
    bool IsSensorEnabled(SensorID id)
    {
        switch (id)
        {
            case SensorID.Plants: return food.useNutrients || food.useStats;
            case SensorID.Microbes: return food.usePos;
            case SensorID.Eggs: return food.useEggs;
            case SensorID.Meat: return food.useVel;
            case SensorID.Corpse: return food.useCorpse;
            case SensorID.Friend: return body.hasAnimalSensor; //friend.usePos || friend.useVel || friend.useDir;
            case SensorID.Foe: return body.hasAnimalSensor;    //threat.usePos || threat.useVel || threat.useDir;
            case SensorID.Water: return environment.useWaterStats;
            case SensorID.Wall: return false;
            case SensorID.Internals: return true;
            case SensorID.Communication: return body.hasComms;
            case SensorID.Contact: return true;
            default: return false;
        }        
    }
    
    string GetSensorTooltipText(Sensor sensor) 
    { 
        return IsSensorInActiveState(sensor.id) ? GetSensorEnabledText(sensor.id) : sensor.disabledText; 
    }

    bool IsSensorInActiveState(SensorID id)
    {
        switch (id)
        {
            case SensorID.Plants: return agent.foodModule.nutrientGradX != null;
            case SensorID.Microbes: return true;
            case SensorID.Eggs: return food.useEggs;
            case SensorID.Meat: return agent.foodModule.foodAnimalVelX != null;
            case SensorID.Corpse: return food.useCorpse;
            case SensorID.Friend: return agent.friendModule.friendDirX != null;
            case SensorID.Foe: return agent.threatsModule.enemyDirX != null;
            case SensorID.Water: return agent.environmentModule.waterDepth != null;
            case SensorID.Internals: return true;
            case SensorID.Communication: return body.hasComms;
            case SensorID.Contact: return true;
            default: return false;
        }        
    }
    
    string GetSensorEnabledText(SensorID id)
    {
        switch (id)
        {
            case SensorID.Plants: return "nutrientDir [" + agent.foodModule.nutrientGradX[0].ToString("F2") + 
                "," + agent.foodModule.nutrientGradY[0].ToString("F2");
            case SensorID.Microbes: return "Microbes: " + agent.foodModule.nearestAnimalParticlePos;
            case SensorID.Eggs: return "Eggs [" + agent.foodModule.foodEggDirX[0].ToString("F2") + 
                "," + agent.foodModule.foodEggDirY[0].ToString("F2") + 
                "] d: " + agent.foodModule.foodEggDistance[0].ToString("F2");
            case SensorID.Meat: return "Animal vel [" + agent.foodModule.foodAnimalVelX[0].ToString("F2") + 
                "," + agent.foodModule.foodAnimalVelY[0].ToString("F2") + "]";
            case SensorID.Corpse: return "Carrion [" + agent.foodModule.foodCorpseDirX[0].ToString("F2") + 
                "," + agent.foodModule.foodCorpseDirY[0].ToString("F2") + 
                "] d: " + agent.foodModule.foodCorpseDistance[0].ToString("F2");
            case SensorID.Friend: return "Friend [" + agent.friendModule.friendDirX[0].ToString("F2") + 
                "," + agent.friendModule.friendDirY[0].ToString("F2") + 
                "] vel [" + agent.friendModule.friendVelX[0].ToString("F2") + 
                "," + agent.friendModule.friendVelY[0].ToString("F2") + "]";
            case SensorID.Foe: return "Foe: " + agent.threatsModule.enemyDirX[0].ToString("F2") + 
                "," + agent.threatsModule.enemyDirY[0].ToString("F2");
            case SensorID.Water: return "Water: " + agent.environmentModule.waterDepth[0].ToString("F2") + 
                ", vel [" + agent.environmentModule.waterVelX[0].ToString("F2") + 
                "," + agent.environmentModule.waterVelY[0].ToString("F2") + "]";
            case SensorID.Internals: return "Health: " + agent.coreModule.hitPoints[0].ToString("F2") + 
                ", Energy: " + agent.coreModule.energyStored[0].ToString("F2");
            case SensorID.Communication: return "CommsIn " + agent.communicationModule.inComm0[0].ToString("F2") + 
                ", " + agent.communicationModule.inComm1[0].ToString("F2") + 
                ", " + agent.communicationModule.inComm2[0].ToString("F2") + 
                ", " + agent.communicationModule.inComm3[0].ToString("F2");
            case SensorID.Contact: return "Contact Force [" + agent.coreModule.contactForceX[0].ToString("F2") + 
                "," + agent.coreModule.contactForceY[0].ToString("F2") + "]";
            default: return "";
        }
    }

    [Serializable] 
    public class Sensor
    {
        Lookup lookup => Lookup.instance;

        public SensorID id;    
        [SerializeField] Image image;
        [SerializeField] TooltipUI tooltip;
        public string disabledText;

        public void SetSensorEnabled(bool value) 
        { 
            //tooltip.isSensorEnabled = value; //***EAC re-implement this later
            image.color = lookup.GetSensorColor(value);
        }
        public void SetSensorTooltip(string txt) {
            tooltip.tooltipString = txt;
        }
    }
}
