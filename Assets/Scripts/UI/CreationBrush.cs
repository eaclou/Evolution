using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreationBrush {

    public string name;
    public float patternColumn = 0f;
    public float patternRow = 0f;

    public int burstEmitDuration;
    public int burstTotalDuration;
    private int burstFrameCounter = 0;
    public bool isBurstActive = false;

    public float baseAmplitude;
    public float baseScale;
    public int numParticles;
    public float emitMinVel;
    public float emitMaxVel;
    public BrushType type;
    public enum BrushType {
        Drag,
        Burst
    }
    // noise, external forces, blah blah blah

    public struct BrushDataStruct {

    }

	public CreationBrush() {

    }

    public void Initialize(int index) {

    }
}
