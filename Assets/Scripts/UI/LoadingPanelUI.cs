using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoadingPanelUI : MonoBehaviour
{
    SimulationManager simulationManager => SimulationManager.instance;
    AudioManager audioManager => AudioManager.instance;

    [Header("Settings")]
    public float warmupTime;
    [Tooltip("Percent of time into warmup when UI refreshes")]
    [Range(0, 1)] public float refreshWarmupPercent = 0.5f;

    [Header("References")]
    public Image imageLoadingStartBG;
    public Image imageLoadingStrokes01;
    public Image imageLoadingStrokes02;
    public Image imageLoadingStrokes03;
    public Image imageLoadingStrokesFull;
    public Image imageLoadingGemGrowing;
    public Button buttonLoadingGemStart;
    public Text textLoadingTooltips;
    
    public void Refresh(string tooltip, int stage) {
        textLoadingTooltips.text = tooltip;
        
        var  loading = stage < 4;
        imageLoadingStartBG.gameObject.SetActive(loading);
        imageLoadingStrokes01.gameObject.SetActive(loading && stage > 0);
        imageLoadingStrokes02.gameObject.SetActive(loading && stage > 1);
        imageLoadingStrokes03.gameObject.SetActive(loading && stage > 2);
        imageLoadingStrokesFull.gameObject.SetActive(!loading);        
    }
    
    public void SetCursorActive(bool value) {
        Cursor.visible = value;
        imageLoadingGemGrowing.gameObject.SetActive(!value);
        buttonLoadingGemStart.gameObject.SetActive(value);
    }
    
    public void BeginWarmUp() { 
        StartCoroutine(WarmUpRoutine());
        audioManager.BeginFadeMenuToGame(warmupTime); 
    }
    
    // * If this gets complicated, delegate to ProgressEvent    
    IEnumerator WarmUpRoutine() {
        yield return new WaitForSeconds(warmupTime * refreshWarmupPercent);
        Refresh("", 4);
        yield return new WaitForSeconds(warmupTime * (1 - refreshWarmupPercent));
        simulationManager.LoadingWarmupComplete();
    }
}
