using UnityEngine;

public class TooltipUI : MonoBehaviour
{
    UIManager ui => UIManager.instance;

    public string tooltipString;    
    public int elementID;  // * Not used
    
    public void OnHoverStart() {
        ui.EnterTooltipObject(this);
    }
    
    public void OnHoverExit() {
        ui.ExitTooltipObject();
    }
}
