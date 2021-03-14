using Playcraft;
using UnityEngine;

public enum GameState {
    MainMenu,
    Loading,
    Playing
}

public class GameManager : Singleton<GameManager> {

    public SimulationManager simulationManager;
    public UIManager uiManager;
    public TheRenderKing theRenderKing;
    //public Camera mainCam;
    //private CommandBuffer distortionCommandBuffer;
    //public Material rippleDistortUIMat;

    private GameState currentGameState = GameState.MainMenu;
    public GameState CurrentGameState => currentGameState;
    
    public Profile activeProfile = new Profile();

    private void Awake() {
        //distortionCommandBuffer = new CommandBuffer();
        //distortionCommandBuffer.name = "distortionCommandBuffer";
        //mainCam.AddCommandBuffer(CameraEvent.AfterEverything, distortionCommandBuffer);
    }

    // Use this for initialization ....or don't, it's really up to you.
    void Start () {
        TransitionToGameState(GameState.MainMenu); // better way to do this???
    }

    public void StartNewGameBlank() 
    {
        // Apply Quality Settings from Game Options:
        ApplyQualitySettings();
        simulationManager.isQuickStart = false;
        // SimulationManager needs to Load First:
        TransitionToGameState(GameState.Loading); 
        simulationManager.BeginLoadingNewSimulation();    
    }
    public void StartNewGameQuick() 
    {
        // Apply Quality Settings from Game Options:
        ApplyQualitySettings();
        simulationManager.isQuickStart = true;
        // SimulationManager needs to Load First:
        TransitionToGameState(GameState.Loading);
        simulationManager.BeginLoadingNewSimulation();           
    }

    private void ApplyQualitySettings() {
        // SimulationComplexity:
        switch(uiManager.gameOptionsManager.gameOptions.simulationComplexity) {
            case 0:
                // Low quality
                simulationManager._NumAgents = 40;
                simulationManager._NumEggSacks = 16;
                //simulationManager._NumPredators = 2;
                simulationManager.numInitialHiddenNeurons = 4;
                break;
            case 1:
                // Medium
                simulationManager._NumAgents = 64;
                simulationManager._NumEggSacks = 16;
                simulationManager.numInitialHiddenNeurons = 6;
                break;
            case 2:
                // High
                simulationManager._NumAgents = 96;
                simulationManager._NumEggSacks = 24;
                simulationManager.numInitialHiddenNeurons = 8;
                break;
            case 3:
                // EXTREME
                simulationManager._NumAgents = 128;
                simulationManager._NumEggSacks = 24;
                simulationManager.numInitialHiddenNeurons = 10;
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
                Debug.LogError("No Enum Type Found! (" + currentGameState.ToString() + ")");
                break;
        }
    }

    public void EscapeToMainMenu() {
        TransitionToGameState(GameState.MainMenu);
    }
    public void ResumePlaying() {
        TransitionToGameState(GameState.Playing);
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
                Debug.LogError("No Enum Type Found! (" + nextState.ToString() + ")");
                break;
        }
    }
    
    public void Pause() { SetTimeScale(0f); }
    public void SetNormalTime() { SetTimeScale(1f); }
    public void SetTimeScale(float value) { Time.timeScale = value; }
}
