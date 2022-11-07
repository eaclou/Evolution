using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] Button buttonQuickStartResume; // Future use
    [SerializeField] Button buttonNewSimulation;
    //[SerializeField] Text textMouseOverInfo;
    //[SerializeField] Text quickStartText;
    //[SerializeField] GameObject panelGameOptions;
    [SerializeField] Animator mainMenuAnimator;
    
    GameManager gameManager => GameManager.instance;
    //Profile profile => gameManager.activeProfile;
        
    bool firstTimeStartup
    {
        get => gameManager.firstTimeStartup;
        set => gameManager.firstTimeStartup = value;
    }
    
    //bool optionsMenuOn;  // Game options main menu
    //bool controlsMenuOn;
    
    // * Replace with persistent profile saving
    void Awake()
    {
        //firstTimeStartup = true;
        //firstTimeStartup = true;
    }

    void OnEnable()
    {
        

        Cursor.visible = true;
        //canvasMain.renderMode = RenderMode.ScreenSpaceOverlay;        
        //quickStartText.text = firstTimeStartup ? "QUICK START" : "RESUME";        
        // *** For now, 1 sim at a time ***
        if (firstTimeStartup) 
        {
            //buttonNewSimulation.gameObject.SetActive(false); 
            //textMouseOverInfo.gameObject.SetActive(false);
            AudioManager.instance.PlayMainMenuMusic();
        }
        
        //firstTimeStartup = false; 
        //Initialize();
    }
    
    
   
    private void ClearAnimatorParameters() {
        mainMenuAnimator.SetBool("CreditsON", false);
        mainMenuAnimator.SetBool("SettingsON", false);
        mainMenuAnimator.SetBool("FeedbackON", false);
        mainMenuAnimator.SetBool("NewON", false);
        mainMenuAnimator.SetBool("SaveON", false);
        mainMenuAnimator.SetBool("LoadON", false);
    }

    public void ClickCreditsButton()
    {
        ClearAnimatorParameters();
        mainMenuAnimator.SetBool("CreditsON", true);
    }
    
    public void ClickFeedbackButton()
    {
        ClearAnimatorParameters();
        mainMenuAnimator.SetBool("FeedbackON", true);
    }
    
    public void ClickNewButton()
    {
        ClearAnimatorParameters();
        mainMenuAnimator.SetBool("NewON", true);        
    }
    
    public void ClickSaveButton()
    {
        ClearAnimatorParameters();
        mainMenuAnimator.SetBool("SaveON", true);        
    }
    
    public void ClickLoadButton()
    {
        ClearAnimatorParameters();
        mainMenuAnimator.SetBool("LoadON", true);        
    }

    public void ClickOptionsMenu() 
    {
        //optionsMenuOn = !optionsMenuOn;
        //controlsMenuOn = false;

        ClearAnimatorParameters();
        mainMenuAnimator.SetBool("SettingsON", true);
        //Refresh();
    }
    
    public void ClickQuickStart() { StartGame(true); }
        
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
        UIManager.instance.ExitTooltipObject();
    }
    
}
