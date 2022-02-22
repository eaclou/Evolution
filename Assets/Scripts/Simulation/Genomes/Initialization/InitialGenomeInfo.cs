using UnityEngine;
using Playcraft;

[CreateAssetMenu(menuName = "Pond Water/Agent/Genome Initialization")]
public class InitialGenomeInfo : ScriptableObject
{
    [Header("Creature")]
    public GenomeField creatureBaseLength = new GenomeField(); 
    public GenomeField creatureAspectRatio = new GenomeField();

    // * WPP: convert to GenomeFields, see above for example
    [Tooltip("Mouth/Snout")]
    public GenomeField creatureFrontTaperSize = new GenomeField(); // 0.3f;
    public GenomeField creatureBackTaperSize = new GenomeField(); // 0.4f;
    public GenomeField mouthFeedFrequency = new GenomeField(); //1f;
    public GenomeField mouthAttackAmplitude = new GenomeField(); //1f;
    
    [Header("Body")]
    public InitialGenomeBodyPartInfo mouth;
    public InitialGenomeBodyPartInfo head;
    public InitialGenomeBodyPartInfo body;
    public InitialGenomeBodyPartInfo tail;
    
    [Header("Eyes")]
    public int eyeCount = 2; 
    
    [Tooltip("1f = full hemisphere coverage, 0 = top")]
    public GenomeField eyeSpread = new GenomeField(); // new Vector2(0.3f, 0.7f);
    public GenomeField eyeLocationAmplitude = new GenomeField(); // new Vector2(0.4f, 0.6f);
    public GenomeField eyeLocationFrequency = new GenomeField(); // new Vector2(0.75f, 1.25f);
    public GenomeField eyeLocationOffset = new GenomeField(); // new Vector2(0f, 0.1f);
    [Tooltip("Relative to body size")]
    public GenomeField socketRadius = new GenomeField(); // new Vector2(0.65f, 1f);
    public GenomeField socketHeight = new GenomeField(); // new Vector2(0.15f, 1f);
    public GenomeField socketBulge = new GenomeField(); // new Vector2(0f, 1f);
    public GenomeField eyeballRadius = new GenomeField(); // new Vector2(0.8f, 1.2f);
    public GenomeField eyeBulge = new GenomeField(); // new Vector2(0.25f, 0.5f);
    public GenomeField irisWidthPercent = new GenomeField(); // new Vector2(0.7f, 0.95f);
    public GenomeField pupilWidthPercent = new GenomeField(); // new Vector2(0.2f, 0.5f);
    public GenomeField pupilHeightPercent = new GenomeField(); // new Vector2(0.75f, 1f);
    public Vector3 eyeballHue = Vector3.one;
    public HueInfo irisHue;
    
    [Header("Dorsal Fin")]
    public GenomeField dorsalFinStartY = new GenomeField(); // new Vector2(0f, 0.4f);
    public GenomeField dorsalFinEndY = new GenomeField(); // new Vector2(0.6f, 0.8f);
    public GenomeField dorsalFinSlant = new GenomeField(); // new Vector2(0.25f, 0.4f);
    public GenomeField dorsalFinBaseHeight = new GenomeField(); // new Vector2(0f, 2.5f);
        
    [Header("Tail Fin")]
    public GenomeField tailFinSpreadAngle = new GenomeField(); // new Vector2(0.1f, 0.5f);
    public GenomeField tailFinBaseLength = new GenomeField(); // new Vector2(0f, 1f);
    public Vector3 tailFinFrequencies = Vector3.one;
    public Vector3 tailFinAmplitudes = Vector3.one;
    public Vector3 tailFinOffsets = Vector3.zero;

    [Header("Talents")] //***EAC DEPRECATE!!!
    public Vector2 attackSpecialization = new Vector2(0.4f, 0.6f);
    public Vector2 defenseSpecialization = new Vector2(0.4f, 0.6f);
    public Vector2 speedSpecialization = new Vector2(0.4f, 0.6f);
    public Vector2 utilitySpecialization = new Vector2(0.4f, 0.6f);
    
    [Header("Diet")]
    public Vector2 plantDietSpecialization = new Vector2(0.4f, 0.6f);
    public Vector2 decayDietSpecialization = new Vector2(0.4f, 0.6f);
    public Vector2 meatDietSpecialization = new Vector2(0.4f, 0.6f);
    
    public InitialGenomeData GetInitialGenomeData() { return new InitialGenomeData(this); }
}

public class InitialGenomeData
{
    public float creatureBaseLength;
    public float creatureAspectRatio;
    
    public float creatureFrontTaperSize;
    public float creatureBackTaperSize;
    public float mouthFeedFrequency;
    public float mouthAttackAmplitude;
    
    public InitialGenomeBodyPartData mouth;
    public InitialGenomeBodyPartData head;
    public InitialGenomeBodyPartData body;
    public InitialGenomeBodyPartData tail;
    
    public int eyeCount;
    public float eyeSpread;
    public float eyeLocationAmplitude;
    public float eyeLocationFrequency;
    public float eyeLocationOffset;
    public float socketRadius;
    public float socketHeight;
    public float socketBulge;
    public float eyeballRadius;
    public float eyeBulge;
    public float irisWidthPercent;
    public float pupilWidthPercent;
    public float pupilHeightPercent;
    public Vector3 eyeballHue;
    public Vector3 irisHue;
    
    public float dorsalFinStartY;
    public float dorsalFinEndY;
    public float dorsalFinSlant;
    public float dorsalFinBaseHeight;
        
    public float tailFinSpreadAngle;
    public float tailFinBaseLength;
    public Vector3 tailFinFrequencies;
    public Vector3 tailFinAmplitudes;
    public Vector3 tailFinOffsets;

    public float attackSpecialization;
    public float defenseSpecialization;
    public float speedSpecialization;
    public float utilitySpecialization;
    
    public float plantDietSpecialization;
    public float decayDietSpecialization;
    public float meatDietSpecialization;

    /// Initialize from ranges stored in editor-defined template
    public InitialGenomeData(InitialGenomeInfo template)
    {
        creatureBaseLength = RandomStatics.RandomRange(template.creatureBaseLength.initialRange);
        creatureAspectRatio = RandomStatics.RandomRange(template.creatureAspectRatio.initialRange);
        
        creatureFrontTaperSize = RandomStatics.RandomRange(template.creatureFrontTaperSize.initialRange);
        creatureBackTaperSize = RandomStatics.RandomRange(template.creatureBackTaperSize.initialRange);
        mouthFeedFrequency = RandomStatics.RandomRange(template.mouthFeedFrequency.initialRange);
        mouthAttackAmplitude = RandomStatics.RandomRange(template.mouthAttackAmplitude.initialRange);
        
        mouth = template.mouth.GetRandomizedData();
        head = template.head.GetRandomizedData();
        body = template.body.GetRandomizedData();
        tail = template.tail.GetRandomizedData();
        
        eyeCount = template.eyeCount;
        eyeSpread = RandomStatics.RandomRange(template.eyeSpread.initialRange);
        eyeLocationAmplitude = RandomStatics.RandomRange(template.eyeLocationAmplitude.initialRange);
        eyeLocationFrequency = RandomStatics.RandomRange(template.eyeLocationFrequency.initialRange);
        eyeLocationOffset = RandomStatics.RandomRange(template.eyeLocationOffset.initialRange);
        socketRadius = RandomStatics.RandomRange(template.socketRadius.initialRange);
        socketHeight = RandomStatics.RandomRange(template.socketHeight.initialRange);
        socketBulge = RandomStatics.RandomRange(template.socketBulge.initialRange);
        eyeballRadius = RandomStatics.RandomRange(template.eyeballRadius.initialRange);
        eyeBulge = RandomStatics.RandomRange(template.eyeBulge.initialRange);
        irisWidthPercent = RandomStatics.RandomRange(template.irisWidthPercent.initialRange);
        pupilWidthPercent = RandomStatics.RandomRange(template.pupilWidthPercent.initialRange);
        pupilHeightPercent = RandomStatics.RandomRange(template.pupilHeightPercent.initialRange);
        eyeballHue = template.eyeballHue;
        irisHue = template.irisHue.GetHue().GetValue();
        
        dorsalFinStartY = RandomStatics.RandomRange(template.dorsalFinStartY.initialRange);
        dorsalFinEndY = RandomStatics.RandomRange(template.dorsalFinEndY.initialRange);
        dorsalFinSlant = RandomStatics.RandomRange(template.dorsalFinSlant.initialRange);
        dorsalFinBaseHeight = RandomStatics.RandomRange(template.dorsalFinBaseHeight.initialRange);
            
        tailFinSpreadAngle = RandomStatics.RandomRange(template.tailFinSpreadAngle.initialRange);
        tailFinBaseLength = RandomStatics.RandomRange(template.tailFinBaseLength.initialRange);
        tailFinFrequencies = template.tailFinFrequencies;
        tailFinAmplitudes = template.tailFinAmplitudes;
        tailFinOffsets = template.tailFinOffsets;

        attackSpecialization = RandomStatics.RandomRange(template.attackSpecialization);
        defenseSpecialization = RandomStatics.RandomRange(template.defenseSpecialization);
        speedSpecialization = RandomStatics.RandomRange(template.speedSpecialization);
        utilitySpecialization = RandomStatics.RandomRange(template.utilitySpecialization);
        
        plantDietSpecialization = RandomStatics.RandomRange(template.plantDietSpecialization);
        decayDietSpecialization = RandomStatics.RandomRange(template.decayDietSpecialization);
        meatDietSpecialization = RandomStatics.RandomRange(template.meatDietSpecialization);
    }
}