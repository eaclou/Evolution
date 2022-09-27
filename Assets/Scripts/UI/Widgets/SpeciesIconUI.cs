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
        
        linkedPool.coatOfArmsMat.SetFloat("_IsSelected", isSelected ? 1f : 0f);
        linkedPool.coatOfArmsMat.SetFloat("_IsEndangered", linkedPool.isFlaggedForExtinction ? 1f : 0f);

        toolString += linkedPool.isExtinct ? "\n(Extinct)" : "\nAvg Lifespan: " + uiManager.clockPanelUI.ConvertFramesToAgeString(linkedPool.avgLifespan);
        tooltip.tooltipString = toolString;

        var iconState = GetIconDisplayState(isSelected, linkedPool.isExtinct);
        transform.localScale = iconState.scaleFactor * Vector3.one;
        //image.color = iconState.imageColor;
        //text.color = iconState.textColor;
    }

    public bool flaggedForDestruction;
    void OnDestroy() { flaggedForDestruction = true; }
    
    #region Icon Display States
    
    [Header("Icon display states")]
    [SerializeField] IconDisplayState extinct;
    [SerializeField] IconDisplayState selected;
    [SerializeField] IconDisplayState unselected;
    
    IconDisplayState GetIconDisplayState(bool isSelected, bool isExtinct)
    {
        if (isExtinct) return extinct;
        return isSelected ? selected : unselected;
    }
    
    [Serializable]
    public struct IconDisplayState
    {
        public float scaleFactor;
        public Color imageColor;
        public Color textColor;
    }
    
    #endregion
}
