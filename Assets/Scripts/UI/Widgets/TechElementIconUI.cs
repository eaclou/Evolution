using UnityEngine;
using UnityEngine.UI;

public class TechElementIconUI : MonoBehaviour
{
    public TechElement techElement;
    public Image image;
    public TooltipUI tooltip;
    
    public Color color { set => image.color = value; }
    public Vector3 scale { set => transform.localScale = value; }
    
    public void OnHoverStart() 
    {
        tooltip.tooltipString = techElement.iconTooltip;
        // * Add text for current neural values
        tooltip.OnHoverStart();
    }
    
    public void OnHoverExit() { tooltip.OnHoverExit(); }
}
