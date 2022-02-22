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
}

public enum TechCategory 
{ 
    SpinalCord, 
    Heterotrophic, 
    SensoryGanglia, 
    LimbDifferentiation, 
    NerveCells, 
    Lysosomes,
    Root
}

[Serializable]
public struct TechCategoryData
{
    public TechCategory category;
    public Color color;
}