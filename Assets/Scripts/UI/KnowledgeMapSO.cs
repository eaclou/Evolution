using UnityEngine;

[CreateAssetMenu(menuName = "Pond Water/Knowledge Map")]
public class KnowledgeMapSO : ScriptableObject
{
    public KnowledgeMapId id;
    public KingdomId kingdom;
    public int listIndex;
    public int slotIndex;
    
    public string title;
    [Range(0, 1)] public float amplitude;
    public int channelSoloIndex;
    [Range(0, 1)] public float isChannelSolo;
    [Range(0, 1)] public float gamma;
}

public enum KnowledgeMapId 
{ 
    Undefined,
    Decomposers, 
    Algae, 
    Plants, 
    Microbes, 
    Animals, 
    World, 
    Stone, 
    Pebbles, 
    Sand, 
    Nutrients, 
    Water, 
    Wind, 
}

public enum KingdomId
{
    Undefined,
    Decomposers,
    Plants,
    Animals,
    Terrain,
    Other,
}
