using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreatureIconUI : MonoBehaviour
{
    SimulationManager simulationManager => SimulationManager.instance;
    CameraManager cameraManager => CameraManager.instance;
    UIManager uiManagerRef => UIManager.instance;

    public int index = -1;
    //private SpeciesOverviewUI.SelectionGroup group;
    
    public Image imageBG;
    //public bool isSelected = false;

    private CandidateAgentData candidateData;

	public void UpdateButtonPrefab(CandidateAgentData data) {
        candidateData = data;
        
    }

    public void Clicked() {
        
    }
}
