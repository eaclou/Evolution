using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRandomizer : MonoBehaviour {

    public Mesh[] meshPool;
    
	// Use this for initialization
	void Start () {
        this.transform.rotation = UnityEngine.Random.rotation;

        if(meshPool != null) {
            Mesh randMesh = meshPool[UnityEngine.Random.Range(0, meshPool.Length)];
            this.GetComponent<MeshFilter>().sharedMesh = randMesh;
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
