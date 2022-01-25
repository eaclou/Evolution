using UnityEngine;
using Playcraft;

/// Get and set data for the currently loaded save
public class AccessSaveData : Singleton<AccessSaveData>
{
    SimulationManager simulation => SimulationManager.instance;
    [SerializeField] SaveData data = new SaveData();
    
    public int saveIndex = 1;
    string saveName => "Save Game " + saveIndex;
    
    public void Save()
    {
        data.agents = new AgentData[simulation.agents.Length];
        for (int i = 0; i < data.agents.Length; i++)
        {
            data.agents[i] = simulation.agents[i].data;
            data.agents[i].position = simulation.agents[i].position;
        }
        
        ES3.Save(saveName, data);
    }
    
    public void Load()
    {
        data = ES3.Load(saveName, data);
        
        if (data == null)
        {
            Debug.LogError($"Data for {saveName} not found.");
            return;
        }
        
        for (int i = 0; i < data.agents.Length; i++)
        {
            simulation.agents[i].data = data.agents[i];
            simulation.agents[i].position = data.agents[i].position;
        }
    }
    
    public void DeleteActiveSave() { ES3.DeleteFile(saveName); }
}