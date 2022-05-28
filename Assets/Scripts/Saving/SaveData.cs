using System;

/// Stores all data for a single save game
[Serializable] [ES3Serializable]
public class SaveData
{
    Lookup lookup => Lookup.instance;

    public AgentData[] agents;
    public AchievementData[] achievements;
    
    public SaveData() 
    {
        if (!lookup) return;
        achievements = lookup.achievements.Instantiate().data; 
    }
    
    public SaveData(SaveData original)
    {
        agents = original.agents;
        achievements = original.achievements;
    }
}
