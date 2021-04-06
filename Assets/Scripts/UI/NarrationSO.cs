using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Narration")]
public class NarrationSO : ScriptableObject
{
    public string message;
    public Color color;
    public FeatSO[] feats;
}
