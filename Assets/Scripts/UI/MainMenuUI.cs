using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] Button buttonQuickStartResume; // Future use
    [SerializeField] Button buttonNewSimulation;
    //[SerializeField] Text textMouseOverInfo;
    //[SerializeField] Text quickStartText;
    [SerializeField] GameObject panelGameOptions;
    
    GameManager gameManager => GameManager.instance;
    Profile profile => gameManager.activeProfile;
        
    bool firstTimeStartup
    {
        get => profile.firstTimeStartup;
        set => profile.firstTimeStartup = value;
    }
    
    bool optionsMenuOn;  // Game options main menu
    bool controlsMenuOn;
    
    // * Replace with persistent profile saving
    void Start()
    {
        firstTimeStartup = true;
    }

    void OnEnable()
    {
        Cursor.visible = true;
        //canvasMain.renderMode = RenderMode.ScreenSpaceOverlay;
        
        //quickStartText.text = firstTimeStartup ? "QUICK START" : "RESUME";
        
        // *** For now, 1 sim at a time ***
        if (!firstTimeStartup) 
        {
            buttonNewSimulation.gameObject.SetActive(false); 
            //textMouseOverInfo.gameObject.SetActive(false);
        }
        
        firstTimeStartup = false; 
        Initialize();
    }
    
    void OnDisable()
    {
        Initialize();
    }
    
    void Initialize()
    {
        optionsMenuOn = false;
        controlsMenuOn = false;
        Refresh();        
    }
    
    public void ClickControlsMenu()
    {
        controlsMenuOn = !controlsMenuOn;
        optionsMenuOn = false;
        Refresh();
    }
    
    public void ClickOptionsMenu() 
    {
        optionsMenuOn = !optionsMenuOn;
        controlsMenuOn = false;
        Refresh();
    }
    
    public void ClickQuickStart() { StartGame(true); }
    
    public void ClickNewSimulation() { StartGame(false); }
    
    void StartGame(bool isQuickStart)
    {
        if (firstTimeStartup) 
        {
            gameManager.StartNewGame(isQuickStart);
        }
        else 
        {
            gameManager.ResumePlaying();
        }          
    }
    /*
    public void MouseEnterQuickStart() {
        //textMouseOverInfo.gameObject.SetActive(true);
        //textMouseOverInfo.text = "Start with an existing ecosystem full of various living organisms.";
    }
    
    public void MouseExitQuickStart() {
        textMouseOverInfo.gameObject.SetActive(false);
    }

    public void MouseEnterNewSimulation() {
        //textMouseOverInfo.gameObject.SetActive(true);
        //textMouseOverInfo.text = "Create a brand new ecosystem from scratch. It might take a significant amount of time for intelligent creatures to evolve.\n*Not recommended for first-time players.";
    }
    
    public void MouseExitNewSimulation() {
        textMouseOverInfo.gameObject.SetActive(false);
    }

    public void MouseEnterControlsButton() {
        if (optionsMenuOn) return;
        textMouseOverInfo.gameObject.SetActive(true);
        textMouseOverInfo.text = "Arrows or WASD for movement, scrollwheel for zoom. 'R' and 'F' tilt Camera.\nKeyboard & Mouse only - Controller support coming soon.";
    }
    
    public void MouseExitControlsButton() {
        textMouseOverInfo.gameObject.SetActive(false);
    }
    */
    public void Refresh() 
    {
        panelGameOptions.SetActive(optionsMenuOn);
    
        if (optionsMenuOn) 
        {
            //textMouseOverInfo.gameObject.SetActive(false);
        }
    }
}
