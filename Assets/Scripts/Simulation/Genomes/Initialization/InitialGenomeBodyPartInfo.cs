using UnityEngine;
using Playcraft;

[CreateAssetMenu(menuName = "Pond Water/Agent/Genome Body Part Initialization")]
public class InitialGenomeBodyPartInfo : ScriptableObject
{
    [Header("Randomized Ranges")]
    public GenomeField length = new GenomeField();//
    
    // * WPP: convert to GenomeFields, see above for example
    public GenomeField frontWidth = new GenomeField();//new Vector2(0.75f, 1.25f);
    public GenomeField frontHeight = new GenomeField();//new Vector2(0.75f, 1.25f);
    public GenomeField frontVerticalOffset = new GenomeField();//new Vector2(-.25f, .25f);
    public GenomeField backWidth = new GenomeField();//new Vector2(0.75f, 1.25f);
    public GenomeField backHeight = new GenomeField();//new Vector2(0.75f, 1.25f);
    public GenomeField backVerticalOffset = new GenomeField();//new Vector2(-0.25f, 0.25f);
    [Tooltip("0-1 normalized")]
    public GenomeField transitionSize = new GenomeField();//new Vector2(0.35f, 0.65f);
    
    public InitialGenomeBodyPartData GetRandomizedData() { return new InitialGenomeBodyPartData(this); }
}

public class InitialGenomeBodyPartData
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
        length = RandomStatics.RandomRange(template.length.initialRange);
        
        frontWidth = RandomStatics.RandomRange(template.frontWidth.initialRange);
        frontHeight = RandomStatics.RandomRange(template.frontHeight.initialRange);
        frontVerticalOffset = RandomStatics.RandomRange(template.frontVerticalOffset.initialRange);
        backWidth = RandomStatics.RandomRange(template.backWidth.initialRange);
        backHeight = RandomStatics.RandomRange(template.backHeight.initialRange);
        backVerticalOffset = RandomStatics.RandomRange(template.backVerticalOffset.initialRange);
        transitionSize = RandomStatics.RandomRange(template.transitionSize.initialRange);
    }
}
