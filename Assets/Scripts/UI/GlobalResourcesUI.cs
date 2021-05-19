using UnityEngine;
using UnityEngine.UI;

public class GlobalResourcesUI : MonoBehaviour {
    SimulationManager simulation => SimulationManager.instance;
    SimResourceManager resourcesRef => simulation.simResourceManager;
    bool initialized => simulation != null && simulation.simResourceManager != null;

    public bool isUnlocked;
    public bool isOpen;
    public Text textGlobalMass;

    public GameObject panel;
      
    public Text textMeterOxygen;
    public Text textMeterNutrients;
    public Text textMeterDetritus;
    public Text textMeterDecomposers;
    public Text textMeterAlgae;
    public Text textMeterPlants;
    public Text textMeterZooplankton;
    public Text textMeterAnimals;


    public void ClickToolButton() {
        isOpen = !isOpen && isUnlocked;
        Refresh();
    }

    private void SetResourceText() {
        textGlobalMass.text = "Global Biomass: " + resourcesRef.curTotalMass.ToString("F0");
        //textMeterOxygen.text = resourcesRef.curGlobalOxygen.ToString("F0");
        textMeterNutrients.text = resourcesRef.curGlobalNutrients.ToString("F0");
        textMeterDetritus.text = resourcesRef.curGlobalDetritus.ToString("F0");
        textMeterDecomposers.text = resourcesRef.curGlobalDecomposers.ToString("F0");
        textMeterAlgae.text = resourcesRef.curGlobalAlgaeReservoir.ToString("F0");
        textMeterPlants.text = resourcesRef.curGlobalPlantParticles.ToString("F0");
        textMeterZooplankton.text = resourcesRef.curGlobalAnimalParticles.ToString("F0");
        textMeterAnimals.text = resourcesRef.curGlobalAgentBiomass.ToString("F2");
    }

    public void Refresh() {
        if (!initialized) return;
        panel.SetActive(isOpen);
        SetResourceText();
    }   
}
