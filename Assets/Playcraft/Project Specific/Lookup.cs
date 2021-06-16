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

	[Header("Colors")]
    public Color buttonActiveColor = new Color(1f, 1f, 1f, 1f);
    public Color buttonDisabledColor = new Color(0.7f, 0.7f, 0.7f, 1f);
    public Color colorSpiritBrushLight;
    public Color colorSpiritBrushDark;

    // WPP: folded into TrophicLayerSO
    /*
    [Header("Materials")]
	public Material knowledgeGraphOxygenMat;
	public Material knowledgeGraphNutrientsMat;
	public Material knowledgeGraphDetritusMat;
	public Material knowledgeGraphDecomposersMat;
	public Material knowledgeGraphAlgaeMat;
	public Material knowledgeGraphPlantsMat;
	public Material knowledgeGraphZooplanktonMat;
	public Material knowledgeGraphVertebratesMat;
	
	[Header("Narration")]
	public NarrationSO unlockAlgae;
	public NarrationSO unlockDecomposers;
	public NarrationSO unlockZooplankton;
	public NarrationSO unlockVertebrates;
	public NarrationSO unlockPlant;
	*/
    
    [Header("Prefabs")]
    public GameObject genomeIcon;

    #endregion
	
	#region Resource Lookups

	[SerializeField] CauseOfDeathData[] causesOfDeath;

	public CauseOfDeathSO GetCauseOfDeath(CauseOfDeathId id)
    {
      	foreach (var item in causesOfDeath)
      	  	if (item.id == id)
      	  	  	return item.value;
	
	    Debug.LogError("Unable to find cause of death " + id);
        return causesOfDeath[0].value;
    }
    
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
}

#region Resource Lookup Containers

[Serializable]
public struct CauseOfDeathData
{
	public CauseOfDeathId id;
	public CauseOfDeathSO value;
}

#endregion