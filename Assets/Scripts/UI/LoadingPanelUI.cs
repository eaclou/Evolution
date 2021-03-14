using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoadingPanelUI : MonoBehaviour
{
    [Header("Settings")]
    public float warmupTime;

    [Header("References")]
    public Image imageLoadingStartBG;
    public Image imageLoadingStrokes01;
    public Image imageLoadingStrokes02;
    public Image imageLoadingStrokes03;
    public Image imageLoadingStrokesFull;
    public Image imageLoadingGemGrowing;
    public Button buttonLoadingGemStart;
    public Text textLoadingTooltips;
    
    SimulationManager sim => SimulationManager.instance;
    AudioManager audio => AudioManager.instance;
    
    public void Refresh(string tooltip, int stage)
    {
        textLoadingTooltips.text = tooltip;
        
        var  loading = stage < 4;
        imageLoadingStartBG.gameObject.SetActive(loading);
        imageLoadingStrokes01.gameObject.SetActive(loading && stage > 0);
        imageLoadingStrokes02.gameObject.SetActive(loading && stage > 1);
        imageLoadingStrokes03.gameObject.SetActive(loading && stage > 2);
        imageLoadingStrokesFull.gameObject.SetActive(!loading);        
    }
    
    public void SetCursorActive(bool value)
    {
        Cursor.visible = value;
        imageLoadingGemGrowing.gameObject.SetActive(!value);
        buttonLoadingGemStart.gameObject.SetActive(value);
    }
    
    public void BeginWarmUp() 
    { 
        StartCoroutine(WarmUpRoutine());
        audio.BeginFadeMenuToGame(warmupTime); 
    }
    
    // * If this gets complicated, delegate to ProgressEvent    
    IEnumerator WarmUpRoutine()
    {
        var halfDelay = new WaitForSeconds(warmupTime/2f);
        yield return halfDelay;
        Refresh("", 4);
        yield return halfDelay;
        sim.LoadingWarmupComplete();
    }
}
