using UnityEngine;
using UnityEngine.UI;

public class TechElementIconUI : MonoBehaviour
{
    SelectionManager selection => SelectionManager.instance;

    public TechElement techElement;
    public Image image;
    public TooltipUI tooltip;
    [SerializeField] float refreshRate = 0.1f;
    
    public Color color { set => image.color = value; }
    public Vector3 scale { set => transform.localScale = value; }
    
    public void OnHoverStart() 
    {
        InvokeRepeating(nameof(Refresh), 0f, refreshRate);
        tooltip.OnHoverStart();
    }
    
    public void OnHoverExit() 
    {
        CancelInvoke(nameof(Refresh)); 
        tooltip.OnHoverExit();
    }
    
    void Refresh()
    {
        tooltip.tooltipString = techElement.iconTooltip + selection.SelectedTechValue(techElement);
    }
}
