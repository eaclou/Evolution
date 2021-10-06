using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// Maps neuron IDs to IO types, extend to include other static & centralized mappings
[CreateAssetMenu(menuName = "Pond Water/Brain/Neuron Map", fileName = "Neuron Map")]
public class NeuralMap : ScriptableObject
{
    [SerializeField] MetaNeuron[] catalog;
    
    public NeuronGenome GetGenome(string name) { return new NeuronGenome(GetData(name)); }
    
    public MetaNeuron GetData(string name)
    {
        foreach (var item in catalog)
            if (item.name == name)
                return item;
                
        Debug.LogError($"Unable to find neuron data for {name}");
        return null;
    }
    
    public List<MetaNeuron> GetAllByModule(BrainModuleID moduleID)
    {
        var list = new List<MetaNeuron>();
        
        foreach (var item in catalog)
            if (item.moduleID == moduleID)
                list.Add(item);
                
        return list;
    }

    [Header("Obsolete")]
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