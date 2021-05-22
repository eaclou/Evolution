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

    public struct ClockStampData { 
        public Vector3 pos;
        public Vector4 color;
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

    public void InitializeClockBuffers() {
        // 'Earth Stamps
        ClockStampData[] clockEarthStampDataArray = new ClockStampData[maxNumClockEarthStamps];
        clockEarthStampDataCBuffer?.Release();
        clockEarthStampDataCBuffer = new ComputeBuffer(maxNumClockEarthStamps, sizeof(float) * 7);
        for (int i = 0; i < clockEarthStampDataArray.Length; i++) {                
            ClockStampData data = new ClockStampData();
            clockEarthStampDataArray[i] = data;
        }
        clockEarthStampDataCBuffer.SetData(clockEarthStampDataArray);

        // 'Moon Stamps
        ClockStampData[] clockMoonStampDataArray = new ClockStampData[maxNumClockMoonStamps];
        clockMoonStampDataCBuffer?.Release();
        clockMoonStampDataCBuffer = new ComputeBuffer(maxNumClockMoonStamps, sizeof(float) * 7);
        for (int i = 0; i < clockMoonStampDataArray.Length; i++) {                
            ClockStampData data = new ClockStampData();
            clockMoonStampDataArray[i] = data;
        }
        clockMoonStampDataCBuffer.SetData(clockMoonStampDataArray);

        // 'Sun Stamps
        ClockStampData[] clockSunStampDataArray = new ClockStampData[maxNumClockSunStamps];
        clockSunStampDataCBuffer?.Release();
        clockSunStampDataCBuffer = new ComputeBuffer(maxNumClockSunStamps, sizeof(float) * 7);
        for (int i = 0; i < clockSunStampDataArray.Length; i++) {                
            ClockStampData data = new ClockStampData();
            clockSunStampDataArray[i] = data;
        }
        clockSunStampDataCBuffer.SetData(clockSunStampDataArray);
    }

    public void UpdateEarthStampData() {        
        ClockStampData[] clockEarthStampDataArray = new ClockStampData[maxNumClockEarthStamps];
        
        int numStamps = Mathf.Min(Mathf.RoundToInt((float)simulation.simAgeTimeSteps / (float)numTicksPerEarthStamp), maxNumClockEarthStamps);
        float totalDistanceTraveled = simulation.simAgeTimeSteps * earthSpeed;
        for(int i = 0; i < numStamps; i++) {
            ClockStampData data = new ClockStampData();
            
            float stampWorldPosX = i * numTicksPerEarthStamp * earthSpeed;
            float stampWorldPosY = 0f;

            float xCoord = Mathf.Clamp01(stampWorldPosX / totalDistanceTraveled);
            float yCoord = Mathf.Clamp01(stampWorldPosY / totalDistanceTraveled);

            xCoord = xCoord * 0.8f + 0.1f;  // rescaling --> make this more robust
            yCoord = yCoord * 0.2f + 0.8f;

            data.pos = new Vector3(xCoord, yCoord, 0f);
            // add scale info
            data.color = Color.white;

            clockEarthStampDataArray[i] = data;
        }
        clockEarthStampDataCBuffer.SetData(clockEarthStampDataArray);
    }

    public void UpdateMoonStampData() {        
        ClockStampData[] clockMoonStampDataArray = new ClockStampData[maxNumClockMoonStamps];
        
        int numStamps = Mathf.Min(Mathf.RoundToInt((float)simulation.simAgeTimeSteps / (float)numTicksPerMoonStamp), maxNumClockMoonStamps);
        float totalDistanceTraveled = simulation.simAgeTimeSteps * earthSpeed;
        for(int i = 0; i < numStamps; i++) {
            ClockStampData data = new ClockStampData();
            
            float stampWorldPosX = i * numTicksPerMoonStamp * earthSpeed;
            float stampWorldPosY = 0f;

            float xCoord = Mathf.Clamp01(stampWorldPosX / totalDistanceTraveled);
            float yCoord = Mathf.Clamp01(stampWorldPosY / totalDistanceTraveled);

            xCoord = xCoord * 0.8f + 0.1f;  // rescaling --> make this more robust
            yCoord = yCoord * 0.2f + 0.8f;
            yCoord = 0.5f; //TEMP!!!

            data.pos = new Vector3(xCoord, yCoord, 0f);
            // add scale info
            data.color = Color.white;

            clockMoonStampDataArray[i] = data;
        }
        clockMoonStampDataCBuffer.SetData(clockMoonStampDataArray);
    }

    public void UpdateSunStampData() {        
        ClockStampData[] clockSunStampDataArray = new ClockStampData[maxNumClockSunStamps];
        
        int numStamps = Mathf.Min(Mathf.RoundToInt((float)simulation.simAgeTimeSteps / (float)numTicksPerSunStamp), maxNumClockSunStamps);
        float totalDistanceTraveled = simulation.simAgeTimeSteps * earthSpeed;
        for(int i = 0; i < numStamps; i++) {
            ClockStampData data = new ClockStampData();
            
            float stampWorldPosX = i * numTicksPerSunStamp * earthSpeed;
            float stampWorldPosY = 0f;

            float xCoord = Mathf.Clamp01(stampWorldPosX / totalDistanceTraveled);
            float yCoord = Mathf.Clamp01(stampWorldPosY / totalDistanceTraveled);
            yCoord = 1f; //TEMP!!!

            xCoord = xCoord * 0.8f + 0.1f;  // rescaling --> make this more robust
            yCoord = yCoord * 0.2f + 0.8f;

            data.pos = new Vector3(xCoord, yCoord, 0f);
            // add scale info
            data.color = Color.white;

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
