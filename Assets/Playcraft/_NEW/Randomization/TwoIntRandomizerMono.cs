using System;
using UnityEngine;

/// Tests random not repeating int generation system
public class TwoIntRandomizerMono : MonoBehaviour
{
    [Header("Settings")]
    public int width, height;
    public bool initialize;
    
    [Header("Validation Trigger")]
    public bool trigger;
    
    [Header("Results")]
    public string value;
    
    public TwoIntRandomizer set;

    void OnValidate()
    {
        if (initialize)
        {
            set = new TwoIntRandomizer(width, height);
            value = String.Empty;
            initialize = false;
        }
    
        if (trigger)
        {
            var d1 = set.RandomNoRepeat();
            value += $"{d1}, ";
            trigger = false;
        }
    }
}
