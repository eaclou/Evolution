using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuBackgroundGenerator : MonoBehaviour
{
    [SerializeField] ComputeShader computeShaderMainMenuBG; 
    [SerializeField] Image imageRenderTexDisplay;
    [SerializeField] Material displayMat;
        
    [SerializeField]
    public List<VoronoiRegion> voronoiRegionsList;
    private Vector2[] voronoiArray;
    private ComputeBuffer voronoiComputeBuffer; // use voronoi as basis for caustics?
    //[ReadOnly]
    //public Vector3 res;
    [SerializeField]
    public float wiggleRadius = 256f;
    [SerializeField]
    public float wiggleSpeed = 0.75235f;

    // computeShader;
    // DisplayRenderTexture;
    // shader needs to do stuff;
    // Regions should instead be ScriptableObjects/MonoBehaviors and Set within UI panel?
    //shader may have to be hardcoded for the Region-specific graphics?
    //animate transion Lerp variables and pipe that into gpuShader for animated transitions
    private RenderTexture sourceRT;
    private RenderTexture targetRT;
    private float mousePosX01;
    private float mousePosY01;
    private bool isInit = false;
    private int defaultResolutionX = 1920;
    private int defaultResolutionY = 1080;
    
    // Start is called before the first frame update
    void Start()
    {
        //res.x = Screen.currentResolution.width;
        //res.y = Screen.currentResolution.height;
        if(voronoiRegionsList == null) {
            voronoiRegionsList = new List<VoronoiRegion>();
            Debug.LogError("no voronoi regions list!!");
        }
        RefreshRegionsCenterPixelPos();

        Initialize(); // shader stuffs
    }

    public void FixedUpdate() {
        if (isInit) {            

            TickComputeShader();
        }  
    }

    public void Initialize() {

        defaultResolutionX = Screen.currentResolution.width;
        defaultResolutionY = Screen.currentResolution.height;

        if (sourceRT != null) {
            sourceRT.Release();
        }
        sourceRT = new RenderTexture(defaultResolutionX, defaultResolutionY, 1, RenderTextureFormat.ARGBFloat);
        sourceRT.enableRandomWrite = true;
        sourceRT.filterMode = FilterMode.Point;
        sourceRT.wrapMode = TextureWrapMode.Repeat;
        sourceRT.Create();   
        
        if (targetRT != null) {
            targetRT.Release();
        }
        targetRT = new RenderTexture(defaultResolutionX, defaultResolutionY, 1, RenderTextureFormat.ARGBFloat);
        
        targetRT.enableRandomWrite = true;
        targetRT.filterMode = FilterMode.Point;
        targetRT.wrapMode = TextureWrapMode.Repeat;
        targetRT.Create();

        displayMat.SetTexture("_MainTex", sourceRT);
             
        UpdateComputeShaderParams();
        int kernelCSInit = computeShaderMainMenuBG.FindKernel("CSInit");
        computeShaderMainMenuBG.SetTexture(kernelCSInit, "SourceRT", sourceRT);
        computeShaderMainMenuBG.SetTexture(kernelCSInit, "TargetRT", targetRT);
        computeShaderMainMenuBG.Dispatch(kernelCSInit, defaultResolutionX / 32, defaultResolutionY / 32, 1);

        kernelCSInit = computeShaderMainMenuBG.FindKernel("CSInit");
        UpdateComputeShaderParams();
        computeShaderMainMenuBG.SetTexture(kernelCSInit, "SourceRT", targetRT);
        computeShaderMainMenuBG.SetTexture(kernelCSInit, "TargetRT", sourceRT);
        computeShaderMainMenuBG.Dispatch(kernelCSInit, defaultResolutionX / 32, defaultResolutionY / 32, 1);


        isInit = true;
    }
    private void UpdateComputeShaderParams() {
                
        // Parameters:        
        computeShaderMainMenuBG.SetFloat("_ResolutionX", defaultResolutionX);    
        computeShaderMainMenuBG.SetFloat("_ResolutionY", defaultResolutionY);   
        computeShaderMainMenuBG.SetFloat("_MouseCoordX", mousePosX01);
        computeShaderMainMenuBG.SetFloat("_MouseCoordY", mousePosY01);        
        computeShaderMainMenuBG.SetFloat("_Time", Time.realtimeSinceStartup);
    }

    public void TickComputeShader() {
        
        voronoiArray = new Vector2[32];
        for(int i = 0; i < voronoiArray.Length; i++) {
            voronoiArray[i] = new Vector2(0f, 0f);
            if (i < voronoiRegionsList.Count) {
                //float wiggleRadius = 256f;
                //float wiggleSpeed = 0.75235f;
                //Vector3 GameObjectPosition = voronoiRegionsList[i].GO.transform.position;
                //RectTransform rect = voronoiRegionsList[i].GetComponent<RectTransform>();
                //var centerPoint= rectTransform.TransformPoint(rectTransform.rect.center);
                Vector3 gameObjectPosition = voronoiRegionsList[i].GO.GetComponent<RectTransform>().position;
                //gameObjectPosition.x 
                voronoiRegionsList[i].rootPixelPos.x = gameObjectPosition.x + wiggleRadius * Mathf.Sin((float)i*1.793f + Time.realtimeSinceStartup*0.7351f*wiggleSpeed);
                voronoiRegionsList[i].rootPixelPos.y = gameObjectPosition.y + wiggleRadius * Mathf.Cos((float)i*-2.1289f + Time.realtimeSinceStartup*0.97f*wiggleSpeed);
                //rect.anchoredPosition;// new Vector3(i * 40, 500, 0);// Camera.main.WorldToScreenPoint(rect.position);
                voronoiArray[i] = new Vector2(voronoiRegionsList[i].rootPixelPos.x, voronoiRegionsList[i].rootPixelPos.y);
            }
            
        }
        if(voronoiComputeBuffer != null) {
            voronoiComputeBuffer.Dispose();
        }
        voronoiComputeBuffer = new ComputeBuffer(voronoiArray.Length, sizeof(float) * 2);
        voronoiComputeBuffer.SetData(voronoiArray);
        //tick
        UpdateComputeShaderParams();
        int kernelCSTick = computeShaderMainMenuBG.FindKernel("CSTick");
        computeShaderMainMenuBG.SetBuffer(kernelCSTick, "VoronoiBuffer", voronoiComputeBuffer);
        computeShaderMainMenuBG.SetTexture(kernelCSTick, "SourceRT", sourceRT);
        computeShaderMainMenuBG.SetTexture(kernelCSTick, "TargetRT", targetRT);        
        computeShaderMainMenuBG.Dispatch(kernelCSTick, defaultResolutionX / 32, defaultResolutionY / 32, 1);

        //.tock
        kernelCSTick = computeShaderMainMenuBG.FindKernel("CSTick");   
        computeShaderMainMenuBG.SetBuffer(kernelCSTick, "VoronoiBuffer", voronoiComputeBuffer);
        computeShaderMainMenuBG.SetTexture(kernelCSTick, "SourceRT", targetRT); // !!! SWAP TEXTURES --> send back to orig
        computeShaderMainMenuBG.SetTexture(kernelCSTick, "TargetRT", sourceRT);        
        computeShaderMainMenuBG.Dispatch(kernelCSTick, defaultResolutionX / 32, defaultResolutionY / 32, 1);
    }

    void RefreshRegionsCenterPixelPos() {
        for(int i = 0; i < voronoiRegionsList.Count; i++) {
            VoronoiRegion region = voronoiRegionsList[i];

            Vector3 newPixelPos = Vector3.zero;
            newPixelPos.x = region.rootCoords01.x * Screen.currentResolution.width;
            newPixelPos.y = region.rootCoords01.y * Screen.currentResolution.height;
            region.rootPixelPos = newPixelPos;
            voronoiRegionsList[i] = region;
        };
    }

    public int FindNearestRegion(Vector3 PixelPosition) {  // for mouseclick determine which region/button is pressed
        int closestID = -1;
        float closestDistance = float.PositiveInfinity;
        for(int i = 0; i < voronoiRegionsList.Count; i++) {// var region in voronoiRegionsList) {
            VoronoiRegion region = voronoiRegionsList[i];
            float sqDistanceToPixel = (region.rootPixelPos - PixelPosition).sqrMagnitude;
            if(sqDistanceToPixel < closestDistance) {
                closestDistance = sqDistanceToPixel;
                closestID = i;
            }
        }
        if(closestID == -1) {
            Debug.LogError("no regions!");
        }

        return closestID;
    }

    private void OnDisable() {
        if (voronoiComputeBuffer != null) {
            voronoiComputeBuffer.Dispose();
        }
    }
}
