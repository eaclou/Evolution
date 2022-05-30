using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Pond Water/Profile/Achievement")]
public class AchievementInfo : ScriptableObject
{
    public AchievementData data;
    
    void OnValidate() { data.id = this; }
    
    public AchievementData Instantiate() { return new AchievementData(this); }
}

[Serializable] [ES3Serializable]
public class AchievementData
{
    // Static data
    public AchievementInfo id;
    public string title;
    public string description;
    public Sprite icon;
    
    // Mutable data
    [ReadOnly] public bool unlocked;
    
    public AchievementData() { }
    
    public AchievementData(AchievementInfo template)
    {
        id = template;
        title = template.data.title;
        description = template.data.description;
        icon = template.data.icon;
        
        unlocked = false;
    }
}


