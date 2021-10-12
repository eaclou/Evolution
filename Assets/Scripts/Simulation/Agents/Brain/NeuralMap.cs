using System.Collections.Generic;
using UnityEngine;

/// Maps neuron IDs to IO types, extend to include other static & centralized mappings
[CreateAssetMenu(menuName = "Pond Water/Brain/Neuron Map", fileName = "Neuron Map")]
public class NeuralMap : ScriptableObject
{
    public MetaNeuron hiddenTemplate;
    [SerializeField] MetaNeuron[] catalog;

    public NeuronGenome GetData(string name)
    {
        foreach (var item in catalog)
            if (item.name == name)
                return new NeuronGenome(item);
                
        Debug.LogError($"Unable to find neuron data for {name}");
        return null;
    }
    
    public List<NeuronGenome> GetAllByModule(BrainModuleID moduleID)
    {
        var list = new List<NeuronGenome>();
        
        foreach (var item in catalog)
            if (item.moduleID == moduleID)
                list.Add(new NeuronGenome(item));
                
        return list;
    }
}