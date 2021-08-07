using UnityEngine;

public class WorldLayerTerrainGenome 
{
    public string name;
    public string textDescriptionMutation;
    public Color color;
    public float elevationChange;
    
    public WorldLayerTerrainGenome() 
    {  
        color = Color.white;
        elevationChange = 1f;
    }
}
