using UnityEngine;

// Contains static data for brain visualization settings
// Fields that change per individual agent at runtime should be moved to GenerateBrainVisualization.cs
// Quality-setting based groups of fields will be in separate ScriptableObjects
[CreateAssetMenu(menuName = "Pond Water/Brain/Brain Visualization Data", fileName = "Brain Visualization Data")]
public class BrainSettings : ScriptableObject
{
    [Header("Display Resources")]
    public ComputeShader shaderComputeBrain;
    public ComputeShader shaderComputeFloatingGlowyBits;
    public ComputeShader shaderComputeExtraBalls;  // quads w/ nml maps to like like extra blobs attached to neurons & axons
    public Material displayMaterialCore;
    public Material displayMaterialCables;
    public Material floatingGlowyBitsMaterial;
    public Material extraBallsMaterial;

    [Header("General Settings")]
    public int maxTrisPerNeuron = 1024;
    public int maxTrisPerSubNeuron = 8 * 8 * 2 * 2;
    public int maxTrisPerAxon = 2048;
    public int maxTrisPerCable = 2048;
    public int numFloatingGlowyBits = 8192 * 8;
    public int numAxonBalls = 8 * 128;
    public int numNeuronBalls = 128;

    [Header("Size")]
    public float minNeuronRadius = 0.05f;
    public float maxNeuronRadius = 0.5f;
    public float minAxonRadius = 0.05f;
    public float maxAxonRadius = 0.5f;
    public float minSubNeuronScale = 0.25f;
    public float maxSubNeuronScale = 0.75f;  // max size relative to parent Neuron
    public float maxAxonFlareScale = 0.9f;  // max axon flare size relative to SubNeuron
    public float minAxonFlareScale = 0.2f;
    public float axonFlarePos = 0.92f;
    public float axonFlareWidth = 0.08f;
    public float axonMaxPulseMultiplier = 2.0f;
    public float cableRadius = 0.05f;
    public float neuronBallMaxScale = 1f;

    [Header("Noise")]
    public float neuronExtrudeNoiseFreq = 1.5f;
    public float neuronExtrudeNoiseAmp = 0.0f;
    public float neuronExtrudeNoiseScrollSpeed = 0.6f;
    public float axonExtrudeNoiseFreq = 0.33f;
    public float axonExtrudeNoiseAmp = 0.33f;
    public float axonExtrudeNoiseScrollSpeed = 1.0f;
    public float axonPosNoiseFreq = 0.14f;
    public float axonPosNoiseAmp = 0f;
    public float axonPosNoiseScrollSpeed = 10f;
    public float axonPosSpiralFreq = 20.0f;
    public float axonPosSpiralAmp = 0f;

    [Header("Forces")]
    public float neuronAttractForce = 0.004f;
    public float neuronRepelForce = 2.0f;
    public float axonPerpendicularityForce = 0.01f;
    public float axonAttachStraightenForce = 0.01f;
    public float axonAttachSpreadForce = 0.025f;
    public float axonRepelForce = 0.2f;
    public float cableAttractForce = 0.01f;
    
    [Header("Other Options")]
    public bool disablePhysics;
    public bool disableCables;
}
