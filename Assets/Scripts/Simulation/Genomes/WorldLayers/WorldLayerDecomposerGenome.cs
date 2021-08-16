using UnityEngine;

public class WorldLayerDecomposerGenome 
{
    public string name;
    public string textDescriptionMutation;
    public Color displayColorPri;
    public Color displayColorSec;
    public int patternRowID;
    public int patternColumnID;
    public float patternThreshold;
    public float metabolicRate;
    public float growthEfficiency;
	
    public WorldLayerDecomposerGenome(string name, float metabolicRate, float growthEfficiency) 
    {
        this.name = name;
        this.metabolicRate = metabolicRate;
        this.growthEfficiency = growthEfficiency;
       
        displayColorPri = Color.white;
        displayColorSec = Color.black;
        patternRowID = Random.Range(0, 8);
        patternColumnID = Random.Range(0, 8);
        patternThreshold = Random.Range(0f, 1f);
        textDescriptionMutation = "Metabolic Rate: " + (metabolicRate * 100f).ToString("F2");
    }
    
    public WorldLayerDecomposerGenome(WorldLayerDecomposerGenome original, float tempSharedIntakeRate)
    {
        name = original.name;
        growthEfficiency = Random.Range(0.1f, 2f);
        float mutationSizeLerp = 0.3f;
        Color randColorPri = Random.ColorHSV();
        Color randColorSec = Random.ColorHSV();
        displayColorPri = Color.Lerp(original.displayColorPri, randColorPri, mutationSizeLerp);
        displayColorSec = Color.Lerp(original.displayColorSec, randColorSec, mutationSizeLerp);
        bool useRandomID = Random.Range(0f, 1f) < mutationSizeLerp;
        patternRowID = useRandomID ? Random.Range(0, 8) : original.patternRowID;
        patternColumnID = useRandomID ? Random.Range(0, 8) : original.patternColumnID;
        float minIntakeRate = tempSharedIntakeRate * 0.1f;
        float maxIntakeRate = tempSharedIntakeRate * 4f;
        float lnLerp = Mathf.Pow(Random.Range(0f, 1f), 2);
        float baseMetabolicRate = Mathf.Lerp(minIntakeRate, maxIntakeRate, lnLerp);
        metabolicRate = Mathf.Lerp(original.metabolicRate, baseMetabolicRate, mutationSizeLerp);
        //textDescriptionMutation = "Metabolic Rate: " + (metabolicRate * 100f).ToString("F2");
        patternThreshold = Mathf.Lerp(original.patternThreshold, Random.Range(0f, 1f), mutationSizeLerp);
    }
}
