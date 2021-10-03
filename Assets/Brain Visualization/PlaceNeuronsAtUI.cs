using System;
using System.Collections.Generic;
using UnityEngine;

public class PlaceNeuronsAtUI : MonoBehaviour
{
    [SerializeField] UIPlacement[] uiPlacements;

    public SocketInitData[] AssignNeuralPositions(List<Neuron> neurons)
    {
        SocketInitData[] sockets = new SocketInitData[neurons.Count];
    
        for (int i = 0; i < neurons.Count; i++)
            sockets[i].pos = GetNeuronLocation(neurons[i]).position;

        return sockets;
    }
    
    RectTransform GetNeuronLocation(Neuron neuron)
    {
        foreach (var placement in uiPlacements)
            if (placement.moduleID == neuron.moduleID)
                return placement.location;
                
        Debug.LogError($"Unable to find placement for neuron of type {neuron.moduleID}");
        return null;
    }

    [Serializable]
    public struct UIPlacement
    {
        public BrainModuleID moduleID;
        public RectTransform location;
    }
}
