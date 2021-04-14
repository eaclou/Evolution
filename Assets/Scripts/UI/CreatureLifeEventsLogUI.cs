using UnityEngine;
using UnityEngine.UI;

public class CreatureLifeEventsLogUI : MonoBehaviour
{
    public Text textEventsLog;

    public void Tick(CandidateAgentData agentData) {

        if (agentData.candidateEventDataList == null)
            return;
                         
        int maxEventsToDisplay = 8;
        //int numEvents = Mathf.Min(agent.agentEventDataList.Count, maxEventsToDisplay);
        int startIndex = Mathf.Max(0, agentData.candidateEventDataList.Count - maxEventsToDisplay);                   
        string eventString = "";
        for(int q = agentData.candidateEventDataList.Count - 1; q >= startIndex; q--) {
            eventString += "\n[" + agentData.candidateEventDataList[q].eventFrame.ToString() + "] " + agentData.candidateEventDataList[q].eventText;
        }                

        string eventsLog = "Event Log! Candidate#[" + agentData.candidateID.ToString() + "]";                    
        // Agent Event Log:
        int maxEventsToDisplayLog = 12;
        //int numEventsLog = Mathf.Min(agent.agentEventDataList.Count, maxEventsToDisplayLog);
        int startIndexLog = Mathf.Max(0, agentData.candidateEventDataList.Count - maxEventsToDisplayLog);                   
        string eventLogString = "";
        for(int q = agentData.candidateEventDataList.Count - 1; q >= startIndexLog; q--) {
            float dimAmount = Mathf.Clamp01((float)(agentData.candidateEventDataList.Count - q - 1) * 0.55f);
            //Color displayColor = Color.Lerp(Color.red, Color.green, agent.agentEventDataList[q].goodness);
            string goodColorStr = "#00FF00FF";
            if(dimAmount > 0.5f) {
                goodColorStr = "#007700FF";
            }
            string badColorStr = "#FF0000FF";
            if(dimAmount > 0.5f) {
                badColorStr = "#770000FF";
            }
            if(agentData.candidateEventDataList[q].goodness > 0.5f) {
                eventLogString += "<color=" + goodColorStr + ">";
            }
            else {
                eventLogString += "<color=" + badColorStr + ">";
            }
                            
            eventLogString += "\n[" + agentData.candidateEventDataList[q].eventFrame.ToString() + "] " + agentData.candidateEventDataList[q].eventText;
            eventLogString += "</color>";
        }
        
        eventsLog += eventLogString;
        textEventsLog.text = eventsLog;       
    }
    
}
