using UnityEngine;
using UnityEngine.UI;

public class SpecializationPanel : MonoBehaviour
{
    SelectionManager selectionManager => SelectionManager.instance;
    //CritterModuleCoreGenome coreGenome => selectionManager.currentSelection.candidate.candidateGenome.bodyGenome.coreGenome;
    [SerializeField] TechElementIconUI[] techElementIcons;
    [SerializeField] TechTree techTree;
    [SerializeField] Color inactiveColor = new Color(0.1f, 0.1f, 0.1f, 0f);
    [SerializeField] float activeScale = 1f;
    [SerializeField] float inactiveScale = 0.5f;
    //[SerializeField] StatUI attack;
    //[SerializeField] StatUI defense;
    //[SerializeField] StatUI speed;
    //[SerializeField] StatUI energy;

    public void Refresh() {
        //attack.RefreshDisplay(coreGenome.talentSpecializationAttack * 100f, coreGenome.talentSpecializationAttack, false);
        //defense.RefreshDisplay(coreGenome.talentSpecializationDefense * 100f, coreGenome.talentSpecializationDefense, false);
        //speed.RefreshDisplay(coreGenome.talentSpecializationSpeed * 100f, coreGenome.talentSpecializationSpeed, false);
        //energy.RefreshDisplay(coreGenome.talentSpecializationUtility * 100f, coreGenome.talentSpecializationUtility, false);
        
        foreach (var tech in techElementIcons) {
            bool hasTech = selectionManager.SelectedAgentHasTech(tech.techElement);
            tech.color = hasTech ? techTree.CategoryColor(tech.techElement) : inactiveColor;
            tech.scale = Vector3.one * (hasTech ? activeScale : inactiveScale);
        }
    }
}
