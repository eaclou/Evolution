﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour {

    public Vector3 curCameraFocusPivotPos;
    private Vector3 prevCameraFocusPivotPos;
    public Vector3 curCameraPos;
    private Vector3 prevCameraPos;
    public float curTiltAngleDegrees;
    private float prevTiltAngleDegrees;

    public Vector3 masterTargetCamPosition;

    public bool isFollowing = false;
    public Transform targetTransform;
    public Agent targetAgent;
    public int targetCritterIndex = 0;

    public int mouseHoverAgentIndex = 0;
    public bool isMouseHoverAgent = false;
    public Agent mouseHoverAgentRef;

    public float masterTargetDistance = 50f;
    public float masterTargetTiltAngle = 25f;
    public float masterLerpSpeed = 2.5f; 
    
    public float masterPanSpeed = 1f;
    public float masterZoomSpeed = 1f;
    public float masterTiltSpeed = 1f;
    //private float curTiltAngle;
    //public float centeringOffset = 0f;
    //public Vector3 targetCamPos;
    //Vector2 prevCameraPosition, prevTargetPosition;
    //public float targetAgentSize = 0f;
    //public float cameraZoomAmount = 0f;
    
    // Use this for initialization
    void Start () {
        InitializeCamera();
        //targetCamPos = new Vector3(128f, 128f, -64f);
    }

    private void InitializeCamera() {
        curCameraFocusPivotPos = new Vector3(128f, 128f, 1f);
    }

    private void Update() {
       
        UpdateCam();
    }

    private void UpdateCam() {
        // Calculate where the focus pivot should be:
        if (targetTransform != null && isFollowing)
        {
            curCameraFocusPivotPos = targetTransform.position;
            curCameraFocusPivotPos.z = 1.0f; // *** for now this is where creatures are for now           
        }

        curTiltAngleDegrees = Mathf.Lerp(curTiltAngleDegrees, -masterTargetTiltAngle, 3.5f * Time.deltaTime);        
        // Calculate Target cam pos:
        //float rotateX = curTiltAngleDegrees;
        //this.transform.localEulerAngles = new Vector3(rotateX, 0f, 0f);

        float offsetY = Mathf.Abs(masterTargetDistance) * Mathf.Sin(masterTargetTiltAngle * Mathf.Deg2Rad); // compensate for camera tilt
        float offsetZ = Mathf.Abs(masterTargetDistance) * Mathf.Cos(masterTargetTiltAngle * Mathf.Deg2Rad);

        
        curCameraFocusPivotPos.x = Mathf.Min(curCameraFocusPivotPos.x, 256f);
        curCameraFocusPivotPos.x = Mathf.Max(curCameraFocusPivotPos.x, 0f);
        curCameraFocusPivotPos.y = Mathf.Min(curCameraFocusPivotPos.y, 256f);
        curCameraFocusPivotPos.y = Mathf.Max(curCameraFocusPivotPos.y, 0f);

        masterTargetCamPosition = curCameraFocusPivotPos;
        masterTargetCamPosition.y -= offsetY;
        masterTargetCamPosition.z -= offsetZ;  // camera is towards the negative Z axis.... a bit awkward.

        //masterTargetCamPosition.x = Mathf.Min(masterTargetCamPosition.x, 256f);
        //masterTargetCamPosition.x = Mathf.Max(masterTargetCamPosition.x, 0f);

        //masterTargetCamPosition.y = Mathf.Min(masterTargetCamPosition.y, 256f);
        //masterTargetCamPosition.y = Mathf.Max(masterTargetCamPosition.y, 0f);

        // Lerp towards Target Transform Position & Orientation:
        float minDistance = 2.5f;
        float maxDistance = 256f;
        float relSize = (masterTargetDistance - minDistance) / (minDistance + maxDistance);

        float minSizeLerpSpeed = 12f;
        float maxSizeLerpSpeed = 0.5f;
        masterLerpSpeed = Mathf.Lerp(minSizeLerpSpeed, maxSizeLerpSpeed, relSize);
        masterLerpSpeed = Mathf.Max(masterLerpSpeed, 2.647f);  // cap

        masterTargetDistance = Mathf.Min(masterTargetDistance, maxDistance);
        masterTargetDistance = Mathf.Max(masterTargetDistance, minDistance);

        curCameraPos = Vector3.Lerp(this.transform.position, masterTargetCamPosition, masterLerpSpeed * Time.deltaTime);
        this.transform.position = curCameraPos;
        this.transform.localEulerAngles = new Vector3(curTiltAngleDegrees, 0f, 0f);

        // store info for next frame:
        prevCameraPos = curCameraPos;
        prevTiltAngleDegrees = curTiltAngleDegrees;
        prevCameraFocusPivotPos = curCameraFocusPivotPos;
    }

    public void MoveCamera(Vector2 dir) {
        float minDistance = 1f;
        float maxDistance = 420f;
        float relSize = Mathf.Clamp01((masterTargetDistance - minDistance) / (minDistance + maxDistance));

        float minSizePanSpeedMult = 0.1f;
        float maxSizePanSpeedMult = 12f;
        float panSpeedMult = Mathf.Lerp(minSizePanSpeedMult, maxSizePanSpeedMult, relSize);
        
        float camPanSpeed = masterPanSpeed * panSpeedMult * Time.deltaTime;

        curCameraFocusPivotPos += new Vector3(dir.x * camPanSpeed, dir.y * camPanSpeed, 0f);
    }
    public void TiltCamera(float tiltAngle) {
        float minDistance = 1f;
        float maxDistance = 420f;
        float relSize = Mathf.Clamp01((masterTargetDistance - minDistance) / (minDistance + maxDistance));

        float minSizeTiltSpeedMult = 1f;
        float maxSizeTiltSpeedMult = 1f;
        float tiltSpeedMult = Mathf.Lerp(minSizeTiltSpeedMult, maxSizeTiltSpeedMult, relSize);

        masterTargetTiltAngle += tiltAngle * masterTiltSpeed * tiltSpeedMult * Time.deltaTime;

        masterTargetTiltAngle = Mathf.Min(masterTargetTiltAngle, 55f);
        masterTargetTiltAngle = Mathf.Max(masterTargetTiltAngle, 0);

        //cameraManager.masterTargetTiltAngle -= cameraManager.masterTiltSpeed * tiltSpeedMult * Time.deltaTime;
    }
    public void ZoomCamera(float zoomValue) {
        float minDistance = 1f;
        float maxDistance = 420f;
        float relSize = Mathf.Clamp01((masterTargetDistance - minDistance) / (minDistance + maxDistance));

        float minSizeZoomSpeedMult = 0.075f;
        float maxSizeZoomSpeedMult = 2.5f;
        float zoomSpeedMult = Mathf.Lerp(minSizeZoomSpeedMult, maxSizeZoomSpeedMult, relSize);

        float zoomSpeed = zoomValue * masterZoomSpeed * zoomSpeedMult * Time.deltaTime;

        masterTargetDistance += zoomSpeed;

        //masterTargetDistance = Mathf.Min(masterTargetDistance, maxDistance);
        //masterTargetDistance = Mathf.Max(masterTargetDistance, minDistance);
    }

    public void SetTarget(Agent agent, int index) {
        //Debug.Log("SetTarget! " + index.ToString());
        targetAgent = agent;
        targetTransform = agent.bodyGO.transform;
        targetCritterIndex = index;
    }
    
    private Vector2 SmoothApproach(Vector2 pastPosition, Vector2 pastTargetPosition, Vector2 targetPosition, float speed) {
        float t = Time.deltaTime * speed;
        Vector2 v = (targetPosition - pastTargetPosition) / t;
        Vector2 f = pastPosition - pastTargetPosition + v;
        return targetPosition - v + f * Mathf.Exp(-t);
    } 
    
    /*private void UpdateCameraNew() {

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

    }*/
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
}
