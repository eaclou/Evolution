using System;

/// Stores all data for a single save game
[Serializable]
public struct SaveData
{
    public AgentData[] agents;
}
