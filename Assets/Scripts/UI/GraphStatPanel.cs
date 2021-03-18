using UnityEngine;

public class GraphStatPanel : MonoBehaviour
{
    [SerializeField] GameObject panelSpeciesTree;
    [SerializeField] GameObject panelGraphs;

    public void SetOpen(bool value) {
        panelGraphs.SetActive(value);
        panelSpeciesTree.SetActive(false);
    }
    
    public void ToggleOpen() {
        panelGraphs.SetActive(!panelGraphs.activeSelf);
        panelSpeciesTree.SetActive(!panelSpeciesTree.activeSelf);
    }
}
