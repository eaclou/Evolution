using Playcraft;
using UnityEngine;

public enum GameState {
    MainMenu,
    Loading,
    Playing
}

public class GameManager : Singleton<GameManager> {
    SimulationManager simulationManager => SimulationManager.instance;
    UIManager uiManager => UIManager.instance;
    
    [SerializeField] SetTimeScale timeControl;

    private GameState currentGameState = GameState.MainMenu;
    public GameState CurrentGameState => currentGameState;
    
    public Profile activeProfile = new Profile();

    private void Awake() {
        //distortionCommandBuffer = new CommandBuffer();
        //distortionCommandBuffer.name = "distortionCommandBuffer";
        //mainCam.AddCommandBuffer(CameraEvent.AfterEverything, distortionCommandBuffer);
    }

    void Start () {
        TransitionToGameState(GameState.MainMenu); // better way to do this???
    }

    public void StartNewGame(bool isQuickStart) 
    {
        // Apply Quality Settings from Game Options:
        simulationManager.ApplyQualitySettings();
        simulationManager.isQuickStart = isQuickStart;
        
        // SimulationManager needs to Load First:
        TransitionToGameState(GameState.Loading); 
        simulationManager.BeginLoadingNewSimulation();    
    }
    	
	/*
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
    */

    // $$$$$$ THIS IS THE MAIN CODE THREAD HOOK!!! $$$$$$$$$
    private void FixedUpdate() {        
        // Depending on GameState, execute Frame:

        switch(currentGameState) 
        {
            case GameState.MainMenu:
                // Do Nothing... hmm
                // Test ripple distortion ui shader
                break;
            case GameState.Loading:
                // Check on status of Simulation manager, wait for it to be fully loaded and ready to go:                
                if(simulationManager.loadingComplete && simulationManager._BigBangOn) 
                {
                    TransitionToGameState(GameState.Playing);
                }
                break;
            case GameState.Playing:
                simulationManager.TickSimulation();
                break;
            default:
                Debug.LogError("No Enum Type Found! (" + currentGameState + ")");
                break;
        }
    }

    public void EscapeToMainMenu() {
        TransitionToGameState(GameState.MainMenu);
    }
    
    public void ResumePlaying() {
        TransitionToGameState(GameState.Playing);
        timeControl.SetPaused(false);
    }
    
    private void TransitionToGameState(GameState nextState) {
        //Debug.Log("TransitionToGameState(" + nextState.ToString() + ")");
        switch (nextState) {
            case GameState.MainMenu:
                // Can add more safety checks and additional logic later:
                currentGameState = nextState;
                uiManager.TransitionToNewGameState(nextState);
                break;
            case GameState.Loading:
                // Can add more safety checks and additional logic later:
                currentGameState = nextState;
                
                // temp remove UI commandBuffer **********
                //mainCam.RemoveAllCommandBuffers();

                uiManager.TransitionToNewGameState(nextState);
                break;
            case GameState.Playing:
                // Can add more safety checks and additional logic later:
                currentGameState = nextState;
                
                uiManager.TransitionToNewGameState(nextState);
                break;
            default:
                Debug.LogError("No Enum Type Found! (" + nextState + ")");
                break;
        }
    }
}