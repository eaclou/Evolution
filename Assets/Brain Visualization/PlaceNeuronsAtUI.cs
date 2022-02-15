using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlaceNeuronsAtUI : MonoBehaviour
{
    [SerializeField] UIPlacement[] uiPlacements;

    /// INCOMPLETE: visualization is finicky about placement
    /// ui placement may need to be mapped to a valid area
    public Vector3 GetInputNeuronPosition(Neuron neuron)
    {
        //return Random.insideUnitSphere;
        //***EAC IF TWO OR MORE NEURONS SHARE SAME EXACT POSITION IT CREATES A DIVIDE BY ZERO ERROR!!!!!!!!!***

        var data = neuron.genome.data;

        foreach (var placement in uiPlacements)
            if (placement.id == data.iconID)
                return (placement.location.position - new Vector3(960f, 600f, 0f)).normalized * 0.85f + Random.insideUnitSphere * 0.01f;

                
        Debug.LogError($"Unable to find placement for {neuron.moduleID} {data}");
        return Vector3.zero;
    }

    public Vector3 GetOutputNeuronPosition(Neuron neuron)
    {
        
        var data = neuron.genome.data;

        foreach (var placement in uiPlacements)
            if (placement.id == data.iconID)
                return (placement.location.position - new Vector3(760f, 1060f, 0f)).normalized * 0.75f + Random.insideUnitSphere * 0.01f;

                
        Debug.LogError($"Unable to find placement for {neuron.moduleID} {data}");
        return Vector3.zero;
    }
    
    /// NOT IMPLEMENTED
    public Vector3 GetHiddenNeuronPosition(Neuron neuron)
    {
        // Get input location
        // Get output location
        // Calculate & return intermediate location
        // return Vector3.zero;
        
        return new Vector3(-0.9f, 0f, 0f) + Random.insideUnitSphere * 0.01f;
    }

    [Serializable]
    public struct UIPlacement
    {
        public BrainIconID id;
        public RectTransform location;
    }
}
