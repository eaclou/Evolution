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
    public float creatureBaseAspectRatio;

    //public float creatureComplexShapeLerp;
    // Mouth/Snout:
    public float creatureFrontTaperSize;
    //public float creatureFrontTaperCurve;
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
    
    
    // Doodads / Attachments???
    

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

        creatureBaseLength = UnityEngine.Random.Range(0.4f, 0.6f);
        creatureBaseAspectRatio = 2f;

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
        NeuronGenome foodPosX = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 1);
        NeuronGenome foodPosY = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 2);
        NeuronGenome foodDirX = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 3);
        NeuronGenome foodDirY = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 4);
        //NeuronGenome foodTypeR = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 5);
        //NeuronGenome foodTypeG = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 6);
        //NeuronGenome foodTypeB = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 7);
        NeuronGenome foodRelSize = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 5);
        
        NeuronGenome friendPosX = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 8);
        NeuronGenome friendPosY = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 9);
        NeuronGenome friendVelX = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 10);
        NeuronGenome friendVelY = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 11);
        NeuronGenome friendDirX = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 12);
        NeuronGenome friendDirY = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 13);

        NeuronGenome enemyPosX = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 14);
        NeuronGenome enemyPosY = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 15);
        NeuronGenome enemyVelX = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 16);
        NeuronGenome enemyVelY = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 17);
        NeuronGenome enemyDirX = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 18);
        NeuronGenome enemyDirY = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 19);

        NeuronGenome enemyRelSize = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 200);
        NeuronGenome enemyHealth = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 201);
        NeuronGenome enemyGrowthStage = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 202);
        NeuronGenome enemyThreatRating = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 203);
                
        //NeuronGenome temperature = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 22); // 22
        //NeuronGenome pressure = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 23); // 23
        NeuronGenome isContact = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 24); // 24
        NeuronGenome contactForceX = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 25); // 25
        NeuronGenome contactForceY = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 26); // 26
        
        //NeuronGenome foodAmountR = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 29); // 29
        //NeuronGenome foodAmountG = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 30); // 30
        //NeuronGenome foodAmountB = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 31); // 31
        NeuronGenome hitPoints = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 27); // 27
        NeuronGenome stamina = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 28); // 28
        NeuronGenome energyStored = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 204); // 27
        NeuronGenome foodStored = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 205); // 28

        NeuronGenome distUp = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 32); // 32 // start up and go clockwise!
        NeuronGenome distTopRight = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 33); // 33
        NeuronGenome distRight = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 34);  // 34
        NeuronGenome distBottomRight = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 35); // 35
        NeuronGenome distDown = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 36);  // 36
        NeuronGenome distBottomLeft = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 37);  // 37
        NeuronGenome distLeft = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 38);  // 38
        NeuronGenome distTopLeft = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 39);  // 39

        NeuronGenome inComm0 = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 40); // 40
        NeuronGenome inComm1 = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 41);// 41
        NeuronGenome inComm2 = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 42); // 42
        NeuronGenome inComm3 = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 43); // 43 
        // 44 Total Inputs
        NeuronGenome outComm0 = new NeuronGenome(NeuronGenome.NeuronType.Out, inno, 103); // 103
        NeuronGenome outComm1 = new NeuronGenome(NeuronGenome.NeuronType.Out, inno, 104); // 104
        NeuronGenome outComm2 = new NeuronGenome(NeuronGenome.NeuronType.Out, inno, 105); // 105
        NeuronGenome outComm3 = new NeuronGenome(NeuronGenome.NeuronType.Out, inno, 106); // 106

        NeuronGenome mouthEffector = new NeuronGenome(NeuronGenome.NeuronType.Out, inno, 206); // 106

        neuronList.Add(bias);   //0
        neuronList.Add(foodPosX);  //1
        neuronList.Add(foodPosY); // 2
        neuronList.Add(foodDirX);  // 3
        neuronList.Add(foodDirY);  // 4
        //neuronList.Add(foodTypeR); // 5
        //neuronList.Add(foodTypeG); // 6
        //neuronList.Add(foodTypeB); // 7
        neuronList.Add(foodRelSize);  // 5

        neuronList.Add(friendPosX); // 8
        neuronList.Add(friendPosY); // 9
        neuronList.Add(friendVelX); // 10
        neuronList.Add(friendVelY); // 11
        neuronList.Add(friendDirX); // 12
        neuronList.Add(friendDirY); // 13

        neuronList.Add(enemyPosX); // 14
        neuronList.Add(enemyPosY); // 15
        neuronList.Add(enemyVelX); // 16
        neuronList.Add(enemyVelY); // 17
        neuronList.Add(enemyDirX); // 18
        neuronList.Add(enemyDirY); // 19

        neuronList.Add(enemyRelSize); // 200
        neuronList.Add(enemyHealth); // 201
        neuronList.Add(enemyGrowthStage); // 202
        neuronList.Add(enemyThreatRating); // 203
        
        //neuronList.Add(temperature); // 22
        //neuronList.Add(pressure); // 23
        neuronList.Add(isContact); // 24
        neuronList.Add(contactForceX); // 25
        neuronList.Add(contactForceY); // 26
        
        //neuronList.Add(foodAmountR); // 29
        //neuronList.Add(foodAmountG); // 30
        //neuronList.Add(foodAmountB); // 31
        neuronList.Add(hitPoints); // 27
        neuronList.Add(stamina); // 28
        neuronList.Add(energyStored); // 204
        neuronList.Add(foodStored); // 205

        neuronList.Add(distUp); // 32 // start up and go clockwise!
        neuronList.Add(distTopRight); // 33
        neuronList.Add(distRight); // 34
        neuronList.Add(distBottomRight); // 35
        neuronList.Add(distDown); // 36
        neuronList.Add(distBottomLeft); // 37
        neuronList.Add(distLeft); // 38
        neuronList.Add(distTopLeft); // 39

        neuronList.Add(inComm0); // 40
        neuronList.Add(inComm1); // 41
        neuronList.Add(inComm2); // 42
        neuronList.Add(inComm3); // 43 
        // 44 Total Inputs
                
        neuronList.Add(outComm0); // 103
        neuronList.Add(outComm1); // 104
        neuronList.Add(outComm2); // 105
        neuronList.Add(outComm3); // 106 
        // 7 Total Outputs
        neuronList.Add(mouthEffector); // 206
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
        creatureBaseLength = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.creatureBaseLength, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.5f, 3f);
        creatureBaseAspectRatio = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.creatureBaseAspectRatio, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 3f, 12f);
        
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
        numEyes = UtilityMutationFunctions.GetMutatedIntAdditive(parentGenome.numEyes, settings.defaultBodyMutationChance, 1, 1, 5);
        eyePosSpread = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.eyePosSpread, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.25f, 1f);  // 1f == full hemisphere coverage, 0 == top
        eyeLocAmplitude = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.eyeLocAmplitude, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0f, 1f);
        eyeLocFrequency = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.eyeLocFrequency, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.25f, 6f);
        eyeLocOffset = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.eyeLocOffset, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0f, 5f);        
        socketRadius = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.socketRadius, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.2f, 1f);
        socketHeight = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.socketHeight, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.5f, 10f);
        socketBulge = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.socketBulge, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, -0.75f, 0.75f);
        eyeballRadius = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.eyeballRadius, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.5f, 1.5f);
        eyeBulge = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.eyeBulge, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.25f, 1.25f);
        irisWidthFraction = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.irisWidthFraction, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.67f, 1f);       
        pupilWidthFraction = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.pupilWidthFraction, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.33f, 1f);  // percentage of iris size
        pupilHeightFraction = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.pupilHeightFraction, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.33f, 1f);
        eyeballHue = UtilityMutationFunctions.GetMutatedVector3Additive(parentGenome.eyeballHue, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.9f, 1f);
        irisHue = UtilityMutationFunctions.GetMutatedVector3Additive(parentGenome.irisHue, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0f, 1f);

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
