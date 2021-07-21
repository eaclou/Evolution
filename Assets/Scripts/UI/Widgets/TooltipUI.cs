using UnityEngine;

public class TooltipUI : MonoBehaviour
{
    UIManager ui => UIManager.instance;

    [SerializeField]
    public string tooltipString;
    //public bool isSensorEnabled;

    public void OnHoverStart() {
        ui.observerModeUI.EnterTooltipObject(this);
    }
    
    public void OnHoverExit() {
        ui.observerModeUI.ExitTooltipObject();
    }
}
