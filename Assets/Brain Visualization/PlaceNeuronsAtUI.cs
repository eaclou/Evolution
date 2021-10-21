using System;
using UnityEngine;

public class PlaceNeuronsAtUI : MonoBehaviour
{
    [SerializeField] UIPlacement[] uiPlacements;

    public Vector3 GetNeuronPosition(Neuron neuron)
    {
        var data = neuron.genome.data;

        foreach (var placement in uiPlacements)
            if (placement.id == data.iconID)
            {
                //Debug.Log($"Found placement at {placement.location.position}");  // OK
                return placement.location.position;
            }
                
        Debug.LogError($"Unable to find placement for {neuron.moduleID} {data}");
        return Vector3.zero;
    }
    
    public Vector3 GetHiddenNeuronPosition(Neuron neuron)
    {
        // Get input location
        // Get output location
        // Calculate & return intermediate location
        return Vector3.zero;
    }

    [Serializable]
    public struct UIPlacement
    {
        public BrainIconID id;
        public RectTransform location;
    }
}
