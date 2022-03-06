using UnityEngine;

public class SpecializationPanel : MonoBehaviour
{
    SelectionManager selectionManager => SelectionManager.instance;
    CritterModuleCoreGenome coreGenome => selectionManager.focusedCandidate.candidateGenome.bodyGenome.coreGenome;
    [SerializeField]
    TechElementIconUI[] techElementIcons;

    [SerializeField] StatUI attack;
    [SerializeField] StatUI defense;
    [SerializeField] StatUI speed;
    [SerializeField] StatUI energy;

    public void Refresh() {
        attack.RefreshDisplay(coreGenome.talentSpecializationAttack * 100f, coreGenome.talentSpecializationAttack, false);
        defense.RefreshDisplay(coreGenome.talentSpecializationDefense * 100f, coreGenome.talentSpecializationDefense, false);
        speed.RefreshDisplay(coreGenome.talentSpecializationSpeed * 100f, coreGenome.talentSpecializationSpeed, false);
        energy.RefreshDisplay(coreGenome.talentSpecializationUtility * 100f, coreGenome.talentSpecializationUtility, false);

        // WPP: expression is always true, foreach handles 0-length condition, invert conditional (if needed) to reduce nesting
        //if (techElementIcons.Length >= 0) {
            foreach (var tech in techElementIcons) {
                tech.gameObject.transform.localScale = selectionManager.FocusedAgentHasTech(tech.techElement.id) ? Vector3.one : Vector3.one * 0.12f;
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
