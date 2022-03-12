using System;
using UnityEngine;

// * Not clear if this is needed, consider removal
[CreateAssetMenu(menuName = "Pond Water/Tech Tree/Tree")]
public class TechTree : ScriptableObject
{
    public TechElement[] elements;
    public TechCategoryData[] categories;
    
    public bool HasPrerequisites(TechElement[] abilities, TechElement requestedAbility)
    {
        return requestedAbility.HasPrerequisites(abilities);
    }
    
    public Color CategoryColor(TechElement element)
    {
        foreach (var category in categories)
            if (category.category == element.category)
                return category.color;
                
        return Color.gray;
    }
}

public enum TechCategory 
{ 
    Social, 
    FoodSensors, 
    SelfStatus, 
    Swim, 
    Environment, 
    Predation,
    Root
}

[Serializable]
public struct TechCategoryData
{
    public TechCategory category;
    public Color color;
}