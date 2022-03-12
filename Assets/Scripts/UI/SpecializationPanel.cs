using UnityEngine;
using UnityEngine.UI;

public class SpecializationPanel : MonoBehaviour
{
    SelectionManager selectionManager => SelectionManager.instance;
    //CritterModuleCoreGenome coreGenome => selectionManager.currentSelection.candidate.candidateGenome.bodyGenome.coreGenome;
    [SerializeField]
    TechElementIconUI[] techElementIcons;
    [SerializeField]
    TechTree techTree;
    //[SerializeField] StatUI attack;
    //[SerializeField] StatUI defense;
    //[SerializeField] StatUI speed;
    //[SerializeField] StatUI energy;

    public void Refresh() {
        //attack.RefreshDisplay(coreGenome.talentSpecializationAttack * 100f, coreGenome.talentSpecializationAttack, false);
        //defense.RefreshDisplay(coreGenome.talentSpecializationDefense * 100f, coreGenome.talentSpecializationDefense, false);
        //speed.RefreshDisplay(coreGenome.talentSpecializationSpeed * 100f, coreGenome.talentSpecializationSpeed, false);
        //energy.RefreshDisplay(coreGenome.talentSpecializationUtility * 100f, coreGenome.talentSpecializationUtility, false);

        // WPP: expression is always true, foreach handles 0-length condition, invert conditional (if needed) to reduce nesting
        //if (techElementIcons.Length >= 0) {
        foreach (var tech in techElementIcons) {
            bool hasTech = selectionManager.SelectedAgentHasTech(tech.techElement);
            if (hasTech) {
                foreach (var category in techTree.categories) {
                    if (tech.techElement.category == category.category) {
                        tech.image.color = category.color;
                    }
                }
            }
            else {
                tech.image.color = new Color(0.1f, 0.1f, 0.1f);
            }
            
            tech.gameObject.transform.localScale = hasTech ? Vector3.one : Vector3.one * 0.5f;

            //tech.gameObject.GetComponent<Image>().color = selectionManager.FocusedAgentHasTech(tech.techElement.id) ?  : Vector3.one * 0.12f;
            //if (selectionManager.focusedCandidate.candidateGenome.bodyGenome.data.HasTech(tech.techElement.id)) {
            //    tech.gameObject.transform.localScale = Vector3.one;
            //}
            //else {
            //    tech.gameObject.transform.localScale = Vector3.one * 0.12f;
            //}
        }
        //}
    }
}
