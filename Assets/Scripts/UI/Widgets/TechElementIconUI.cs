using System;
using UnityEngine;
using UnityEngine.UI;

public class TechElementIconUI : MonoBehaviour
{
    SelectionManager selection => SelectionManager.instance;

    public TechElement techElement;
    public Image image;
    public TooltipUI tooltip;
    [SerializeField] float refreshRate = 0.1f;
    [ReadOnly] public TechElementIconData data;
    [ReadOnly] [SerializeField] bool hasTech;
    private bool isHover = false;
    
    public Color color { set => image.color = value; }
    public Vector3 scale { set => transform.localScale = value; }
    
    public void OnHoverStart() 
    {
        InvokeRepeating(nameof(Refresh), 0f, refreshRate);
        tooltip.OnHoverStart();
        isHover = true;
    }
    
    public void OnHoverExit() 
    {
        CancelInvoke(nameof(Refresh)); 
        tooltip.OnHoverExit();
        isHover = false;
    }
    
    void Refresh()
    {
        tooltip.tooltipString = techElement.iconTooltip + "\n" + selection.SelectedTechValue(techElement);
        //scale = Vector3.one * 1.25f;
    }
    
    public void SetActive(bool hasTech)
    {
        this.hasTech = hasTech;
        color = hasTech ? data.activeColor : data.inactiveColor;
        scale = Vector3.one * (hasTech ? data.activeScale : data.inactiveScale);
        gameObject.SetActive(hasTech);
        if(selection.currentSelection.agent == null) {
            return;
        }
        bool isCurAction = false;
        if(this.techElement.id == TechElementId.Predation && selection.currentSelection.agent.curActionState == AgentActionState.Feeding) {
            isCurAction = true; }
        if(this.techElement.id == TechElementId.Swim && selection.currentSelection.agent.curActionState == AgentActionState.Default) {
            isCurAction = true; }
        if(this.techElement.id == TechElementId.Attack && selection.currentSelection.agent.curActionState == AgentActionState.Attacking) {
            isCurAction = true; }
        if(this.techElement.id == TechElementId.Dash && selection.currentSelection.agent.curActionState == AgentActionState.Dashing) {
            isCurAction = true; }
        if(this.techElement.id == TechElementId.Protect && selection.currentSelection.agent.curActionState == AgentActionState.Defending) {
            isCurAction = true; }
        if(this.techElement.id == TechElementId.Sleep && selection.currentSelection.agent.curActionState == AgentActionState.Resting) {
            isCurAction = true; }

        if(isCurAction && selection.currentSelection.agent.curLifeStage == AgentLifeStage.Mature && !selection.currentSelection.isGenomeOnly) {
            color = Color.white;
            scale = Vector3.one;
        }

        if(isHover) {
            //color = Color.white;
            scale = Vector3.one * 1.25f;
        }
    }
}

[Serializable]
public struct TechElementIconData
{
    public Color activeColor;
    public Color inactiveColor;
    public float activeScale;
    public float inactiveScale;
    
    public TechElementIconData(TechElementIconData original, Color activeColor)
    {
        this.activeColor = activeColor;
        inactiveColor = original.inactiveColor;
        activeScale = original.activeScale;
        inactiveScale = original.inactiveScale;
    }
}

