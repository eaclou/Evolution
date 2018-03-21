using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodGenome {

    public int index = -1;

    public Vector2 fullSize = new Vector2(2f, 2f);

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

    public Vector2 randomSeed; // ??

    // In Future Add:
    // Brush type
    // Leaf texture type
    // Fruit Texture type
    // Stem texture type
    // Stem Girth
    // Preferred Branching Angle
    // etc. etc. etc....

	public FoodGenome(int index) {
        this.index = index;
    }

    public void InitializeAsRandomGenome() {

        fullSize = new Vector2(UnityEngine.Random.Range(2.5f, 2.5f), UnityEngine.Random.Range(5f, 5f));
        
        fruitHue = new Vector3(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
        leafHue = new Vector3(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
        stemHue = new Vector3(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));

        stemBrushType = UnityEngine.Random.Range(0, 8); // 8 texture types
        leafBrushType = UnityEngine.Random.Range(0, 8);
        fruitBrushType = UnityEngine.Random.Range(0, 8);

        stemWidth = UnityEngine.Random.Range(0.4f, 0.99f);

        leafScale = new Vector2(UnityEngine.Random.Range(0.15f, 0.25f), UnityEngine.Random.Range(0.15f, 0.25f));
        fruitScale = new Vector2(UnityEngine.Random.Range(0.06f, 0.15f), UnityEngine.Random.Range(0.06f, 0.15f));
    }

    public void SetToMutatedCopyOfParentGenome(FoodGenome parentFoodGenome, MutationSettings settings) {
        // *** Result needs to be fully independent copy and share no references!!!
        
        fullSize = UtilityMutationFunctions.GetMutatedVector2Additive(parentFoodGenome.fullSize, settings.defaultFoodMutationChance, settings.defaultFoodMutationStepSize, 2.5f, 6f);
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
