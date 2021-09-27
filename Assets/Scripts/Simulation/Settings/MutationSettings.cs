using System;
using UnityEngine;

// * WPP: convert to Scriptable Object to enable multiple test configurations,
// Make globally accessible via Lookup.cs so not necessary to pass around as parameter
[CreateAssetMenu(menuName = "Pond Water/Mutation Settings", fileName = "Mutation Settings")]
public class MutationSettings : ScriptableObject
{
    /*
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
    */
    
    public MutationSettingsInstance data;
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
    
    public MutationSettingsInstance(MutationSettings template) { CopyFromTemplate(template); }
    
    public void CopyFromTemplate(MutationSettings template)
    {
        brainInitialConnectionChance = template.data.brainInitialConnectionChance;
        brainWeightMutationChance = template.data.brainWeightMutationChance;
        brainWeightMutationStepSize = template.data.brainWeightMutationStepSize;
        brainRemoveLinkChance = template.data.brainRemoveLinkChance;
        brainWeightDecayAmount = template.data.brainWeightDecayAmount;
        brainCreateNewLinkChance = template.data.brainCreateNewLinkChance;
        brainCreateNewHiddenNodeChance = template.data.brainCreateNewHiddenNodeChance;

        bodyColorsMutationChance = template.data.bodyColorsMutationChance;
        bodyColorsMutationStepSize = template.data.brainWeightMutationStepSize;
        bodyCoreSizeMutationChance = template.data.bodyCoreSizeMutationChance;
        bodyCoreMutationStepSize = template.data.bodyCoreMutationStepSize;
        bodyProportionsMutationChance = template.data.bodyProportionsMutationChance;
        bodyProportionsMutationStepSize = template.data.bodyProportionsMutationStepSize;
        bodyEyeProportionsMutationChance = template.data.bodyEyeProportionsMutationChance;
        bodyEyeProportionsMutationStepSize = template.data.bodyEyeProportionsMutationStepSize;
        bodyModuleCreateNewChance = template.data.bodyModuleCreateNewChance;
        bodyModuleInternalMutationChance = template.data.bodyModuleInternalMutationChance;
        bodyModuleInternalMutationStepSize = template.data.bodyModuleInternalMutationStepSize;
        bodyModuleRemoveExistingChance = template.data.bodyModuleRemoveExistingChance;
        bodyTalentSpecMutationChance = template.data.bodyTalentSpecMutationChance;
        bodyTalentSpecMutationStepSize = template.data.bodyTalentSpecMutationStepSize;
        bodyDietSpecMutationChance = template.data.bodyDietSpecMutationChance;
        bodyDietSpecMutationStepSize = template.data.bodyDietSpecMutationStepSize;

        defaultFoodMutationChance = template.data.defaultFoodMutationChance;
        defaultFoodMutationStepSize = template.data.defaultFoodMutationStepSize; 
    }   
}
