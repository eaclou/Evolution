using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodModule : MonoBehaviour {

    public float amountR;
    public float amountG;
    public float amountB;

	// Use this for initialization
	void Start () {
        amountR = UnityEngine.Random.Range(0f, 1f);
        amountG = UnityEngine.Random.Range(0f, 1f);
        amountB = UnityEngine.Random.Range(0f, 1f);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
