using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StartPositionGenome {
    public Vector3 agentStartPosition;
    public Quaternion agentStartRotation;

    public StartPositionGenome(StartPositionGenome templateGenome) {
        agentStartPosition = new Vector3(templateGenome.agentStartPosition.x, templateGenome.agentStartPosition.y, templateGenome.agentStartPosition.z);
        agentStartRotation = new Quaternion(templateGenome.agentStartRotation.x, templateGenome.agentStartRotation.y, templateGenome.agentStartRotation.z, templateGenome.agentStartRotation.w);
    }

    public StartPositionGenome(Vector3 pos, Quaternion rot) {
        agentStartPosition = pos;
        agentStartRotation = rot;
    }

    public void InitializeRandomGenome() {

    }
}
