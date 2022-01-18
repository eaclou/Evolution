using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlaceNeuronsAtUI : MonoBehaviour
{
    [SerializeField] UIPlacement[] uiPlacements;

    /// INCOMPLETE: visualization is finicky about placement
    /// ui placement may need to be mapped to a valid area
    public Vector3 GetNeuronPosition(Neuron neuron)
    {
        return Random.insideUnitSphere;

        var data = neuron.genome.data;

        foreach (var placement in uiPlacements)
            if (placement.id == data.iconID)
                return placement.location.position;

                
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
        
        return Random.insideUnitSphere;
    }

    [Serializable]
    public struct UIPlacement
    {
        public BrainIconID id;
        public RectTransform location;
    }
}
