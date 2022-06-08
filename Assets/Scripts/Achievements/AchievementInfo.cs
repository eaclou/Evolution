using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Pond Water/Profile/Achievement")]
public class AchievementInfo : ScriptableObject
{
    public AchievementData data;
    public AchievementData Instantiate() { return new AchievementData(this); }
}

[Serializable]
public class AchievementData
{
    // Static data
    public AchievementId id;
    public string title;
    public string description;
    //public Sprite icon;
    
    // Mutable data
    [ReadOnly] public bool unlocked;
    
    public AchievementData() { }
    
    public AchievementData(AchievementInfo template)
    {
        id = template.data.id;
        title = template.data.title;
        description = template.data.description;
        //icon = template.data.icon;
        
        unlocked = false;
    }
}

public enum AchievementId
{
    Test1,
    Test2,
    Test3,
    NewSpecies,
}


