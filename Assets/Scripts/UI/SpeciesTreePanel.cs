using UnityEngine;

public class SpeciesTreePanel : MonoBehaviour
{
    [SerializeField] GameObject panelSpeciesTree;
    [SerializeField] GameObject panelGraphs;

    public void SetOpen(bool value) {
        panelSpeciesTree.SetActive(value);
        panelGraphs.SetActive(false);
    }
}
