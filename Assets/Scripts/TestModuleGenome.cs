using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TestModuleGenome {
    public int parentID;
    public int inno;

    public float maxSpeed;
    public float accel;
    public float radius;

    public TestModuleGenome(int parentID, int inno) {
        this.parentID = parentID;
        this.inno = inno;

        maxSpeed = 2f;
        accel = 0.04f;
        radius = 1f;
    }

    public TestModuleGenome(TestModuleGenome template) {
        this.parentID = template.parentID;
        this.inno = template.inno;
        this.maxSpeed = template.maxSpeed;
        this.accel = template.accel;
        this.radius = template.radius;
    }

    public void InitializeBrainGenome(List<NeuronGenome> neuronList) {
        NeuronGenome bias = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 0);
        NeuronGenome ownPosX = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 1);
        NeuronGenome ownPosY = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 2);
        NeuronGenome ownVelX = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 3);
        NeuronGenome ownVelY = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 4);
        NeuronGenome enemyPosX = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 5);
        NeuronGenome enemyPosY = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 6);
        NeuronGenome enemyVelX = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 7);
        NeuronGenome enemyVelY = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 8);
        NeuronGenome enemyDirX = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 9);
        NeuronGenome enemyDirY = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 10);

        NeuronGenome distLeft = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 11);
        NeuronGenome distRight = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 12);
        NeuronGenome distUp = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 13);
        NeuronGenome distDown = new NeuronGenome(NeuronGenome.NeuronType.In, inno, 14);

        NeuronGenome throttleX = new NeuronGenome(NeuronGenome.NeuronType.Out, inno, 15);
        NeuronGenome throttleY = new NeuronGenome(NeuronGenome.NeuronType.Out, inno, 16);

        neuronList.Add(bias);
        neuronList.Add(ownPosX);
        neuronList.Add(ownPosY);
        neuronList.Add(ownVelX);
        neuronList.Add(ownVelY);
        neuronList.Add(enemyPosX);
        neuronList.Add(enemyPosY);
        neuronList.Add(enemyVelX);
        neuronList.Add(enemyVelY);
        neuronList.Add(enemyDirX);
        neuronList.Add(enemyDirY);

        neuronList.Add(distLeft);
        neuronList.Add(distRight);
        neuronList.Add(distUp);
        neuronList.Add(distDown);

        neuronList.Add(throttleX);
        neuronList.Add(throttleY);
    }
}