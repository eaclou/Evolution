using UnityEngine;
using UnityEngine.UI;

/// Project-specific helper functions
public static class PondHelpers
{
    /// Display a 0-1 number as a percentage
    public static void SetPercentText(Text text, float value) 
    { 
        text.text = (value * 100f).ToString("F0"); 
    }
    
    /// Vertically scales an image in local space
    public static void SetLocalYScale(Image image, float yScale) 
    { 
        image.transform.localScale = new Vector3(1f, yScale, 1f);
    }
    
    /// Increment current until it reaches max, then return to zero. 
    /// Max is exclusive, extend if non-exclusive maximum needed
    public static int CycleInt(int current, int max)
    {
        var result = current + 1;
        if (current >= max) result = 0;
        return result;
    }
}
