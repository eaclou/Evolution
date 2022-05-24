﻿using System;

/// Stores all data for a single save game
[Serializable] [ES3Serializable]
public class SaveData
{
    public AgentData[] agents;
    public AchievementData[] achievements;
    
    public SaveData() { }
    
    public SaveData(SaveData original)
    {
        agents = original.agents;
        achievements = original.achievements;
    }
}
