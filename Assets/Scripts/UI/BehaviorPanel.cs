using UnityEngine;

public class BehaviorPanel : MonoBehaviour
{
    PerformanceData data => UIManager.instance.selectionManager.focusedCandidate.performanceData;
    
    [SerializeField] StatUI attack;
    [SerializeField] StatUI defend;
    [SerializeField] StatUI dash;
    [SerializeField] StatUI rest;
    [SerializeField] StatUI feed;

    public void Refresh()
    {
        attack.RefreshDisplay(data.totalTimesAttacked, data.attackActionPercent);
        defend.RefreshDisplay(data.totalTimesDefended, data.defendActionPercent);
        dash.RefreshDisplay(data.totalTimesDashed, data.dashActionPercent);
        rest.RefreshDisplay(data.totalTicksRested * .01f, Mathf.Clamp01(data.totalTicksRested/600f));
        feed.RefreshDisplay(data.totalTimesPregnant, data.totalTimesPregnant/4f);
    }
}
