using System;
using UnityEngine;

// Globally-accessible point of access to prefabs and scriptable object resources
//[CreateAssetMenu(menuName = "Playcraft/Resource Lookup")]
public class Lookup : ScriptableObject
{
	#region Singleton initialization
	public static Lookup instance { get; private set; } 

	[RuntimeInitializeOnLoadMethod]
	private static void Initialize() { instance = (Lookup)Resources.Load("Lookup"); }
	#endregion
	
	#region Global Resource References
	
	[Header("Prefabs")]
	public GameObject agent;
	public GameObject eggSack;
	public GameObject genomeIcon;
	
    [Header("Scriptable Objects")]
    public SimEventLookup simEvents;
    public NeuralMap neuralMap;
    public MetaNeuron hiddenTemplate => neuralMap.hiddenTemplate;

    [Header("Colors")]
    public Color buttonActiveColor = new Color(1f, 1f, 1f, 1f);
    public Color buttonDisabledColor = new Color(0.7f, 0.7f, 0.7f, 1f);
    public Color colorSpiritBrushLight;
    public Color colorSpiritBrushDark;
    public Color sensorEnabled = Color.white;
    public Color sensorDisabled = Color.gray * 0.75f;
    public Color GetSensorColor(bool enabled) { return enabled ? sensorEnabled : sensorDisabled; }

    #endregion
	
	[Header("Internal Lookups")]
	#region Resource Lookups
	
	[SerializeField] MutationSettingsData[] mutationSettings;
	
	public MutationSettingsInstance GetMutationSettingsCopy(MutationSettingsId id)
	{
		return GetMutationSettingsTemplate(id).GetCopy();
	}
	
	public MutationSettings GetMutationSettingsTemplate(MutationSettingsId id)
	{
		foreach (var setting in mutationSettings)
			if (setting.id == id)
				return setting.value;
		
		Debug.LogError($"{id} Mutation Settings not found in resource lookup");	
		return null;
	}
	
	[Serializable]
	public struct MutationSettingsData
	{
		public MutationSettingsId id;
		public MutationSettings value;
	}

	// * Refactor to Serialized Dictionary
	[SerializeField] CauseOfDeathData[] causesOfDeath;

	public CauseOfDeathSO GetCauseOfDeath(CauseOfDeathId id)
    {
      	foreach (var item in causesOfDeath)
      	  	if (item.id == id)
      	  	  	return item.value;
	
	    Debug.LogError("Unable to find cause of death " + id);
        return causesOfDeath[0].value;
    }
    
    // * Refactor to Serialized Dictionary
    public TrophicLayerSO[] knowledgeMaps;
    
    // TBD: cache specific elements for faster lookup
    public TrophicLayerSO GetTrophicSlotData(KnowledgeMapId id)
    {
	    foreach (var map in knowledgeMaps)
            if (map.id == id)
                return map;
        
        Debug.LogError("Invalid map state " + id);        
        return knowledgeMaps[0];
    }

    #endregion
    
    #region Agent State Icons
    
    [SerializeField] AgentLifeStageMetaData[] agentLifeStageData;
    [SerializeField] AgentLifeStageMetaData defaultLifeStage;
    
    public Sprite GetAgentLifeStageIcon(AgentLifeStage id, bool useSecondary = false) { return GetAgentLifeStageData(id, useSecondary).icon; }

    public AgentLifeStageData GetAgentLifeStageData(AgentLifeStage id, bool useSecondary = false)
    {
		foreach (var metadata in agentLifeStageData)
			if (metadata.id == id)
				return metadata.GetData(useSecondary);
				
	    return defaultLifeStage.GetData(useSecondary);
    }
    
    [Serializable]
    public class AgentLifeStageMetaData
    {
		public AgentLifeStage id;
		public AgentLifeStageData[] substages;
		
		public AgentLifeStageData GetData(bool useSecondary = false) 
		{ 
			return useSecondary && substages.Length > 1 ? substages[1] : substages[0]; 
	    }
    }
    
    [Serializable]
    public struct AgentLifeStageData
    {
		public Sprite icon;
		public string stateName;
    }
    
    #endregion
}

#region Resource Lookup Containers

[Serializable]
public struct CauseOfDeathData
{
	public CauseOfDeathId id;
	public CauseOfDeathSO value;
}

#endregion