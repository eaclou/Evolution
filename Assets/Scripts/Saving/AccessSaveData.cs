using Playcraft;

/// Get and set data for the currently loaded save
public class AccessSaveData : Singleton<AccessSaveData>
{
    SimulationManager simulation => SimulationManager.instance;

    public SaveData data;

    public void Save()
    {
        data.agents = new AgentData[simulation.agents.Length];
        for (int i = 0; i < data.agents.Length; i++)
        {
            data.agents[i] = simulation.agents[i].data;
            data.agents[i].position = simulation.agents[i].position;
        }
        
        ES3.Save("Save Game 1", data);
    }
    
    public void Load()
    {
        data = ES3.Load("Save Game 1", data);
    
        for (int i = 0; i < data.agents.Length; i++)
        {
            simulation.agents[i].data = data.agents[i];
            simulation.agents[i].position = data.agents[i].position;
        }
    }
}