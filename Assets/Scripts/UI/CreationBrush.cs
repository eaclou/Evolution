using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreationBrush {

    public string name;
    public float baseAmplitude;
    public float baseRadius;
    public int numParticles;
    public float emitMinVel;
    public float emitMaxVel;
    public enum BrushType {
        Continuous,
        Burst
    }
    // noise, external forces, blah blah blah

	public CreationBrush() {

    }
}
