using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class ClockPanelUI : MonoBehaviour
{ 
    UIManager uiManager => UIManager.instance;
    SimulationManager simulation => SimulationManager.instance;
    TheCursorCzar theCursorCzar => TheCursorCzar.instance;
    TheRenderKing renderKing => TheRenderKing.instance;
    
    float curTimeStep => simulation.simAgeTimeSteps;
    CommandBuffer cmdBufferWorldTree => renderKing.cmdBufferWorldTree;
    ComputeBuffer quadVerticesCBuffer => renderKing.quadVerticesCBuffer;

    [SerializeField]
    GameObject clockFaceGroup;

    [SerializeField]
    Text textCurYear;
    [SerializeField]
    Text textDisplayStats;

    [SerializeField]
    Image imageClockHandA;
    [SerializeField]
	Image imageClockHandB;
    [SerializeField]
    Image imageClockHandC;
    [SerializeField]
    Image imageClockPlanet;
    [SerializeField]
    Image imageClockMoon;
    [SerializeField]
    Image imageClockSun;

    [SerializeField]
    Material clockPlanetMatA;
    [SerializeField]
    Material clockMoonMatA;
    [SerializeField]
    Material clockSunMatA;
    
    public Material clockEarthStampMat;    
    public Material clockMoonStampMat;    
    public Material clockSunStampMat;

    public float cursorTimeStep;

    public ComputeBuffer clockEarthStampDataCBuffer;    
    private int maxNumClockEarthStamps = 1024;
    //[SerializeField]
    //int numTicksPerEarthStamp = 60;
    [SerializeField]
    private float earthSpeed = 1f;
    public ComputeBuffer clockMoonStampDataCBuffer;    
    private int maxNumClockMoonStamps = 1024;
    //[SerializeField]
    //int numTicksPerMoonStamp = 120;
    public ComputeBuffer clockSunStampDataCBuffer;    
    private int maxNumClockSunStamps = 1024;
    //[SerializeField]
    //int numTicksPerSunStamp = 1024;

    [SerializeField]
    float clockRadiusEarth;
    [SerializeField]
    float clockPlanetRPM;
    [SerializeField]
    float clockRadiusMoon;
    [SerializeField]
    float clockRadiusSun;
    [SerializeField]
    float clockMoonOrbitRadius;
    [SerializeField]
    float clockSunOrbitRadius;
    [SerializeField]
    float clockZoomSpeed;
    [SerializeField]
    float clockMoonRPM;
    [SerializeField]
    float clockSunRPM;

    public struct ClockStampData { 
        public Vector3 pos;
        public float radius;
        public Vector4 color;
        public float animPhase; // which frame of the flipbook
        public float rotateZ;
        public float timeStep;
    }
    public int GetClockStampDataBufferStride() {
        return sizeof(float) * 11;
    }

    private Season currentSeason;
    private enum Season {
        Summer,
        Autumn,
        Winter,
        Spring
    }
    public void UpdateResourceStatsText() {
        if(!uiManager.historyPanelUI.isResourceMode) {
            textDisplayStats.text = "";
            return; 
        }
        string statsString = "";
        foreach(var resource in simulation.simResourceManager.simResourcesArray) {
            if(resource.resourceDataPointList.Count == 0) {
                return;
            }
            statsString += resource.name + ": ( " + resource.resourceDataPointList[resource.resourceDataPointList.Count - 1].value.ToString("F0") + " / " + resource.GetMaxValue().ToString("F0") + " )\n\n";
        }
        textDisplayStats.text = statsString;
        //Debug.Log(statsString);
    }
    public void Tick() 
    {
        // redundant? could store this higher up and share
        //float cursorCoordsX = Mathf.Clamp01((theCursorCzar.GetCursorPixelCoords().x) / 360f);
        //float cursorCoordsY = Mathf.Clamp01((theCursorCzar.GetCursorPixelCoords().y - 720f) / 360f);                
        //float curTimeStep = simulation.simAgeTimeSteps;

        //cursorTimeStep = curTimeStep;// Mathf.RoundToInt((curTimeStep - uiManager.historyPanelUI.graphBoundsMinX) * cursorCoordsX);
        float clockFacePosX = HistoryPanelUI.panelSizePixels;
        if(uiManager.historyPanelUI.mouseWithinPanelBounds) {
            cursorTimeStep = Mathf.RoundToInt(Mathf.Lerp(uiManager.historyPanelUI.graphBoundsMinX, uiManager.historyPanelUI.graphBoundsMaxX, uiManager.historyPanelUI.mousePosPanelCoords.x / HistoryPanelUI.panelSizePixels));
            clockFacePosX = uiManager.historyPanelUI.mousePosPanelCoords.x;
        }
        else {
            cursorTimeStep = curTimeStep;
        }
        float sunOrbitPhase = GetSunOrbitPhase(cursorTimeStep) + Mathf.PI * 0.5f;

        //int cursorYear = Mathf.FloorToInt(cursorTimeStep / (float)simulation.GetNumTimeStepsPerYear());
        //int monthInt = Mathf.FloorToInt(cursorTimeStep / (float)simulation.GetNumTimeStepsPerYear() * 12f) % 12;
        //int dayInt = Mathf.FloorToInt(cursorTimeStep / (float)simulation.GetNumTimeStepsPerYear() * 365f) % 365;
        textCurYear.text = ConvertFramesToTimeSpanString(cursorTimeStep);// dayInt.ToString();// + "/ " + monthInt + "/ " + (cursorYear + 0).ToString() + "\n(" + cursorTimeStep + ")";
        
        clockFaceGroup.transform.localPosition = new Vector3(Mathf.Max(0f,Mathf.Min(HistoryPanelUI.panelSizePixels, clockFacePosX)), 324f, 0f);
                
        //**** PLANET!!!!!!
        clockPlanetMatA.SetFloat("_CurFrame", (cursorTimeStep / simulation.GetNumTimeStepsPerYear() * 365f * 16f + 4) % 16);
        if (imageClockPlanet) {            
            imageClockPlanet.rectTransform.localPosition = Vector3.zero;            
            imageClockPlanet.rectTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Rad2Deg * sunOrbitPhase);
            imageClockHandA.gameObject.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Rad2Deg * GetPlanetSpinPhase(cursorTimeStep));
        }
        // MOON:
        if (imageClockMoon) {            
            Vector2 moonDir = GetMoonDir(cursorTimeStep);
            imageClockMoon.rectTransform.localPosition = new Vector3(moonDir.x * 8f, moonDir.y * 8f, 0f);
            imageClockMoon.rectTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Rad2Deg * sunOrbitPhase);
            imageClockHandB.gameObject.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Rad2Deg * GetMoonOrbitPhase(cursorTimeStep));
        }
        // SUN:
        if (imageClockSun) {
            Vector2 sunDir = GetSunDir(cursorTimeStep);
            imageClockSun.rectTransform.localPosition = new Vector3(sunDir.x * 16f, sunDir.y * 16f, 0f);
            imageClockHandC.gameObject.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Rad2Deg * GetSunOrbitPhase(cursorTimeStep));
        }        
        
        UpdateSunStampData();
        UpdateMoonStampData();
        UpdateEarthStampData();

    }

    public float GetPlanetSpinPhase(float timeStep) {
        float dayCount = timeStep / simulation.GetNumTimeStepsPerYear() * 365f;
        float phase = (dayCount % 1f) * Mathf.PI * 2f;
        return -phase;
    }
    
    public float GetMoonOrbitPhase(float timeStep) {
        float monthCount = timeStep / simulation.GetNumTimeStepsPerYear() * 12f;
        float phase = (monthCount % 1f) * Mathf.PI * 2f;
        return -phase;
    }
    
    public Vector2 GetMoonDir(float timeStep) {        
        float localPosX = Mathf.Cos(GetMoonOrbitPhase(timeStep) + Mathf.PI * 0.5f);
        float localPosY = Mathf.Sin(GetMoonOrbitPhase(timeStep) + Mathf.PI * 0.5f);
        return new Vector2(localPosX, localPosY).normalized;
    }
    
    public float GetSunOrbitPhase(float timeStep) {
        float phase = ((timeStep / simulation.GetNumTimeStepsPerYear()) % 1f) * Mathf.PI * 2f;
        return -phase;
    }
    
    public Vector2 GetSunDir(float timeStep) {        
        float localPosX = Mathf.Cos(GetSunOrbitPhase(timeStep) + Mathf.PI * 0.5f);
        float localPosY = Mathf.Sin(GetSunOrbitPhase(timeStep) + Mathf.PI * 0.5f);
        return new Vector2(localPosX, localPosY).normalized;
    }

    public void InitializeClockBuffers() {

        // 'Earth Stamps
        ClockStampData[] clockEarthStampDataArray = new ClockStampData[maxNumClockEarthStamps];
        clockEarthStampDataCBuffer?.Release();
        clockEarthStampDataCBuffer = new ComputeBuffer(maxNumClockEarthStamps, GetClockStampDataBufferStride());
        for (int i = 0; i < clockEarthStampDataArray.Length; i++) {                
            ClockStampData data = new ClockStampData();
            clockEarthStampDataArray[i] = data;
        }
        clockEarthStampDataCBuffer.SetData(clockEarthStampDataArray);

        // 'Moon Stamps
        ClockStampData[] clockMoonStampDataArray = new ClockStampData[maxNumClockMoonStamps];
        clockMoonStampDataCBuffer?.Release();
        clockMoonStampDataCBuffer = new ComputeBuffer(maxNumClockMoonStamps, GetClockStampDataBufferStride());
        for (int i = 0; i < clockMoonStampDataArray.Length; i++) {                
            ClockStampData data = new ClockStampData();
            clockMoonStampDataArray[i] = data;
        }
        clockMoonStampDataCBuffer.SetData(clockMoonStampDataArray);

        // 'Sun Stamps
        ClockStampData[] clockSunStampDataArray = new ClockStampData[maxNumClockSunStamps];
        clockSunStampDataCBuffer?.Release();
        clockSunStampDataCBuffer = new ComputeBuffer(maxNumClockSunStamps, GetClockStampDataBufferStride());
        for (int i = 0; i < clockSunStampDataArray.Length; i++) {                
            ClockStampData data = new ClockStampData();
            clockSunStampDataArray[i] = data;
        }
        clockSunStampDataCBuffer.SetData(clockSunStampDataArray);

        clockPlanetMatA.SetFloat("_NumRows", 4f);
        clockPlanetMatA.SetFloat("_NumColumns", 4f);
        clockMoonMatA.SetFloat("_NumRows", 4f);
        clockMoonMatA.SetFloat("_NumColumns", 4f);
        clockSunMatA.SetFloat("_NumRows", 4f);
        clockSunMatA.SetFloat("_NumColumns", 4f);
    }

    public void UpdateEarthStampData() {
        float timeRange = (uiManager.historyPanelUI.graphBoundsMaxX - uiManager.historyPanelUI.graphBoundsMinX);
        ClockStampData[] clockEarthStampDataArray = new ClockStampData[maxNumClockEarthStamps];
        float timeStepsPerStamp = simulation.GetNumTimeStepsPerYear() / 365f;
        float stampsTotalLength = maxNumClockEarthStamps * timeStepsPerStamp;
        int simAgeStampCount = Mathf.FloorToInt(simulation.simAgeTimeSteps / timeStepsPerStamp);
        int stampOffsetNum = 0;// 1 - maxNumClockEarthStamps; //simulation.simAgeTimeSteps
        
        if(simAgeStampCount > maxNumClockEarthStamps) {
            stampOffsetNum = Mathf.Max(0, simAgeStampCount - maxNumClockEarthStamps);
        }
        
        float offsetDistanceTime = stampOffsetNum * timeStepsPerStamp;//simulation.simAgeTimeSteps - stampsTotalLength;

        for (int i = 0; i < maxNumClockEarthStamps; i++) {
            ClockStampData data = new ClockStampData();
            
            float stampTimeStep = offsetDistanceTime + i * timeStepsPerStamp;
            
            float xCoord = (stampTimeStep - uiManager.historyPanelUI.graphBoundsMinX) / timeRange;
            float yCoord = 0f;// stampWorldPosY / totalDistanceTraveled;
            
            data.pos = new Vector3(xCoord * uiManager.historyPanelUI.displayWidth + uiManager.historyPanelUI.marginLeft, yCoord + 0.9f, 0f);
            data.radius = clockRadiusEarth / timeRange; // zoom speed? right approach?
            data.color = new Color(0.55f, 1f, 0.65f);
            data.animPhase = 0.75f;
            data.rotateZ =  GetSunOrbitPhase(stampTimeStep);
            data.timeStep = stampTimeStep;

            clockEarthStampDataArray[i] = data;
        }
        clockEarthStampDataCBuffer.SetData(clockEarthStampDataArray);
    }

    public void UpdateMoonStampData() {        
        float timeRange = (uiManager.historyPanelUI.graphBoundsMaxX - uiManager.historyPanelUI.graphBoundsMinX);
        ClockStampData[] clockMoonStampDataArray = new ClockStampData[maxNumClockMoonStamps];
        float timeStepsPerStamp = simulation.GetNumTimeStepsPerYear() / 12f;
        int simAgeStampCount = Mathf.FloorToInt(simulation.simAgeTimeSteps / timeStepsPerStamp);
        int stampOffsetNum = 0;
        if(simAgeStampCount > maxNumClockSunStamps) {
            stampOffsetNum = Mathf.Max(0, simAgeStampCount - maxNumClockSunStamps);
        }
        float offsetDistanceTime = stampOffsetNum * timeStepsPerStamp;
        
        for (int i = 0; i < maxNumClockMoonStamps; i++) {
            //float lerp = (float)i / (float)(numStamps - 1);
            ClockStampData data = new ClockStampData();
            float stampTimeStep = offsetDistanceTime + i * timeStepsPerStamp;
            float stampWorldPosX = stampTimeStep + Mathf.Cos(clockMoonRPM * (float)(i * timeStepsPerStamp) + Mathf.PI * 0.5f) * clockMoonOrbitRadius; // for now
            float stampWorldPosY = Mathf.Sin(clockMoonRPM * (float)(i * timeStepsPerStamp) + Mathf.PI * 0.5f) * clockMoonOrbitRadius;

            float xCoord = (stampWorldPosX - uiManager.historyPanelUI.graphBoundsMinX) / timeRange;
            float yCoord = stampWorldPosY / timeRange;
                     
            data.pos = new Vector3(xCoord, yCoord + 0.9f, 0f);
            data.radius = clockRadiusMoon / timeRange;// clockRadiusMoon / (totalDistanceTraveled * clockZoomSpeed);
            data.color = Color.white;
            data.animPhase = 0.75f;
            
            float angle = GetSunOrbitPhase(stampTimeStep);
            data.rotateZ = angle;
            data.timeStep = stampTimeStep;

            clockMoonStampDataArray[i] = data;
        }
        clockMoonStampDataCBuffer.SetData(clockMoonStampDataArray);
    }

    public void UpdateSunStampData() {  
        float timeRange = (uiManager.historyPanelUI.graphBoundsMaxX - uiManager.historyPanelUI.graphBoundsMinX);
        ClockStampData[] clockSunStampDataArray = new ClockStampData[maxNumClockSunStamps];
        float timeStepsPerStamp = simulation.GetNumTimeStepsPerYear();
        int simAgeStampCount = Mathf.FloorToInt(simulation.simAgeTimeSteps / timeStepsPerStamp);
        int stampOffsetNum = 0;// 1 - maxNumClockEarthStamps; //simulation.simAgeTimeSteps
        if(simAgeStampCount > maxNumClockSunStamps) {
            stampOffsetNum = Mathf.Max(0, simAgeStampCount - maxNumClockSunStamps);
        }
        float offsetDistanceTime = stampOffsetNum * timeStepsPerStamp;
        
        for (int i = 0; i < maxNumClockSunStamps; i++) {
        //for(int i = 0; i < numStamps; i++) {
            ClockStampData data = new ClockStampData();
            float stampTimeStep = offsetDistanceTime + i * timeStepsPerStamp;
            //float stampTimeStep = i * simulation.GetNumTimeStepsPerYear();
            float stampWorldPosX = stampTimeStep + Mathf.Cos(clockSunRPM * (float)(i * simulation.GetNumTimeStepsPerYear()) + Mathf.PI * 0.5f) * clockSunOrbitRadius; // for now
            float stampWorldPosY = Mathf.Sin(clockSunRPM * (float)(i * simulation.GetNumTimeStepsPerYear()) + Mathf.PI * 0.5f) * clockSunOrbitRadius;

            float xCoord = (stampWorldPosX - uiManager.historyPanelUI.graphBoundsMinX) / timeRange;
            float yCoord = stampWorldPosY / timeRange;
                     
            data.pos = new Vector3(xCoord, yCoord + 0.9f, 0f);
            data.radius = clockRadiusSun / timeRange;
            data.color = Color.white;
            data.animPhase = 1f;
            
            //float angle = GetSunOrbitPhase(stampTimeStep);
            //data.rotateZ = angle;
            data.timeStep = stampTimeStep;

            clockSunStampDataArray[i] = data;
            
        }
        clockSunStampDataCBuffer.SetData(clockSunStampDataArray);
    }
    
    public void RefreshMaterials()
    {
        RefreshMaterial(clockEarthStampMat, clockEarthStampDataCBuffer);
        RefreshMaterial(clockMoonStampMat, clockMoonStampDataCBuffer);
        RefreshMaterial(clockSunStampMat, clockSunStampDataCBuffer);
    }

    public string ConvertFramesToTimeSpanString(float numFrames) {
        //float ageFrames = numFrames;
        float ageDays = (numFrames / simulation.GetNumTimeStepsPerYear() * 365f) % 365f;
        float ageMonths = (numFrames / simulation.GetNumTimeStepsPerYear() * 12f) % 12f;
        float ageYears = numFrames / simulation.GetNumTimeStepsPerYear();
        string ageString = "";
        if(ageYears >= 1f) {
            ageString += "YEAR " + (Mathf.FloorToInt(ageYears) + 1) + ", \n";
        }
        if(ageMonths >= 1f) {
            ageString += "MONTH " + (Mathf.FloorToInt(ageMonths) + 1) + ", \n";
        }
        if(ageDays >= 1f) {            
            ageString += "DAY " + (Mathf.FloorToInt(ageDays) % 31);
            if(numFrames < simulation.GetNumTimeStepsPerYear() / 12f) { // first month
                ageString += ", HOUR " + (Mathf.FloorToInt(((numFrames / simulation.GetNumTimeStepsPerYear() * 365f * 24f) % 24f)) + 1);
            }
        }
        if(numFrames < simulation.GetNumTimeStepsPerYear() / 365f) {  // less than one day
            ageString = "DAY 1, HOUR " + (Mathf.FloorToInt(numFrames / simulation.GetNumTimeStepsPerYear() * 365f * 24f) + 1);
        }

        return ageString;
    }

    public string ConvertFramesToAgeString(float numFrames) {
        //float ageFrames = numFrames;
        float ageDays = (numFrames / simulation.GetNumTimeStepsPerYear() * 365f) % 365f;
        float ageMonths = (numFrames / simulation.GetNumTimeStepsPerYear() * 12f) % 12f;
        float ageYears = numFrames / simulation.GetNumTimeStepsPerYear();
        string ageString = "";
        if(ageYears >= 1f) {
            ageString += Mathf.FloorToInt(ageYears) + " Year";
            if (ageYears >= 2f) {
                ageString += "s";
            }
            ageString += ", ";
        }
        if(ageMonths >= 1f) {
            ageString += Mathf.FloorToInt(ageMonths) + " Month";
            if (ageMonths >= 2f) {
                ageString += "s";
            }
            ageString += ", ";
        }
        if(ageDays >= 1f) {
            ageString += Mathf.FloorToInt(ageDays) % 31 + " Day";
            
            if (ageDays >= 2f) {
                ageString += "s";
            }

            if(numFrames < simulation.GetNumTimeStepsPerYear() / 12f) { // first month also include hours
                ageString += ", " + ((numFrames / simulation.GetNumTimeStepsPerYear() * 365f * 24f) % 24f).ToString("F0") + " Hours";
            }
        }
        else {

        }
        if(numFrames < simulation.GetNumTimeStepsPerYear() / 365f) {  // less than 1 day
            ageString = (numFrames / simulation.GetNumTimeStepsPerYear() * 365f * 24f).ToString("F0") + " Hours";
        }

        return ageString;
    }
    
    void RefreshMaterial(Material material, ComputeBuffer computeBuffer)
    {
        material.SetPass(0);
        material.SetBuffer("quadVerticesCBuffer", quadVerticesCBuffer);
        material.SetBuffer("clockOrbitLineDataCBuffer", computeBuffer);        
        material.SetFloat("_CurFrame", cursorTimeStep * 1f % 16);
        material.SetFloat("_NumRows", 4f);
        material.SetFloat("_NumColumns", 4f);
        cmdBufferWorldTree.DrawProcedural(Matrix4x4.identity, material, 0, MeshTopology.Triangles, 6, computeBuffer.count);
    }

    private void OnDisable() {
        clockEarthStampDataCBuffer?.Release();
        clockMoonStampDataCBuffer?.Release();
        clockSunStampDataCBuffer?.Release();
    }
}
