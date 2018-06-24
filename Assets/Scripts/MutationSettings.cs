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

    public float defaultFoodMutationChance;
    public float defaultFoodMutationStepSize;

    public MutationSettings(float initialConnectionChance, float mutationChance, float mutationStepSize, float zeroWeightChance, float weightDecayAmount, float newLinkChance, float newHiddenNodeChance) {
        this.initialConnectionChance = initialConnectionChance;
        this.mutationChance = mutationChance;
        this.mutationStepSize = mutationStepSize;
        this.zeroWeightChance = zeroWeightChance;
        this.weightDecayAmount = weightDecayAmount;
        this.newLinkChance = newLinkChance;
        this.newHiddenNodeChance = newHiddenNodeChance;

        defaultBodyMutationChance = 0.033f;
        defaultBodyMutationStepSize = 0.33f;

        defaultFoodMutationChance = 0.01f;
        defaultFoodMutationStepSize = 0.4f;

    }
}
