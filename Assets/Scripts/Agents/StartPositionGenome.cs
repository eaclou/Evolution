using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StartPositionGenome {
    public Vector3 startPosition;
    public Quaternion agentStartRotation;

    public StartPositionGenome(StartPositionGenome templateGenome) {
        startPosition = new Vector3(templateGenome.startPosition.x, templateGenome.startPosition.y, templateGenome.startPosition.z);
        agentStartRotation = new Quaternion(templateGenome.agentStartRotation.x, templateGenome.agentStartRotation.y, templateGenome.agentStartRotation.z, templateGenome.agentStartRotation.w);
    }

    public StartPositionGenome(Vector3 pos, Quaternion rot) {
        startPosition = pos;
        agentStartRotation = rot;
    }

    public void InitializeRandomGenome() {

    }
}
