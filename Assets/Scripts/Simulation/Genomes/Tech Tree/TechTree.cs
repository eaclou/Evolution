using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Pond Water/Tech Tree/Tree")]
public class TechTree : ScriptableObject
{
    public TechElement[] elements;  // * Not used
    public TechCategoryData[] categories;
    
    public bool HasPrerequisites(TechElement[] abilities, TechElement requestedAbility)
    {
        return requestedAbility.HasPrerequisites(abilities);
    }
    
    public Color CategoryColor(TechElement element)
    {
        if (!element) 
        {
            //Debug.LogError($"Cannot find category color for null TechElement");
            return Color.gray;
        }
    
        foreach (var category in categories)
        {
            if (category.category == element.category)
                return category.color;
        }
                
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