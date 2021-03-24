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
	#endregion


	#region Resource Lookups
	/*public ExampleLookup[] examples;

	public Example GetExample(ExampleType type)
    {
      	foreach (var item in examples)
      	  	if (item.type == type)
      	  	  	return item.value;
	
      return null;
    }*/
	#endregion
}

#region Resource Lookup Containers
/*[Serializable]
public struct ExampleLookup
{
	public ExampleType type;
	public Example value;
}*/
#endregion

#region Resource Identifier Enums
// public enum ExampleType { ExampleA, ExampleB, ExampleC }
#endregion