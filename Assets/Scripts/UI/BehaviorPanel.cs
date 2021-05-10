using System;
using UnityEngine;

public class BehaviorPanel : MonoBehaviour
{
    [SerializeField] StatUI attack;
    [SerializeField] StatUI defend;
    [SerializeField] StatUI dash;
    [SerializeField] StatUI rest;
    [SerializeField] StatUI feed;

    public void Refresh(PerformanceData data)
    {
        attack.RefreshDisplay(data.totalTimesAttacked, data.attackActionPercent);
        defend.RefreshDisplay(data.totalTimesDefended, data.defendActionPercent);
        dash.RefreshDisplay(data.totalTimesDashed, data.dashActionPercent);
        rest.RefreshDisplay(data.totalTicksRested * .01f, Mathf.Clamp01(data.totalTicksRested/600f));
        feed.RefreshDisplay(data.totalTimesPregnant, data.totalTimesPregnant/4f);
    }
}
