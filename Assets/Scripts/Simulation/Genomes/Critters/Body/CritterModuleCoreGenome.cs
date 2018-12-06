using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CritterModuleCoreGenome {

    public int parentID;
    public int inno;

    // 4 main sections: 
    //    Mouth/Snout
    //    Head
    //    Body/Torso
    //    Tail

    public float creatureBaseLength;
    public float creatureAspectRatio;
    
    // Mouth/Snout:
    public float creatureFrontTaperSize;
    public float creatureBackTaperSize;

    //public float mouthComplexShapeLerp;  // 0 = spherical simple creature, 1 = use section proportions
    public float mouthLength;
    public float mouthFrontWidth;  // width of snout at front of critter
    public float mouthFrontHeight; // height of snout at front of critter
    public float mouthFrontVerticalOffset; // shift up/down pivot/cylinder center
    public float mouthBackWidth; 
    public float mouthBackHeight;
    public float mouthBackVerticalOffset;

    public float mouthToHeadTransitionSize;  // 0-1 normalized
    // Head
    //public float headComplexShapeLerp;
    public float headLength;
    public float headFrontWidth;
    public float headFrontHeight;
    public float headFrontVerticalOffset;
    public float headBackWidth; 
    public float headBackHeight;
    public float headBackVerticalOffset;

    public float headToBodyTransitionSize;  // 0-1 normalized
    // Body:
    //public float bodyComplexShapeLerp;
    public float bodyLength;
    public float bodyFrontWidth;  // width of snout at front of critter
    public float bodyFrontHeight; // height of snout at front of critter
    public float bodyFrontVerticalOffset; // shift up/down pivot/cylinder center
    public float bodyBackWidth; 
    public float bodyBackHeight;
    public float bodyBackVerticalOffset;

    public float bodyToTailTransitionSize;  // 0-1 normalized
    //Tail:
    //public float tailComplexShapeLerp;
    public float tailLength;
    public float tailFrontWidth;  // width of snout at front of critter
    public float tailFrontHeight; // height of snout at front of critter
    public float tailFrontVerticalOffset; // shift up/down pivot/cylinder center
    public float tailBackWidth; 
    public float tailBackHeight;
    public float tailBackVerticalOffset;

    // EYES:::  (Eventually separate these out better into different subclasses/structs)
    public int numEyes = 2;
    public float eyePosSpread = 1f;  // 1f == full hemisphere coverage, 0 == top
    public float eyeLocAmplitude = 0.5f;
    public float eyeLocFrequency = 1f;
    public float eyeLocOffset = 0f;
        
    public float socketRadius = 1f;  // relative to body size?
    public float socketHeight = 0.25f; 
    public float socketBulge = 0f;
    public float eyeballRadius = 1f;
    public float eyeBulge = 0.33f;
    public float irisWidthFraction = 0.9f;        
    public float pupilWidthFraction = 0.5f;  // percentage of iris size
    public float pupilHeightFraction = 1f;
    public Vector3 eyeballHue;
    public Vector3 irisHue;
    
    // Dorsal Fin:
    public float dorsalFinStartCoordY = 0.2f;
    public float dorsalFinEndCoordY = 0.7f;
    public float dorsalFinSlantAmount = 0.35f;
    public float dorsalFinBaseHeight = 1f;

    // TAIL FIN:
    public float tailFinSpreadAngle = 0.35f;
    public float tailFinBaseLength = 1f;
    public Vector3 tailFinFrequencies = Vector3.one;
    public Vector3 tailFinAmplitudes = Vector3.one;
    public Vector3 tailFinOffsets = Vector3.zero;

    // Specialization Paths first try:
    public float talentSpecializationAttack;
    public float talentSpecializationDefense;
    public float talentSpecializationSpeed;
    public float talentSpecializationUtility;

    // Diet specialization:
    public float dietSpecializationDecay;
    public float dietSpecializationPlant;    
    public float dietSpecializationMeat;

    public float mouthFeedFrequency;
    public float mouthAttackAmplitude;

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

    [System.Serializable]
    public enum ShapeModifierType {
        Extrude,  // shift vertex along major normal
        UniformScale
    }
    [System.Serializable]
    public enum MaskCoordinateType {
        Lengthwise,
        //Radial,  // uv distance
        //SingleAxis,  // arbitrary direction of uv coordinate flow
        Polygonize  // facet body circumference        
    }
    [System.Serializable]
    public enum MaskFunctionType {
        Linear,
        Sin,
        Cos      
    }

    [System.Serializable]
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
    }
    [System.Serializable]
    public struct ShapeModifierData {  // each modifer gets interpreted by script, picks from correct maskData list
        public ShapeModifierType modifierTypeID;  // extrude; axis-aligned scale; twist/pinch/etc?
        public List<int> maskIndicesList;  // method of falloff
        public float taperDistance; // keep/remove?
        public float amplitude;  // move to Range of mult like (0.8 to 1.25)?
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
    

	public CritterModuleCoreGenome(int parentID, int inno) {
        this.parentID = parentID;
        this.inno = inno;
        
    }
    
    public void GenerateRandomInitialGenome() {
        // Do stuff:
        //Debug.Log("GenerateRandomGenome()");

        isPassive = true;  // mouth type -- change later
        if(UnityEngine.Random.Range(0f, 1f) < 0.5f) {
            isPassive = false;
        }

        shapeModifiersList = new List<ShapeModifierData>();  // empty
        masksList = new List<MaskData>();

        //modifierCoordinateDataList = new List<ModifierCoordinateData>();
        //maskDataListGeneric = new List<CritterGenomeInterpretor.FalloffMaskDataGeneric>();
        //maskListSin = new List<CritterGenomeInterpretor.MaskDataSin>();
        //maskListLinearFalloff = new List<CritterGenomeInterpretor.MaskDataLinearFalloff>();

        // TEMP HARDCODED:
        ShapeModifierData initModifier = new ShapeModifierData();
        initModifier.modifierTypeID = ShapeModifierType.Extrude;
        initModifier.maskIndicesList = new List<int>();
        initModifier.amplitude = 0.05f;
        initModifier.taperDistance = 1f;

        // Masks for this modifier:
        MaskData maskData = new MaskData();
        maskData.coordinateTypeID = MaskCoordinateType.Polygonize;
        maskData.functionTypeID = MaskFunctionType.Cos;
        maskData.origin = 0.5f; // normalized along length of creature
        maskData.amplitude = 0.5f;
        maskData.cycleDistance = 0.5f;
        maskData.phase = 0f;
        maskData.numPolyEdges = 1;
        maskData.axisDir = new Vector2(0f, 1f);
        maskData.repeat = true;

        masksList.Add(maskData);
        initModifier.maskIndicesList.Add(masksList.Count - 1); // reference mask by index to allow re-use by other shape modifiers    
        shapeModifiersList.Add(initModifier);

        creatureBaseLength = UnityEngine.Random.Range(0.2f, 0.3f);
        creatureAspectRatio = UnityEngine.Random.Range(0.35f, 0.55f);

        //creatureComplexShapeLerp = 0f;
        // Or start with deformed sphere???? *****
        // Mouth/Snout:
        creatureFrontTaperSize = 0.33f;
        creatureBackTaperSize = 0.33f;

        //mouthComplexShapeLerp = 0f;
        mouthLength = 1f;
        mouthFrontWidth = 1f;  // width of snout at front of critter
        mouthFrontHeight = 1f; // height of snout at front of critter
        mouthFrontVerticalOffset = 0f; // shift up/down pivot/cylinder center
        mouthBackWidth = 1f; 
        mouthBackHeight = 1f;
        mouthBackVerticalOffset = 0f;        

        mouthToHeadTransitionSize = 0.5f;  // 0-1 normalized
        // Head
        //headComplexShapeLerp = 0f;
        headLength = 1f;
        headFrontWidth = 1f;
        headFrontHeight = 1f;
        headFrontVerticalOffset = 0f;
        headBackWidth = 1f; 
        headBackHeight = 1f;
        headBackVerticalOffset = 0f;

        headToBodyTransitionSize = 0.5f;  // 0-1 normalized
        // Body:
        //bodyComplexShapeLerp = 0f;
        bodyLength = 1f;
        bodyFrontWidth = 1f;  // width of snout at front of critter
        bodyFrontHeight = 1f; // height of snout at front of critter
        bodyFrontVerticalOffset = 0f; // shift up/down pivot/cylinder center
        bodyBackWidth = 1f; 
        bodyBackHeight = 1f;
        bodyBackVerticalOffset = 0f;

        bodyToTailTransitionSize = 0.5f;  // 0-1 normalized
        //Tail:
        //tailComplexShapeLerp = 0f;
        tailLength = 1f;
        tailFrontWidth = 1f;  // width of snout at front of critter
        tailFrontHeight = 1f; // height of snout at front of critter
        tailFrontVerticalOffset = 0f; // shift up/down pivot/cylinder center
        tailBackWidth = 1f; 
        tailBackHeight = 1f;
        tailBackVerticalOffset = 0f;

        //tailEndCapTaperSize = 0.5f;

        numEyes = 2;
        eyePosSpread = 1f;  // 1f == full hemisphere coverage, 0 == top
        eyeLocAmplitude = 0.5f;
        eyeLocFrequency = 1f;
        eyeLocOffset = 0f;        
        socketRadius = 1f;  // relative to body size?
        socketHeight = 0.25f; 
        socketBulge = 0f;
        eyeballRadius = 1f;
        eyeBulge = 0.33f;
        irisWidthFraction = 0.9f;        
        pupilWidthFraction = 0.5f;  // percentage of iris size
        pupilHeightFraction = 1f;
        eyeballHue = Vector3.one;
        irisHue = new Vector3(0.25f, 0.5f, 0.5f);
        

        // Dorsal Fin:
        dorsalFinStartCoordY = 0.2f;
        dorsalFinEndCoordY = 0.7f;
        dorsalFinSlantAmount = 0.35f;
        dorsalFinBaseHeight = 0f;

        // Tail Fin:
        tailFinSpreadAngle = 0.3f;
        tailFinBaseLength = 0f;
        tailFinFrequencies = Vector3.one;
        tailFinAmplitudes = Vector3.one;
        tailFinOffsets = Vector3.zero;

        talentSpecializationAttack = 0.5f;
        talentSpecializationDefense = 0.5f;
        talentSpecializationSpeed = 0.5f;
        talentSpecializationUtility = 0.5f;

        dietSpecializationPlant = 0.5f;
        dietSpecializationDecay = 0.5f;
        dietSpecializationMeat = 0.5f;

        mouthFeedFrequency = 1f;
        mouthAttackAmplitude = 1f;

        //numSegments = 1;
        /*
        fullBodyWidth = UnityEngine.Random.Range(0.33f, 0.5f);
        fullBodyLength = fullBodyWidth * UnityEngine.Random.Range(1.25f, 2f);

        relWidthSnout = UnityEngine.Random.Range(0.25f, 1f);
        relWidthHead = UnityEngine.Random.Range(0.5f, 2f);
        relWidthTorso = UnityEngine.Random.Range(0.5f, 2f);
        relWidthTail = UnityEngine.Random.Range(0.1f, 1f);
        relLengthSnout = UnityEngine.Random.Range(0.5f, 2f);
        relLengthHead = UnityEngine.Random.Range(0.5f, 2f);
        relLengthTorso = UnityEngine.Random.Range(0.5f, 2f);
        relLengthTail = UnityEngine.Random.Range(0.5f, 2f);

        snoutTaper = UnityEngine.Random.Range(0f, 1f);
        tailTaper = UnityEngine.Random.Range(0f, 1f);
        */
    }

    public void AppendModuleNeuronsToMasterList(ref List<NeuronGenome> neuronList) {
        NeuronGenome bias = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 0);

        NeuronGenome isMouthTrigger = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 21);
        //NeuronGenome temperature = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 22); // 22
        //NeuronGenome pressure = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 23); // 23
        NeuronGenome isContact = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 24); // 24
        NeuronGenome contactForceX = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 25); // 25
        NeuronGenome contactForceY = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 26); // 26
        
        NeuronGenome hitPoints = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 27); // 27
        NeuronGenome stamina = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 28); // 28
        NeuronGenome energyStored = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 204); // 27
        NeuronGenome foodStored = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 205); // 28
        
        NeuronGenome mouthFeedEffector = new NeuronGenome(NeuronGenome.NeuronType.Out, inno, 206); // 106

        neuronList.Add(bias);   //0

        neuronList.Add(isMouthTrigger); // 21
        neuronList.Add(isContact); // 24
        neuronList.Add(contactForceX); // 25
        neuronList.Add(contactForceY); // 26 

        //neuronList.Add(temperature); // 22
        //neuronList.Add(pressure); // 23
        neuronList.Add(hitPoints); // 27
        neuronList.Add(stamina); // 28
        neuronList.Add(energyStored); // 204
        neuronList.Add(foodStored); // 205
           
        neuronList.Add(mouthFeedEffector); // 206

        float talentSpecTotal = talentSpecializationAttack + talentSpecializationDefense + talentSpecializationSpeed + talentSpecializationUtility;
        float talentSpecAttackNorm = talentSpecializationAttack / talentSpecTotal;
        float talentSpecDefenseNorm = talentSpecializationDefense / talentSpecTotal;
        float talentSpecSpeedNorm = talentSpecializationSpeed / talentSpecTotal;
        float talentSpecUtilityNorm = talentSpecializationUtility / talentSpecTotal;

        if(talentSpecAttackNorm > 0.2f) {
            NeuronGenome mouthAttackEffector = new NeuronGenome(NeuronGenome.NeuronType.Out, inno, 207);
            neuronList.Add(mouthAttackEffector);
        }
        if(talentSpecDefenseNorm > 0.2f) {
            NeuronGenome defendEffector = new NeuronGenome(NeuronGenome.NeuronType.Out, inno, 208);
            neuronList.Add(defendEffector);
        }
        if(talentSpecSpeedNorm > 0.2f) {
            NeuronGenome dashEffector = new NeuronGenome(NeuronGenome.NeuronType.Out, inno, 209);
            neuronList.Add(dashEffector);
        }
        if(talentSpecUtilityNorm > 0.2f) {
            NeuronGenome healEffector = new NeuronGenome(NeuronGenome.NeuronType.Out, inno, 210);
            neuronList.Add(healEffector);
        }
    }

    public void SetToMutatedCopyOfParentGenome(CritterModuleCoreGenome parentGenome, MutationSettings settings) {

        isPassive = UtilityMutationFunctions.GetMutatedBool(parentGenome.isPassive, 0.033f, settings.defaultBodyMutationStepSize);

        // Copy modifiers list? do I need to create copies of each entry? or staright copy should work since they are structs (value-typed)
        shapeModifiersList = new List<ShapeModifierData>();  // empty
        for(int i = 0; i < parentGenome.shapeModifiersList.Count; i++) {
            ShapeModifierData newData = new ShapeModifierData();
            newData.maskIndicesList = new List<int>();
            for(int j = 0; j < parentGenome.shapeModifiersList[i].maskIndicesList.Count; j++) {
                int maskIndex = parentGenome.shapeModifiersList[i].maskIndicesList[j];
                // MUTATE?
                maskIndex = UtilityMutationFunctions.GetMutatedIntAdditive(maskIndex, settings.defaultBodyMutationChance, 2, 0, parentGenome.shapeModifiersList[i].maskIndicesList.Count - 1);
                newData.maskIndicesList.Add(maskIndex); // make sure this isn't passing as a reference? it's an 'int' (data-type) so should be ok... // newMaskData);
            }            
            newData.modifierTypeID = parentGenome.shapeModifiersList[i].modifierTypeID;  // only works if this is NOT a reference type!!! ***
            newData.amplitude = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.shapeModifiersList[i].amplitude, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, -0.5f, 0.5f); //parentGenome.shapeModifiersList[i].amplitude;
            newData.taperDistance = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.shapeModifiersList[i].taperDistance, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.2f, 2f); //;
            shapeModifiersList.Add(newData);
        }
        masksList = new List<MaskData>();
        for(int i = 0; i < parentGenome.masksList.Count; i++) {
            MaskData newMask = new MaskData();
            newMask = parentGenome.masksList[i];
            int maskCoordinateTypeID = (int)newMask.coordinateTypeID;
            newMask.coordinateTypeID = MaskCoordinateType.Polygonize; // (MaskCoordinateType)UtilityMutationFunctions.GetMutatedIntAdditive(maskCoordinateTypeID, settings.defaultBodyMutationChance, 1, 0, 1);
            int maskFunctionTypeID = (int)newMask.functionTypeID;
            newMask.functionTypeID = MaskFunctionType.Cos; // (MaskFunctionType)UtilityMutationFunctions.GetMutatedIntAdditive(maskFunctionTypeID, settings.defaultBodyMutationChance, 2, 0, 2);
            newMask.numPolyEdges = UtilityMutationFunctions.GetMutatedIntAdditive(newMask.numPolyEdges, settings.defaultBodyMutationChance, 4, 1, 6);
            newMask.origin = UtilityMutationFunctions.GetMutatedFloatAdditive(newMask.origin, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0f, 1f);
            newMask.phase = UtilityMutationFunctions.GetMutatedFloatAdditive(newMask.phase, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, -5f, 5f);
            newMask.cycleDistance = UtilityMutationFunctions.GetMutatedFloatAdditive(newMask.origin, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.1f, 1f);
            newMask.amplitude = UtilityMutationFunctions.GetMutatedFloatAdditive(newMask.origin, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.05f, 0.5f);
            newMask.axisDir = UtilityMutationFunctions.GetMutatedVector2Additive(newMask.axisDir, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, -1f, 1f).normalized;
            masksList.Add(newMask);
        }
        // Mutate Add New Modifiers Here:?
        if(shapeModifiersList.Count < 6) {
            float randRoll = UnityEngine.Random.Range(0f, 1f);
            if(randRoll < 0.02f) {
                // Add new shapeModifier:
                ShapeModifierData initModifier = new ShapeModifierData();
                initModifier.modifierTypeID = ShapeModifierType.Extrude;
                initModifier.maskIndicesList = new List<int>();
                initModifier.amplitude = 0.4f;
                initModifier.taperDistance = 0.2f;

                shapeModifiersList.Add(initModifier);
            }
        }
        if(masksList.Count < 4) {
            float randRoll = UnityEngine.Random.Range(0f, 1f);
            if(randRoll < 0.04f) {
                // Add new MASK:
                MaskData maskData = new MaskData();
                maskData.coordinateTypeID = MaskCoordinateType.Polygonize;
                maskData.functionTypeID = MaskFunctionType.Cos;
                maskData.origin = 0.5f; // normalized along length of creature
                maskData.amplitude = 1f;
                maskData.cycleDistance = 0.5f;
                maskData.phase = 0f;
                maskData.numPolyEdges = 1;
                maskData.axisDir = new Vector2(0f, 1f);
                maskData.repeat = true;

                masksList.Add(maskData);
            }
        }        
        
        // Or start with deformed sphere???? *****
        creatureBaseLength = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.creatureBaseLength, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.05f, 2.25f);
        creatureAspectRatio = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.creatureAspectRatio, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.1f, 0.75f);
        
        //creatureComplexShapeLerp = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.creatureComplexShapeLerp, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0f, 1f);
        // Mouth/Snout:
        creatureFrontTaperSize = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.creatureFrontTaperSize, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.05f, 0.33f);
        creatureBackTaperSize = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.creatureBackTaperSize, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.05f, 0.33f);
                
        mouthLength = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.mouthLength, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.2f, 2f);
        mouthFrontWidth = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.mouthFrontWidth, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.05f, 1.5f);  // width of snout at front of critter (RELATIVE TO LENGTH OF SEGMENT!)
        mouthFrontHeight = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.mouthFrontHeight, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.05f, 1.5f);
        mouthFrontVerticalOffset = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.mouthFrontVerticalOffset, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, -1f, 1f);
        mouthBackWidth = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.mouthBackWidth, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.05f, 1.5f);
        mouthBackHeight = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.mouthBackHeight, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.05f, 1.5f);
        mouthBackVerticalOffset = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.mouthBackVerticalOffset, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, -1f, 1f);
        
        mouthToHeadTransitionSize = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.mouthToHeadTransitionSize, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.25f, 1f);
        // Head        
        headLength = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.headLength, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.2f, 2f);
        headFrontWidth = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.headFrontWidth, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.05f, 1.5f);
        headFrontHeight = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.headFrontHeight, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.05f, 1.5f);
        headFrontVerticalOffset = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.headFrontVerticalOffset, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, -1f, 1f);
        headBackWidth = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.headBackWidth, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.05f, 1.5f);
        headBackHeight = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.headBackHeight, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.05f, 1.5f);
        headBackVerticalOffset = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.headBackVerticalOffset, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, -1f, 1f);

        headToBodyTransitionSize = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.headToBodyTransitionSize, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.25f, 1f);
        // Body:
        bodyLength = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.bodyLength, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.5f, 5f);
        bodyFrontWidth = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.bodyFrontWidth, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.05f, 1.5f);
        bodyFrontHeight = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.bodyFrontHeight, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.05f, 1.5f);
        bodyFrontVerticalOffset = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.bodyFrontVerticalOffset, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, -1f, 1f);
        bodyBackWidth = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.bodyBackWidth, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.05f, 1.5f);
        bodyBackHeight = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.bodyBackHeight, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.05f, 1.5f);
        bodyBackVerticalOffset = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.bodyBackVerticalOffset, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, -1f, 1f);

        bodyToTailTransitionSize = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.bodyToTailTransitionSize, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.25f, 1f);
        //Tail:
        tailLength = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.tailLength, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.5f, 5f);
        tailFrontWidth = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.tailFrontWidth, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.05f, 1.5f);
        tailFrontHeight = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.tailFrontHeight, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.05f, 1.5f);
        tailFrontVerticalOffset = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.tailFrontVerticalOffset, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, -1f, 1f);
        tailBackWidth = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.tailBackWidth, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.05f, 1.5f);
        tailBackHeight = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.tailBackHeight, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.05f, 1.5f);
        tailBackVerticalOffset = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.tailBackVerticalOffset, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, -1f, 1f);

        //tailEndCapTaperSize = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.tailEndCapTaperSize, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.5f, 1f);

        // EYES EYES EYES EYES:::
        numEyes = 2; // UtilityMutationFunctions.GetMutatedIntAdditive(parentGenome.numEyes, settings.defaultBodyMutationChance, 1, 1, 3);
        eyePosSpread = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.eyePosSpread, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.5f, 1f);  // 1f == full hemisphere coverage, 0 == top
        eyeLocAmplitude = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.eyeLocAmplitude, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0f, 1f);
        eyeLocFrequency = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.eyeLocFrequency, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.25f, 6f);
        eyeLocOffset = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.eyeLocOffset, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0f, 5f);        
        socketRadius = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.socketRadius, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.2f, 0.9f);
        socketHeight = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.socketHeight, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.25f, 1.25f);
        socketBulge = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.socketBulge, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, -0.75f, 0.75f);
        eyeballRadius = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.eyeballRadius, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.5f, 1.75f); // relative to socket radius
        eyeBulge = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.eyeBulge, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.4f, 1.35f);
        irisWidthFraction = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.irisWidthFraction, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.6f, 0.9f);       
        pupilWidthFraction = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.pupilWidthFraction, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.33f, 1f);  // percentage of iris size
        pupilHeightFraction = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.pupilHeightFraction, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.33f, 1f);
        eyeballHue = UtilityMutationFunctions.GetMutatedVector3Additive(parentGenome.eyeballHue, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.9f, 1f);
        irisHue = UtilityMutationFunctions.GetMutatedVector3Additive(parentGenome.irisHue, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0f, 1f);

        // DORSAL FIN:
        dorsalFinStartCoordY = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.dorsalFinStartCoordY, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0f, 0.49f);
        dorsalFinEndCoordY = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.dorsalFinEndCoordY, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.51f, 1f);
        dorsalFinSlantAmount = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.dorsalFinSlantAmount, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0f, 0.67f);
        dorsalFinBaseHeight = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.dorsalFinBaseHeight, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.2f, 2f);

        // Tail Fin:
        tailFinSpreadAngle = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.tailFinSpreadAngle, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.05f, 0.8f);
        tailFinBaseLength = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.tailFinBaseLength, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.1f, 1f);
        tailFinFrequencies = UtilityMutationFunctions.GetMutatedVector3Additive(parentGenome.tailFinFrequencies, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.1f, 10f);
        tailFinAmplitudes = UtilityMutationFunctions.GetMutatedVector3Additive(parentGenome.tailFinAmplitudes, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0f, 1f);
        tailFinOffsets = UtilityMutationFunctions.GetMutatedVector3Additive(parentGenome.tailFinOffsets, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, -10f, 10f);

        talentSpecializationAttack = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.talentSpecializationAttack, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.01f, 1f);
        talentSpecializationDefense = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.talentSpecializationDefense, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.01f, 1f);
        talentSpecializationSpeed = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.talentSpecializationSpeed, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.01f, 1f);
        talentSpecializationUtility = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.talentSpecializationUtility, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.01f, 1f);

        dietSpecializationPlant = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.dietSpecializationPlant, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.01f, 1f);
        dietSpecializationDecay = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.dietSpecializationDecay, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.01f, 1f);
        dietSpecializationMeat = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.dietSpecializationMeat, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.01f, 1f);
        
        mouthFeedFrequency = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.mouthFeedFrequency, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.25f, 4f);
        mouthAttackAmplitude = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.mouthAttackAmplitude, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.25f, 4f);

        /*fullBodyWidth = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.fullBodyWidth, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.1f, 4.5f);
        fullBodyLength = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.fullBodyLength, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, fullBodyWidth * 1.25f, fullBodyWidth * 4f);
        if(fullBodyLength < fullBodyWidth * 1.25f) {
            fullBodyLength = fullBodyWidth * 1.25f;
        }

        //numSegments = parentGenome.numSegments;
        //numSegments = 1; // UtilityMutationFunctions.GetMutatedIntAdditive(parentGenome.numSegments, settings.defaultBodyMutationChance, 3, 1, 12);
        
        relWidthSnout = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.relWidthSnout, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.25f, 1f);
        relWidthHead = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.relWidthHead, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.5f, 2f);
        relWidthTorso = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.relWidthTorso, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.5f, 2f);
        relWidthTail = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.relWidthTail, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.1f, 1f);

        relLengthSnout = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.relLengthSnout, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.5f, 2f);
        relLengthHead = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.relLengthHead, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.5f, 2f);
        relLengthTorso = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.relLengthTorso, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.5f, 2f);
        relLengthTail = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.relLengthTail, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.5f, 2f);

        snoutTaper = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.snoutTaper, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0f, 1f);
        tailTaper = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.tailTaper, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0f, 1f);
        */
    }
}
