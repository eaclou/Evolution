using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnZone : MonoBehaviour {

    public float radius = 10f;

    public bool active = true;
    public int refactoryCounter = 0;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void FixedUpdate() {
        if(!active) {
            refactoryCounter++;
        }
        else {
            float randRoll = UnityEngine.Random.Range(0f, 1f);
            if(randRoll < 0.002f) {
                active = false;
                refactoryCounter = 0;
            }
        }        
    }
}
