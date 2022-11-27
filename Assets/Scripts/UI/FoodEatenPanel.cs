using UnityEngine;

public class FoodEatenPanel : MonoBehaviour
{
    SelectionManager selectionManager => SelectionManager.instance;
    PerformanceData data => selectionManager.currentSelection.candidate.performanceData;
    
    [SerializeField] StatUI plants;
    [SerializeField] StatUI microbes;
    [SerializeField] StatUI animals;
    [SerializeField] StatUI eggs;
    [SerializeField] StatUI corpse;

    public void Refresh()
    {
        //Debug.Log("REFRECH");
        plants.RefreshDisplay(data.totalFoodEatenPlant, data.plantEatenPercent, true);
        microbes.RefreshDisplay(data.totalFoodEatenZoop, data.zooplanktonEatenPercent, true);
        animals.RefreshDisplay(data.totalFoodEatenCreature, data.creatureEatenPercent, true);
        eggs.RefreshDisplay(data.totalFoodEatenEgg, data.eggEatenPercent, true);
        corpse.RefreshDisplay(data.totalFoodEatenCorpse, data.corpseEatenPercent, true);      
    }
}
