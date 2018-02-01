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
        simulationManager.UpdateDebugUI();

    }

    private void FixedUpdate() {
        simulationManager.TickSimulation();
        if(simulationManager.trainingRequirementsMet) {
            if(simulationManager.isTraining) {
                for (int i = 0; i < 64; i++) {
                    simulationManager.TickTrainingMode();
                }
            }                      
        }
    }


}
