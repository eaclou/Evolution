using UnityEngine;

/// Maps neuron IDs to IO types, extend to include other static & centralized mappings
[CreateAssetMenu(menuName = "Pond Water/Brain/Neuron Map", fileName = "Neuron Map")]
public class NeuralMap : ScriptableObject
{
    public MetaNeuron hiddenTemplate;
    [SerializeField] MetaNeuron[] catalog;

    public NeuronGenome GetData(string name, int index)
    {
        foreach (var item in catalog)
            if (item.name == name)
                return item.GetNeuronGenome(index);
                
        Debug.LogError($"Unable to find neuron data for {name}");
        return null;
    }
}