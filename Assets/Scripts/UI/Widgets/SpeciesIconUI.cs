using System;
using UnityEngine;
using UnityEngine.UI;

/// Draws species coat-of-arms
public class SpeciesIconUI : MonoBehaviour 
{
    UIManager uiManager => UIManager.instance;
    
    [SerializeField] Image image;

    public int speciesID;

    public SpeciesGenomePool linkedPool;

    public Vector2 targetCoords; // UI canvas
    private Vector2 currentCoords;

    public TooltipUI tooltip;

    public Text text;
    public Text textDropShadow;

    public void Initialize(int index, SpeciesGenomePool pool, Transform anchor, Color color) 
    {
        if (pool == null) 
        {
            Debug.LogError("SpeciesIconUI.Initialize: species pool is null");
            return;
        }
        
        linkedPool = pool;
        speciesID = pool.speciesID;
        targetCoords = Vector2.zero;
        //Debug.Log("NEW BUTTON! " + index + ", " + pool.speciesID);
        
        transform.SetParent(anchor, false);
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
        image.color = Color.white;

        image.material = pool.coatOfArmsMat;
        //image.material.SetPass(0);
        
        //sprite = Sprite.Create(pool.GetCoatOfArms(), image.rectTransform.rect, Vector2.one * 0.5f);
        //sprite.name = "whoaSprite!";
        //image.sprite = sprite;
        //image.GetComponent<CanvasRenderer>().SetTexture(pool.GetCoatOfArms());

        //text.text = "[" + pool.speciesID + "]";// " + masterGenomePool.completeSpeciesPoolsList[pool.speciesID].foundingCandidate.candidateGenome.bodyGenome.coreGenome.name;
    }

    /// Updates focusedCandidate 
    public void Clicked() {
        uiManager.historyPanelUI.ClickSpeciesIcon(this);
    }
    
    public void SetTargetCoords(Vector2 newCoords) {
        targetCoords = newCoords;
    }
    
    public void UpdateSpeciesIconDisplay(int panelPixelSize, bool isSelected) 
    {
        string toolString = $"Species {linkedPool.speciesID}";
        var speciesIDText = linkedPool.speciesID.ToString();
        text.text = speciesIDText;
        textDropShadow.text = speciesIDText;
        
        // Move icon to desired position on graph
        currentCoords = Vector2.Lerp(currentCoords, targetCoords, 0.67f);
        transform.localPosition = new Vector3(currentCoords.x * panelPixelSize, currentCoords.y * panelPixelSize, 0f);

        var appearance = linkedPool.foundingCandidate.candidateGenome.bodyGenome.appearanceGenome;
        Color primaryColor = new Color(appearance.huePrimary.x, appearance.huePrimary.y, appearance.huePrimary.z);
        Color secondaryColor = new Color(appearance.hueSecondary.x, appearance.hueSecondary.y, appearance.hueSecondary.z);
        
        linkedPool.coatOfArmsMat.SetFloat("_IsSelected", isSelected ? 1f : 0f);

        toolString += linkedPool.isExtinct ? "\n(Extinct)" : "\nAvg Life: " + linkedPool.avgCandidateData.performanceData.totalTicksAlive.ToString("F0");
        tooltip.tooltipString = toolString;

        //imageColor3.color = linkedPool.isExtinct ? Color.black : Color.white;

        // WPP: exposed via Icon states
        var iconState = GetIconDisplayState(isSelected, linkedPool.isExtinct);
        if (iconState != null)
        {
            transform.localScale = iconState.scaleFactor * Vector3.one;
            //image.color = iconState.imageColor;
            //text.color = iconState.textColor;
        }
        /*
        //transform.localScale = isSelected ? new Vector3(1.2f, 1.2f, 1f) : Vector3.one;
        //image.color = isSelected ? Color.white : Color.gray * 0.5f;

        if (linkedPool.isExtinct) {
            //text.color = Color.gray * 0.05f;
            gameObject.transform.localScale = Vector3.one * 0.5f;
        }
        else {
           // text.color = linkedPool.isFlaggedForExtinction ? Color.gray : Color.white;
        }
        */
    }

    public bool flaggedForDestruction;
    void OnDestroy() { flaggedForDestruction = true; }
    
    [SerializeField] IconDisplayState[] iconDisplayStates;
    
    IconDisplayState GetIconDisplayState(bool isSelected, bool isExtinct)
    {
        foreach (var state in iconDisplayStates)
            if (state.Applies(isSelected, isExtinct))
                return state;
                
        Debug.LogError($"No display state defined for selected = {isSelected}, extinct = {isExtinct}");
        return null;
    }
    
    [Serializable]
    public class IconDisplayState
    {
        [Header("Conditions")]
        public Trinary isExtinct;
        public Trinary isSelected;
        
        [Header("Properties")]
        public float scaleFactor = 1f;
        public Color imageColor;
        public Color textColor;
        
        public bool Applies(bool isSelected, bool isExtinct)
        {
            var extinctCondition = new TrinaryCondition(isExtinct, this.isExtinct);
            var selectedCondition = new TrinaryCondition(isSelected, this.isSelected);
            TrinaryCondition[] conditions = {extinctCondition, selectedCondition};
            return TrinaryLogic.AllConditionsMet(conditions);
        }
    }
}
