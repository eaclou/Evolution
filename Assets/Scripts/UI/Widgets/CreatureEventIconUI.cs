using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreatureEventIconUI : MonoBehaviour
{
    UIManager uiManagerRef => UIManager.instance;

    public int index = -1;
    public Image imageBG;
    public TooltipUI tooltip;
    public Vector2 targetCoords; // UI canvas
    private Vector2 currentCoords;

    CandidateAgentData.CandidateEventData eventData;
    //public bool isSelected = false;
    [SerializeField]
    public Color[] eventTypeColors;

    public void UpdateIconPrefabData(CandidateAgentData.CandidateEventData data, int eventIndex) {
        index = eventIndex;
        eventData = data;
        tooltip.tooltipString = eventData.eventText;
        
    }
    public void SetTargetCoords(Vector2 newCoords) {
        //targetCoords = new Vector2((float)eventIndex / 32f + 0.1f, 0.5f);
        targetCoords = newCoords;
    }

    public void Clicked() {
        
    }

    public void SetDisplay()
    {        
        // POSITION
        currentCoords = Vector2.Lerp(currentCoords, targetCoords, 0.75f);

        gameObject.transform.localPosition = new Vector3(currentCoords.x * 360f, currentCoords.y * 360f, 0f);
        
        if(eventTypeColors != null) {
            imageBG.color = eventTypeColors[this.eventData.type];
        }
        else {
            imageBG.color = Color.Lerp(Color.red, Color.green, eventData.goodness);
        }
        
        
    }
}
