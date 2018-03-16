using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MutationSettings {
    public float initialConnectionChance;

    public float mutationChance;
    public float mutationStepSize;

    public float zeroWeightChance;
    public float weightDecayAmount;
    public float newLinkChance;
    public float newHiddenNodeChance;

    public float defaultBodyMutationChance;
    public float defaultBodyMutationStepSize;

    public MutationSettings(float initialConnectionChance, float mutationChance, float mutationStepSize, float zeroWeightChance, float weightDecayAmount, float newLinkChance, float newHiddenNodeChance) {
        this.initialConnectionChance = initialConnectionChance;
        this.mutationChance = mutationChance;
        this.mutationStepSize = mutationStepSize;
        this.zeroWeightChance = zeroWeightChance;
        this.weightDecayAmount = weightDecayAmount;
        this.newLinkChance = newLinkChance;
        this.newHiddenNodeChance = newHiddenNodeChance;

        defaultBodyMutationChance = 0.05f;
        defaultBodyMutationStepSize = 0.1f;
    }
}
