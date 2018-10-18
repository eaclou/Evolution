using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CritterGenomeInterpretor {

	
    public CritterGenomeInterpretor() {
        // empty constructor
    }

    // static helper methods :::: ?

    public static Vector3 GetBindPosFromNormalizedCoords(Vector3 coords, AgentGenome genome) {
        //float tempDist = 1f - Mathf.Clamp01(Mathf.Abs(0.5f - coords.z) * 2f);
        //return tempDist;

        float radius = 1f;

        Vector3 bindPos = coords;

        return bindPos;
    }
    // Read genome, spit out renderBuffer data for different render passes:

    // Read genome, spit out collider object positions, sizes, orientations:

    // Create mutated copy of input Genome:

}
