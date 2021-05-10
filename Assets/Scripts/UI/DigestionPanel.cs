using UnityEngine;

public class DigestionPanel : MonoBehaviour
{
    [SerializeField] StatUI plant;
    [SerializeField] StatUI meat;
    [SerializeField] StatUI decay;

    // WPP 5/9/21: Only pass coreGenome since it is the only part being used
    // + delegate logic to StatUI, shared with other panels
    public void Refresh(CritterModuleCoreGenome coreGenome) {
        plant.RefreshPercent(coreGenome.dietSpecializationPlant, 100f);
        meat.RefreshPercent(coreGenome.dietSpecializationMeat, 100f);
        decay.RefreshPercent(coreGenome.dietSpecializationDecay, 100f);
        //plant.text = (coreGenome.dietSpecializationPlant * 100f).ToString("F0");
        //meat.text = (coreGenome.dietSpecializationMeat * 100f).ToString("F0");
        //decay.text = (coreGenome.dietSpecializationDecay * 100f).ToString("F0");

        //plant.transform.localScale = new Vector3(1f, coreGenome.dietSpecializationPlant, 1f);
        //meat.transform.localScale = new Vector3(1f, coreGenome.dietSpecializationMeat, 1f);
        //decay.transform.localScale = new Vector3(1f, coreGenome.dietSpecializationDecay, 1f);
    }
}
