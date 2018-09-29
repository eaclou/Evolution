using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeOfLifeNodeRaycastTarget : MonoBehaviour {

    public TreeOfLifeSpeciesNodeData nodeData;
    //public int speciesID;
    //public bool isActive; // extinct or not?

    public CapsuleCollider rayCollider;

    public void Initialize(TreeOfLifeSpeciesNodeData nodeData) {
        this.nodeData = nodeData;
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
