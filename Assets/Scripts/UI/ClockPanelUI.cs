using UnityEngine;
using UnityEngine.UI;

public class ClockPanelUI : MonoBehaviour
{
    public Text textCurYear;

    public Image imageClockHandA;
	public Image imageClockHandB;
    public Image imageClockHandC;

    public Material clockEarthStampMat;
    public Material clockMoonStampMat;
    public Material clockSunStampMat;
    
    SimulationManager simulation => SimulationManager.instance;

    public ComputeBuffer clockEarthStampDataCBuffer;    
    private int maxNumClockEarthStamps = 1024;
    private int numTicksPerEarthStamp = 30;
    private float earthSpeed = 1f;
    public ComputeBuffer clockMoonStampDataCBuffer;    
    private int maxNumClockMoonStamps = 1024;
    private int numTicksPerMoonStamp = 120;
    public ComputeBuffer clockSunStampDataCBuffer;    
    private int maxNumClockSunStamps = 1024;
    private int numTicksPerSunStamp = 2000;

    [SerializeField]
    float clockRadiusEarth;
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
    }

    public void Tick() {
        textCurYear.text = (simulation.curSimYear + 1).ToString();

        int numTicks = simulation.simAgeTimeSteps;
        float angVelA = -2.25f;
        float angVelB = -0.25f;
        float angVelC = -0.002f;
        imageClockHandA.gameObject.transform.rotation = Quaternion.Euler(0f, 0f, (float)numTicks * angVelA);
        imageClockHandB.gameObject.transform.rotation = Quaternion.Euler(0f, 0f, (float)numTicks * angVelB);
        imageClockHandC.gameObject.transform.rotation = Quaternion.Euler(0f, 0f, (float)numTicks * angVelC);
	
    }

    public Vector2 GetMoonDir() {
        float worldPosX = ((float)simulation.simAgeTimeSteps * earthSpeed) + Mathf.Cos(clockMoonRPM * (float)simulation.simAgeTimeSteps + Mathf.PI * 0.5f) * clockMoonOrbitRadius;
        float worldPosY = Mathf.Sin(clockMoonRPM * (float)simulation.simAgeTimeSteps + Mathf.PI * 0.5f) * clockMoonOrbitRadius;

        return new Vector2(worldPosX, worldPosY).normalized;
    }
    public Vector2 GetSunDir() {
        float worldPosX = (float)simulation.simAgeTimeSteps * earthSpeed + Mathf.Cos(clockSunRPM * (float)simulation.simAgeTimeSteps + Mathf.PI * 0.5f) * clockSunOrbitRadius;
        float worldPosY = Mathf.Sin(clockSunRPM * (float)simulation.simAgeTimeSteps + Mathf.PI * 0.5f) * clockSunOrbitRadius;
        return new Vector2(worldPosX, worldPosY).normalized;
    }

    public void InitializeClockBuffers() {
        // 'Earth Stamps
        ClockStampData[] clockEarthStampDataArray = new ClockStampData[maxNumClockEarthStamps];
        clockEarthStampDataCBuffer?.Release();
        clockEarthStampDataCBuffer = new ComputeBuffer(maxNumClockEarthStamps, sizeof(float) * 10);
        for (int i = 0; i < clockEarthStampDataArray.Length; i++) {                
            ClockStampData data = new ClockStampData();
            clockEarthStampDataArray[i] = data;
        }
        clockEarthStampDataCBuffer.SetData(clockEarthStampDataArray);

        // 'Moon Stamps
        ClockStampData[] clockMoonStampDataArray = new ClockStampData[maxNumClockMoonStamps];
        clockMoonStampDataCBuffer?.Release();
        clockMoonStampDataCBuffer = new ComputeBuffer(maxNumClockMoonStamps, sizeof(float) * 10);
        for (int i = 0; i < clockMoonStampDataArray.Length; i++) {                
            ClockStampData data = new ClockStampData();
            clockMoonStampDataArray[i] = data;
        }
        clockMoonStampDataCBuffer.SetData(clockMoonStampDataArray);

        // 'Sun Stamps
        ClockStampData[] clockSunStampDataArray = new ClockStampData[maxNumClockSunStamps];
        clockSunStampDataCBuffer?.Release();
        clockSunStampDataCBuffer = new ComputeBuffer(maxNumClockSunStamps, sizeof(float) * 10);
        for (int i = 0; i < clockSunStampDataArray.Length; i++) {                
            ClockStampData data = new ClockStampData();
            clockSunStampDataArray[i] = data;
        }
        clockSunStampDataCBuffer.SetData(clockSunStampDataArray);
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

            //xCoord = xCoord * 0.8f + 0.1f;  // rescaling --> make this more robust
            //yCoord = yCoord * 0.2f + 0.8f;

            data.pos = new Vector3(xCoord, yCoord + 0.5f, 0f);
            data.radius = clockRadiusEarth / (totalDistanceTraveled * clockZoomSpeed);
            // add scale info
            data.color = Color.white;
            data.animPhase = 0.25f;
            //float xDir = Mathf.Cos(clockSunRPM * (float)(i * numTicksPerSunStamp) + Mathf.PI * 0.5f);
            //float yDir = Mathf.Sin(clockSunRPM * (float)(i * numTicksPerSunStamp) + Mathf.PI * 0.5f);
            float angle = clockSunRPM * (float)(i * numTicksPerSunStamp) + Mathf.PI * 0.5f;
            
            data.rotateZ = angle;

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

            //xCoord = xCoord * 0.8f + 0.1f;  // rescaling --> make this more robust
            //yCoord = yCoord * 0.2f + 0.8f;
            //yCoord = 0.5f; //TEMP!!!

            data.pos = new Vector3(xCoord, yCoord + 0.5f, 0f);
            data.radius = clockRadiusMoon / (totalDistanceTraveled * clockZoomSpeed);
            data.color = Color.white;
            data.animPhase = 0.75f;
            data.rotateZ = 90f;

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
            
            float stampWorldPosX = i * numTicksPerSunStamp * earthSpeed + Mathf.Cos(clockSunRPM * (float)(i * numTicksPerSunStamp) + Mathf.PI * 0.5f) * clockSunOrbitRadius;
            float stampWorldPosY = Mathf.Sin(clockSunRPM * (float)(i * numTicksPerSunStamp) + Mathf.PI * 0.5f) * clockSunOrbitRadius;

            float xCoord = stampWorldPosX / totalDistanceTraveled;
            float yCoord = stampWorldPosY / totalDistanceTraveled;
            //yCoord = 1f; //TEMP!!!

            //xCoord = xCoord * 0.8f + 0.1f;  // rescaling --> make this more robust
            //yCoord = yCoord * 0.2f + 0.8f;

            data.pos = new Vector3(xCoord, yCoord + 0.5f, 0f);
            data.radius = clockRadiusSun / (totalDistanceTraveled * clockZoomSpeed);
            data.color = Color.white;
            data.animPhase = 0f;
            data.rotateZ = 0f;

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
