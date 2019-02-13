using System.Collections;
//using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AgentSettings", menuName = "ScriptableObjects/AgentSettings", order = 1)]
public class SettingsAgents : ScriptableObject {

	public float _MasterSwimSpeed = 0.35f;
    public float _AlignMaskRange = 0.025f;
    public float _AlignMaskOffset = 0.0833f;
    public float _AlignSpeedMult = 0.00015f;
    public float _AttractMag = 0.0000137f;
    public float _AttractMaskMaxDistance = 0.0036f;
    public float _AttractMaskOffset = 0.5f;
    public float _SwimNoiseMag = 0.000086f;
    public float _SwimNoiseFreqMin = 0.00002f;
    public float _SwimNoiseFreqRange = 0.0002f;
    public float _SwimNoiseOnOffFreq = 0.0001f;
    public float _ShoreCollisionMag = 0.0065f;
    public float _ShoreCollisionDistOffset = 0.15f;
    public float _ShoreCollisionDistSlope = 3.5f;


}
