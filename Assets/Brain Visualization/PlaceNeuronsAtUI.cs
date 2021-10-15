using System;
using UnityEngine;

public class PlaceNeuronsAtUI : MonoBehaviour
{
    [SerializeField] UIPlacement[] uiPlacements;

    public Vector3 GetNeuronPosition(Neuron neuron)
    {
        foreach (var placement in uiPlacements)
            if (placement.id == neuron.genome.data.iconID)
                return placement.location.position;
                
        Debug.LogError($"Unable to find placement for neuron of type {neuron.moduleID}");
        return Vector3.zero;
    }

    [Serializable]
    public struct UIPlacement
    {
        public BrainIconID id;
        public RectTransform location;
    }
}
