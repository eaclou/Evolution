﻿using UnityEngine;

// OBSOLETE: folded into TrophicSlotSO
[CreateAssetMenu(menuName = "Pond Water/Narration/Unlock Event")]
public class NarrationSO : ScriptableObject
{
    public string message;
    public Color color;
    public FeatSO[] feats;
}
