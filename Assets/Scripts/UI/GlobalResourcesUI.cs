using UnityEngine;
using UnityEngine.UI;

public class GlobalResourcesUI : MonoBehaviour {
    SimulationManager simulationManager => SimulationManager.instance;
    SimResourceManager resourcesRef => simulationManager.simResourceManager;
    
    public bool isUnlocked;
    public bool isOpen;
    public Text textGlobalMass;

    public GameObject panelGlobalResourcesMain;
      
    public Text textMeterOxygen;
    public Text textMeterNutrients;
    public Text textMeterDetritus;
    public Text textMeterDecomposers;
    public Text textMeterAlgae;
    public Text textMeterPlants;
    public Text textMeterZooplankton;
    public Text textMeterAnimals;

    public Material knowledgeGraphOxygenMat;
    public Material knowledgeGraphNutrientsMat;
    public Material knowledgeGraphDetritusMat;
    public Material knowledgeGraphDecomposersMat;
    public Material knowledgeGraphAlgaeMat;
    public Material knowledgeGraphPlantsMat;
    public Material knowledgeGraphZooplanktonMat;
    public Material knowledgeGraphVertebratesMat;
        
    
    public void ClickToolButton() {
        Debug.Log("Click globalResourcesUI toggle button)");
        isOpen = !isOpen;
    }

    private void UpdateUI() {        
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

    public void UpdateGlobalResourcesPanelUpdate() {
        isOpen = true;
        panelGlobalResourcesMain.SetActive(isOpen);
        UpdateUI();
    }   
}
