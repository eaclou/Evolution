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
	
    [Header("Spirit Icons")]
    public Sprite spiritWorldIcon;
    public Sprite spiritStoneIcon;
    public Sprite spiritPebblesIcon;
    public Sprite spiritSandIcon;
    public Sprite spiritMineralsIcon;
    public Sprite spiritWaterIcon;
    public Sprite spiritAirIcon;
    public Sprite spiritDecomposerIcon;
    public Sprite spiritAlgaeIcon;
    public Sprite spiritPlantIcon;
    public Sprite spiritZooplanktonIcon;
    public Sprite spiritVertebrateIcon;
    public Sprite spiritBrushStirIcon;
    public Sprite spiritBrushCreationIcon;
    
    [Header("Colors")]
    public Color buttonActiveColor = new Color(1f, 1f, 1f, 1f);
    public Color buttonDisabledColor = new Color(0.7f, 0.7f, 0.7f, 1f);
    public Color colorSpiritBrushLight;
    public Color colorSpiritBrushDark;
    public Color colorWorldLayer;
    public Color colorTerrainLayer;
    public Color colorMineralLayer;
    public Color colorWaterLayer;
    public Color colorAirLayer;
    public Color colorDecomposersLayer;
    public Color colorAlgaeLayer;
    public Color colorPlantsLayer;
    public Color colorZooplanktonLayer;
    public Color colorVertebratesLayer;
    
    [Header("Materials")]
	public Material knowledgeGraphOxygenMat;
	public Material knowledgeGraphNutrientsMat;
	public Material knowledgeGraphDetritusMat;
	public Material knowledgeGraphDecomposersMat;
	public Material knowledgeGraphAlgaeMat;
	public Material knowledgeGraphPlantsMat;
	public Material knowledgeGraphZooplanktonMat;
	public Material knowledgeGraphVertebratesMat;
    
    [Header("Prefabs")]
    public GameObject genomeIcon;
    
    [Header("Narration")]
    public NarrationSO unlockAlgae;
    public NarrationSO unlockDecomposers;
    public NarrationSO unlockZooplankton;
    public NarrationSO unlockVertebrates;
    public NarrationSO unlockPlant;
    
    void OnValidate()
    {
        unlockAlgae.color = colorAlgaeLayer;
        unlockDecomposers.color = colorDecomposersLayer;
        unlockZooplankton.color = colorZooplanktonLayer;
        unlockVertebrates.color = colorVertebratesLayer;
        unlockPlant.color = colorPlantsLayer;
    }
    
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