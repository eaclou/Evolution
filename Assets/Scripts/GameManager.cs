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
        float sTime = Time.realtimeSinceStartup;

        simulationManager.TickSimulation();
        if(simulationManager.trainingRequirementsMetSupervised) {
            if(simulationManager.isTrainingSupervised) {
                for (int i = 0; i < 32; i++) {
                    simulationManager.TickTrainingMode();
                }
            }                      
        }

        //if(Time.realtimeSinceStartup - sTime > 0.05f) {
        //Debug.Log("FixedUpdate SLOW! " + (Time.realtimeSinceStartup - sTime).ToString());
        //}
    }


}
