using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeOfLifeSpeciesNodeData {

    public SpeciesGenomePool speciesPool;   
	//public int speciesID;
    //public bool isActive; // extinct or not?

    public TreeOfLifeSpeciesNodeData(SpeciesGenomePool speciesPool) {
        this.speciesPool = speciesPool;
        //speciesID = speciesPool.speciesID;
        //isActive = !speciesPool.isExtinct;
    }
}
