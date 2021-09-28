using System;
using UnityEngine;

public enum MutationSettingsId
{
    None,
    Vertebrate,
    Supervised,
}

// ScriptableObject stores static default setting variants,
// MutationSettingsInstance stores mutable data
// Access templates or get copies through Lookup 
[CreateAssetMenu(menuName = "Pond Water/Mutation Settings", fileName = "Mutation Settings")]
public class MutationSettings : ScriptableObject
{
    public MutationSettingsInstance data;
    public MutationSettingsInstance GetCopy() { return new MutationSettingsInstance(data); }
    
    //public float mutationStrengthSlot;

    // WPP: variations stored in SO instances
    /*
    public MutationSettings(float initialConnectionChance, float mutationChance, float mutationStepSize, float removeLinkChance, float weightDecayAmount, float newLinkChance, float newHiddenNodeChance, float mutationStrengthSlot) {
        brainInitialConnectionChance = initialConnectionChance;
        brainWeightMutationChance = mutationChance;
        brainWeightMutationStepSize = mutationStepSize;
        brainRemoveLinkChance = removeLinkChance;
        brainWeightDecayAmount = weightDecayAmount;
        brainCreateNewLinkChance = newLinkChance;
        brainCreateNewHiddenNodeChance = newHiddenNodeChance;
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

    /// Zero-chance settings init
    public MutationSettings() {
        brainInitialConnectionChance = 0f;
        brainWeightMutationChance = 0f;
        brainWeightMutationStepSize = 0f;
        brainRemoveLinkChance = 0f;
        brainWeightDecayAmount = 1f;
        brainCreateNewLinkChance = 0f;
        brainCreateNewHiddenNodeChance = 0f;        
              
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
    */
}

[Serializable]
public class MutationSettingsInstance
{
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
    
    public MutationSettingsInstance(MutationSettingsInstance template) { Copy(template); }
    
    public void Copy(MutationSettingsInstance template)
    {
        brainInitialConnectionChance = template.brainInitialConnectionChance;
        brainWeightMutationChance = template.brainWeightMutationChance;
        brainWeightMutationStepSize = template.brainWeightMutationStepSize;
        brainRemoveLinkChance = template.brainRemoveLinkChance;
        brainWeightDecayAmount = template.brainWeightDecayAmount;
        brainCreateNewLinkChance = template.brainCreateNewLinkChance;
        brainCreateNewHiddenNodeChance = template.brainCreateNewHiddenNodeChance;

        bodyColorsMutationChance = template.bodyColorsMutationChance;
        bodyColorsMutationStepSize = template.brainWeightMutationStepSize;
        bodyCoreSizeMutationChance = template.bodyCoreSizeMutationChance;
        bodyCoreMutationStepSize = template.bodyCoreMutationStepSize;
        bodyProportionsMutationChance = template.bodyProportionsMutationChance;
        bodyProportionsMutationStepSize = template.bodyProportionsMutationStepSize;
        bodyEyeProportionsMutationChance = template.bodyEyeProportionsMutationChance;
        bodyEyeProportionsMutationStepSize = template.bodyEyeProportionsMutationStepSize;
        bodyModuleCreateNewChance = template.bodyModuleCreateNewChance;
        bodyModuleInternalMutationChance = template.bodyModuleInternalMutationChance;
        bodyModuleInternalMutationStepSize = template.bodyModuleInternalMutationStepSize;
        bodyModuleRemoveExistingChance = template.bodyModuleRemoveExistingChance;
        bodyTalentSpecMutationChance = template.bodyTalentSpecMutationChance;
        bodyTalentSpecMutationStepSize = template.bodyTalentSpecMutationStepSize;
        bodyDietSpecMutationChance = template.bodyDietSpecMutationChance;
        bodyDietSpecMutationStepSize = template.bodyDietSpecMutationStepSize;

        defaultFoodMutationChance = template.defaultFoodMutationChance;
        defaultFoodMutationStepSize = template.defaultFoodMutationStepSize; 
    }   
}
