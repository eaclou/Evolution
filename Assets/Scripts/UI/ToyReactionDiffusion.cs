using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToyReactionDiffusion : MonoBehaviour
{
    [SerializeField] ComputeShader computeShaderToyReactionDiffusion;
    
    // UI Elements:
    
    [SerializeField] Image imageRenderTexDisplay;
    [SerializeField] Slider sliderReactionRate;
    [SerializeField] Button buttonResetToy;
    [SerializeField] Button buttonBrushSmall;
    [SerializeField] Button buttonBrushMedium;
    [SerializeField] Button buttonBrushLarge;
    [SerializeField] Button buttonClearCanvas;
    [SerializeField] Image imageParameterSpace;
    [SerializeField] Image imageParameterPos;
    [SerializeField] Toggle toggleParameterAutoDrift;
    [SerializeField] Slider sliderParameterDriftSpeed;
    [SerializeField] Text textDebug;

    [SerializeField] Material displayMat;
    //computeshader
    [SerializeField] Color colorA;
    [SerializeField] Color colorB;
    //renderTargetMain
    //renderTexSwap

    private int numSimulationIters = 1;
    private bool isDrifting;
    private float reactionRate01;
    private float driftSpeed01;
    private float feedRate01;
    private float killRate01;
    private int brushSizeIndex = 0; // 0 small, 1 med, 2 large
    private int colorSchemeIndex = 0; // make some presets, set in inspector, use SOs?
    //TimeScale/numIters    
    //Parameter-Space -- _IsDrifting
    //Parameter-space -- _DriftSpeed // randomwalk killRateXfeedRate
    //killRate, feedRate 0-1 square
    //Change Colors/Visual Settings
    //ClearAll
    //BrushOn/Off
    //brushSize
    [SerializeField] private float killRateMin;
    [SerializeField] private float killRateMax;
    [SerializeField] private float feedRateMin;
    [SerializeField] private float feedRateMax;
    [SerializeField] private float driftSpeedMin;
    [SerializeField] private float driftSpeedMax;
    [SerializeField] private int reactionRateMin;
    [SerializeField] private int reactionRateMax;
    private float diffusionRateA = 1f;
    private float diffusionRateB = 0.5f;

    private float mousePosX01;
    private float mousePosY01;

    private RenderTexture sourceRT;
    private RenderTexture targetRT;

    private float isPainting01;

    private int defaultResolution = 512;
    private bool isInit = false;

    private bool isPresetA = false;
    private bool isPresetB = false;
    private bool isPresetC = false;
    

    public void ResetToy() {
        isInit = false;
        numSimulationIters = 8; // reactionrate
        isDrifting = false;
        reactionRate01 = 0.5f;
        driftSpeed01 = 0.5f; // 0-1 range?
        feedRate01 = 0.5f;
        killRate01 = 0.5f;
        brushSizeIndex = 0; // 0 small, 1 med, 2 large
        colorSchemeIndex = 0;

        if (sourceRT != null) {
            sourceRT.Release();
        }
        sourceRT = new RenderTexture(defaultResolution, defaultResolution, 1, RenderTextureFormat.ARGBFloat);
        sourceRT.enableRandomWrite = true;
        sourceRT.filterMode = FilterMode.Point;
        sourceRT.wrapMode = TextureWrapMode.Repeat;
        sourceRT.Create();   
        
        if (targetRT != null) {
            targetRT.Release();
        }
        targetRT = new RenderTexture(defaultResolution, defaultResolution, 1, RenderTextureFormat.ARGBFloat);
        
        targetRT.enableRandomWrite = true;
        targetRT.filterMode = FilterMode.Point;
        targetRT.wrapMode = TextureWrapMode.Repeat;
        targetRT.Create();

        displayMat.SetTexture("_MainTex", sourceRT);
        //imageRenderTexDisplay.PixelAdjustPoint = sourceRT;

        Initialize();
        UpdateDisplayUI();

        isInit = true;
    }

    public void Initialize() {

        RunComputeShaderInit();
    }
    private void RunComputeShaderInit() {
        
        UpdateComputeShaderParams();
        int kernelCSInit = computeShaderToyReactionDiffusion.FindKernel("CSInit");
        computeShaderToyReactionDiffusion.SetTexture(kernelCSInit, "SourceRT", sourceRT);
        computeShaderToyReactionDiffusion.SetTexture(kernelCSInit, "TargetRT", targetRT);
        computeShaderToyReactionDiffusion.Dispatch(kernelCSInit, defaultResolution / 32, defaultResolution / 32, 1);

        kernelCSInit = computeShaderToyReactionDiffusion.FindKernel("CSInit");
        UpdateComputeShaderParams();
        computeShaderToyReactionDiffusion.SetTexture(kernelCSInit, "SourceRT", targetRT);
        computeShaderToyReactionDiffusion.SetTexture(kernelCSInit, "TargetRT", sourceRT);
        computeShaderToyReactionDiffusion.Dispatch(kernelCSInit, defaultResolution / 32, defaultResolution / 32, 1);
    }
    private void UpdateComputeShaderParams() {
        // Parameters:        
        computeShaderToyReactionDiffusion.SetFloat("_Resolution", defaultResolution);
        computeShaderToyReactionDiffusion.SetFloat("_DiffusionRateA", diffusionRateA);
        computeShaderToyReactionDiffusion.SetFloat("_DiffusionRateB", diffusionRateB);
        float killRate = Mathf.Lerp(killRateMin, killRateMax, killRate01);
        float feedRate = Mathf.Lerp(feedRateMin, feedRateMax, feedRate01);
        if(isPresetA) {
            killRate = 0.03693f;//  k=0.03693   f=0.00583
            feedRate = 0.00583f;
        }
        else if(isPresetB) {

        }
        else if(isPresetC) {

        }
        computeShaderToyReactionDiffusion.SetFloat("_KillRate", killRate);
        computeShaderToyReactionDiffusion.SetFloat("_FeedRate", feedRate);
        computeShaderToyReactionDiffusion.SetFloat("_IsPainting", isPainting01);
        computeShaderToyReactionDiffusion.SetFloat("_MouseCoordX", mousePosX01);
        computeShaderToyReactionDiffusion.SetFloat("_MouseCoordY", mousePosY01);
        computeShaderToyReactionDiffusion.SetFloat("_BrushRadiusA", (brushSizeIndex + 0.065f) * 0.137f);

        displayMat.SetColor("_ColorA", colorA);
        displayMat.SetColor("_ColorB", colorB);
    }

    public void FixedUpdate() {
        if(isInit) {
            int iters = Mathf.FloorToInt(Mathf.Lerp(1, 16, reactionRate01));
            for(int i = 0; i < iters; i++) {
            
                TickComputeShader();
            }
            //Debug.Log(Time.timeScale);
            isPainting01 = 0f; // single frame painting for now

            textDebug.text = "killRate: " + Mathf.Lerp(killRateMin, killRateMax, killRate01) +
                             "\nfeedRate: " + Mathf.Lerp(feedRateMin, feedRateMax, feedRate01) +
                             "\nmouseX: " + mousePosX01 +
                             "\niters: " + iters +
                             "\nt; " + Time.fixedTime;

            if(isDrifting) {
                float driftRate = Mathf.Lerp(driftSpeedMin, driftSpeedMax, driftSpeed01);
                killRate01 = Mathf.Clamp01(killRate01 + UnityEngine.Random.Range(-1f, 1f) * 0.01f * driftRate);
                feedRate01 = Mathf.Clamp01(feedRate01 + UnityEngine.Random.Range(-1f, 1f) * 0.01f * driftRate);
            }
            UpdateDisplayUI();   
        }
            
    }

    public void TickComputeShader() {
        //tick
        int kernelCSReactionDiffusionStep = computeShaderToyReactionDiffusion.FindKernel("CSReactionDiffusionStep");        UpdateComputeShaderParams();
        computeShaderToyReactionDiffusion.SetTexture(kernelCSReactionDiffusionStep, "SourceRT", sourceRT);
        computeShaderToyReactionDiffusion.SetTexture(kernelCSReactionDiffusionStep, "TargetRT", targetRT);        
        computeShaderToyReactionDiffusion.Dispatch(kernelCSReactionDiffusionStep, defaultResolution / 32, defaultResolution / 32, 1);

        //.tock
        kernelCSReactionDiffusionStep = computeShaderToyReactionDiffusion.FindKernel("CSReactionDiffusionStep");        
        computeShaderToyReactionDiffusion.SetTexture(kernelCSReactionDiffusionStep, "SourceRT", targetRT); // !!! SWAP TEXTURES --> send back to orig
        computeShaderToyReactionDiffusion.SetTexture(kernelCSReactionDiffusionStep, "TargetRT", sourceRT);        
        computeShaderToyReactionDiffusion.Dispatch(kernelCSReactionDiffusionStep, defaultResolution / 32, defaultResolution / 32, 1);
    }

    private void UpdateDisplayUI() {
        if(brushSizeIndex == 0) {
            buttonBrushSmall.interactable = false;//*
            buttonBrushMedium.interactable = true;
            buttonBrushLarge.interactable = true;
        }
        else if(brushSizeIndex == 1) {
            buttonBrushSmall.interactable = true;
            buttonBrushMedium.interactable = false;//*
            buttonBrushLarge.interactable = true;
        }
        else if(brushSizeIndex == 2) {
            buttonBrushSmall.interactable = true;
            buttonBrushMedium.interactable = true;
            buttonBrushLarge.interactable = false;//*
        }

        toggleParameterAutoDrift.isOn = isDrifting;

        float pix = imageParameterSpace.rectTransform.rect.width;
        Vector3 parameterLocPos = new Vector3(killRate01 * pix, feedRate01 * pix, 0f);
        imageParameterPos.rectTransform.localPosition = parameterLocPos;

        sliderParameterDriftSpeed.value = driftSpeed01;
        sliderReactionRate.value = reactionRate01;
    }

    public void CursorDownInPaintSpace() {
        Vector2 result;
        Vector2 clickPosition = Input.mousePosition;// eventData.position;
        RectTransform thisRect = imageRenderTexDisplay.rectTransform;// GetComponent<RectTransform>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(thisRect, clickPosition, null, out result);
        //result += thisRect.sizeDelta / 2;
        result.x /= imageRenderTexDisplay.rectTransform.rect.width;
        result.y /= imageRenderTexDisplay.rectTransform.rect.height;

        PaintValues(result);

        isPainting01 = 1f;
    }

    private void PaintValues(Vector2 mouseCoords01) {
        mousePosX01 = mouseCoords01.x;
        mousePosY01 = mouseCoords01.y;
        Debug.Log("Paint at: " + mouseCoords01);
    }
    public void CursorDownInParameterSpace() {

        isDrifting = false;
        
        Vector2 result;
        Vector2 clickPosition = Input.mousePosition;// eventData.position;
        RectTransform thisRect = imageParameterSpace.rectTransform;// GetComponent<RectTransform>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(thisRect, clickPosition, null, out result);
        //result += thisRect.sizeDelta / 2;
        result.x /= imageParameterSpace.rectTransform.rect.width;
        result.y /= imageParameterSpace.rectTransform.rect.height;
        SetParameters(result.x, result.y);
        //Debug.Log("CursorDownInParameterSpace " + result);
    }

    public void ClickSliderReactionRate(float val) {
        reactionRate01 = val;
        //Debug.Log("ClickSliderReactionRate " + val);
        UpdateDisplayUI();
    }

    public void ClickPresetA() {
        isDrifting = false;
        isPresetB = false;
        isPresetC = false;

        isPresetA = true;
    }
    public void ClickPresetB() {

    }
    public void ClickPresetC() {

    }

    public void ClickButtonBrush(int brushSize) {
        brushSizeIndex = brushSize;
        //Debug.Log("ClickButtonBrush " + brushSize);
        UpdateDisplayUI();
    }
    public void ClickButtonClearCanvas() {
        //Debug.Log("ClickButtonClearCanvas");
        //UpdateDisplayUI();
        RunComputeShaderInit();
        UpdateDisplayUI();
    }

    public void ClickSliderDriftSpeed(float val) {
        //Debug.Log("ClickSliderDriftSpeed " + val);
        driftSpeed01 = Mathf.Clamp01(val);
        UpdateDisplayUI();
    }

    public void ClickToggleParameterDrift(bool val) {
        //Debug.Log("ClickToggleParameterDrift " + val);
        isDrifting = val;
        UpdateDisplayUI();
    }

    private void SetParameters(float x, float y) {
        killRate01 = Mathf.Clamp01(x);
        feedRate01 = Mathf.Clamp01(y);
        UpdateDisplayUI();
    }

    public void Open() {
        ResetToy();
    }

    public void Close() {

    }
}
