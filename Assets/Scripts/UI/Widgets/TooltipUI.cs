using UnityEngine;

public class TooltipUI : MonoBehaviour
{
    UIManager ui => UIManager.instance;

    [SerializeField]
    public string tooltipString;    
    [SerializeField]
    public int elementID;
    
    public void OnHoverStart() {
        ui.observerModeUI.EnterTooltipObject(this);
    }
    
    public void OnHoverExit() {
        ui.observerModeUI.ExitTooltipObject();
    }
}
