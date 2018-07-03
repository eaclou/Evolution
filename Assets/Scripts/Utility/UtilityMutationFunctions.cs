using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UtilityMutationFunctions {

    public static bool GetMutatedBool(bool curValue, float mutationChance, float mutationStepSize) {
        bool mutatedValue = curValue;

        float randomRoll = UnityEngine.Random.Range(0f, 1f);

        if (randomRoll < mutationChance) {
            float newVal = UnityEngine.Random.Range(0f, 1f);
            if(newVal < 0.5f) {
                mutatedValue = false;
            }
            else {
                mutatedValue = true;
            }
        }

        return mutatedValue;
    }

	public static float GetMutatedFloatAdditive(float curValue, float mutationChance, float mutationStepSize, float minValue, float maxValue) {
        float mutatedValue = curValue;

        float randomRoll = UnityEngine.Random.Range(0f, 1f);
        if (randomRoll < mutationChance) {
            float randomPerturbation = Gaussian.GetRandomGaussian();
            mutatedValue += Mathf.Lerp(0f, randomPerturbation, mutationStepSize);
            
            mutatedValue = Mathf.Max(mutatedValue, minValue);
            mutatedValue = Mathf.Min(mutatedValue, maxValue);
        }

        return mutatedValue;
    }

    public static Vector2 GetMutatedVector2Additive(Vector2 curValue, float mutationChance, float mutationStepSize, float minValue, float maxValue) {
        Vector2 mutatedValue = curValue;

        mutatedValue.x = GetMutatedFloatAdditive(mutatedValue.x, mutationChance, mutationStepSize, minValue, maxValue);
        mutatedValue.y = GetMutatedFloatAdditive(mutatedValue.y, mutationChance, mutationStepSize, minValue, maxValue);

        return mutatedValue;
    }
    public static Vector2 GetMutatedVector2Additive(Vector2 curValue, float mutationChance, float mutationStepSize, Vector2 minValue, Vector2 maxValue) {
        Vector2 mutatedValue = curValue;

        mutatedValue.x = GetMutatedFloatAdditive(mutatedValue.x, mutationChance, mutationStepSize, minValue.x, maxValue.x);
        mutatedValue.y = GetMutatedFloatAdditive(mutatedValue.y, mutationChance, mutationStepSize, minValue.y, maxValue.y);

        return mutatedValue;
    }

    public static Vector3 GetMutatedVector3Additive(Vector3 curValue, float mutationChance, float mutationStepSize, float minValue, float maxValue) {
        Vector3 mutatedValue = curValue;

        mutatedValue.x = GetMutatedFloatAdditive(mutatedValue.x, mutationChance, mutationStepSize, minValue, maxValue);
        mutatedValue.y = GetMutatedFloatAdditive(mutatedValue.y, mutationChance, mutationStepSize, minValue, maxValue);
        mutatedValue.z = GetMutatedFloatAdditive(mutatedValue.z, mutationChance, mutationStepSize, minValue, maxValue);

        return mutatedValue;
    }

    public static int GetMutatedIntAdditive(int curValue, float mutationChance, int maxMutationSize, int minValue, int maxValue) {
        int mutatedValue = curValue;

        float randomRoll = UnityEngine.Random.Range(0f, 1f);
        if (randomRoll < mutationChance) {
            int randomPerturbation = UnityEngine.Random.Range(-maxMutationSize, maxMutationSize + 1);
            mutatedValue += randomPerturbation;

            mutatedValue = Mathf.Max(mutatedValue, minValue);
            mutatedValue = Mathf.Min(mutatedValue, maxValue);
        }

        return mutatedValue;
    }
}
