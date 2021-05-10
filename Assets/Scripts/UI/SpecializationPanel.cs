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

        //textSpecAttack.text = (coreGenome.talentSpecializationAttack * 100f).ToString("F0");
        //textSpecDefense.text = (coreGenome.talentSpecializationDefense * 100f).ToString("F0");
        //textSpecSpeed.text = (coreGenome.talentSpecializationSpeed * 100f).ToString("F0");
        //textSpecEnergy.text = (coreGenome.talentSpecializationUtility * 100f).ToString("F0");

        //imageSpecAttack.transform.localScale = new Vector3(1f, coreGenome.talentSpecializationAttack, 1f);
        //imageSpecDefense.transform.localScale = new Vector3(1f, coreGenome.talentSpecializationDefense, 1f);
        //imageSpecSpeed.transform.localScale = new Vector3(1f, coreGenome.talentSpecializationSpeed, 1f);
        //imageSpecEnergy.transform.localScale = new Vector3(1f, coreGenome.talentSpecializationUtility, 1f);
    }
}
