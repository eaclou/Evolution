using UnityEngine;
using UnityEngine.UI;

public class SpecializationPanel : MonoBehaviour
{
    SelectionManager selectionManager => SelectionManager.instance;
    //CritterModuleCoreGenome coreGenome => selectionManager.currentSelection.candidate.candidateGenome.bodyGenome.coreGenome;
    [SerializeField] TechElementIconUI[] techElementIcons;
    [SerializeField] TechTree techTree;
    [SerializeField] TechElementIconData iconData;
    //[SerializeField] Color inactiveColor = new Color(0.1f, 0.1f, 0.1f, 0f);
    //[SerializeField] float activeScale = 1f;
    //[SerializeField] float inactiveScale = 0.5f;


    public void Refresh() {
        foreach (var icon in techElementIcons) {
            icon.SetActive(selectionManager.SelectedAgentHasTech(icon.techElement));
            
            // WPP: delegated to icons        
            //bool hasTech = selectionManager.SelectedAgentHasTech(icon.techElement);
            //icon.color = hasTech ? techTree.CategoryColor(icon.techElement) : inactiveColor;
            //icon.scale = Vector3.one * (hasTech ? activeScale : inactiveScale);
            //icon.gameObject.SetActive(hasTech);
        }
    }
    
    // WPP: specify common data here to be used by all icons
    void OnValidate()
    {
        foreach (var icon in techElementIcons)
            icon.data = new TechElementIconData(iconData, techTree.CategoryColor(icon.techElement));
    }
}

//[SerializeField] StatUI attack;
//[SerializeField] StatUI defense;
//[SerializeField] StatUI speed;
//[SerializeField] StatUI energy;

//attack.RefreshDisplay(coreGenome.talentSpecializationAttack * 100f, coreGenome.talentSpecializationAttack, false);
//defense.RefreshDisplay(coreGenome.talentSpecializationDefense * 100f, coreGenome.talentSpecializationDefense, false);
//speed.RefreshDisplay(coreGenome.talentSpecializationSpeed * 100f, coreGenome.talentSpecializationSpeed, false);
//energy.RefreshDisplay(coreGenome.talentSpecializationUtility * 100f, coreGenome.talentSpecializationUtility, false);
