using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public SimulationManager simulationManager;

	// Use this for initialization
	void Start () {
        InitializeGame();

    }

    public void InitializeGame() {
        //simulationManager = new SimulationManager();
        simulationManager.InitializeNewSimulation();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void FixedUpdate() {
        simulationManager.TickSimulation();
    }


}
