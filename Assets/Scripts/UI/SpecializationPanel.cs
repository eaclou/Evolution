using UnityEngine;

public class SpecializationPanel : MonoBehaviour
{
    CritterModuleCoreGenome coreGenome => UIManager.instance.focusedCandidate.candidateGenome.bodyGenome.coreGenome;
    
    [SerializeField] StatUI attack;
    [SerializeField] StatUI defense;
    [SerializeField] StatUI speed;
    [SerializeField] StatUI energy;

    public void Refresh() {
        attack.RefreshDisplay(coreGenome.talentSpecializationAttack * 100f, coreGenome.talentSpecializationAttack, false);
        defense.RefreshDisplay(coreGenome.talentSpecializationDefense * 100f, coreGenome.talentSpecializationDefense, false);
        speed.RefreshDisplay(coreGenome.talentSpecializationSpeed * 100f, coreGenome.talentSpecializationSpeed, false);
        energy.RefreshDisplay(coreGenome.talentSpecializationUtility * 100f, coreGenome.talentSpecializationUtility, false);
    }
}
