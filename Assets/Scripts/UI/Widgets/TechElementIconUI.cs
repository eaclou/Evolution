﻿using System;
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
        tooltip.tooltipString = techElement.iconTooltip + "\n" + selection.SelectedTechValue(techElement);
        
    }
    
    public void SetActive(bool hasTech)
    {
        this.hasTech = hasTech;
        color = hasTech ? data.activeColor : data.inactiveColor;
        scale = Vector3.one * (hasTech ? data.activeScale : data.inactiveScale);
        gameObject.SetActive(hasTech);
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

