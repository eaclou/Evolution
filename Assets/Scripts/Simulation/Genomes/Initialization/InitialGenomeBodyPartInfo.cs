using UnityEngine;
using Playcraft;

[CreateAssetMenu(menuName = "Pond Water/Agent/Genome Body Part Initialization")]
public class InitialGenomeBodyPartInfo : ScriptableObject
{
    [Header("Randomized Ranges")]
    public Vector2 length = new Vector2(0.75f, 1.25f);
    public Vector2 frontWidth = new Vector2(0.75f, 1.25f);
    public Vector2 frontHeight = new Vector2(0.75f, 1.25f);
    public Vector2 frontVerticalOffset = new Vector2(-.25f, .25f);
    public Vector2 backWidth = new Vector2(0.75f, 1.25f);
    public Vector2 backHeight = new Vector2(0.75f, 1.25f);
    public Vector2 backVerticalOffset = new Vector2(-0.25f, 0.25f);
    [Tooltip("0-1 normalized")]
    public Vector2 transitionSize = new Vector2(0.35f, 0.65f);
    
    public InitialGenomeBodyPartData GetRandomizedData() { return new InitialGenomeBodyPartData(this); }
}

public struct InitialGenomeBodyPartData
{
    public float length;
    public float frontWidth;
    public float frontHeight;
    public float frontVerticalOffset;
    public float backWidth;
    public float backHeight;
    public float backVerticalOffset;
    public float transitionSize; 
    
    public InitialGenomeBodyPartData(InitialGenomeBodyPartInfo template)
    {
        length = RandomStatics.RandomRange(template.length);
        frontWidth = RandomStatics.RandomRange(template.frontWidth);
        frontHeight = RandomStatics.RandomRange(template.frontHeight);
        frontVerticalOffset = RandomStatics.RandomRange(template.frontVerticalOffset);
        backWidth = RandomStatics.RandomRange(template.backWidth);
        backHeight = RandomStatics.RandomRange(template.backHeight);
        backVerticalOffset = RandomStatics.RandomRange(template.backVerticalOffset);
        transitionSize = RandomStatics.RandomRange(template.transitionSize);
    }
}
