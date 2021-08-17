using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldLayerAlgaeGenome 
{
    public string name;
    public float metabolicRate;
    public float growthEfficiency;
    public Color displayColorPri;
    public Color displayColorSec;
    public int patternRowID;
    public int patternColumnID;
    public float patternThreshold;
    
    /// Create a new Algae genome from scratch
    public WorldLayerAlgaeGenome() 
    {
        float minIntakeRate = 0.009f;
        float maxIntakeRate = 0.012f; // init around 1?
        float lnLerp = Random.Range(0f, 1f);
        lnLerp *= lnLerp;
        metabolicRate = Mathf.Lerp(minIntakeRate, maxIntakeRate, lnLerp);        
        growthEfficiency = 1f;

        displayColorPri = Color.white;
        displayColorSec = Color.black;

        patternRowID = Random.Range(0, 8);
        patternColumnID = Random.Range(0, 8);
        patternThreshold = Random.Range(0f, 1f);
    }
    
    /// Mutate an existing Algae genome
    public WorldLayerAlgaeGenome(WorldLayerAlgaeGenome original, float tempSharedIntakeRate)
    {
        float jLerp = 0.3f;
        Color randColorPri = Random.ColorHSV();
        Color randColorSec = Random.ColorHSV();
        Color mutatedColorPri = Color.Lerp(original.displayColorPri, randColorPri, jLerp);
        Color mutatedColorSec = Color.Lerp(original.displayColorSec, randColorSec, jLerp);
        bool useAlgaeSlotPattern = Random.Range(0f, 1f) < jLerp;
        patternRowID = useAlgaeSlotPattern ? original.patternRowID : Random.Range(0, 8);
        patternColumnID = useAlgaeSlotPattern ? original.patternColumnID : Random.Range(0, 8);
        float minIntakeRate = tempSharedIntakeRate * 0.1f;
        float maxIntakeRate = tempSharedIntakeRate * 10f; // init around 1?
        float lnLerp = Mathf.Pow(Random.Range(0f, 1f), 2);
            
        displayColorPri = mutatedColorPri;
        displayColorSec = mutatedColorSec;
        patternThreshold = Mathf.Lerp(original.patternThreshold, Random.Range(0f, 1f), jLerp);
        metabolicRate = Mathf.Lerp(minIntakeRate, maxIntakeRate, lnLerp);
        metabolicRate = Mathf.Lerp(original.metabolicRate, metabolicRate, jLerp);
        growthEfficiency = Random.Range(0.1f, 2f);
    }
}
