using Playcraft;
using UnityEngine;

// is this even needed anymore??? *****
public class EggSackGenome {  
    public int index = -1;  // * WPP: assigned but not used 

    public Vector2 fullSize = new Vector2(2f, 2f);

    // * WPP: never used
    public float foodProportionR = 1f;
    public float foodProportionG = 1f;
    public float foodProportionB = 1f;
        
    public Vector3 stemHue;
    public Vector3 leafHue;
    public Vector3 fruitHue;

    public int stemBrushType;
    public int leafBrushType;
    public int fruitBrushType;

    public float stemWidth;
    public Vector2 leafScale;
    public Vector2 fruitScale;
    
    const int TEXTURE_COUNT = 8;
    Vector2 fullSizeRange = new Vector2(.25f, 4.5f);
    Vector2 leafScaleRange = new Vector2(0.15f, 0.25f);
    Vector2 fruitScaleRange = new Vector2(0.06f, 0.15f);
    Vector2 stemWidthRange = new Vector2(.4f, .99f);

    public Vector2 randomSeed; // ??
    
	public EggSackGenome(int index) {
        this.index = index;
    }

    // WPP 5/1/21: Delegated nontrivial randomization math to statics, 
    // exposed values, and applied constant.
    public void InitializeAsRandomGenome() {
    
        fullSize = VectorMath.RandomVector2(fullSizeRange);
        
        fruitHue = VectorMath.RandomPercent3();
        leafHue = VectorMath.RandomPercent3();
        stemHue = VectorMath.RandomPercent3();

        stemBrushType = Random.Range(0, TEXTURE_COUNT);
        leafBrushType = Random.Range(0, TEXTURE_COUNT);
        fruitBrushType = Random.Range(0, TEXTURE_COUNT);

        stemWidth = Random.Range(stemWidthRange.x, stemWidthRange.y);

        leafScale = VectorMath.RandomVector2(leafScaleRange);
        fruitScale = VectorMath.RandomVector2(fruitScaleRange);
    }

    public void SetToMutatedCopyOfParentGenome(EggSackGenome parentFoodGenome, MutationSettingsInstance settings) {
        // *** Result needs to be fully independent copy and share no references!!!
        
        fullSize = UtilityMutationFunctions.GetMutatedVector2Additive(parentFoodGenome.fullSize, settings.defaultFoodMutationChance, settings.defaultFoodMutationStepSize, 0.25f, 4.5f);
        if(fullSize.y < fullSize.x) {
            fullSize.y = fullSize.x;
        }
        
        // Set equal to parent at first, then check for possible mutation of that value:        
        fruitHue = UtilityMutationFunctions.GetMutatedVector3Additive(parentFoodGenome.fruitHue, settings.defaultFoodMutationChance, settings.defaultFoodMutationStepSize, 0f, 1f);
        leafHue = UtilityMutationFunctions.GetMutatedVector3Additive(parentFoodGenome.leafHue, settings.defaultFoodMutationChance, settings.defaultFoodMutationStepSize, 0f, 1f);
        stemHue = UtilityMutationFunctions.GetMutatedVector3Additive(parentFoodGenome.stemHue, settings.defaultFoodMutationChance, settings.defaultFoodMutationStepSize, 0f, 1f);
        
        stemBrushType = UtilityMutationFunctions.GetMutatedIntAdditive(parentFoodGenome.stemBrushType, settings.defaultFoodMutationChance, 3, 0, 7);
        leafBrushType = UtilityMutationFunctions.GetMutatedIntAdditive(parentFoodGenome.leafBrushType, settings.defaultFoodMutationChance, 3, 0, 7);
        fruitBrushType = UtilityMutationFunctions.GetMutatedIntAdditive(parentFoodGenome.fruitBrushType, settings.defaultFoodMutationChance, 3, 0, 7);

        stemWidth = UtilityMutationFunctions.GetMutatedFloatAdditive(parentFoodGenome.stemWidth, settings.defaultFoodMutationChance, settings.defaultFoodMutationStepSize, 0.1f, 0.8f);

        leafScale = UtilityMutationFunctions.GetMutatedVector2Additive(parentFoodGenome.leafScale, settings.defaultFoodMutationChance, settings.defaultFoodMutationStepSize, 0.15f, 0.25f);
        fruitScale = UtilityMutationFunctions.GetMutatedVector2Additive(parentFoodGenome.fruitScale, settings.defaultFoodMutationChance, settings.defaultFoodMutationStepSize, 0.06f, 0.15f);
    }
}
