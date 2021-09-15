using System;

// * WPP: convert to Scriptable Object to enable multiple test configurations,
// Make globally accessible via Lookup.cs so not necessary to pass around as parameter
[Serializable]
public class MutationSettings {
    public float brainInitialConnectionChance;
    public float brainWeightMutationChance;
    public float brainWeightMutationStepSize;
    public float brainRemoveLinkChance;
    public float brainWeightDecayAmount;
    public float brainCreateNewLinkChance;
    public float brainCreateNewHiddenNodeChance;

    public float bodyColorsMutationChance;
    public float bodyColorsMutationStepSize;
    public float bodyCoreSizeMutationChance;
    public float bodyCoreMutationStepSize;
    public float bodyProportionsMutationChance;
    public float bodyProportionsMutationStepSize;
    public float bodyEyeProportionsMutationChance;
    public float bodyEyeProportionsMutationStepSize;    
    public float bodyModuleCreateNewChance;
    public float bodyModuleInternalMutationChance;
    public float bodyModuleInternalMutationStepSize;
    public float bodyModuleRemoveExistingChance;
    public float bodyTalentSpecMutationChance;
    public float bodyTalentSpecMutationStepSize;
    public float bodyDietSpecMutationChance;
    public float bodyDietSpecMutationStepSize;

    public float defaultFoodMutationChance;
    public float defaultFoodMutationStepSize;
    //public float mutationStrengthSlot;


    public MutationSettings(float initialConnectionChance, float mutationChance, float mutationStepSize, float removeLinkChance, float weightDecayAmount, float newLinkChance, float newHiddenNodeChance, float mutationStrengthSlot) {
        this.brainInitialConnectionChance = initialConnectionChance;
        this.brainWeightMutationChance = mutationChance;
        this.brainWeightMutationStepSize = mutationStepSize;
        this.brainRemoveLinkChance = removeLinkChance;
        this.brainWeightDecayAmount = weightDecayAmount;
        this.brainCreateNewLinkChance = newLinkChance;
        this.brainCreateNewHiddenNodeChance = newHiddenNodeChance;
        //this.mutationStrengthSlot = mutationStrengthSlot;
        
              
        bodyColorsMutationChance = 0.15f;
        bodyColorsMutationStepSize = 0.02f;
        bodyCoreSizeMutationChance = 0.25f;
        bodyCoreMutationStepSize = 0.015f;
        bodyProportionsMutationChance = 0.36f;
        bodyProportionsMutationStepSize = 0.0005f;
        bodyEyeProportionsMutationChance = 0.35f;
        bodyEyeProportionsMutationStepSize = 0.0005f; 
        bodyModuleCreateNewChance = 0.01f;
        bodyModuleInternalMutationChance = 0.01f;
        bodyModuleInternalMutationStepSize = 0.25f;
        bodyModuleRemoveExistingChance = 0.01f;
        bodyTalentSpecMutationChance = 0.15f;
        bodyTalentSpecMutationStepSize = 0.175f;
        bodyDietSpecMutationChance = 0.15f;
        bodyDietSpecMutationStepSize = 0.175f;

        defaultFoodMutationChance = 0.01f;
        defaultFoodMutationStepSize = 0.01f;
        //mutationStrengthSlot = 0f;
    }

    public MutationSettings() {
        // Zero-chance settings init
        this.brainInitialConnectionChance = 0f;
        this.brainWeightMutationChance = 0f;
        this.brainWeightMutationStepSize = 0f;
        this.brainRemoveLinkChance = 0f;
        this.brainWeightDecayAmount = 1f;
        this.brainCreateNewLinkChance = 0f;
        this.brainCreateNewHiddenNodeChance = 0f;        
              
        bodyColorsMutationChance = 0f;
        bodyColorsMutationStepSize = 0f;
        bodyCoreSizeMutationChance = 0f;
        bodyCoreMutationStepSize = 0f;
        bodyProportionsMutationChance = 0f;
        bodyProportionsMutationStepSize = 0f;
        bodyEyeProportionsMutationChance = 0f;
        bodyEyeProportionsMutationStepSize = 0f; 
        bodyModuleCreateNewChance = 0f;
        bodyModuleInternalMutationChance = 0f;
        bodyModuleInternalMutationStepSize = 0f;
        bodyModuleRemoveExistingChance = 0f;
        bodyTalentSpecMutationChance = 0f;
        bodyTalentSpecMutationStepSize = 0f;
        bodyDietSpecMutationChance = 0f;
        bodyDietSpecMutationStepSize = 0f;

        defaultFoodMutationChance = 0f;
        defaultFoodMutationStepSize = 0f;
    }
}
