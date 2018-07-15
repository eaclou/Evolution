using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour {

    public Camera camera;

    public float targetZoomValueA = 8f;
    public float targetZoomValueB = 18f;
    public float targetZoomValueC = 46f;
    private float targetZoomValue;

    public float perspZoomDistNear = 10f;
    public float perspZoomDistMid = 36f;
    public float perspZoomDistFar = 120f;
    private float perspZoomDist;
    public float targetTiltAngleDegrees = 12.5f;

    public float lerpSpeedA = 1f;
    public float lerpSpeedB = 2f;
    public float lerpSpeedC = 4f;
    private float lerpSpeed;

    public float centeringOffset = 0f;

    public float camMaxSpeed = 1f;
    public float camAccel = 0.05f;

    public Vector3 targetCamPos;
    public Transform targetTransform;
    public Agent targetAgent;
    public int targetCritterIndex = 0;

    Vector2 prevCameraPosition, prevTargetPosition;

    public float orbitRadius = 35f;
    public float orbitSpeed = 1f;
    public float curOrbitAngle = 0f;

    private int debugFrameCounter = 0;

    private GameMode curMode;
    public enum GameMode {
        ModeA,
        ModeB,
        ModeC
    }

    // Use this for initialization
    void Start () {
        ChangeGameMode(GameMode.ModeB);
        //this.transform.position = new Vector3(0f, 0f, -50f);
    }

    public void SetTarget(Agent agent, int index) {
        Debug.Log("SetTarget! " + index.ToString());
        targetAgent = agent;
        targetTransform = agent.bodyGO.transform;
        targetCritterIndex = index;
    }

    private void Update() {
       
        UpdateCameraOld();
    }

    private void UpdateCameraOld() {
        //float orthoLerp = 0.9f;
        float timeScaleLerp = 0.02f;

        //float targetTimeScale = Mathf.Lerp(0.5f, 1.5f, (camera.orthographicSize - 5f) / 40f);
        //Time.timeScale = Mathf.Lerp(Time.timeScale, targetTimeScale, timeScaleLerp);        
        //Time.fixedDeltaTime = Time.timeScale * 0.02f; // Mathf.Lerp(Time.fixedDeltaTime, targetFixedTimeStep, timeScaleLerp);
        float targetPosX = 0f;
        float targetPosY = 0f;
        float targetPosZ = 0f;

        if (targetTransform != null) {
            switch (curMode) {
                case GameMode.ModeA:
                    //
                    targetPosX = targetTransform.position.x; // Mathf.Lerp(targetCamPos.x, targetTransform.position.x, 0.08f);
                    targetPosY = targetTransform.position.y; // Mathf.Lerp(targetCamPos.y, targetTransform.position.y, 0.08f);
                    //targetCamPos = Vector3.Lerp(targetCamPos, targetTransform.position, 0.08f);
                    lerpSpeed = lerpSpeedA;
                    perspZoomDist = perspZoomDistNear;
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
                    perspZoomDist = perspZoomDistMid;
                    //}
                    //orthoLerp = 0.9f;
                    break;
                case GameMode.ModeC:
                    targetPosX = SimulationManager._MapSize * 0.5f; // Mathf.Lerp(targetCamPos.x, 0f, 0.08f);
                    targetPosY = SimulationManager._MapSize * 0.5f; // Mathf.Lerp(targetCamPos.y, 0f, 0.08f);
                    //targetCamPos = Vector3.Lerp(targetCamPos, Vector3.zero, 0.08f);
                    lerpSpeed = lerpSpeedC;
                    perspZoomDist = perspZoomDistFar;
                    //orthoLerp = 0.6f;
                    //
                    break;
                default:
                    //
                    break;
            }
        }        

        targetPosZ = -perspZoomDist;
       
        this.transform.localEulerAngles = new Vector3(-targetTiltAngleDegrees, 0f, 0f);
        float centeringVerticalOffset = Mathf.Abs(targetPosZ) * Mathf.Tan(targetTiltAngleDegrees * Mathf.Deg2Rad); // compensate for camera tilt
        targetPosY -= centeringVerticalOffset;
        centeringOffset = centeringVerticalOffset;

        targetCamPos = new Vector3(targetPosX, targetPosY, targetPosZ);

        this.transform.position = Vector3.Lerp(this.transform.position, targetCamPos, lerpSpeed * Time.deltaTime);
        
    }
  
    
    private Vector2 SmoothApproach(Vector2 pastPosition, Vector2 pastTargetPosition, Vector2 targetPosition, float speed) {
        float t = Time.deltaTime * speed;
        Vector2 v = (targetPosition - pastTargetPosition) / t;
        Vector2 f = pastPosition - pastTargetPosition + v;
        return targetPosition - v + f * Mathf.Exp(-t);
    }

    public void ChangeGameMode(GameMode mode) {
        curMode = mode;
        Debug.Log("ChangeGameMode(" + mode.ToString() + ")");
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
        }
    }
}
