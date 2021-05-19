using UnityEngine;

public class DigestionPanel : MonoBehaviour
{
    CritterModuleCoreGenome coreGenome => UIManager.instance.focusedCandidate.candidateGenome.bodyGenome.coreGenome;
    
    [SerializeField] StatUI plant;
    [SerializeField] StatUI meat;
    [SerializeField] StatUI decay;

    public void Refresh() {
        plant.RefreshPercent(coreGenome.dietSpecializationPlant, 100f);
        meat.RefreshPercent(coreGenome.dietSpecializationMeat, 100f);
        decay.RefreshPercent(coreGenome.dietSpecializationDecay, 100f);
    }
}
