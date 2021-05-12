using UnityEngine;

public class SpecializationPanel : MonoBehaviour
{
    [SerializeField] StatUI attack;
    [SerializeField] StatUI defense;
    [SerializeField] StatUI speed;
    [SerializeField] StatUI energy;

    public void Refresh(CritterModuleCoreGenome coreGenome) {
        attack.RefreshPercent(coreGenome.talentSpecializationAttack, 100f);
        defense.RefreshPercent(coreGenome.talentSpecializationDefense, 100f);
        speed.RefreshPercent(coreGenome.talentSpecializationSpeed, 100f);
        energy.RefreshPercent(coreGenome.talentSpecializationUtility, 100f);
    }
}
