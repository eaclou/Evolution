using UnityEngine;

public class FoodEatenPanel : MonoBehaviour
{
    PerformanceData data => UIManager.instance.focusedCandidate.performanceData;
    
    [SerializeField] StatUI plants;
    [SerializeField] StatUI microbes;
    [SerializeField] StatUI animals;
    [SerializeField] StatUI eggs;
    [SerializeField] StatUI corpse;

    public void Refresh()
    {
        plants.RefreshDisplay(data.totalFoodEatenPlant, data.plantEatenPercent, true);
        microbes.RefreshDisplay(data.totalFoodEatenZoop, data.zooplanktonEatenPercent, true);
        animals.RefreshDisplay(data.totalFoodEatenCreature, data.creatureEatenPercent, true);
        eggs.RefreshDisplay(data.totalFoodEatenEgg, data.eggEatenPercent, true);
        corpse.RefreshDisplay(data.totalFoodEatenCorpse, data.corpseEatenPercent, true);      
    }
}
