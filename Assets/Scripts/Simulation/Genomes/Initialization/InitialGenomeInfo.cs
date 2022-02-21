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
    public float creatureFrontTaperSize = 0.3f;
    public float creatureBackTaperSize = 0.4f;
    public float mouthFeedFrequency = 1f;
    public float mouthAttackAmplitude = 1f;
    
    [Header("Body")]
    public InitialGenomeBodyPartInfo mouth;
    public InitialGenomeBodyPartInfo head;
    public InitialGenomeBodyPartInfo body;
    public InitialGenomeBodyPartInfo tail;
    
    [Header("Eyes")]
    public int eyeCount = 2;
    
    // * WPP: Convert to GenomeFields, see above for example
    [Tooltip("1f = full hemisphere coverage, 0 = top")]
    public Vector2 eyeSpread = new Vector2(0.3f, 0.7f);
    public Vector2 eyeLocationAmplitude = new Vector2(0.4f, 0.6f);
    public Vector2 eyeLocationFrequency = new Vector2(0.75f, 1.25f);
    public Vector2 eyeLocationOffset = new Vector2(0f, 0.1f);
    [Tooltip("Relative to body size")]
    public Vector2 socketRadius = new Vector2(0.65f, 1f);
    public Vector2 socketHeight = new Vector2(0.15f, 1f);
    public Vector2 socketBulge = new Vector2(0f, 1f);
    public Vector2 eyeballRadius = new Vector2(0.8f, 1.2f);
    public Vector2 eyeBulge = new Vector2(0.25f, 0.5f);
    public Vector2 irisWidthPercent = new Vector2(0.7f, 0.95f);
    public Vector2 pupilWidthPercent = new Vector2(0.2f, 0.5f);
    public Vector2 pupilHeightPercent = new Vector2(0.75f, 1f);
    public Vector3 eyeballHue = Vector3.one;
    public HueInfo irisHue;
    
    [Header("Dorsal Fin")]
    public Vector2 dorsalFinStartY = new Vector2(0f, 0.4f);
    public Vector2 dorsalFinEndY = new Vector2(0.6f, 0.8f);
    public Vector2 dorsalFinSlant = new Vector2(0.25f, 0.4f);
    public Vector2 dorsalFinBaseHeight = new Vector2(0f, 2.5f);
        
    [Header("Tail Fin")]
    public Vector2 tailFinSpreadAngle = new Vector2(0.1f, 0.5f);
    public Vector2 tailFinBaseLength = new Vector2(0f, 1f);
    public Vector3 tailFinFrequencies = Vector3.one;
    public Vector3 tailFinAmplitudes = Vector3.one;
    public Vector3 tailFinOffsets = Vector3.zero;

    [Header("Talents")]
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
        
        creatureFrontTaperSize = template.creatureFrontTaperSize;
        creatureBackTaperSize = template.creatureBackTaperSize;
        mouthFeedFrequency = template.mouthFeedFrequency;
        mouthAttackAmplitude = template.mouthAttackAmplitude;
        
        mouth = template.mouth.GetRandomizedData();
        head = template.head.GetRandomizedData();
        body = template.body.GetRandomizedData();
        tail = template.tail.GetRandomizedData();
        
        eyeCount = template.eyeCount;
        eyeSpread = RandomStatics.RandomRange(template.eyeSpread);
        eyeLocationAmplitude = RandomStatics.RandomRange(template.eyeLocationAmplitude);
        eyeLocationFrequency = RandomStatics.RandomRange(template.eyeLocationFrequency);
        eyeLocationOffset = RandomStatics.RandomRange(template.eyeLocationOffset);
        socketRadius = RandomStatics.RandomRange(template.socketRadius);
        socketHeight = RandomStatics.RandomRange(template.socketHeight);
        socketBulge = RandomStatics.RandomRange(template.socketBulge);
        eyeballRadius = RandomStatics.RandomRange(template.eyeballRadius);
        eyeBulge = RandomStatics.RandomRange(template.eyeBulge);
        irisWidthPercent = RandomStatics.RandomRange(template.irisWidthPercent);
        pupilWidthPercent = RandomStatics.RandomRange(template.pupilWidthPercent);
        pupilHeightPercent = RandomStatics.RandomRange(template.pupilHeightPercent);
        eyeballHue = template.eyeballHue;
        irisHue = template.irisHue.GetHue().GetValue();
        
        dorsalFinStartY = RandomStatics.RandomRange(template.dorsalFinStartY);
        dorsalFinEndY = RandomStatics.RandomRange(template.dorsalFinEndY);
        dorsalFinSlant = RandomStatics.RandomRange(template.dorsalFinSlant);
        dorsalFinBaseHeight = RandomStatics.RandomRange(template.dorsalFinBaseHeight);
            
        tailFinSpreadAngle = RandomStatics.RandomRange(template.tailFinSpreadAngle);
        tailFinBaseLength = RandomStatics.RandomRange(template.tailFinBaseLength);
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