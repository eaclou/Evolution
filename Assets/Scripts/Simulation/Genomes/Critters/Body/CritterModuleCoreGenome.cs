using System;
using System.Collections.Generic;
using UnityEngine;
using Playcraft;
using Random = UnityEngine.Random;

[Serializable]
public class CritterModuleCoreGenome 
{
    Lookup lookup => Lookup.instance;
    NeuralMap map => lookup.neuralMap;
    NameList nameList => lookup.nameList;
    InitialGenomeInfo genomeInitialization => lookup.genomeInitialization;

    public int parentID;
    public readonly BrainModuleID moduleID = BrainModuleID.Core;

    public int generation;
    public string name;

    // 4 main sections: 
    //    Mouth/Snout
    //    Head
    //    Body/Torso
    //    Tail
    
    #region Data Access
    
    private InitialGenomeData _data;
    public InitialGenomeData data 
    { 
        get
        {
             if (_data == null) 
                _data = new InitialGenomeData(genomeInitialization); 
             
             return _data;   
        }
        set => _data = value; 
    }
    
    public float creatureBaseLength { get => data.creatureBaseLength; set => data.creatureBaseLength = value; }
    public float creatureAspectRatio { get => data.creatureAspectRatio; set => data.creatureAspectRatio = value; }
    
    // Mouth/Snout:
    public float creatureFrontTaperSize { get => data.creatureFrontTaperSize; set => data.creatureFrontTaperSize = value; }
    public float creatureBackTaperSize { get => data.creatureBackTaperSize; set => data.creatureBackTaperSize = value; }

    //public float mouthComplexShapeLerp;  // 0 = spherical simple creature, 1 = use section proportions
    public float mouthLength { get => data.mouth.length; set => data.mouth.length = value; }
    /// Width of snout at front of critter
    public float mouthFrontWidth { get => data.mouth.frontWidth; set => data.mouth.frontWidth = value; }
    /// Height of snout at front of critter 
    public float mouthFrontHeight { get => data.mouth.frontHeight; set => data.mouth.frontHeight = value; }
    /// Shift up/down pivot/cylinder center
    public float mouthFrontVerticalOffset { get => data.mouth.frontVerticalOffset; set => data.mouth.frontVerticalOffset = value; }
    public float mouthBackWidth { get => data.mouth.backWidth; set => data.mouth.backWidth = value; }
    public float mouthBackHeight { get => data.mouth.backHeight; set => data.mouth.backHeight = value; }
    public float mouthBackVerticalOffset { get => data.mouth.backVerticalOffset; set => data.mouth.backVerticalOffset = value; }
    public float mouthToHeadTransitionSize { get => data.mouth.transitionSize; set => data.mouth.transitionSize = value; }  
    // Head
    //public float headComplexShapeLerp;
    public float headLength { get => data.head.length; set => data.head.length = value; }
    public float headFrontWidth { get => data.head.frontWidth; set => data.head.frontWidth = value; }
    public float headFrontHeight { get => data.head.frontHeight; set => data.head.frontHeight = value; }
    public float headFrontVerticalOffset { get => data.head.frontVerticalOffset; set => data.head.frontVerticalOffset = value; }
    public float headBackWidth { get => data.head.backWidth; set => data.head.backWidth = value; }
    public float headBackHeight { get => data.head.backHeight; set => data.head.backHeight = value; }
    public float headBackVerticalOffset { get => data.head.backVerticalOffset; set => data.head.backVerticalOffset = value; }
    public float headToBodyTransitionSize { get => data.head.transitionSize; set => data.head.transitionSize = value; } 
    // Body:
    //public float bodyComplexShapeLerp;
    public float bodyLength { get => data.body.length; set => data.body.length = value; } 
    /// Width of snout at front of critter
    public float bodyFrontWidth { get => data.body.frontWidth; set => data.body.frontWidth = value; } 
    /// Height of snout at front of critter  
    public float bodyFrontHeight { get => data.body.frontHeight; set => data.body.frontHeight = value; } 
    /// Shift up/down pivot/cylinder center 
    public float bodyFrontVerticalOffset { get => data.body.frontVerticalOffset; set => data.body.frontVerticalOffset = value; } 
    public float bodyBackWidth { get => data.body.backWidth; set => data.body.backWidth = value; } 
    public float bodyBackHeight { get => data.body.backHeight; set => data.body.backHeight = value; } 
    public float bodyBackVerticalOffset { get => data.body.backVerticalOffset; set => data.body.backVerticalOffset = value; }
    public float bodyToTailTransitionSize { get => data.body.transitionSize; set => data.body.transitionSize = value; } 
    //Tail:
    //public float tailComplexShapeLerp;
    public float tailLength { get => data.tail.length; set => data.tail.length = value; } 
    /// width of snout at front of critter
    public float tailFrontWidth { get => data.tail.frontWidth; set => data.tail.frontWidth = value; }  
    /// height of snout at front of critter
    public float tailFrontHeight { get => data.tail.frontHeight; set => data.tail.frontHeight = value; } 
    /// shift up/down pivot/cylinder center
    public float tailFrontVerticalOffset { get => data.tail.frontVerticalOffset; set => data.tail.frontVerticalOffset = value; } 
    public float tailBackWidth { get => data.tail.backWidth; set => data.tail.backWidth = value; }  
    public float tailBackHeight { get => data.tail.backHeight; set => data.tail.backHeight = value; } 
    public float tailBackVerticalOffset { get => data.tail.backVerticalOffset; set => data.tail.backVerticalOffset = value; } 
    
    public float fullLength => tailLength + bodyLength + headLength + mouthLength;
    public float bodyCoord => tailLength / fullLength;
    public float headCoord => (tailLength + bodyLength) / fullLength;
    public float mouthCoord => (tailLength + bodyLength + headLength) / fullLength;

    // EYES:::  (Eventually separate these out better into different subclasses/structs)
    public int numEyes { get => data.eyeCount; set => data.eyeCount = value; }
    /// 1f == full hemisphere coverage, 0 == top
    public float eyePosSpread { get => data.eyeSpread; set => data.eyeSpread = value; } 
    public float eyeLocAmplitude { get => data.eyeLocationAmplitude; set => data.eyeLocationAmplitude = value; } 
    public float eyeLocFrequency { get => data.eyeLocationFrequency; set => data.eyeLocationFrequency = value; } 
    public float eyeLocOffset { get => data.eyeLocationOffset; set => data.eyeLocationOffset = value; } 
    /// Relative to body size?    
    public float socketRadius { get => data.socketRadius; set => data.socketRadius = value; }   
    public float socketHeight { get => data.socketHeight; set => data.socketHeight = value; }  
    public float socketBulge { get => data.socketBulge; set => data.socketBulge = value; } 
    public float eyeballRadius { get => data.eyeballRadius; set => data.eyeballRadius = value; } 
    public float eyeBulge { get => data.eyeBulge; set => data.eyeBulge = value; } 
    public float irisWidthFraction { get => data.irisWidthPercent; set => data.irisWidthPercent = value; }    
    /// Percentage of iris size    
    public float pupilWidthFraction { get => data.pupilWidthPercent; set => data.pupilWidthPercent = value; }   
    public float pupilHeightFraction { get => data.pupilHeightPercent; set => data.pupilHeightPercent = value; } 
    public Vector3 eyeballHue { get => data.eyeballHue; set => data.eyeballHue = value; } 
    public Vector3 irisHue { get => data.irisHue; set => data.irisHue = value; } 
    
    // Dorsal Fin:
    public float dorsalFinStartCoordY { get => data.dorsalFinStartY; set => data.dorsalFinStartY = value; } 
    public float dorsalFinEndCoordY { get => data.dorsalFinEndY; set => data.dorsalFinEndY = value; } 
    public float dorsalFinSlantAmount { get => data.dorsalFinSlant; set => data.dorsalFinSlant = value; } 
    public float dorsalFinBaseHeight { get => data.dorsalFinBaseHeight; set => data.dorsalFinBaseHeight = value; } 

    // TAIL FIN:
    public float tailFinSpreadAngle { get => data.tailFinSpreadAngle; set => data.tailFinSpreadAngle = value; } 
    public float tailFinBaseLength { get => data.tailFinBaseLength; set => data.tailFinBaseLength = value; } 
    public Vector3 tailFinFrequencies { get => data.tailFinFrequencies; set => data.tailFinFrequencies = value; } 
    public Vector3 tailFinAmplitudes { get => data.tailFinAmplitudes; set => data.tailFinAmplitudes = value; } 
    public Vector3 tailFinOffsets { get => data.tailFinOffsets; set => data.tailFinOffsets = value; } 

    // Specialization Paths first try:
    public float talentSpecializationAttack { get => data.attackSpecialization; set => data.attackSpecialization = value; } 
    public float talentSpecializationDefense { get => data.defenseSpecialization; set => data.defenseSpecialization = value; } 
    public float talentSpecializationSpeed { get => data.speedSpecialization; set => data.speedSpecialization = value; } 
    public float talentSpecializationUtility { get => data.utilitySpecialization; set => data.utilitySpecialization = value; } 
    
    public float talentSpecTotal => talentSpecializationAttack + talentSpecializationDefense + talentSpecializationSpeed + talentSpecializationUtility;
    public float talentSpecAttackNorm => talentSpecializationAttack / talentSpecTotal;
    public float talentSpecDefenseNorm => talentSpecializationDefense / talentSpecTotal;
    public float talentSpecSpeedNorm => talentSpecializationSpeed / talentSpecTotal;
    public float talentSpecUtilityNorm => talentSpecializationUtility / talentSpecTotal;

    // Diet specialization:
    public float dietSpecializationDecay { get => data.decayDietSpecialization; set => data.decayDietSpecialization = value; }
    public float dietSpecializationPlant { get => data.plantDietSpecialization; set => data.plantDietSpecialization = value; }   
    public float dietSpecializationMeat { get => data.meatDietSpecialization; set => data.meatDietSpecialization = value; }

    public float mouthFeedFrequency { get => data.mouthFeedFrequency; set => data.mouthFeedFrequency = value; }
    public float mouthAttackAmplitude { get => data.mouthAttackAmplitude; set => data.mouthAttackAmplitude = value; }
    
    #endregion

    public TalentsAttack[] talentSpecAttack; // 5 tiers?    // tiers at specialization levels:  55%, 65%, 75%, 85%, 95%    
    public TalentsDefend[] talentSpecDefend; // 5 tiers?
    public TalentsSpeed[] talentSpecSpeed; // 5 tiers?
    public TalentsUtility[] talentSpecUtility; // 5 tiers?

    public TalentsDecay[] talentSpecDecay;
    public TalentsPlant[] talentSpecPlant;
    public TalentsMeat[] talentSpecMeat;
    //public 

    public enum TalentsAttack {
        RawDamageBonus,
        QuickBite,
        ReducedSpeedPenalty,
        ReducedBiteCooldown
    }
    
    public enum TalentsDefend {
        RawHealthBonus,
        SafetyInNumbers,
        HealRate,
        Stamina
    }
    
    public enum TalentsSpeed {
        RawSpeedBonus,
        TurnBonus,
        SurfTheCurrent,
        Dodge
    }   
     
    public enum TalentsUtility {
        RawEfficiencyBonus,
        SharedMeal,
        RestingEfficiency,  // while not moving, barely lose energy
        StoredEggEnergy  // newborns start with extra food/energy
    }

    public enum TalentsDecay {
        None
    }
     
    public enum TalentsPlant {
        None
    }
     
    public enum TalentsMeat {
        None
    } 

    //public SkillsDamage[] skillDamage;  // up to 2 skills per category -- if choose same skill for both slots, use upgraded version of that skill    
    //public SkillsHealth[] skillHealth;
    //public SkillsSpeed[] skillSpeed;    // first skill at 70% specialization, second at 95%
    //public SkillsEnergy[] skillEnergy;
    
    /*public enum SkillsDamage {
        TailStrike,
        MegaBite,
        AmbushBite,
    }
    public enum SkillsSpeed {
        Evade,
        Sprint,
        Whirlpool,  // spin in place and create current
    }
    public enum SkillsHealth {
        ActiveHeal,
        CurlUpArmor,
        BonusFoodSpecEfficiency,  // can more easily eat any kind of food        
    }
    public enum SkillsEnergy {
        SecondWind,
        Burrow,
        Stun,
    }*/

    // List of Shape/Form modifiers here???:::
    //public CritterGenomeInterpretor.MaskDataSin maskDataSinTemp;

    [Serializable]
    public enum ShapeModifierType {
        Extrude,  // shift vertex along major normal
        UniformScale
    }
    
    [Serializable]
    public enum MaskCoordinateType {
        Lengthwise,
        //Radial,  // uv distance
        //SingleAxis,  // arbitrary direction of uv coordinate flow
        Polygonize  // facet body circumference        
    }
    
    [Serializable]
    public enum MaskFunctionType {
        Linear,
        Sin,
        Cos      
    }

    [Serializable]
    public struct MaskData {
        public MaskCoordinateType coordinateTypeID;
        public MaskFunctionType functionTypeID;     
        public float origin;
        public float amplitude;
        public float cycleDistance;
        public float phase;
        public int numPolyEdges;
        public Vector2 axisDir;
        public bool repeat;
        
        public MaskData(int maxPolyEdges)
        {
            coordinateTypeID = MaskCoordinateType.Polygonize;
            functionTypeID = MaskFunctionType.Cos;
            origin = Random.Range(0f, 1f); // normalized along length of creature
            amplitude = Random.Range(0f, 1f); 
            cycleDistance = Random.Range(0f, 1f); 
            phase = Random.Range(0f, 1f); 
            numPolyEdges = Random.Range(1, maxPolyEdges); 
            axisDir = new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f));
            repeat = true;
        }
        
        public MaskData(float origin, float amplitude, float cycleDistance, float phase, int numPolyEdges, Vector2 axisDir)
        {
            coordinateTypeID = MaskCoordinateType.Polygonize;
            functionTypeID = MaskFunctionType.Cos;
            this.origin = origin; // normalized along length of creature
            this.amplitude = amplitude;
            this.cycleDistance = cycleDistance;
            this.phase = phase;
            this.numPolyEdges = numPolyEdges;
            this.axisDir = axisDir;
            repeat = true;
        }
    }
    
    // Each modifer gets interpreted by script, picks from correct maskData list
    [Serializable]
    public struct ShapeModifierData {  
        public ShapeModifierType modifierTypeID;  // extrude; axis-aligned scale; twist/pinch/etc?
        public List<int> maskIndicesList;  // method of falloff
        public float taperDistance; // keep/remove?
        public float amplitude;  // move to Range of mult like (0.8 to 1.25)?
        
        public ShapeModifierData(float amplitude, float taperDistance, ShapeModifierType modifierTypeID = ShapeModifierType.Extrude)
        {
            this.modifierTypeID = modifierTypeID;
            maskIndicesList = new List<int>();
            this.amplitude = amplitude;
            this.taperDistance = taperDistance;
        }
    }
    
    // or should I keep masks as separate list and refer to them through indices to allow for mask re-use?
    public List<ShapeModifierData> shapeModifiersList;  // holds list of all modifiers to overall critter shape/structure, in order of application
    public List<MaskData> masksList;
    
    // OLD:
    public bool isPassive;
    public Vector2 mouthSize;  // relative to head size?
    public Vector2 biteZoneDimensions;
    public float biteZoneOffset;
    public int biteChargeUpDuration = 6;
    public int biteCooldownDuration = 16;
    public float biteStrength;
    public float biteSharpness;
    

	public CritterModuleCoreGenome(int parentID) {
        this.parentID = parentID;
    }
    
    public void GenerateRandomInitialGenome() {
        generation = 0;
        name = nameList.GetRandomName();
        
        isPassive = RandomStatics.CoinToss();

        shapeModifiersList = new List<ShapeModifierData>();  // empty
        masksList = new List<MaskData>();
        
        ShapeModifierData initModifier = new ShapeModifierData(Random.Range(0f, 1f), 1f);

        // Masks for this modifier:
        MaskData maskData = new MaskData(7);

        masksList.Add(maskData);
        initModifier.maskIndicesList.Add(masksList.Count - 1); // reference mask by index to allow re-use by other shape modifiers    
        shapeModifiersList.Add(initModifier);

        data = genomeInitialization.GetInitialGenomeData();
        // WPP: exposed hardcoded values (include ranges), see InitialGenomeInfo
        /*creatureBaseLength = Random.Range(0.4f, 0.4f);
        creatureAspectRatio = Random.Range(0.2f, 0.3f);

        //creatureComplexShapeLerp = 0f;
        // Or start with deformed sphere???? *****
        // Mouth/Snout:
        creatureFrontTaperSize = 0.3f;
        creatureBackTaperSize = 0.4f;

        //mouthComplexShapeLerp = 0f;
        mouthLength = Random.Range(0.75f, 1.25f); 
        mouthFrontWidth = Random.Range(0.75f, 1.25f);  // width of snout at front of critter
        mouthFrontHeight = Random.Range(0.75f, 1.25f); // height of snout at front of critter
        mouthFrontVerticalOffset = Random.Range(-0.25f, 0.25f); // shift up/down pivot/cylinder center
        mouthBackWidth = Random.Range(0.75f, 1.25f);  
        mouthBackHeight = Random.Range(0.75f, 1.25f); 
        mouthBackVerticalOffset = Random.Range(-0.25f, 0.25f);         

        mouthToHeadTransitionSize = Random.Range(0.35f, 0.65f);  // 0-1 normalized
        // Head
        //headComplexShapeLerp = 0f;
        headLength = Random.Range(0.75f, 1.25f);  
        headFrontWidth = Random.Range(0.75f, 1.25f);  
        headFrontHeight = Random.Range(0.75f, 1.25f); 
        headFrontVerticalOffset = Random.Range(-0.25f, 0.25f);
        headBackWidth = Random.Range(0.75f, 1.25f);   
        headBackHeight = Random.Range(0.75f, 1.25f); 
        headBackVerticalOffset = Random.Range(-0.25f, 0.25f);

        headToBodyTransitionSize = Random.Range(0.35f, 0.65f);  // 0-1 normalized
        // Body:
        //bodyComplexShapeLerp = 0f;
        bodyLength = Random.Range(0.75f, 1.25f);  //1f;
        bodyFrontWidth = Random.Range(0.75f, 1.25f);  //1f;  // width of snout at front of critter
        bodyFrontHeight = Random.Range(0.75f, 1.25f);  //1f; // height of snout at front of critter
        bodyFrontVerticalOffset = Random.Range(-0.25f, 0.25f); //0f; // shift up/down pivot/cylinder center
        bodyBackWidth = Random.Range(0.75f, 1.25f);  //1f; 
        bodyBackHeight = Random.Range(0.75f, 1.25f);  //1f;
        bodyBackVerticalOffset = Random.Range(-0.25f, 0.25f); //0f;

        bodyToTailTransitionSize = Random.Range(0.35f, 0.65f); //0.5f;  // 0-1 normalized
        //Tail:
        //tailComplexShapeLerp = 0f;
        tailLength = Random.Range(0.75f, 1.25f);  //1f;
        tailFrontWidth = Random.Range(0.75f, 1.25f);  //1f;  // width of snout at front of critter
        tailFrontHeight = Random.Range(0.75f, 1.25f);  //1f; // height of snout at front of critter
        tailFrontVerticalOffset = Random.Range(-0.25f, 0.25f); //0f; // shift up/down pivot/cylinder center
        tailBackWidth = Random.Range(0.75f, 1.25f);  //1f; 
        tailBackHeight = Random.Range(0.75f, 1.25f);  //1f;
        tailBackVerticalOffset = Random.Range(-0.25f, 0.25f); //0f;

        //tailEndCapTaperSize = 0.5f;

        numEyes = 2; // UnityEngine.Random.Range(2,4);
        eyePosSpread = Random.Range(0.3f, 0.7f);  // 1f == full hemisphere coverage, 0 == top
        eyeLocAmplitude = Random.Range(0.4f, 0.6f);
        eyeLocFrequency = Random.Range(0.75f, 1.25f);
        eyeLocOffset = Random.Range(0f, 0.1f);       
        socketRadius = Random.Range(0.65f, 1f);  // relative to body size?
        socketHeight = Random.Range(0.15f, 1f);
        socketBulge = Random.Range(0f, 1f);
        eyeballRadius = Random.Range(0.8f, 1.2f);
        eyeBulge = Random.Range(0.25f, 0.5f);
        irisWidthFraction = Random.Range(0.7f, 0.95f);       
        pupilWidthFraction = Random.Range(0.2f, 0.5f);  // percentage of iris size
        pupilHeightFraction = Random.Range(0.75f, 1f);
        eyeballHue = Vector3.one;
        irisHue = new Vector3(Random.Range(0.25f, 0.75f), Random.Range(0.25f, 0.75f), Random.Range(0.25f, 0.75f));

        // Dorsal Fin:
        dorsalFinStartCoordY = Random.Range(0f, 0.4f);
        dorsalFinEndCoordY = Random.Range(0.6f, 0.8f);
        dorsalFinSlantAmount = Random.Range(0.25f, 0.4f);
        dorsalFinBaseHeight = Random.Range(0f, 2.5f);

        // Tail Fin:
        tailFinSpreadAngle = Random.Range(0.1f, 0.5f);
        tailFinBaseLength = Random.Range(0f, 1f);
        tailFinFrequencies = Vector3.one;
        tailFinAmplitudes = Vector3.one;
        tailFinOffsets = Vector3.zero;

        talentSpecializationAttack = Random.Range(0.4f, 0.6f);
        talentSpecializationDefense = Random.Range(0.4f, 0.6f);
        talentSpecializationSpeed = Random.Range(0.4f, 0.6f);
        talentSpecializationUtility = Random.Range(0.4f, 0.6f);

        dietSpecializationPlant = Random.Range(0.4f, 0.6f);
        dietSpecializationDecay = Random.Range(0.4f, 0.6f);
        dietSpecializationMeat = Random.Range(0.4f, 0.6f);

        mouthFeedFrequency = 1f;
        mouthAttackAmplitude = 1f;*/
    }

    List<NeuronGenome> masterList;
    public void AppendModuleNeuronsToMasterList(List<NeuronGenome> masterList)
    {
        this.masterList = masterList;
        AddNeuron("Bias");
        AddNeuron("isMouthTrigger");
        AddNeuron("isContact");
        AddNeuron("contactForceX");
        AddNeuron("contactForceY");
        AddNeuron("hitPoints");
        AddNeuron("stamina");
        AddNeuron("energyStored");
        AddNeuron("foodStored");
        AddNeuron("mouthFeedEffector"); 
        
        if(talentSpecAttackNorm > 0.2f) {
            AddNeuron("mouthAttackEffector");
        }
        if(talentSpecDefenseNorm > 0.2f) {
            AddNeuron("defendEffector");
        }
        if(talentSpecSpeedNorm > 0.2f) {
            AddNeuron("dashEffector");
        }
        if(talentSpecUtilityNorm > 0.2f) {
            AddNeuron("healEffector");
        }
    }
    
    void AddNeuron(string name) { masterList.Add(map.GetData(name)); }

    public void SetToMutatedCopyOfParentGenome(CritterModuleCoreGenome parentGenome, MutationSettingsInstance settings) {
        string parentName = parentGenome.name;
        //int parentNameLength = parentName;
        int randIndex = Random.Range(0, parentName.Length - 1);
        
        string frontHalf = parentName.Substring(0, randIndex);
        string middleChar = parentName.Substring(randIndex, 1);
        string backHalf = parentName.Substring(randIndex + 1);
        name = parentName;
        
        if (RandomStatics.CoinToss(.05f)) {
            middleChar = RandomStatics.GetRandomLetter();
        }

        frontHalf += middleChar;

        name = RandomStatics.CoinToss(.025f) ? backHalf + frontHalf : frontHalf + backHalf;

        // This is incremented elsewhere (simManager at time of reproduction)
        generation = parentGenome.generation; 

        isPassive = UtilityMutationFunctions.GetMutatedBool(parentGenome.isPassive, 0.033f);

        //float slotMult = settings.mutationStrengthSlot;
        
        // Copy modifiers list? do I need to create copies of each entry? or straight copy should work since they are structs (value-typed)
        shapeModifiersList = new List<ShapeModifierData>();  // empty
        for(int i = 0; i < parentGenome.shapeModifiersList.Count; i++) {
            ShapeModifierData newData = new ShapeModifierData();
            
            newData.maskIndicesList = new List<int>();
            for (int j = 0; j < parentGenome.shapeModifiersList[i].maskIndicesList.Count; j++) {
                int maskIndex = parentGenome.shapeModifiersList[i].maskIndicesList[j];
                // MUTATE?
                maskIndex = UtilityMutationFunctions.GetMutatedIntAdditive(maskIndex, settings.bodyProportionsMutationChance, 2, 0, parentGenome.shapeModifiersList[i].maskIndicesList.Count - 1);
                newData.maskIndicesList.Add(maskIndex); // make sure this isn't passing as a reference? it's an 'int' (data-type) so should be ok... // newMaskData);
            }
                    
            newData.modifierTypeID = parentGenome.shapeModifiersList[i].modifierTypeID;  // only works if this is NOT a reference type!!! ***
            newData.amplitude = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.shapeModifiersList[i].amplitude, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, -0.5f, 0.5f); //parentGenome.shapeModifiersList[i].amplitude;
            newData.taperDistance = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.shapeModifiersList[i].taperDistance, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, 0.2f, 2f); //;
            shapeModifiersList.Add(newData);
        }
        
        masksList = new List<MaskData>();
        for(int i = 0; i < parentGenome.masksList.Count; i++) {
            MaskData newMask = parentGenome.masksList[i];
            newMask.coordinateTypeID = MaskCoordinateType.Polygonize; // (MaskCoordinateType)UtilityMutationFunctions.GetMutatedIntAdditive(maskCoordinateTypeID, settings.defaultBodyMutationChance, 1, 0, 1);
            newMask.functionTypeID = MaskFunctionType.Cos; // (MaskFunctionType)UtilityMutationFunctions.GetMutatedIntAdditive(maskFunctionTypeID, settings.defaultBodyMutationChance, 2, 0, 2);
            newMask.numPolyEdges = UtilityMutationFunctions.GetMutatedIntAdditive(newMask.numPolyEdges, settings.bodyProportionsMutationChance, 4, 1, 6);
            newMask.origin = UtilityMutationFunctions.GetMutatedFloatAdditive(newMask.origin, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, 0f, 1f);
            newMask.phase = UtilityMutationFunctions.GetMutatedFloatAdditive(newMask.phase, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, -5f, 5f);
            newMask.cycleDistance = UtilityMutationFunctions.GetMutatedFloatAdditive(newMask.origin, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, 0.1f, 1f);
            newMask.amplitude = UtilityMutationFunctions.GetMutatedFloatAdditive(newMask.origin, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, 0.05f, 0.5f);
            newMask.axisDir = UtilityMutationFunctions.GetMutatedVector2Additive(newMask.axisDir, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, -1f, 1f).normalized;
            masksList.Add(newMask);
        }
        
        // Mutate Add New Modifiers Here:?
        if(shapeModifiersList.Count < 6 && RandomStatics.CoinToss(.02f)) {
            ShapeModifierData initModifier = new ShapeModifierData(.4f, .2f);
            shapeModifiersList.Add(initModifier);
        }
        
        if(masksList.Count < 4 && RandomStatics.CoinToss(.04f)) {
            MaskData maskData = new MaskData(.5f, 1f, .5f, 0f, 1, new Vector2(0f, 1f));
            masksList.Add(maskData);
        }        
        
        // * WPP: expose hardcoded values, move logic to InitialGenomeData
        // Or start with deformed sphere???? *****
        creatureBaseLength = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.creatureBaseLength, settings.bodyCoreSizeMutationChance, settings.bodyCoreMutationStepSize, 0.4f, 0.4f);
        creatureAspectRatio = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.creatureAspectRatio, settings.bodyCoreSizeMutationChance, settings.bodyCoreMutationStepSize, 0.2f, 0.3f);
        
        // Mouth/Snout:
        creatureFrontTaperSize = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.creatureFrontTaperSize, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, 0.05f, 0.33f);
        creatureBackTaperSize = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.creatureBackTaperSize, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, 0.05f, 0.33f);
                
        mouthLength = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.mouthLength, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, 0.2f, 2f);
        mouthFrontWidth = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.mouthFrontWidth, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, 0.05f, 1.5f);  // width of snout at front of critter (RELATIVE TO LENGTH OF SEGMENT!)
        mouthFrontHeight = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.mouthFrontHeight, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, 0.05f, 1.5f);
        mouthFrontVerticalOffset = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.mouthFrontVerticalOffset, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, -1f, 1f);
        mouthBackWidth = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.mouthBackWidth, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, 0.05f, 1.5f);
        mouthBackHeight = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.mouthBackHeight, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, 0.05f, 1.5f);
        mouthBackVerticalOffset = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.mouthBackVerticalOffset, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, -1f, 1f);
        
        mouthToHeadTransitionSize = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.mouthToHeadTransitionSize, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, 0.25f, 1f);
        // Head        
        headLength = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.headLength, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, 0.2f, 2f);
        headFrontWidth = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.headFrontWidth, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, 0.05f, 1.5f);
        headFrontHeight = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.headFrontHeight, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, 0.05f, 1.5f);
        headFrontVerticalOffset = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.headFrontVerticalOffset, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, -1f, 1f);
        headBackWidth = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.headBackWidth, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, 0.05f, 1.5f);
        headBackHeight = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.headBackHeight, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, 0.05f, 1.5f);
        headBackVerticalOffset = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.headBackVerticalOffset, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, -1f, 1f);

        headToBodyTransitionSize = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.headToBodyTransitionSize, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, 0.25f, 1f);
        // Body:
        bodyLength = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.bodyLength, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, 0.5f, 5f);
        bodyFrontWidth = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.bodyFrontWidth, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, 0.05f, 1.5f);
        bodyFrontHeight = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.bodyFrontHeight, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, 0.05f, 1.5f);
        bodyFrontVerticalOffset = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.bodyFrontVerticalOffset, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, -1f, 1f);
        bodyBackWidth = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.bodyBackWidth, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, 0.05f, 1.5f);
        bodyBackHeight = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.bodyBackHeight, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, 0.05f, 1.5f);
        bodyBackVerticalOffset = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.bodyBackVerticalOffset, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, -1f, 1f);

        bodyToTailTransitionSize = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.bodyToTailTransitionSize, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, 0.25f, 1f);
        //Tail:
        tailLength = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.tailLength, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, 0.5f, 5f);
        tailFrontWidth = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.tailFrontWidth, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, 0.05f, 1.5f);
        tailFrontHeight = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.tailFrontHeight, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, 0.05f, 1.5f);
        tailFrontVerticalOffset = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.tailFrontVerticalOffset, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, -1f, 1f);
        tailBackWidth = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.tailBackWidth, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, 0.05f, 1.5f);
        tailBackHeight = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.tailBackHeight, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, 0.05f, 1.5f);
        tailBackVerticalOffset = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.tailBackVerticalOffset, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, -1f, 1f);

        //tailEndCapTaperSize = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.tailEndCapTaperSize, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.5f, 1f);

        // EYES EYES EYES EYES:::
        numEyes = UtilityMutationFunctions.GetMutatedIntAdditive(parentGenome.numEyes, settings.bodyEyeProportionsMutationChance, 1, 2, 2);
        eyePosSpread = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.eyePosSpread, settings.bodyEyeProportionsMutationChance, settings.bodyEyeProportionsMutationStepSize, 0.5f, 1f);  // 1f == full hemisphere coverage, 0 == top
        eyeLocAmplitude = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.eyeLocAmplitude, settings.bodyEyeProportionsMutationChance, settings.bodyEyeProportionsMutationStepSize, 0f, 1f);
        eyeLocFrequency = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.eyeLocFrequency, settings.bodyEyeProportionsMutationChance, settings.bodyEyeProportionsMutationStepSize, 0.25f, 6f);
        eyeLocOffset = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.eyeLocOffset, settings.bodyEyeProportionsMutationChance, settings.bodyEyeProportionsMutationStepSize, 0f, 5f);        
        socketRadius = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.socketRadius, settings.bodyEyeProportionsMutationChance, settings.bodyEyeProportionsMutationStepSize, 0.2f, 0.9f);
        socketHeight = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.socketHeight, settings.bodyEyeProportionsMutationChance, settings.bodyEyeProportionsMutationStepSize, 0.25f, 1.25f);
        socketBulge = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.socketBulge, settings.bodyEyeProportionsMutationChance, settings.bodyEyeProportionsMutationStepSize, -0.75f, 0.75f);
        eyeballRadius = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.eyeballRadius, settings.bodyEyeProportionsMutationChance, settings.bodyEyeProportionsMutationStepSize, 0.5f, 1.75f); // relative to socket radius
        eyeBulge = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.eyeBulge, settings.bodyEyeProportionsMutationChance, settings.bodyEyeProportionsMutationStepSize, 0.4f, 1.35f);
        irisWidthFraction = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.irisWidthFraction, settings.bodyEyeProportionsMutationChance, settings.bodyEyeProportionsMutationStepSize, 0.6f, 0.9f);       
        pupilWidthFraction = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.pupilWidthFraction, settings.bodyEyeProportionsMutationChance, settings.bodyEyeProportionsMutationStepSize, 0.33f, 1f);  // percentage of iris size
        pupilHeightFraction = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.pupilHeightFraction, settings.bodyEyeProportionsMutationChance, settings.bodyEyeProportionsMutationStepSize, 0.33f, 1f);

        eyeballHue = UtilityMutationFunctions.GetMutatedVector3Additive(parentGenome.eyeballHue, settings.bodyColorsMutationChance, settings.bodyColorsMutationStepSize, 0.9f, 1f);
        irisHue = UtilityMutationFunctions.GetMutatedVector3Additive(parentGenome.irisHue, settings.bodyColorsMutationChance, settings.bodyColorsMutationStepSize, 0f, 1f);

        // DORSAL FIN:
        dorsalFinStartCoordY = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.dorsalFinStartCoordY, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, 0f, 0.49f);
        dorsalFinEndCoordY = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.dorsalFinEndCoordY, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, 0.51f, 1f);
        dorsalFinSlantAmount = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.dorsalFinSlantAmount, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, 0f, 0.67f);
        dorsalFinBaseHeight = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.dorsalFinBaseHeight, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, 0.2f, 2f);

        // Tail Fin:
        tailFinSpreadAngle = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.tailFinSpreadAngle, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, 0.05f, 0.8f);
        tailFinBaseLength = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.tailFinBaseLength, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, 0.1f, 1f);
        tailFinFrequencies = UtilityMutationFunctions.GetMutatedVector3Additive(parentGenome.tailFinFrequencies, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, 0.1f, 10f);
        tailFinAmplitudes = UtilityMutationFunctions.GetMutatedVector3Additive(parentGenome.tailFinAmplitudes, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, 0f, 1f);
        tailFinOffsets = UtilityMutationFunctions.GetMutatedVector3Additive(parentGenome.tailFinOffsets, settings.bodyProportionsMutationChance, settings.bodyProportionsMutationStepSize, -10f, 10f);

        talentSpecializationAttack = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.talentSpecializationAttack, settings.bodyTalentSpecMutationChance, settings.bodyTalentSpecMutationStepSize, 0.01f, 1f);
        talentSpecializationDefense = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.talentSpecializationDefense, settings.bodyTalentSpecMutationChance, settings.bodyTalentSpecMutationStepSize, 0.01f, 1f);
        talentSpecializationSpeed = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.talentSpecializationSpeed, settings.bodyTalentSpecMutationChance, settings.bodyTalentSpecMutationStepSize, 0.01f, 1f);
        talentSpecializationUtility = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.talentSpecializationUtility, settings.bodyTalentSpecMutationChance, settings.bodyTalentSpecMutationStepSize, 0.01f, 1f);

        dietSpecializationPlant = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.dietSpecializationPlant, settings.bodyDietSpecMutationChance, settings.bodyDietSpecMutationStepSize, 0.01f, 1f);
        dietSpecializationDecay = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.dietSpecializationDecay, settings.bodyDietSpecMutationChance, settings.bodyDietSpecMutationStepSize, 0.01f, 1f);
        dietSpecializationMeat = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.dietSpecializationMeat, settings.bodyDietSpecMutationChance, settings.bodyDietSpecMutationStepSize, 0.01f, 1f);
        
        mouthFeedFrequency = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.mouthFeedFrequency, settings.bodyCoreSizeMutationChance, settings.bodyCoreMutationStepSize, 0.25f, 4f);
        mouthAttackAmplitude = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.mouthAttackAmplitude, settings.bodyCoreSizeMutationChance, settings.bodyCoreMutationStepSize, 0.25f, 4f);
    }
    
    public void SetToAverage(List<CandidateAgentData> leaderboard, float inverseCount)
    {
        // Clear out existing values
        dietSpecializationDecay = 0f;
        dietSpecializationPlant = 0f;
        dietSpecializationMeat = 0f;
        talentSpecializationAttack = 0f;
        talentSpecializationDefense = 0f;
        talentSpecializationSpeed = 0f;
        talentSpecializationUtility = 0f;

        CritterModuleCoreGenome leader;
    
        // Sum the average leaderboard values
        foreach (var agent in leaderboard)
        {
            leader = agent.candidateGenome.bodyGenome.coreGenome;

            dietSpecializationDecay += leader.dietSpecializationDecay;
            dietSpecializationPlant += leader.dietSpecializationPlant;
            dietSpecializationMeat += leader.dietSpecializationMeat;
            talentSpecializationAttack += leader.talentSpecializationAttack;
            talentSpecializationDefense += leader.talentSpecializationDefense;
            talentSpecializationSpeed += leader.talentSpecializationSpeed;
            talentSpecializationUtility += leader.talentSpecializationUtility;
            bodyLength += leader.bodyLength;
            creatureAspectRatio += leader.creatureAspectRatio;
        }
        
        // Multiply the result by the inverse of the leaderboard count for the average values
        dietSpecializationDecay *= inverseCount;
        dietSpecializationPlant *= inverseCount;
        dietSpecializationMeat *= inverseCount;
        talentSpecializationAttack *= inverseCount;
        talentSpecializationDefense *= inverseCount;
        talentSpecializationSpeed *= inverseCount;
        talentSpecializationUtility *= inverseCount;
        bodyLength *= inverseCount;
        creatureAspectRatio *= inverseCount;
    }
}
