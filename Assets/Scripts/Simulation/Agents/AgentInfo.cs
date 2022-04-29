using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Pond Water/Game Data/Agent")]
public class AgentInfo : ScriptableObject
{
    [SerializeField] AgentData data;
    public AgentData GetData() { return new AgentData(data); }
}

// * WPP: move immutable values (if any) to AgentInfo
[Serializable] [ES3Serializable]
public class AgentData
{
    public Vector3 position;
    public float speed;
    public float smoothedThrottleLerp;
    public float animationCycle;
    public float turningAmount;
    public float swimAnimationCycleSpeed;
    public float spawnStartingScale;
    public bool isInert;   
    public bool isActing;    
    public bool isDecaying;
    public int feedAnimDuration;  
    public int feedAnimCooldown;
    public int attackAnimDuration;
    public int attackAnimCooldown;
    public int dashDuration;
    public int dashCooldown;
    public int defendDuration;
    public int defendCooldown;
    public int cooldownDuration; 
    public bool isMarkedForDeathByUser;
    public AgentActionState curActionState;
    public int index;
    public int speciesIndex;  
    public AgentLifeStage curLifeStage;
    
    public AgentData() { }
    
    /// Deep Copy
    public AgentData(AgentData original)
    {
        position = original.position;
        speed = original.speed;
        smoothedThrottleLerp = original.smoothedThrottleLerp;
        animationCycle = original.animationCycle;
        turningAmount = original.turningAmount;
        swimAnimationCycleSpeed = original.swimAnimationCycleSpeed;
        spawnStartingScale = original.spawnStartingScale;
        isInert = original.isInert;
        isActing = original.isActing;
        isDecaying = original.isDecaying;
        feedAnimDuration = original.feedAnimDuration;
        feedAnimCooldown = original.feedAnimCooldown;
        attackAnimDuration = original.attackAnimDuration;
        attackAnimCooldown = original.attackAnimCooldown;
        dashDuration = original.dashDuration;
        dashCooldown = original.dashCooldown;
        defendDuration = original.defendDuration;
        defendCooldown = original.defendCooldown;
        //isResting = original.isResting;
        cooldownDuration = original.cooldownDuration;
        isMarkedForDeathByUser = original.isMarkedForDeathByUser;
        curActionState = original.curActionState;
        index = original.index;
        speciesIndex = original.speciesIndex;
        curLifeStage = original.curLifeStage;
    }
}