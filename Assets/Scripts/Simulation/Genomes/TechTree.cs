using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Pond Water/Tech Tree/Tree")]
public class TechTree : ScriptableObject
{
    // * Not sure if these are needed
    public TechElement[] elements;
    public TechCategoryData[] categories;
    
    public bool HasPrerequisites(TechElement[] abilities, TechElement requestedAbility)
    {
        return requestedAbility.HasPrerequisites(abilities);
    }
}

public enum TechCategory 
{ 
    SpinalCord, 
    Heterotrophic, 
    SensoryGanglia, 
    LimbDifferentiation, 
    NerveCells, 
    Lysosomes 
}

[Serializable]
public struct TechCategoryData
{
    public TechCategory category;
    public Color color;
}