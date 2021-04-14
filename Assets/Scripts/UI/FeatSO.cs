using UnityEngine;

[CreateAssetMenu(menuName = "Pond Water/Narration/Feat")]
public class FeatSO : ScriptableObject
{
    public FeatType type;
    public string message;
    public string description;
    public Color color = Color.white;
    public bool useEventFrame = true;
}
