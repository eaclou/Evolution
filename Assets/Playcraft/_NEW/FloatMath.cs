using UnityEngine;

public static class FloatMath
{
    public static float GetHighest(float[] array) { return array[GetHighestIndex(array)]; }
    public static int GetHighestIndex(float[] array)
    {
        var index = 0;
        var value = -Mathf.Infinity;
        
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] > value)
            {
                value = array[i];
                index = i;
            }
        }
        
        return index;
    }
}
