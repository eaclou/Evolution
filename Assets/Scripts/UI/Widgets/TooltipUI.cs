using UnityEngine;

public class TooltipUI : MonoBehaviour
{
    UIManager ui => UIManager.instance;

    public string tooltipString;    
    // sprite??? (icon)
    
    public void OnHoverStart() {
        ui.EnterTooltipObject(this);
    }
    
    public void OnHoverExit() {
        ui.ExitTooltipObject();
    }
}
