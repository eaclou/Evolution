using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public SimulationManager simulationManager;
    public UIManager uiManager;
    public TheRenderKing theRenderKing;

    private GameState currentGameState = GameState.MainMenu;
    public GameState CurrentGameState
    {
        get
        {
            return currentGameState;
        }
        set
        {

        }
    }
    public enum GameState {
        MainMenu,
        Loading,
        Playing
    }

	// Use this for initialization ....or don't, it's really up to you.
	void Start () {
        TransitionToGameState(GameState.MainMenu); // better way to do this???
    }

    public void StartNewGame() {
        // Apply Quality Settings from Game Options:
        ApplyQualitySettings();        

        // SimulationManager needs to Load First:
        TransitionToGameState(GameState.Loading);        
    }

    private void ApplyQualitySettings() {
        // SimulationComplexity:
        switch(uiManager.gameOptionsManager.gameOptions.simulationComplexity) {
            case 0:
                // Low quality
                simulationManager._NumAgents = 32;
                simulationManager._NumFood = 32;
                simulationManager._NumPredators = 32;
                simulationManager.numInitialHiddenNeurons = 0;
                break;
            case 1:
                // Medium
                simulationManager._NumAgents = 48;
                simulationManager._NumFood = 48;
                simulationManager._NumPredators = 48;
                simulationManager.numInitialHiddenNeurons = 16;
                break;
            case 2:
                // High
                simulationManager._NumAgents = 64;
                simulationManager._NumFood = 64;
                simulationManager._NumPredators = 64;
                simulationManager.numInitialHiddenNeurons = 40;
                break;
            default:
                Debug.LogError("NO SWITCH ENTRY FOUND!!! " + uiManager.gameOptionsManager.gameOptions.simulationComplexity.ToString());
                break;
        }

        // Fluid Sim Resolution:
        switch(uiManager.gameOptionsManager.gameOptions.fluidPhysicsQuality) {
            case 0:
                // Low quality
                simulationManager.environmentFluidManager.resolution = 128;
                break;
            case 1:
                // Medium
                simulationManager.environmentFluidManager.resolution = 256;
                break;
            case 2:
                // High
                simulationManager.environmentFluidManager.resolution = 512;
                break;
            case 3:
                // Max
                simulationManager.environmentFluidManager.resolution = 768;
                break;
            default:
                Debug.LogError("NO SWITCH ENTRY FOUND!!! " + uiManager.gameOptionsManager.gameOptions.fluidPhysicsQuality.ToString());
                break;
        }
    }
	
	// Update is called once per frame
	void Update () {
        //simulationManager.UpdateDebugUI();
    }

    // $$$$$$ THIS IS THE MAIN CODE THREAD HOOK!!! $$$$$$$$$
    private void FixedUpdate() {        

        // Depending on GameState, execute Frame:

        switch(currentGameState) {
            case GameState.MainMenu:
                // Do Nothing... hmm
                break;
            case GameState.Loading:
                // Check on status of Simulation manager, wait for it to be fully loaded and ready to go:                
                if(simulationManager._LoadingComplete) {
                    if(simulationManager._SimulationWarmUpComplete) {                        
                        TransitionToGameState(GameState.Playing);
                    }
                    else {
                        simulationManager.TickLoading(); // continue Warming Up
                    }
                }
                else {
                    simulationManager.TickLoading(); // *** change this to one-time call rather than continual???
                }
                break;
            case GameState.Playing:
                simulationManager.TickSimulation();
                break;
            default:
                Debug.LogError("No Enum Type Found! (" + currentGameState.ToString() + ")");
                break;
        }


        // OLD CRAP::::
        //float sTime = Time.realtimeSinceStartup;
        /*if(simulationManager.isTrainingSupervised && simulationManager.isGridSearching) {

        }
        else {
            simulationManager.TickSimulation();
        }
        
        if(simulationManager.trainingRequirementsMetSupervised) {
            if(simulationManager.isTrainingSupervised) {                
                for (int i = 0; i < 32; i++) {
                    simulationManager.TickTrainingMode();
                }
            }                      
        }
        */
        //if(Time.realtimeSinceStartup - sTime > 0.05f) {
        //Debug.Log("FixedUpdate SLOW! " + (Time.realtimeSinceStartup - sTime).ToString());
        //}
    }

    private void TransitionToGameState(GameState nextState) {
        Debug.Log("TransitionToGameState(" + nextState.ToString() + ")");
        switch (nextState) {
            case GameState.MainMenu:
                // Can add more safety checks and additional logic later:
                this.currentGameState = nextState;
                uiManager.TransitionToNewGameState(nextState);
                break;
            case GameState.Loading:
                // Can add more safety checks and additional logic later:
                this.currentGameState = nextState;
                uiManager.TransitionToNewGameState(nextState);
                break;
            case GameState.Playing:
                // Can add more safety checks and additional logic later:
                this.currentGameState = nextState;
                uiManager.TransitionToNewGameState(nextState);
                break;
            default:
                Debug.LogError("No Enum Type Found! (" + nextState.ToString() + ")");
                break;
        }
    }
}
