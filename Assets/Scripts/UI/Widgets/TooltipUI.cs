using UnityEngine;

public class TooltipUI : MonoBehaviour
{
    UIManager ui => UIManager.instance;

    public string tooltipString;    
    public int elementID;
    
    public void OnHoverStart() {
        ui.observerModeUI.EnterTooltipObject(this);
    }
    
    public void OnHoverExit() {
        ui.observerModeUI.ExitTooltipObject();
    }
}
