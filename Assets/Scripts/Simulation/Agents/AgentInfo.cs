using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Pond Water/Game Data/Agent")]
public class AgentInfo : ScriptableObject
{
    public AgentData data;
}

[Serializable]
public struct AgentData
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
    public bool isResting;
    public int cooldownDuration; 
    public bool isMarkedForDeathByUser;
    public int index;    
    public int speciesIndex;  
    public AgentLifeStage curLifeStage;
}