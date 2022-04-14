using UnityEngine;
using UnityEngine.UI;

public class SpeciesIconUI : MonoBehaviour 
{
    UIManager uiManager => UIManager.instance;
    
    [SerializeField] Image image;
    //[SerializeField] Text text;

    //public AllSpeciesTreePanelUI allSpeciesTreePanelUI;
    public int speciesID;

    public SpeciesGenomePool linkedPool;

    public Vector2 targetCoords; // UI canvas
    private Vector2 currentCoords;

    public TooltipUI tooltip;

    //public Sprite sprite;
    //public Image imageColor1;
    //public Image imageColor2;
    //public Image imageColor3;

    public void Initialize(int index, SpeciesGenomePool pool, Transform anchor, Color color) 
    {        
        linkedPool = pool;
        speciesID = pool.speciesID;
        targetCoords = Vector2.zero;
        //Debug.Log("NEW BUTTON! " + index + ", " + pool.speciesID);
        
        transform.SetParent(anchor, false);
        transform.localPosition = Vector3.zero;
        transform.localScale = new Vector3(1f, 1f, 1f);
        image.color = Color.white;

        image.material = pool.coatOfArmsMat;
        //image.material.SetPass(0);

        
        //sprite = Sprite.Create(pool.GetCoatOfArms(), image.rectTransform.rect, Vector2.one * 0.5f);
        //sprite.name = "whoaSprite!";
        //image.sprite = sprite;
        //image.GetComponent<CanvasRenderer>().SetTexture(pool.GetCoatOfArms());
        if(pool == null) {
            Debug.LogError("pool NULL");
            return;
        }
        //text.text = "[" + pool.speciesID + "]";// " + masterGenomePool.completeSpeciesPoolsList[pool.speciesID].foundingCandidate.candidateGenome.bodyGenome.coreGenome.name;
    }

    // Updates focusedCandidate 
    public void Clicked() {
        uiManager.historyPanelUI.ClickSpeciesIcon(this);
        //uiManager.SetSelectedSpeciesUI(speciesID);        
    }
    
    public void SetTargetCoords(Vector2 newCoords) {
        targetCoords = newCoords;
    }
    
    public void UpdateSpeciesIconDisplay(int panelPixelSize, bool isSelected) 
    {
        string toolString = "Species " + linkedPool.speciesID;
        
        // POSITION
        currentCoords = Vector2.Lerp(currentCoords, targetCoords, 0.67f);
        transform.localPosition = new Vector3(currentCoords.x * (float)panelPixelSize, currentCoords.y * (float)panelPixelSize, 0f);

        var appearance = linkedPool.foundingCandidate.candidateGenome.bodyGenome.appearanceGenome;
        Color colorPri = new Color(appearance.huePrimary.x, appearance.huePrimary.y, appearance.huePrimary.z);
        Color colorSec = new Color(appearance.hueSecondary.x, appearance.hueSecondary.y, appearance.hueSecondary.z);   
        //imageColor1.color = colorPri;
        //imageColor2.color = colorSec;
        
        transform.localScale = isSelected ? new Vector3(1.2f, 1.2f, 1f) : Vector3.one;
        //image.color = isSelected ? Color.white : Color.gray * 0.5f;

        linkedPool.coatOfArmsMat.SetFloat("_IsSelected", isSelected ? 1f : 0f);

        toolString += linkedPool.isExtinct ? "\n(Extinct)" : "\nAvg Life: " + linkedPool.avgCandidateData.performanceData.totalTicksAlive.ToString("F0");
        //imageColor3.color = linkedPool.isExtinct ? Color.black : Color.white;

        if(linkedPool.isExtinct) {
            //text.color = Color.gray * 0.05f;
            gameObject.transform.localScale = Vector3.one * 0.5f;
        }
        else {
           // text.color = linkedPool.isFlaggedForExtinction ? Color.gray : Color.white;
        }

        tooltip.tooltipString = toolString;
    }
    
    public bool flaggedForDestruction;
    void OnDestroy() { flaggedForDestruction = true; }
}
