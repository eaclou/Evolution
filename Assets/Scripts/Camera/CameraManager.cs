using Playcraft;
using UnityEngine;

public class CameraManager : Singleton<CameraManager> 
{
    SimulationManager simulation => SimulationManager.instance;

    public Vector3 curCameraFocusPivotPos;
    public Vector3 curCameraPos;
    public float curTiltAngleDegrees;

    public Vector3 masterTargetCamPosition;

    public bool isFollowingAgent = false;
    public Transform targetAgentTransform;
    public Agent targetAgent;
    public int targetAgentIndex = 0;

    public int mouseHoverAgentIndex = 0;
    public bool isMouseHoverAgent = false;
    public Agent mouseHoverAgentRef;

    public bool isFollowingPlantParticle = false;
    public Vector2 targetPlantWorldPos;
    public bool isFollowingAnimalParticle = false;
    public Vector2 targetZooplanktonWorldPos;

    [SerializeField]
    float camMinAltitude = 5f;
    [SerializeField]
    float camMaxAltitude = 420f;
    
    public float masterTargetDistance = 50f;
    public float masterTargetTiltAngle = 33f;
    public float masterLerpSpeed = 2.5f; 
    
    public float masterPanSpeed = 1f;
    public float masterZoomSpeed = 1f;
    public float masterTiltSpeed = 1f;

    public Camera cameraRef;
    public float worldSpaceCornersDistance = 10f;
    public Vector4 worldSpaceTopLeft;
    public Vector4 worldSpaceTopRight;
    public Vector4 worldSpaceBottomLeft;
    public Vector4 worldSpaceBottomRight;
    public Vector4 worldSpaceCameraRightDir;
    public Vector4 worldSpaceCameraUpDir;

    
    // Use this for initialization
    void Start () {
        InitializeCamera();
        //targetCamPos = new Vector3(128f, 128f, -64f);
    }

    private void InitializeCamera() {
        cameraRef = GetComponent<Camera>();
        curCameraFocusPivotPos = new Vector3(128f, 128f, 1f);
    }

    private void Update() {
        UpdateCam();
    }

    private void UpdateCam() 
    {
        // Calculate where the focus pivot should be:
        if (targetAgentTransform && isFollowingAgent)
        {
            curCameraFocusPivotPos = targetAgentTransform.position;
            curCameraFocusPivotPos.z = -SimulationManager._GlobalWaterLevel * SimulationManager._MaxAltitude; // *** This is where creatures are for now          
        }

        if(isFollowingPlantParticle) {
            curCameraFocusPivotPos.x = targetPlantWorldPos.x;
            curCameraFocusPivotPos.y = targetPlantWorldPos.y;
            //curCameraFocusPivotPos.z = 0f;
        }
        if(isFollowingAnimalParticle) {
            curCameraFocusPivotPos.x = targetZooplanktonWorldPos.x;
            curCameraFocusPivotPos.y = targetZooplanktonWorldPos.y;
            //curCameraFocusPivotPos.z = 0f;
        }

        curTiltAngleDegrees = Mathf.Lerp(curTiltAngleDegrees, -masterTargetTiltAngle, 12f * Time.deltaTime);        
        
        float offsetY = Mathf.Abs(masterTargetDistance) * Mathf.Sin(masterTargetTiltAngle * Mathf.Deg2Rad); // compensate for camera tilt
        float offsetZ = Mathf.Abs(masterTargetDistance) * Mathf.Cos(masterTargetTiltAngle * Mathf.Deg2Rad);
                
        curCameraFocusPivotPos.x = Mathf.Min(curCameraFocusPivotPos.x, SimulationManager._MapSize);
        curCameraFocusPivotPos.x = Mathf.Max(curCameraFocusPivotPos.x, 0f);
        curCameraFocusPivotPos.y = Mathf.Min(curCameraFocusPivotPos.y, SimulationManager._MapSize);
        curCameraFocusPivotPos.y = Mathf.Max(curCameraFocusPivotPos.y, 0f);

        masterTargetCamPosition = curCameraFocusPivotPos;
        masterTargetCamPosition.y -= offsetY;
        masterTargetCamPosition.z -= offsetZ;  // camera is towards the negative Z axis.... a bit awkward.

        // Lerp towards Target Transform Position & Orientation:        
        float relSize = Mathf.Clamp01((masterTargetDistance - camMinAltitude) / (camMinAltitude + camMaxAltitude));

        float minSizeLerpSpeed = 15f;
        float maxSizeLerpSpeed = 5f;
        masterLerpSpeed = Mathf.Lerp(minSizeLerpSpeed, maxSizeLerpSpeed, relSize);
        masterLerpSpeed = Mathf.Max(masterLerpSpeed, 5f);  // cap

        masterTargetDistance = Mathf.Min(masterTargetDistance, camMaxAltitude);
        masterTargetDistance = Mathf.Max(masterTargetDistance, camMinAltitude);        

        curCameraPos = Vector3.Lerp(transform.position, masterTargetCamPosition, masterLerpSpeed * Time.deltaTime);
        transform.position = curCameraPos;
        transform.localEulerAngles = new Vector3(curTiltAngleDegrees, 0f, 0f);
        
        UpdateWorldSpaceCorners();
    }
    
    private void UpdateWorldSpaceCorners() 
    {
        Vector3 botLeft = cameraRef.ScreenToWorldPoint( new Vector3(0f, 0f, worldSpaceCornersDistance));
        worldSpaceBottomLeft = new Vector4(botLeft.x, botLeft.y, botLeft.z, 0f);
        Vector3 topLeft = cameraRef.ViewportToWorldPoint(new Vector3(0f,1f,10f)); //cameraRef.ScreenToWorldPoint( new Vector3(0f, cameraRef.pixelWidth, worldSpaceCornersDistance));
        worldSpaceTopLeft = new Vector4(topLeft.x, topLeft.y, topLeft.z, 0f);
        Vector3 topRight = cameraRef.ScreenToWorldPoint( new Vector3(cameraRef.pixelHeight, cameraRef.pixelWidth, worldSpaceCornersDistance));
        worldSpaceTopRight = new Vector4(topRight.x, topRight.y, topRight.z, 0f);
        Vector3 botRight = cameraRef.ViewportToWorldPoint(new Vector3(1f,0f,10f)); //cameraRef.ScreenToWorldPoint( new Vector3(cameraRef.pixelHeight, 0f, worldSpaceCornersDistance));
        worldSpaceBottomRight = new Vector4(botRight.x, botRight.y, botRight.z, 0f);
        Vector3 camRight = gameObject.transform.right;
        worldSpaceCameraRightDir = new Vector4(camRight.x, camRight.y, camRight.z, 0f);
        Vector3 camUp = gameObject.transform.up;
        worldSpaceCameraUpDir = new Vector4(camUp.x, camUp.y, camUp.z, 0f);
    }

    public void MoveCamera(Vector2 dir) 
    {
        float relSize = Mathf.Clamp01((masterTargetDistance - camMinAltitude) / (camMinAltitude + camMaxAltitude));

        float minSizePanSpeedMult = 0.1f;
        float maxSizePanSpeedMult = 12f;
        float panSpeedMult = Mathf.Lerp(minSizePanSpeedMult, maxSizePanSpeedMult, relSize);
        
        float camPanSpeed = masterPanSpeed * panSpeedMult * Time.deltaTime;

        curCameraFocusPivotPos += new Vector3(dir.x * camPanSpeed, dir.y * camPanSpeed, 0f);
    }
    
    public void TiltCamera(float tiltAngle) 
    {
        float relSize = Mathf.Clamp01((masterTargetDistance - camMinAltitude) / (camMinAltitude + camMaxAltitude));

        float minSizeTiltSpeedMult = 1f;
        float maxSizeTiltSpeedMult = 1f;
        float tiltSpeedMult = Mathf.Lerp(minSizeTiltSpeedMult, maxSizeTiltSpeedMult, relSize);

        masterTargetTiltAngle += tiltAngle * masterTiltSpeed * tiltSpeedMult * Time.deltaTime;

        masterTargetTiltAngle = Mathf.Clamp(masterTargetTiltAngle, 0f, 60f);

    }

    public void ZoomCameraFixed(float zoomValue) 
    {
        if (zoomValue > 0f) zoomValue = 1f;
        else if (zoomValue < 0f) zoomValue = -1f;
        ZoomCamera(zoomValue);
    }
        
    public void ZoomCamera(float zoomValue) 
    {
        float relSize = Mathf.Clamp01((masterTargetDistance - camMinAltitude) / (camMinAltitude + camMaxAltitude));

        float minSizeZoomSpeedMult = 0.075f;
        float maxSizeZoomSpeedMult = 2.5f;
        float zoomSpeedMult = Mathf.Lerp(minSizeZoomSpeedMult, maxSizeZoomSpeedMult, relSize);

        float zoomSpeed = zoomValue * masterZoomSpeed * zoomSpeedMult * Time.deltaTime;

        masterTargetDistance += zoomSpeed;
    }
    
    public void SetTargetAgent() {
        SetTargetAgent(simulation.agents[targetAgentIndex], targetAgentIndex);
    }

    public void SetTargetAgent(Agent agent, int index) 
    {
        //Debug.Log("SetTarget! " + index.ToString());
        targetAgent = agent;
        targetAgentTransform = agent.bodyGO.transform;
        targetAgentIndex = index;
    }

    public void MouseOverAgent(Agent agent, bool clicked)
    {
        if (clicked) 
        {
            SetTargetAgent(agent, agent.index);
            SetFollowing(KnowledgeMapId.Animals);
        }
            
        SetMouseHoverAgent(agent, true);
    }
    
    public void SetMouseHoverAgent(Agent agent, bool value)
    {
        isMouseHoverAgent = value;
        mouseHoverAgentRef = agent;
        mouseHoverAgentIndex = agent ? agent.index : 0;
    }
    
    public void SetFollowing(KnowledgeMapId id)
    {
        isFollowingAgent = id == KnowledgeMapId.Animals;
        isFollowingPlantParticle = id == KnowledgeMapId.Plants;
        isFollowingAnimalParticle = id == KnowledgeMapId.Microbes;
    }

    private Vector2 SmoothApproach(Vector2 pastPosition, Vector2 pastTargetPosition, Vector2 targetPosition, float speed) 
    {
        float t = Time.deltaTime * speed;
        Vector2 v = (targetPosition - pastTargetPosition) / t;
        Vector2 f = pastPosition - pastTargetPosition + v;
        return targetPosition - v + f * Mathf.Exp(-t);
    } 
    public void DidFollowedCreatureDie(Agent agentRef) {

        if(agentRef == targetAgent) {
            isFollowingAgent = false;
            Logger.Log("followed creature died!", true);
        }
        else {

        }
    }
    public Vector4 Get4DFocusBox(Vector2 boxSizeHalf)
    {
        return new Vector4(curCameraFocusPivotPos.x - boxSizeHalf.x,
            curCameraFocusPivotPos.y - boxSizeHalf.y,
            curCameraFocusPivotPos.x + boxSizeHalf.x,
            curCameraFocusPivotPos.y + boxSizeHalf.y);
    }    
}
