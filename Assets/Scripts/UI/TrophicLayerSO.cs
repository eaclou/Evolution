using UnityEngine;

[CreateAssetMenu(menuName = "Pond Water/Trophic Layer")]
public class TrophicLayerSO : ScriptableObject
{
    public KnowledgeMapId id;
    
    public KingdomId kingdom;
    
    public int startingSlotCount = 1;
    
    [Tooltip("Sets shader channel for Terrain and Other types")]
    public int layerIndex;
    
    public string defaultSpeciesName;
    public Sprite icon;
    public Color color;
    
    public TrophicSlotStatus initialStatus;

    [Header("Knowledge map settings")]
    public string title;
    [Range(0, 1)] public float amplitude;
    public int channelSoloIndex;
    [Range(0, 1)] public float isChannelSolo;
    [Range(0, 1)] public float gamma;
    
    public string brushDescription;
}

// RENAME -> TrophicSlotId?
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
    Detritus,
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
