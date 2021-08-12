using UnityEngine;
using UnityEngine.UI;

public class CreatureLifeEventsLogUI : MonoBehaviour
{
    public Text textEventsLog;
    
    [SerializeField] Color goodColor;
    [SerializeField] Color dimGoodColor;
    [SerializeField] Color badColor;
    [SerializeField] Color dimBadColor;
    [SerializeField] [Range(0f, 1f)] float dimThreshold = 0.5f;
    [SerializeField] [Range(0f, 1f)] float goodThreshold = 0.5f;
    
    // * WPP: use getter for AgentData plus RepeatWhileEnabled to remove UIManager dependency
    public void Tick(CandidateAgentData agentData) {
        if (agentData.candidateEventDataList == null)
            return;
                         
        int maxEventsToDisplay = 8;
        //int numEvents = Mathf.Min(agent.agentEventDataList.Count, maxEventsToDisplay);
        int startIndex = Mathf.Max(0, agentData.candidateEventDataList.Count - maxEventsToDisplay);                   
        string eventString = "";
        for(int q = agentData.candidateEventDataList.Count - 1; q >= startIndex; q--) {
            eventString += "\n[" + agentData.candidateEventDataList[q].eventFrame + "] " + agentData.candidateEventDataList[q].eventText;
        }                

        string eventsLog = "Event Log! Candidate#[" + agentData.candidateID + "] " + agentData.candidateEventDataList.Count;                    
        // Agent Event Log:
        int maxEventsToDisplayLog = 12;
        //int numEventsLog = Mathf.Min(agent.agentEventDataList.Count, maxEventsToDisplayLog);
        int startIndexLog = Mathf.Max(0, agentData.candidateEventDataList.Count - maxEventsToDisplayLog);                   
        string eventLogString = "";
        
        for(int q = agentData.candidateEventDataList.Count - 1; q >= startIndexLog; q--) {
            float dimAmount = Mathf.Clamp01((agentData.candidateEventDataList.Count - q - 1) * 0.55f);
            
            bool isDim = dimAmount > dimThreshold;
            bool isGood = agentData.candidateEventDataList[q].goodness > goodThreshold;
            eventLogString += EventColorString(isDim, isGood);
            
            eventLogString += "\n[" + agentData.candidateEventDataList[q].eventFrame + "] " + agentData.candidateEventDataList[q].eventText;
            eventLogString += "</color>";
        }
        
        eventsLog += eventLogString;
        textEventsLog.text = eventsLog;       
    }
    
    string EventColorString(bool isDim, bool isGood)
    {
        string goodColorStr = isDim ? ColorString(dimGoodColor) : ColorString(goodColor);
        string badColorStr = isDim ? ColorString(dimBadColor) : ColorString(badColor);
        return isGood ? goodColorStr : badColorStr;
    }
    
    string ColorString(Color color) { return "<color=#" + ColorUtility.ToHtmlStringRGBA(color) + ">"; }
}
