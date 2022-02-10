using UnityEngine;
using Playcraft;

[CreateAssetMenu(menuName = "Pond Water/Agent/Hue Range")]
public class HueInfo : ScriptableObject
{
    public Vector2 red = new Vector2(0.25f, 0.75f);
    public Vector2 green = new Vector2(0.25f, 0.75f);
    public Vector2 blue = new Vector2(0.25f, 0.75f);
    
    public HueData GetHue() { return new HueData(this); }
}

public struct HueData
{
    public float red;
    public float green;
    public float blue;

    public HueData(HueInfo template)
    {
        red = RandomStatics.RandomRange(template.red);
        green = RandomStatics.RandomRange(template.green);
        blue = RandomStatics.RandomRange(template.blue);
    }
    
    public Vector3 GetValue() { return new Vector3(red, green, blue); }
}