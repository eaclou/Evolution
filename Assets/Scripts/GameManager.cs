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
        // SimulationManager needs to Load First:
        TransitionToGameState(GameState.Loading);        
    }
	
	// Update is called once per frame
	void Update () {
        //simulationManager.UpdateDebugUI();
    }

    private void FixedUpdate() {        

        // Depending on GameState, execute Frame:

        switch(currentGameState) {
            case GameState.MainMenu:
                // Do Nothing... hmm
                break;
            case GameState.Loading:
                // Check on status of Simulation manager, wait for it to be fully loaded and ready to go:                
                if(simulationManager.LoadingCompleteAllSystemsGo) {
                    TransitionToGameState(GameState.Playing);
                }
                else {
                    simulationManager.TickLoading(); // *** change this to one-time call rather than continual!
                }
                break;
            case GameState.Playing:
                //simulationManager.TickSimulation();
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
