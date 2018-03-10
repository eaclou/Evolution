using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

    public GameManager gameManager;
    //public SimulationManager simManager;
    public CameraManager cameraManager;

    public Texture2D healthDisplayTex;

    public GameObject panelTint;

    public GameObject panelHUD;
    public Image imageFood;
    public Image imageHitPoints;
    public Material foodMat;
    public Material hitPointsMat;
    public Text textScore;

    public GameObject panelDebug;
    public Material fitnessDisplayMat;
    public Button buttonPause;
    public Button buttonPlaySlow;
    public Button buttonPlayNormal;
    public Button buttonPlayFast;
    public Button buttonModeA;
    public Button buttonModeB;
    public Button buttonModeC;

    public Text textDebugTrainingInfo;
    public Button buttonToggleRecording;
    public Button buttonToggleTrainingSupervised;
    public Button buttonResetGenomes;
    public Button buttonClearTrainingData;
    public Button buttonToggleTrainingPersistent;

    // TOP PANEL::::
    public GameObject panelTop;
    public Button buttonToggleHUD;
    public bool isActiveHUD = true;
    public Button buttonToggleDebug;
    public bool isActiveDebug = false;

    public GameObject panelMainMenu;    
    public GameObject panelLoading;
    public GameObject panelPlaying;

    public Texture2D fitnessDisplayTexture;

    
	// Use this for initialization
	void Start () {
        
        
    }
	
	// Update is called once per frame
	void Update () {
        switch (gameManager.CurrentGameState) {
            case GameManager.GameState.MainMenu:
                UpdateMainMenuUI();
                break;
            case GameManager.GameState.Loading:
                UpdateLoadingUI();
                break;
            case GameManager.GameState.Playing:
                UpdateSimulationUI();
                break;
            default:
                Debug.LogError("No Enum Type Found! (" + gameManager.CurrentGameState.ToString() + ")");
                break;
        }
    }

    public void TransitionToNewGameState(GameManager.GameState gameState) {
        switch (gameState) {
            case GameManager.GameState.MainMenu:
                EnterMainMenuUI();
                break;
            case GameManager.GameState.Loading:
                EnterLoadingUI();
                break;
            case GameManager.GameState.Playing:
                EnterPlayingUI();
                break;
            default:
                Debug.LogError("No Enum Type Found! (" + gameState.ToString() + ")");
                break;
        }
    }
    private void EnterMainMenuUI() {
        panelMainMenu.SetActive(true);
        panelLoading.SetActive(false);
        panelPlaying.SetActive(false);

        panelHUD.SetActive(isActiveHUD);
        panelDebug.SetActive(isActiveDebug);

        fitnessDisplayTexture = new Texture2D(1, 1, TextureFormat.RGBAFloat, true);
        fitnessDisplayTexture.wrapMode = TextureWrapMode.Clamp;
        fitnessDisplayMat.SetTexture("_MainTex", fitnessDisplayTexture);

        ClickButtonModeC();
    }
    private void EnterLoadingUI() {
        panelMainMenu.SetActive(false);
        panelLoading.SetActive(true);
        panelPlaying.SetActive(false);
    }
    private void EnterPlayingUI() {
        panelMainMenu.SetActive(false);
        panelLoading.SetActive(false);
        panelPlaying.SetActive(true);
    }
    private void UpdateMainMenuUI() {
        
    }
    private void UpdateLoadingUI() {

    }
    private void UpdateSimulationUI() {
        //UpdateScoreText(int score);
        //SetDisplayTextures();
    }

    public void UpdateDebugUI() {

        // DISABLED!!!! -- Need to establish good method for grabbing data from SimulationManager!

        /*string debugTxt = "Training: False";
        if (trainingRequirementsMetSupervised) {
            debugTxt = "Training: ACTIVE   numSamples: " + dataSamplesList.Count.ToString() + "\n";
            debugTxt += "Gen: " + curGen.ToString() + ", Agent: " + curTestingGenomeSupervised.ToString() + ", Sample: " + curTestingSample.ToString() + "\n";
            debugTxt += "Fitness Best: " + bestFitnessScoreSupervised.ToString() + " ( Avg: " + avgFitnessLastGenSupervised.ToString() + " ) Blank: " + lastGenBlankAgentFitnessSupervised.ToString() + "\n";
            debugTxt += "Agent[0] # Neurons: " + agentsArray[0].brain.neuronList.Count.ToString() + ", # Axons: " + agentsArray[0].brain.axonList.Count.ToString() + "\n";
            debugTxt += "CurOldestAge: " + currentOldestAgent.ToString() + ", numChildrenBorn: " + numPersistentAgentsBorn.ToString() + ", ~Gen: " + ((float)numPersistentAgentsBorn / (float)numAgents).ToString();
            debugTxt += "\nBotRecordAge: " + recordBotAge.ToString() + ", PlayerRecordAge: " + recordPlayerAge.ToString();
            debugTxt += "\nAverageAgentScore: " + rollingAverageAgentScore.ToString();
        }
        textDebugTrainingInfo.text = debugTxt;

        if (recording) {
            ColorBlock colorBlock = buttonToggleRecording.colors;
            colorBlock.normalColor = Color.red;
            colorBlock.highlightedColor = Color.red;
            buttonToggleRecording.colors = colorBlock;

            buttonToggleRecording.GetComponentInChildren<Text>().color = Color.white;
            buttonToggleRecording.GetComponentInChildren<Text>().text = "RECORDING";
        }
        else {
            ColorBlock colorBlock = buttonToggleRecording.colors;
            colorBlock.normalColor = Color.white;
            colorBlock.highlightedColor = Color.white;
            buttonToggleRecording.colors = colorBlock;

            buttonToggleRecording.GetComponentInChildren<Text>().color = Color.black;
            buttonToggleRecording.GetComponentInChildren<Text>().text = "OFF";
        }

        if (isTrainingSupervised) {
            buttonToggleTrainingSupervised.GetComponentInChildren<Text>().text = "Supervised\nTraining: ON";
        }
        else {
            buttonToggleTrainingSupervised.GetComponentInChildren<Text>().text = "Supervised\nTraining: OFF";
        }

        if (isTrainingPersistent) {
            buttonToggleTrainingPersistent.GetComponentInChildren<Text>().text = "Persistent\nTraining: ON";
        }
        else {
            buttonToggleTrainingPersistent.GetComponentInChildren<Text>().text = "Persistent\nTraining: OFF";
        }*/
    }

    public void UpdateScoreText(int score) {
        //textScore.text = "Score: " + score.ToString();
    }

    public void SetDisplayTextures() {
        //foodMat.SetTexture("_MainTex", healthDisplayTex);
        //hitPointsMat.SetTexture("_MainTex", healthDisplayTex);
    }

    public void ClickStartGame() {
        gameManager.StartNewGame();
    }

    public void ClickButtonQuit() {
        Application.Quit();
    }

    public void ClickButtonPause() {
        Time.timeScale = 0f;
    }
    public void ClickButtonPlaySlow() {
        Time.timeScale = 0.4f;
    }
    public void ClickButtonPlayNormal() {
        Time.timeScale = 1f;
    }
    public void ClickButtonPlayFast() {
        Time.timeScale = 2.5f;
    }
    public void ClickButtonModeA() {
        cameraManager.ChangeGameMode(CameraManager.GameMode.ModeA);
    }
    public void ClickButtonModeB() {
        cameraManager.ChangeGameMode(CameraManager.GameMode.ModeB);
    }
    public void ClickButtonModeC() {
        cameraManager.ChangeGameMode(CameraManager.GameMode.ModeC);
    }

    public void ClickButtonToggleHUD() {
        isActiveHUD = !isActiveHUD;
        panelHUD.SetActive(isActiveHUD);
    }
    public void ClickButtonToggleDebug() {
        isActiveDebug = !isActiveDebug;
        panelDebug.SetActive(isActiveDebug);
    }


}
