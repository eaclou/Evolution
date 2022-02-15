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
        //return Random.insideUnitSphere;
        //***EAC IF TWO OR MORE NEURONS SHARE SAME EXACT POSITION IT CREATES A DIVIDE BY ZERO ERROR!!!!!!!!!***

        var data = neuron.genome.data;

        foreach (var placement in uiPlacements)
            if (placement.id == data.iconID)
                return (placement.location.position - new Vector3(960f, 920f, 0f)).normalized * 0.85f + Random.insideUnitSphere * 0.01f;

                
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
        
        return Random.insideUnitSphere * 0.25f;
    }

    [Serializable]
    public struct UIPlacement
    {
        public BrainIconID id;
        public RectTransform location;
    }
}
