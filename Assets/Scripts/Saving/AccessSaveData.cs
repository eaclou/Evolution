using UnityEngine;
using Playcraft;
using System.IO;
using Newtonsoft.Json;

/// Get and set data for the currently loaded save
public class AccessSaveData : Singleton<AccessSaveData>
{
    SimulationManager simulation => SimulationManager.instance;
    
    [ReadOnly] public SaveData data;
    
    public int saveIndex = 1;
    string saveName => $"SaveGame{saveIndex}";
    string saveFilePath => $"{Application.persistentDataPath}/{saveName}.json";
    
    public void Initialize()
    {
        data = new SaveData();
    }

    public void Save()
    {
        GatherAgentData();
        var json = JsonConvert.SerializeObject(data);
        File.WriteAllText(saveFilePath, json);
    }

    public void Load()
    {
        var json = File.ReadAllText(saveFilePath);
        data = JsonConvert.DeserializeObject<SaveData>(json);
        
        DistributeAgentData();
    }
    
    /// Transfer agent data from simulation to profile
    void GatherAgentData()
    {
        data.agents = new AgentData[simulation.agents.Length];
        for (int i = 0; i < data.agents.Length; i++)
        {
            data.agents[i] = simulation.agents[i].data;
            data.agents[i].position = simulation.agents[i].position;
        }        
    }
    
    /// Transfer agent data from profile to simulation
    void DistributeAgentData()
    {
        if (data == null)
        {
            Debug.LogError($"Data for {saveName} not found.");
            return;
        }
        
        for (int i = 0; i < data.agents.Length && i < simulation.agents.Length; i++)
        {
            simulation.agents[i].data = data.agents[i];
            simulation.agents[i].position = data.agents[i].position;
        }        
    }
}