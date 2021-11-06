using UnityEngine;
using UnityEngine.UI;

public class CreatureEventIconUI : MonoBehaviour
{
    public Image imageBG;
    public TooltipUI tooltip;
    public Vector2 targetCoords; // UI canvas
    private Vector2 currentCoords;

    CandidateAgentData.CandidateEventData eventData;
    //public bool isSelected = false;
    [SerializeField]
    public Color[] eventTypeColors;
    
    public void OnCreate(Transform anchor)
    {
        transform.SetParent(anchor, false);
        transform.localPosition = Vector3.zero;
        gameObject.SetActive(false);
    }

    public void UpdateIconPrefabData(CandidateAgentData.CandidateEventData data, int eventIndex) 
    {
        eventData = data;
        tooltip.tooltipString = eventData.eventText;
    }
    
    public void SetTargetCoords(Vector2 newCoords) 
    {
        //targetCoords = new Vector2((float)eventIndex / 32f + 0.1f, 0.5f);
        targetCoords = newCoords;
    }

    public void Clicked() { }

    /// Set position and color
    public void SetDisplay()
    {        
        currentCoords = Vector2.Lerp(currentCoords, targetCoords, 0.75f);
        gameObject.transform.localPosition = new Vector3(currentCoords.x * (float)HistoryPanelUI.panelSizePixels, currentCoords.y * (float)HistoryPanelUI.panelSizePixels, 0f);
        imageBG.color = eventTypeColors == null ? Color.Lerp(Color.red, Color.green, eventData.goodness) : eventTypeColors[eventData.type];
    }
    
    public bool flaggedForDestruction;
    void OnDestroy() { flaggedForDestruction = true; }
}
