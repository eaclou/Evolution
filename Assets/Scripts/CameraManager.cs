using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour {

    public Camera camera;
    /*
    public float targetZoomValueA = 8f;
    public float targetZoomValueB = 18f;
    public float targetZoomValueC = 46f;
    private float targetZoomValue;

    public float perspZoomDistNear = 10f;
    public float perspZoomDistMid = 36f;
    public float perspZoomDistFar = 120f;
    private float perspZoomDist;
    
    
    public float targetTiltAngleA = 10.5f;
    public float targetTiltAngleB = 26.5f;
    public float targetTiltAngleC = 42.5f;

    public float lerpSpeedA = 1f;
    public float lerpSpeedB = 2f;
    public float lerpSpeedC = 4f;
    //private float lerpSpeed;
    */

    public float masterPanSpeed = 1f;
    public float masterZoomSpeed = 1f;
    public float masterTiltSpeed = 1f;

    private float curTiltAngle;
    private float targetTiltAngleDegrees = 12.5f;
    public float centeringOffset = 0f;

    //public float camMaxSpeed = 1f;
    //public float camAccel = 0.05f;

    public Vector3 targetCamPos;
    public Transform targetTransform;
    public Agent targetAgent;
    public int targetCritterIndex = 0;

    public int mouseHoverAgentIndex = 0;
    public bool isMouseHoverAgent = false;
    public Agent mouseHoverAgentRef;

    public float masterTargetDistance = 50f;
    public float masterTiltAngle = 25f;
    public float masterLerpSpeed = 2f;
    

    public bool isFollowing = false;

    Vector2 prevCameraPosition, prevTargetPosition;

    //public float orbitRadius = 35f;
    //public float orbitSpeed = 1f;
   // public float curOrbitAngle = 0f;

    private int debugFrameCounter = 0;

    public float targetAgentSize = 0f;
    public float cameraZoomAmount = 0f;
    /*
    private GameMode curMode;
    public enum GameMode {
        ModeA,
        ModeB,
        ModeC
    }*/

    // Use this for initialization
    void Start () {
        //ChangeGameMode(GameMode.ModeC);
        //this.transform.position = new Vector3(0f, 0f, -50f);

        targetCamPos = new Vector3(128f, 128f, -64f);
    }

    public void SetTarget(Agent agent, int index) {
        Debug.Log("SetTarget! " + index.ToString());
        targetAgent = agent;
        targetTransform = agent.bodyGO.transform;
        targetCritterIndex = index;
    }

    private void Update() {
       
        UpdateCameraNew();
    }

    private void UpdateCameraNew() {

        float targetPosX = targetCamPos.x;
        float targetPosY = targetCamPos.y;
        float targetPosZ = targetCamPos.z;

        targetPosZ = -masterTargetDistance;
        cameraZoomAmount = targetPosZ;

        targetPosX = Mathf.Min(targetPosX, 256f);
        targetPosX = Mathf.Max(targetPosX, 0f);

        targetPosY = Mathf.Min(targetPosY, 320f);
        targetPosY = Mathf.Max(targetPosY, -64f);

        masterTiltAngle = Mathf.Min(masterTiltAngle, 75f); // degree tilt max
        masterTiltAngle = Mathf.Max(masterTiltAngle, 0f); // degree tilt max

        curTiltAngle = Mathf.Lerp(curTiltAngle, -masterTiltAngle, 0.08f);
        float rotateX = curTiltAngle;
        this.transform.localEulerAngles = new Vector3(rotateX, 0f, 0f);

        if (targetTransform != null && isFollowing)
        {
            targetPosX = targetTransform.position.x;
            targetPosY = targetTransform.position.y;
                         
            float centeringVerticalOffset = Mathf.Abs(masterTargetDistance) * Mathf.Tan(masterTiltAngle * Mathf.Deg2Rad); // compensate for camera tilt
            targetPosY -= centeringVerticalOffset;
            centeringOffset = centeringVerticalOffset;

        }
        else
        {
            
        }

        float minDistance = 1f;
        float maxDistance = 256f;
        float relSize = (masterTargetDistance - minDistance) / (minDistance + maxDistance);

        float minSizeLerpSpeed = 12f;
        float maxSizeLerpSpeed = 0.5f;
        masterLerpSpeed = Mathf.Lerp(minSizeLerpSpeed, maxSizeLerpSpeed, relSize);

        masterTargetDistance = Mathf.Min(masterTargetDistance, maxDistance);
        masterTargetDistance = Mathf.Max(masterTargetDistance, minDistance);
        
        
        targetCamPos = new Vector3(targetPosX, targetPosY, targetPosZ);        

        this.transform.position = Vector3.Lerp(this.transform.position, targetCamPos, masterLerpSpeed * Time.deltaTime);

    }

    /*private void UpdateCameraOld() {
        //float orthoLerp = 0.9f;
        float timeScaleLerp = 0.02f;

        //float targetTimeScale = Mathf.Lerp(0.5f, 1.5f, (camera.orthographicSize - 5f) / 40f);
        //Time.timeScale = Mathf.Lerp(Time.timeScale, targetTimeScale, timeScaleLerp);        
        //Time.fixedDeltaTime = Time.timeScale * 0.02f; // Mathf.Lerp(Time.fixedDeltaTime, targetFixedTimeStep, timeScaleLerp);
        float targetPosX = 0f;
        float targetPosY = 0f;
        float targetPosZ = 0f;

        float lerpSpeed = 0.1f;

        if (targetTransform != null) {
            switch (curMode) {
                case GameMode.ModeA:
                    //
                    targetPosX = targetTransform.position.x; // Mathf.Lerp(targetCamPos.x, targetTransform.position.x, 0.08f);
                    targetPosY = targetTransform.position.y; // Mathf.Lerp(targetCamPos.y, targetTransform.position.y, 0.08f);
                    //targetCamPos = Vector3.Lerp(targetCamPos, targetTransform.position, 0.08f);
                    lerpSpeed = lerpSpeedA;
                    float linearAgentSize = Mathf.Lerp(targetAgent.spawnStartingScale, 1f, targetAgent.growthPercentage) * targetAgent.fullSizeBoundingBox.x;

                    perspZoomDist = perspZoomDistNear * linearAgentSize;
                    targetAgentSize = linearAgentSize;
                    targetTiltAngleDegrees = targetTiltAngleA;
                    break;
                case GameMode.ModeB:
                    //
                    targetPosX = targetTransform.position.x;
                    targetPosY = targetTransform.position.y;
                    //targetPosX = Mathf.Lerp(targetCamPos.x, targetTransform.position.x, 0.08f);
                    //targetPosY = Mathf.Lerp(targetCamPos.y, targetTransform.position.y, 0.08f);
                    //if(targetTransform != null) {
                        //targetCamPos = Vector3.Lerp(targetCamPos, targetTransform.position, 0.08f);
                    lerpSpeed = lerpSpeedB;
                    linearAgentSize = Mathf.Lerp(targetAgent.spawnStartingScale, 1f, targetAgent.growthPercentage) * targetAgent.fullSizeBoundingBox.x;
                    targetAgentSize = linearAgentSize;
                    perspZoomDist = perspZoomDistMid * linearAgentSize;
                    targetTiltAngleDegrees = targetTiltAngleB;
                    //}
                    //orthoLerp = 0.9f;
                    break;
                case GameMode.ModeC:
                    targetPosX = SimulationManager._MapSize * 0.5f; // Mathf.Lerp(targetCamPos.x, 0f, 0.08f);
                    targetPosY = SimulationManager._MapSize * 0.4f; // Mathf.Lerp(targetCamPos.y, 0f, 0.08f);
                    //targetCamPos = Vector3.Lerp(targetCamPos, Vector3.zero, 0.08f);
                    lerpSpeed = lerpSpeedC;
                    perspZoomDist = perspZoomDistFar;
                    targetTiltAngleDegrees = targetTiltAngleC;
                    //orthoLerp = 0.6f;
                    //
                    break;
                default:
                    //
                    break;
            }
        }        

        targetPosZ = -perspZoomDist;
        cameraZoomAmount = targetPosZ;

        curTiltAngle = Mathf.Lerp(curTiltAngle, -targetTiltAngleDegrees, 0.08f);
        float rotateX = curTiltAngle;
        this.transform.localEulerAngles = new Vector3(rotateX, 0f, 0f);
        float centeringVerticalOffset = Mathf.Abs(targetPosZ) * Mathf.Tan(targetTiltAngleDegrees * Mathf.Deg2Rad); // compensate for camera tilt
        targetPosY -= centeringVerticalOffset;
        centeringOffset = centeringVerticalOffset;

        targetCamPos = new Vector3(targetPosX, targetPosY, targetPosZ);

        this.transform.position = Vector3.Lerp(this.transform.position, targetCamPos, 0.1f); // lerpSpeed * Time.deltaTime);
        
    }*/
  
    
    private Vector2 SmoothApproach(Vector2 pastPosition, Vector2 pastTargetPosition, Vector2 targetPosition, float speed) {
        float t = Time.deltaTime * speed;
        Vector2 v = (targetPosition - pastTargetPosition) / t;
        Vector2 f = pastPosition - pastTargetPosition + v;
        return targetPosition - v + f * Mathf.Exp(-t);
    }

    public void ChangeGameMode() {
        /*curMode = mode;
        //Debug.Log("ChangeGameMode(" + mode.ToString() + ")");
        switch(curMode) {
            case GameMode.ModeA:
                //
                targetZoomValue = targetZoomValueA;
                break;
            case GameMode.ModeB:
                //
                targetZoomValue = targetZoomValueB;
                break;
            case GameMode.ModeC:
                targetZoomValue = targetZoomValueC;
                //
                break;
            default:
                //
                break;
        }*/
    }
}
