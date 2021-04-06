using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Feat")]
public class FeatSO : ScriptableObject
{
    public FeatType type;
    public string message;
    public string description;
    public Color color = Color.white;
    public bool useEventFrame = true;
}
