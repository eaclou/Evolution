using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] Button buttonQuickStartResume; // Future use
    [SerializeField] Button buttonNewSimulation;
    [SerializeField] Text textMouseOverInfo;  // use!
    [SerializeField] Text quickStartText;
    [SerializeField] GameObject panelGameOptions;
    
    GameManager gameManager => GameManager.instance;
    
        
    bool firstTimeStartup
    {
        get => gameManager.activeProfile.firstTimeStartup;
        set => gameManager.activeProfile.firstTimeStartup = value;
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
        
        if (firstTimeStartup) 
        {
            quickStartText.text = "QUICK START";
        } 
        else 
        {
            quickStartText.text = "RESUME";
            buttonNewSimulation.gameObject.SetActive(false); // *** For now, 1 sim at a time ***
            textMouseOverInfo.gameObject.SetActive(false);
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
    
    public void ClickQuickStart() 
    {
        if(firstTimeStartup) 
        {
            gameManager.StartNewGameQuick();
        }
        else 
        {
            gameManager.ResumePlaying();
            gameManager.SetNormalTime();
        } 
    }
    
    public void ClickNewSimulation() 
    {
        if(firstTimeStartup) 
        {
            gameManager.StartNewGameBlank();
        }
        else 
        {
            gameManager.ResumePlaying();
            gameManager.SetNormalTime();
        }        
    }
    
    #region REFACTOR: delegate to reusable system
    
    public void MouseEnterQuickStart() 
    {
        //textMouseOverInfo.gameObject.SetActive(true);
        //textMouseOverInfo.text = "Start with an existing ecosystem full of various living organisms.";
    }
    
    public void MouseExitQuickStart() 
    {
        textMouseOverInfo.gameObject.SetActive(false);
    }

    public void MouseEnterNewSimulation() 
    {
        //textMouseOverInfo.gameObject.SetActive(true);
        //textMouseOverInfo.text = "Create a brand new ecosystem from scratch. It might take a significant amount of time for intelligent creatures to evolve.\n*Not recommended for first-time players.";
    }
    
    public void MouseExitNewSimulation() 
    {
        textMouseOverInfo.gameObject.SetActive(false);
    }

    public void MouseEnterControlsButton() 
    {
        if (optionsMenuOn) return;
        textMouseOverInfo.gameObject.SetActive(true);
        textMouseOverInfo.text = "Arrows or WASD for movement, scrollwheel for zoom. 'R' and 'F' tilt Camera.\nKeyboard & Mouse only - Controller support coming soon.";
    }
    
    public void MouseExitControlsButton() 
    {
        textMouseOverInfo.gameObject.SetActive(false);
    }
    
    #endregion

    public void Refresh()
    {
        if (optionsMenuOn) 
        {
            panelGameOptions.SetActive(true);
            textMouseOverInfo.gameObject.SetActive(false);
        }
        else 
        {
            panelGameOptions.SetActive(false);
        }
    }
}
