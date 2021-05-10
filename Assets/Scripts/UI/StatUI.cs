using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class StatUI
{
    public Text percent;    // * Consider rename
    public Image bar;
        
    public void RefreshPercent(float value, float scale)
    {
        RefreshDisplay(value * scale, value);
    }
    
    public void RefreshDisplay(float numericValue, float barScale, bool useDecimal = false)
    {
        percent.text = useDecimal ? numericValue.ToString("F2") : numericValue.ToString("F0");
        bar.transform.localScale = new Vector3(1f, barScale, 1f);
    }
}
