using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeOfLifeStemSegmentData {

    public int fromSpeciesID;
    public int toSpeciesID;
	
    public TreeOfLifeStemSegmentData(int fromSpeciesNodeID, int toSpeciesNodeID) {

        this.fromSpeciesID = fromSpeciesNodeID;
        this.toSpeciesID = toSpeciesNodeID;
    }
}
