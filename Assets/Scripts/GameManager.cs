using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GameManager : MonoBehaviour {

    public SimulationManager simulationManager;
    public UIManager uiManager;
    public TheRenderKing theRenderKing;
    //public Camera mainCam;
    //private CommandBuffer distortionCommandBuffer;
    //public Material rippleDistortUIMat;

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

    private void Awake() {
        //distortionCommandBuffer = new CommandBuffer();
        //distortionCommandBuffer.name = "distortionCommandBuffer";
        //mainCam.AddCommandBuffer(CameraEvent.AfterEverything, distortionCommandBuffer);
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
                simulationManager._NumAgents = 40;
                simulationManager._NumFood = 40;
                simulationManager._NumPredators = 2;
                simulationManager.numInitialHiddenNeurons = 4;
                break;
            case 1:
                // Medium
                simulationManager._NumAgents = 64;
                simulationManager._NumFood = 48;
                simulationManager._NumPredators = 2;
                simulationManager.numInitialHiddenNeurons = 6;
                break;
            case 2:
                // High
                simulationManager._NumAgents = 96;
                simulationManager._NumFood = 48;
                simulationManager._NumPredators = 1;
                simulationManager.numInitialHiddenNeurons = 20;
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

        // *** TEMP TEST RIPPLE DISTORTION!! ***
        //distortionCommandBuffer.Clear();

        // Create RenderTargets:
        //int renderedSceneID = Shader.PropertyToID("_RenderedSceneID");
       // distortionCommandBuffer.GetTemporaryRT(renderedSceneID, -1, -1, 0, FilterMode.Bilinear);  // save contents of Standard Rendering Pipeline
        //distortionCommandBuffer.Blit(BuiltinRenderTextureType.CameraTarget, renderedSceneID);  // save contents of Standard Rendering Pipeline        
        //distortionCommandBuffer.Blit(renderedSceneID, BuiltinRenderTextureType.CameraTarget, rippleDistortUIMat);  // apply ripple shit
        
    }

    // $$$$$$ THIS IS THE MAIN CODE THREAD HOOK!!! $$$$$$$$$
    private void FixedUpdate() {        

        // Depending on GameState, execute Frame:

        switch(currentGameState) {
            case GameState.MainMenu:
                // Do Nothing... hmm
                // Test ripple distortion ui shader


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

    public void EscapeToMainMenu() {
        TransitionToGameState(GameState.MainMenu);
    }
    public void ResumePlaying() {
        TransitionToGameState(GameState.Playing);
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
                
                // temp remove UI commandBuffer **********
                //mainCam.RemoveAllCommandBuffers();

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
