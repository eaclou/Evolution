﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CritterModuleCoreGenome {

    public int parentID;
    public int inno;

    public float coreWidth;
    public float coreLength;

    public float relativeWidthHead = 1f;
    public float relativeWidthBody = 1f;
    public float relativeWidthTail = 1f;
    public float headStart = 0.33f;
    public float tailStart = 0.67f;

    public int numSegments; // Number of GameObject / Rigidbodies

	public CritterModuleCoreGenome(int parentID, int inno) {
        this.parentID = parentID;
        this.inno = inno;
        
    }

    public void GenerateRandomGenome() {
        // Do stuff:
        Debug.Log("GenerateRandomGenome()");

        numSegments = 1;
        coreWidth = UnityEngine.Random.Range(0.5f, 2.5f);
        coreLength = coreWidth * UnityEngine.Random.Range(1f, 6f);

        relativeWidthHead = UnityEngine.Random.Range(0.5f, 1f);
        relativeWidthBody = UnityEngine.Random.Range(0.5f, 1f);
        relativeWidthTail = UnityEngine.Random.Range(0.5f, 1f);
        headStart = UnityEngine.Random.Range(0.2f, 5f);
        tailStart = UnityEngine.Random.Range(0.5f, 1f);
    }

    public void AppendModuleNeuronsToMasterList(ref List<NeuronGenome> neuronList) {
        NeuronGenome bias = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 0);
        NeuronGenome foodPosX = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 1);
        NeuronGenome foodPosY = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 2);
        NeuronGenome foodDirX = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 3);
        NeuronGenome foodDirY = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 4);
        NeuronGenome foodTypeR = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 5);
        NeuronGenome foodTypeG = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 6);
        NeuronGenome foodTypeB = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 7);

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

        NeuronGenome ownVelX = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 20); // 20
        NeuronGenome ownVelY = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 21); // 21
        NeuronGenome temperature = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 22); // 22
        NeuronGenome pressure = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 23); // 23
        NeuronGenome isContact = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 24); // 24
        NeuronGenome contactForceX = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 25); // 25
        NeuronGenome contactForceY = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 26); // 26
        NeuronGenome hitPoints = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 27); // 27
        NeuronGenome stamina = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 28); // 28
        NeuronGenome foodAmountR = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 29); // 29
        NeuronGenome foodAmountG = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 30); // 30
        NeuronGenome foodAmountB = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 31); // 31

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

        NeuronGenome throttleX = new NeuronGenome(NeuronGenome.NeuronType.Out, inno, 100); // 100
        NeuronGenome throttleY = new NeuronGenome(NeuronGenome.NeuronType.Out, inno, 101); // 101
        NeuronGenome dash = new NeuronGenome(NeuronGenome.NeuronType.Out, inno, 102); // 102
        NeuronGenome outComm0 = new NeuronGenome(NeuronGenome.NeuronType.Out, inno, 103); // 103
        NeuronGenome outComm1 = new NeuronGenome(NeuronGenome.NeuronType.Out, inno, 104); // 104
        NeuronGenome outComm2 = new NeuronGenome(NeuronGenome.NeuronType.Out, inno, 105); // 105
        NeuronGenome outComm3 = new NeuronGenome(NeuronGenome.NeuronType.Out, inno, 106); // 106


        neuronList.Add(bias);   //0
        neuronList.Add(foodPosX);  //1
        neuronList.Add(foodPosY); // 2
        neuronList.Add(foodDirX);  // 3
        neuronList.Add(foodDirY);  // 4
        neuronList.Add(foodTypeR); // 5
        neuronList.Add(foodTypeG); // 6
        neuronList.Add(foodTypeB); // 7

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

        neuronList.Add(ownVelX); // 20
        neuronList.Add(ownVelY); // 21
        neuronList.Add(temperature); // 22
        neuronList.Add(pressure); // 23
        neuronList.Add(isContact); // 24
        neuronList.Add(contactForceX); // 25
        neuronList.Add(contactForceY); // 26
        neuronList.Add(hitPoints); // 27
        neuronList.Add(stamina); // 28
        neuronList.Add(foodAmountR); // 29
        neuronList.Add(foodAmountG); // 30
        neuronList.Add(foodAmountB); // 31

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

        neuronList.Add(throttleX); // 100
        neuronList.Add(throttleY); // 101
        neuronList.Add(dash); // 102
        neuronList.Add(outComm0); // 103
        neuronList.Add(outComm1); // 104
        neuronList.Add(outComm2); // 105
        neuronList.Add(outComm3); // 106 
        // 7 Total Outputs
    }

    public void SetToMutatedCopyOfParentGenome(CritterModuleCoreGenome parentGenome, MutationSettings settings) {


        coreWidth = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.coreWidth, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.5f, 2.5f);
        coreLength = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.coreLength, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, coreWidth * 1f, coreWidth * 6f);

        //numSegments = parentGenome.numSegments;
        numSegments = 1; // UtilityMutationFunctions.GetMutatedIntAdditive(parentGenome.numSegments, settings.defaultBodyMutationChance, 3, 1, 12);
        
        relativeWidthHead = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.relativeWidthHead, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.1f, 1f);
        relativeWidthBody = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.relativeWidthBody, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.1f, 1f);
        relativeWidthTail = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.relativeWidthTail, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.1f, 1f);
        headStart = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.headStart, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.1f, 1f);
        tailStart = UtilityMutationFunctions.GetMutatedFloatAdditive(parentGenome.tailStart, settings.defaultBodyMutationChance, settings.defaultBodyMutationStepSize, 0.1f, 1f);
        
    }
}