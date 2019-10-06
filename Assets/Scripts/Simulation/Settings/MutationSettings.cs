using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MutationSettings {
    public float initialConnectionChance;

    public float mutationChance;
    public float mutationStepSize;

    public float removeLinkChance;
    public float weightDecayAmount;
    public float newLinkChance;
    public float newHiddenNodeChance;

    public float defaultBodyMutationChance;
    public float defaultBodyMutationStepSize;

    public float newBodyModuleChance;
    public float bodyModuleMutationChance;
    public float removeBodyModuleChance;

    public float defaultFoodMutationChance;
    public float defaultFoodMutationStepSize;

    public float mutationStrengthSlot;


    public MutationSettings(float initialConnectionChance, float mutationChance, float mutationStepSize, float removeLinkChance, float weightDecayAmount, float newLinkChance, float newHiddenNodeChance, float mutationStrengthSlot) {
        this.initialConnectionChance = initialConnectionChance;
        this.mutationChance = mutationChance;
        this.mutationStepSize = mutationStepSize;
        this.removeLinkChance = removeLinkChance;
        this.weightDecayAmount = weightDecayAmount;
        this.newLinkChance = newLinkChance;
        this.newHiddenNodeChance = newHiddenNodeChance;
        this.mutationStrengthSlot = mutationStrengthSlot;

        defaultBodyMutationChance = 0.178f;
        defaultBodyMutationStepSize = 0.0045f;

        defaultFoodMutationChance = 0.01f;
        defaultFoodMutationStepSize = 0.01f;

        newBodyModuleChance = 0.005f;
        bodyModuleMutationChance = 0.01f;
        removeBodyModuleChance = 0.001f;

        //mutationStrengthSlot = 0f;
    }
}
