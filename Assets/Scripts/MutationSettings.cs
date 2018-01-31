using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MutationSettings {

    public float mutationChance;
    public float mutationStepSize;

    public float newLinkChance;
    public float newHiddenNodeChance;

    public MutationSettings(float mutationChance, float mutationStepSize, float newLinkChance, float newHiddenNodeChance) {
        this.mutationChance = mutationChance;
        this.mutationStepSize = mutationStepSize;
        this.newLinkChance = newLinkChance;
        this.newHiddenNodeChance = newHiddenNodeChance;
    }
}
