using System;
using UnityEngine;

[Serializable]
public class StartPositionGenome {
    public Vector3 startPosition;
    public Quaternion agentStartRotation;

    /// Copy from an existing template
    public StartPositionGenome(StartPositionGenome templateGenome) {
        startPosition = new Vector3(templateGenome.startPosition.x, templateGenome.startPosition.y, templateGenome.startPosition.z);
        agentStartRotation = new Quaternion(templateGenome.agentStartRotation.x, templateGenome.agentStartRotation.y, templateGenome.agentStartRotation.z, templateGenome.agentStartRotation.w);
    }

    /// Create from raw data
    public StartPositionGenome(Vector3 pos, Quaternion rot) {
        startPosition = pos;
        agentStartRotation = rot;
    }

    /// WPP: Not used, remove
    public void InitializeRandomGenome() { }
}
