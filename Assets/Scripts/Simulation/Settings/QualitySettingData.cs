using System;
using UnityEngine;

public enum QualitySettingId { Low, Medium, High, Max }

// * Consider splitting into fluid physics and simulation complexity
[CreateAssetMenu(menuName = "ScriptableObjects/Quality Settings")]
public class QualitySettingData : ScriptableObject
{
    [SerializeField] Binding[] bindings;
    
    public int GetAgentCount(int index) { return bindings[index].numberAgents; }
    public int GetAgentCount(QualitySettingId id) { return GetBinding(id).numberAgents; }

    public int GetEggSackCount(int index) { return bindings[index].numberEggSacks; }
    public int GetEggSackCount(QualitySettingId id) { return GetBinding(id).numberEggSacks; }
    
    public int GetHiddenNeuronCont(int index) { return bindings[index].numberInitialHiddenNeurons; }
    public int GetHiddenNeuronCont(QualitySettingId id) { return GetBinding(id).numberInitialHiddenNeurons; }

    public int GetResolution(int index) { return bindings[index].resolution; }
    public int GetResolution(QualitySettingId id) { return GetBinding(id).resolution; }
    
    Binding GetBinding(QualitySettingId id)
    {
        foreach (var item in bindings)
            if (item.id == id)
                return item;
	
        Debug.LogError("Unable to find resolution " + id);
        return bindings[0];        
    }
    
    public QualitySettingId GetBindingId(int index) { return bindings[index].id; }
    
    public int GetBindingIndex(QualitySettingId id)
    {
        for (int i = 0; i < bindings.Length; i++)
            if (bindings[i].id == id)
                return i;
                
        Debug.LogError("Unable to find resolution " + id);
        return 0;
    }

    [Serializable]
    public struct Binding
    {
        public QualitySettingId id;
        public int resolution;
        public int numberAgents;
        public int numberEggSacks;
        public int numberInitialHiddenNeurons;
    }
}
