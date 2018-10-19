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
    
    // Mouth/Snout:
    public float mouthLength;
    public float mouthFrontWidth;  // width of snout at front of critter
    public float mouthFrontHeight; // height of snout at front of critter
    public float mouthFrontVerticalOffset; // shift up/down pivot/cylinder center
    public float mouthBackWidth; 
    public float mouthBackHeight;
    public float mouthBackVerticalOffset;
    public float mouthToHeadTransitionSize;  // 0-1 normalized
    // Head
    public float headLength;
    public float headFrontWidth;
    public float headFrontHeight;
    public float headFrontVerticalOffset;
    public float headBackWidth; 
    public float headBackHeight;
    public float headBackVerticalOffset;
    public float headToBodyTransitionSize;  // 0-1 normalized
    // Body:
    public float bodyLength;
    public float bodyFrontWidth;  // width of snout at front of critter
    public float bodyFrontHeight; // height of snout at front of critter
    public float bodyFrontVerticalOffset; // shift up/down pivot/cylinder center
    public float bodyBackWidth; 
    public float bodyBackHeight;
    public float bodyBackVerticalOffset;
    public float bodyToTailTransitionSize;  // 0-1 normalized
    //Tail:
    public float tailLength;
    public float tailFrontWidth;  // width of snout at front of critter
    public float tailFrontHeight; // height of snout at front of critter
    public float tailFrontVerticalOffset; // shift up/down pivot/cylinder center
    public float tailBackWidth; 
    public float tailBackHeight;
    public float tailBackVerticalOffset;

    // Doodads / Attachments???
    // Eyes???

    // List of Shape/Form modifiers here???:::
    [System.Serializable]
    public enum ShapeModifierType {
        RadialExtrude,  // shift vertex along major normal
        NoisyExtrude,  // same but with
        UniformOffset,
        Polygonify
    }

    [System.Serializable]
    public struct ShapeModifierData {
        public int modifierTypeID;
        public float epicenter01;
        public float falloffDistMask;
        public Vector4 segmentsMask;  // how much to affect each of the 4 major sections
        public Vector2 axesMask;   // scale in XZ -- 
        public Vector2 direction;  // only affects vertices in this direction (XZ)   
        public float directionMask;
        public float frequency;  // generic stats used for periodic functions:
        public float amplitude;
        public float phaseOffset;
        public int numSides;
    }

    public List<ShapeModifierData> shapeModifiersList;  // holds list of all modifiers to overall critter shape/structure


        // OLD:::::
        /*
    public float fullBodyWidth;
    public float fullBodyLength;
    public float relWidthSnout = 1f;  // Relative to Head Width!
    public float relWidthHead = 1f;
    public float relWidthTorso = 1f;
    public float relWidthTail = 1f;  // Relative to Body Width!!
    public float relLengthSnout = 1f;
    public float relLengthHead = 1f;
    public float relLengthTorso = 1f;
    public float relLengthTail = 1f;
    public float snoutTaper = 0f;
    public float tailTaper = 0f;
    */
    // Critter local coordinates!!! Z-forward, Y-Forward????? *************

    //public int numSegments; // Number of GameObject / Rigidbodies

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

        isPassive = true;
        if(UnityEngine.Random.Range(0f, 1f) < 0.5f) {
            isPassive = false;
        }

        shapeModifiersList = new List<ShapeModifierData>();  // empty


        // Or start with deformed sphere???? *****
        // Mouth/Snout:
        mouthLength = 0.25f;
        mouthFrontWidth = 0.3f;  // width of snout at front of critter
        mouthFrontHeight = 0.1f; // height of snout at front of critter
        mouthFrontVerticalOffset = 0f; // shift up/down pivot/cylinder center
        mouthBackWidth = 0.5f; 
        mouthBackHeight = 0.1f;
        mouthBackVerticalOffset = 0f;
        mouthToHeadTransitionSize = 0.5f;  // 0-1 normalized
        // Head
        headLength = 0.5f;
        headFrontWidth = 0.5f;
        headFrontHeight = 0.25f;
        headFrontVerticalOffset = 0f;
        headBackWidth = 0.6f; 
        headBackHeight = 0.6f;
        headBackVerticalOffset = 0f;
        headToBodyTransitionSize = 0.5f;  // 0-1 normalized
        // Body:
        bodyLength = 1.45f;
        bodyFrontWidth = 0.8f;  // width of snout at front of critter
        bodyFrontHeight = 0.8f; // height of snout at front of critter
        bodyFrontVerticalOffset = 0f; // shift up/down pivot/cylinder center
        bodyBackWidth = 0.75f; 
        bodyBackHeight = 0.75f;
        bodyBackVerticalOffset = 0f;
        bodyToTailTransitionSize = 0.5f;  // 0-1 normalized
        //Tail:
        tailLength = 1.16f;
        tailFrontWidth = 0.3f;  // width of snout at front of critter
        tailFrontHeight = 0.2f; // height of snout at front of critter
        tailFrontVerticalOffset = 0f; // shift up/down pivot/cylinder center
        tailBackWidth = 0.05f; 
        tailBackHeight = 0.65f;
        tailBackVerticalOffset = 0f;

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
            newData = parentGenome.shapeModifiersList[i];  // only works if this is NOT a reference type!!! ***
            shapeModifiersList.Add(newData);
        }

        // Or start with deformed sphere???? *****
        // Mouth/Snout:
        mouthLength = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.mouthLength, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.01f, 2f);
        mouthFrontWidth = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.mouthFrontWidth, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.01f, 1.5f);;  // width of snout at front of critter
        mouthFrontHeight = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.mouthFrontHeight, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.01f, 1.5f);
        mouthFrontVerticalOffset = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.mouthFrontVerticalOffset, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, -1f, 1f);
        mouthBackWidth = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.mouthBackWidth, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.01f, 1.5f);
        mouthBackHeight = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.mouthBackHeight, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.01f, 1.5f);
        mouthBackVerticalOffset = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.mouthBackVerticalOffset, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, -1f, 1f);
        mouthToHeadTransitionSize = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.mouthToHeadTransitionSize, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.1f, 1f);
        // Head
        headLength = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.headLength, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.01f, 1.5f);
        headFrontWidth = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.headFrontWidth, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.01f, 1.5f);
        headFrontHeight = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.headFrontHeight, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.01f, 1.5f);
        headFrontVerticalOffset = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.headFrontVerticalOffset, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, -1f, 1f);
        headBackWidth = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.headBackWidth, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.01f, 1.5f);
        headBackHeight = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.headBackHeight, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.01f, 1.5f);
        headBackVerticalOffset = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.headBackVerticalOffset, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, -1f, 1f);
        headToBodyTransitionSize = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.headToBodyTransitionSize, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.1f, 1f);
        // Body:
        bodyLength = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.bodyLength, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.01f, 2.5f);
        bodyFrontWidth = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.bodyFrontWidth, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.01f, 1.5f);
        bodyFrontHeight = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.bodyFrontHeight, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.01f, 1.5f);
        bodyFrontVerticalOffset = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.bodyFrontVerticalOffset, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, -1f, 1f);
        bodyBackWidth = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.bodyBackWidth, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.01f, 1.5f);
        bodyBackHeight = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.bodyBackHeight, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.01f, 1.5f);
        bodyBackVerticalOffset = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.bodyBackVerticalOffset, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, -1f, 1f);
        bodyToTailTransitionSize = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.bodyToTailTransitionSize, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.1f, 1f);
        //Tail:
        tailLength = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.tailLength, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.01f, 2.5f);
        tailFrontWidth = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.tailFrontWidth, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.01f, 1.5f);
        tailFrontHeight = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.tailFrontHeight, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.01f, 1.5f);
        tailFrontVerticalOffset = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.tailFrontVerticalOffset, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, -1f, 1f);
        tailBackWidth = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.tailBackWidth, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.01f, 1.5f);
        tailBackHeight = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.tailBackHeight, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.01f, 1.5f);
        tailBackVerticalOffset = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.tailBackVerticalOffset, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, -1f, 1f);


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
