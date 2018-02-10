﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MutationSettings {

    public float mutationChance;
    public float mutationStepSize;

    public float zeroWeightChance;
    public float weightDecayAmount;
    public float newLinkChance;
    public float newHiddenNodeChance;

    public MutationSettings(float mutationChance, float mutationStepSize, float zeroWeightChance, float weightDecayAmount, float newLinkChance, float newHiddenNodeChance) {
        this.mutationChance = mutationChance;
        this.mutationStepSize = mutationStepSize;
        this.zeroWeightChance = zeroWeightChance;
        this.weightDecayAmount = weightDecayAmount;
        this.newLinkChance = newLinkChance;
        this.newHiddenNodeChance = newHiddenNodeChance;
    }
}
