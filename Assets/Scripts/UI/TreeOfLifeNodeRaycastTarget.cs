using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeOfLifeNodeRaycastTarget : MonoBehaviour {

    public SpeciesGenomePool speciesRef;
    //public TreeOfLifeSpeciesNodeData nodeData;
    //public int speciesID;
    //public bool isActive; // extinct or not?

    public CapsuleCollider rayCollider;

    public void Initialize(SpeciesGenomePool speciesRef) {
        this.speciesRef = speciesRef;
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
