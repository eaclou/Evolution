﻿using System;
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
    
    [Header("Prefabs")]
    public GameObject genomeIcon;
    
	#endregion


	#region Resource Lookups
	
	/*
	[SerializeField] ResolutionData[] resolutions;

	public float GetResolution(ResolutionId id)
    {
      	foreach (var item in resolutions)
      	  	if (item.id == id)
      	  	  	return item.value;
	
	    Debug.LogError("Unable to find resolution " + id);
        return resolutions[0].value;
    }
    */
    
	#endregion
}

#region Resource Lookup Containers

/*
[Serializable]
public struct ResolutionData
{
	public ResolutionId id;
	public float value;
}
*/

#endregion

#region Resource Identifier Enums

//public enum ResolutionId { Low, Medium, High, Max }

#endregion