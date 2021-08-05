using UnityEngine;
using UnityEngine.UI;

public class ClockPanelUI : MonoBehaviour
{ 
    [SerializeField]
    GameObject clockFaceGroup;

    [SerializeField]
    Text textCurYear;

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
    
    SimulationManager simulation => SimulationManager.instance;
    TheCursorCzar theCursorCzar => TheCursorCzar.instance;

    private float cursorTimeStep;

    public ComputeBuffer clockEarthStampDataCBuffer;    
    private int maxNumClockEarthStamps = 1024;
    [SerializeField]
    int numTicksPerEarthStamp = 60;
    [SerializeField]
    private float earthSpeed = 1f;
    public ComputeBuffer clockMoonStampDataCBuffer;    
    private int maxNumClockMoonStamps = 1024;
    [SerializeField]
    int numTicksPerMoonStamp = 120;
    public ComputeBuffer clockSunStampDataCBuffer;    
    private int maxNumClockSunStamps = 1024;
    [SerializeField]
    int numTicksPerSunStamp = 1024;

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

    public void Tick() {
        
        float cursorCoordsX = Mathf.Clamp01((theCursorCzar.GetCursorPixelCoords().x) / 360f);
        float cursorCoordsY = Mathf.Clamp01((theCursorCzar.GetCursorPixelCoords().y - 720f) / 360f);                
        float curTimeStep = simulation.simAgeTimeSteps;
        
        cursorTimeStep = Mathf.RoundToInt(curTimeStep * cursorCoordsX);

        float sunOrbitPhase = GetSunOrbitPhase(cursorTimeStep) + Mathf.PI * 0.5f;

        int cursorYear = Mathf.FloorToInt(cursorTimeStep / (float)simulation.GetNumTimeStepsPerYear());
        int seasonInt = Mathf.FloorToInt(cursorTimeStep / (float)simulation.GetNumTimeStepsPerYear() * 4f) % 4;
        currentSeason = (Season)seasonInt;
        textCurYear.text = (cursorYear + 1).ToString() + "\n" + currentSeason.ToString();
        
        clockFaceGroup.transform.localPosition = new Vector3(Mathf.Min(360f, theCursorCzar.GetCursorPixelCoords().x), 300f, 0f);
                
        //**** PLANET!!!!!!
        if(imageClockPlanet) {            
            imageClockPlanet.rectTransform.localPosition = Vector3.zero;            
            imageClockPlanet.rectTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Rad2Deg * sunOrbitPhase);

            imageClockHandA.gameObject.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Rad2Deg * GetPlanetSpinPhase(cursorTimeStep));
            
        }
        // MOON:
        if(imageClockMoon) {            
            Vector2 moonDir = GetMoonDir(cursorTimeStep);
            imageClockMoon.rectTransform.localPosition = new Vector3(moonDir.x * 16f, moonDir.y * 16f, 0f);
            imageClockMoon.rectTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Rad2Deg * sunOrbitPhase);
            
            imageClockHandB.gameObject.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Rad2Deg * GetMoonOrbitPhase(cursorTimeStep));
        }
        // SUN:
        if(imageClockSun) {
            Vector2 sunDir = GetSunDir(cursorTimeStep);
            imageClockSun.rectTransform.localPosition = new Vector3(sunDir.x * 32f, sunDir.y * 32f, 0f);

            imageClockHandC.gameObject.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Rad2Deg * GetSunOrbitPhase(cursorTimeStep));
            
        }
    }

    public float GetPlanetSpinPhase(float timeStep) {
        float phase = clockPlanetRPM * timeStep;
        return phase;
    }
    public float GetMoonOrbitPhase(float timeStep) {
        float phase = clockMoonRPM * timeStep;
        return phase;
    }
    public Vector2 GetMoonDir(float timeStep) {        
        float localPosX = Mathf.Cos(clockMoonRPM * timeStep + Mathf.PI * 0.5f);
        float localPosY = Mathf.Sin(clockMoonRPM * timeStep + Mathf.PI * 0.5f);
        return new Vector2(localPosX, localPosY).normalized;
    }
    public float GetSunOrbitPhase(float timeStep) {
        float phase = clockSunRPM * timeStep;
        return phase;
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
        ClockStampData[] clockEarthStampDataArray = new ClockStampData[maxNumClockEarthStamps];
        
        int numStamps = Mathf.Min(Mathf.RoundToInt((float)simulation.simAgeTimeSteps / (float)numTicksPerEarthStamp), maxNumClockEarthStamps);
        float totalDistanceTraveled = (float)simulation.simAgeTimeSteps * earthSpeed;
        for(int i = 0; i < numStamps; i++) {
            ClockStampData data = new ClockStampData();
            
            float stampWorldPosX = i * numTicksPerEarthStamp * earthSpeed;
            float stampWorldPosY = 0f;

            float xCoord = stampWorldPosX / totalDistanceTraveled;
            float yCoord = stampWorldPosY / totalDistanceTraveled;
            
            float timeStep = xCoord * (float)simulation.simAgeTimeSteps;
            
            data.pos = new Vector3(xCoord, yCoord + 0.8333f, 0f);
            data.radius = clockRadiusEarth / (totalDistanceTraveled * clockZoomSpeed);
            data.color = Color.white;
            data.animPhase = 0.25f;
            data.rotateZ =  GetSunOrbitPhase(timeStep) + Mathf.PI / 2f;
            data.timeStep = timeStep; // cursorTimeStep;

            clockEarthStampDataArray[i] = data;
        }
        clockEarthStampDataCBuffer.SetData(clockEarthStampDataArray);
    }

    public void UpdateMoonStampData() {        
        ClockStampData[] clockMoonStampDataArray = new ClockStampData[maxNumClockMoonStamps];
        
        int numStamps = Mathf.Min(Mathf.RoundToInt((float)simulation.simAgeTimeSteps / (float)numTicksPerMoonStamp), maxNumClockMoonStamps);
        float totalDistanceTraveled = (float)simulation.simAgeTimeSteps * earthSpeed;
        for(int i = 0; i < numStamps; i++) {
            float lerp = (float)i / (float)(numStamps - 1);
            ClockStampData data = new ClockStampData();
            
            float stampWorldPosX = (i * numTicksPerMoonStamp * earthSpeed) + Mathf.Cos(clockMoonRPM * (float)(i * numTicksPerMoonStamp) + Mathf.PI * 0.5f) * clockMoonOrbitRadius;
            float stampWorldPosY = Mathf.Sin(clockMoonRPM * (float)(i * numTicksPerMoonStamp) + Mathf.PI * 0.5f) * clockMoonOrbitRadius;
            
            float xCoord = stampWorldPosX / totalDistanceTraveled;
            float yCoord = stampWorldPosY / totalDistanceTraveled;
                        
            data.pos = new Vector3(xCoord, yCoord + 0.8333f, 0f);
            data.radius = clockRadiusMoon / (totalDistanceTraveled * clockZoomSpeed);
            data.color = Color.white;
            data.animPhase = 0.75f;
            float timeStep = xCoord * (float)simulation.simAgeTimeSteps;
            float angle = GetSunOrbitPhase(timeStep);
            data.rotateZ = angle + Mathf.PI / 2f;
            data.timeStep = timeStep;

            clockMoonStampDataArray[i] = data;
        }
        clockMoonStampDataCBuffer.SetData(clockMoonStampDataArray);
    }

    public void UpdateSunStampData() {        
        ClockStampData[] clockSunStampDataArray = new ClockStampData[maxNumClockSunStamps];
        
        int numStamps = Mathf.Min(Mathf.RoundToInt((float)simulation.simAgeTimeSteps / (float)numTicksPerSunStamp), maxNumClockSunStamps);
        float totalDistanceTraveled = (float)simulation.simAgeTimeSteps * earthSpeed;

        for(int i = 0; i < numStamps; i++) {
            float lerp = (float)i / (float)(numStamps - 1);
            ClockStampData data = new ClockStampData();
            
            float sunOrbitPhase = GetSunOrbitPhase((float)(i * numTicksPerSunStamp)) + Mathf.PI * 0.5f;

            float stampWorldPosX = i * numTicksPerSunStamp * earthSpeed + Mathf.Cos(sunOrbitPhase) * clockSunOrbitRadius;
            float stampWorldPosY = Mathf.Sin(sunOrbitPhase) * clockSunOrbitRadius;

            float xCoord = stampWorldPosX / totalDistanceTraveled;
            float yCoord = stampWorldPosY / totalDistanceTraveled;
            
            data.pos = new Vector3(xCoord, yCoord + 0.8333f, 0f);
            data.radius = clockRadiusSun / (totalDistanceTraveled * clockZoomSpeed);
            data.color = Color.white;
            float timeStep = xCoord * (float)simulation.simAgeTimeSteps;            
            data.animPhase = 0f;
            data.rotateZ = 0f;
            data.timeStep = timeStep;

            clockSunStampDataArray[i] = data;
        }
        clockSunStampDataCBuffer.SetData(clockSunStampDataArray);
    }

    private void OnDisable() {
        clockEarthStampDataCBuffer?.Release();
        clockMoonStampDataCBuffer?.Release();
        clockSunStampDataCBuffer?.Release();
    }
}
