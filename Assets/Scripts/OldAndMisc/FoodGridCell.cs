using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable] // visible in inspector
public class FoodGridCell {

    public int numFoodLayers = 1;
    public float[] foodAmountsPerLayerArray;
    public Vector2[] gradientFoodAmountsPerLayerArray;

	public FoodGridCell(int numLayers) {
        numFoodLayers = numLayers;
        // different sizes of food
        // one size per layer - number of layers determines granularity
        foodAmountsPerLayerArray = new float[numLayers];
        gradientFoodAmountsPerLayerArray = new Vector2[numLayers];

        float totalInitFoodAmount = UnityEngine.Random.Range(0f, 1f) * UnityEngine.Random.Range(0f, 1f);

        for(int i = 0; i < foodAmountsPerLayerArray.Length; i++) {
            //float 
            foodAmountsPerLayerArray[i] = totalInitFoodAmount / (float)numLayers;
        }
    }

    
}
