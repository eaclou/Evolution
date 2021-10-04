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
