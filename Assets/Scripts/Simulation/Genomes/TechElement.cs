using UnityEngine;

[CreateAssetMenu(menuName = "Pond Water/Tech Tree/Element")]
public class TechElement : ScriptableObject
{
    // * Not sure if this is needed
    public TechCategory category;
    
    public TechElement[] prerequisites;
    
    public bool HasPrerequisites(TechElement[] abilities)
    {
        foreach (var prerequisite in prerequisites)
            if (!HasPrerequisite(abilities, prerequisite))
                return false;
                
        return true;
    }
    
    bool HasPrerequisite(TechElement[] abilities, TechElement prerequisite)
    {
        foreach (var ability in abilities)
            if (ability == prerequisite)
                return true;
                
        return false;
    }
}