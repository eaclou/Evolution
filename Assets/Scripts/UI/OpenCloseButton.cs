using UnityEngine;
using UnityEngine.UI;

public class OpenCloseButton : MonoBehaviour
{
    public Animator buttonAnimator;
    public Animator panelAnimator;
    public Text text;
    [Tooltip("True: panel will open on start")]
    public bool isOpen;
    private bool neverOpened = true;
    
    const string activatePanel = "PanelOpen";
    const string activateButton = "ON";
    
    void Start() { SetOpen(isOpen); }
    
    public void OpenClose() { SetOpen(!isOpen); }
    
    public void SetOpen(bool value)
    {
        isOpen = value;
        var symbol = isOpen ? ">" : "<";
        text.text = symbol;
        panelAnimator.SetBool(activatePanel, isOpen);  
        if(value) {
            neverOpened = false;
            SetHighlight(false);
        }
        //Debug.Log($"SetOpen {value} {symbol}");      
    }
    
    public void SetMouseEnter(bool value) 
    {
        if (!gameObject.activeInHierarchy) return;
        buttonAnimator.SetBool(activateButton, value);
    }

    public void SetHighlight(bool high) {
        
        buttonAnimator.SetBool("HIGHLIGHT", high);
    }
    
    public void MouseEnter() { SetMouseEnter(true); }
    public void MouseExit() { SetMouseEnter(false); }
}
