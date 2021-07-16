using System;
using UnityEngine;

// Tracks whether cursor is in the bounds of a rect transform, accounting for pivot, size, and position.
// Because this system does not rely on Ray-casting it is not affected by overlapping UI elements
[Serializable] 
public class CursorInRectBounds
{
    [SerializeField] RectTransform area;
    
    public bool inBounds => inXBounds && inYBounds;

    float width => area.rect.width;
    float height => area.rect.height;
    
    float xOffset => width * (1 - area.pivot.x);
    float xLeft => area.position.x + xOffset - width;
    float xRight => area.position.x + xOffset;
    bool inXBounds => Input.mousePosition.x >= xLeft && Input.mousePosition.x <= xRight;
    
    float yOffset => width * (1 - area.pivot.y);
    float yBottom => area.position.y + yOffset - height;
    float yTop => area.position.y + yOffset;
    bool inYBounds => Input.mousePosition.y >= yBottom && Input.mousePosition.x <= yTop;
}
