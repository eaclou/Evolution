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
}