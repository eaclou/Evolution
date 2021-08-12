using UnityEngine;

public class DigestionPanel : MonoBehaviour
{
    CritterModuleCoreGenome coreGenome => UIManager.instance.focusedCandidate.candidateGenome.bodyGenome.coreGenome;
    
    [SerializeField] StatUI plant;
    [SerializeField] StatUI meat;
    [SerializeField] StatUI decay;

    public void Refresh() {
        plant.RefreshDisplay(coreGenome.dietSpecializationPlant * 100f, coreGenome.dietSpecializationPlant, false);
        meat.RefreshDisplay(coreGenome.dietSpecializationMeat * 100f, coreGenome.dietSpecializationMeat, false);
        decay.RefreshDisplay(coreGenome.dietSpecializationDecay * 100f, coreGenome.dietSpecializationDecay, false);
    }
}
