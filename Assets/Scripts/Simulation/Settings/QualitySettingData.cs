using System;
using UnityEngine;

public enum QualitySettingId { Low, Medium, High, Max }

[CreateAssetMenu(menuName = "ScriptableObjects/Quality Settings")]
public class QualitySettingData : ScriptableObject
{
    [SerializeField] Binding[] bindings;
    
    public int GetResolution(int index) { return bindings[index].resolution; }
    public int GetAgentCount(int index) { return bindings[index].numberAgents; }
    public int GetEggSackCount(int index) { return bindings[index].numberEggSacks; }
    public int GetHiddenNeuronCont(int index) { return bindings[index].numberInitialHiddenNeurons; }

    public int GetResolution(QualitySettingId id)
    {
        foreach (var item in bindings)
            if (item.id == id)
                return item.resolution;
	
        Debug.LogError("Unable to find resolution " + id);
        return bindings[0].resolution;
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
