using System;
using System.Linq;
using UnityEngine;

/// Maps neuron IDs to IO types, extend to include other static & centralized mappings
[CreateAssetMenu(menuName = "Pond Water/Brain/Neuron Map", fileName = "Neuron Map")]
public class NeuralMap : ScriptableObject
{
    [SerializeField] NeuronMap[] mappings;

    public NeuronType GetIO(int id)
    {
        foreach (var mapping in mappings)
            if (mapping.ids.Contains(id))
                return mapping.io;
                
        Debug.LogError($"No mapping found for id {id}");
        return id < 100 ? NeuronType.In : NeuronType.Out;
    }

    [Serializable]
    public struct NeuronMap
    {
        public NeuronType io;
        public int[] ids;
    }
}