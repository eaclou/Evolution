using UnityEngine;

public class DigestionPanel : MonoBehaviour
{
    [SerializeField] StatUI plant;
    [SerializeField] StatUI meat;
    [SerializeField] StatUI decay;

    public void Refresh(CritterModuleCoreGenome coreGenome) {
        plant.RefreshPercent(coreGenome.dietSpecializationPlant, 100f);
        meat.RefreshPercent(coreGenome.dietSpecializationMeat, 100f);
        decay.RefreshPercent(coreGenome.dietSpecializationDecay, 100f);
    }
}
