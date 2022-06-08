using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Pond Water/Profile/Achievements List")]
public class AchievementListInfo : ScriptableObject
{
    public AchievementInfo[] defaults;
    public AchievementListData Instantiate() { return new AchievementListData(defaults); }
    
    AchievementData[] achievements => AccessSaveData.instance.data.achievements;
    
    public Action<AchievementData> onUnlock;
    
    public bool SetUnlocked(AchievementId id, bool unlocked = true)
    {
        var achievement = GetAchievement(id);
        if (achievement != null && achievement.unlocked != unlocked)
        {
            achievement.unlocked = unlocked;
            onUnlock?.Invoke(achievement);
            return true;
        }
        return false;
    }
    
    public AchievementData GetAchievement(AchievementId id)
    {
        foreach (var achievement in achievements)
            if (achievement.id == id)
                return achievement;
                
        Debug.LogError($"Achievement {id} not found");
        return null;
    }
}

[Serializable]
public class AchievementListData
{
    public AchievementData[] data;
    
    public AchievementListData(AchievementInfo[] defaults)
    {
        data = new AchievementData[defaults.Length];
        
        for (int i = 0; i < data.Length; i++)
            data[i] = defaults[i].Instantiate();
    }
}


