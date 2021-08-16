using UnityEngine;

public class WorldLayerPlantGenome 
{
    public string name;
    public string textDescriptionMutation;
    public float growthRate;
    
    public Color displayColorPri;
    public Color displayColorSec;
    public int patternRowID;
    public int patternColumnID;
    public float patternThreshold;
    public PlantParticleData plantRepData;
   
    public WorldLayerPlantGenome(string name, PlantParticleData plantRepData, float growthRate = 1f) 
    {
        this.name = name;   
        this.growthRate = growthRate;
        this.plantRepData = plantRepData;
        
        textDescriptionMutation = "Growth Rate: " + growthRate.ToString("F2");
        displayColorPri = Color.white;
        displayColorSec = Color.black;
        patternRowID = Random.Range(0, 8);
        patternColumnID = Random.Range(0, 8);
        patternThreshold = Random.Range(0f, 1f);
    }
}
