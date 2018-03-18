﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour {

    public Camera camera;

    public float targetZoomValueA = 8f;
    public float targetZoomValueB = 18f;
    public float targetZoomValueC = 46f;
    private float targetZoomValue;

    public float lerpSpeedA = 1f;
    public float lerpSpeedB = 2f;
    public float lerpSpeedC = 4f;
    private float lerpSpeed;

    public float camMaxSpeed = 1f;
    public float camAccel = 0.05f;

    public Vector3 targetCamPos;
    public Transform targetTransform;

    Vector2 prevCameraPosition, prevTargetPosition;

    public float orbitRadius = 35f;
    public float orbitSpeed = 1f;
    public float curOrbitAngle = 0f;

    private GameMode curMode;
    public enum GameMode {
        ModeA,
        ModeB,
        ModeC
    }

    // Use this for initialization
    void Start () {
        curMode = GameMode.ModeB;

    }

    private void Update() {
        
        //UpdateCameraTestBaseline();
        UpdateCameraOld();

        //this.transform.position = SmoothApproach(Vector2 pastPosition, Vector2 pastTargetPosition, Vector2 targetPosition, float speed);
        //this.transform.position = Vector3.Lerp(this.transform.position, new Vector3(targetCamPos.x, targetCamPos.y, -50f), 2f * Time.deltaTime);
        //new Vector3(targetCamPos.x, targetCamPos.y, -50f);
    }

    private void UpdateCameraOld() {
        switch (curMode) {
            case GameMode.ModeA:
                //
                targetCamPos = Vector3.Lerp(targetCamPos, targetTransform.position, 0.08f);
                lerpSpeed = lerpSpeedA;
                break;
            case GameMode.ModeB:
                //
                targetCamPos = Vector3.Lerp(targetCamPos, targetTransform.position, 0.08f);
                lerpSpeed = lerpSpeedB;
                break;
            case GameMode.ModeC:
                targetCamPos = Vector3.Lerp(targetCamPos, Vector3.zero, 0.08f);
                lerpSpeed = lerpSpeedC;
                //
                break;
            default:
                //
                break;
        }

        camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, targetZoomValue, 0.5f * Time.deltaTime);
        //Vector3 curPos = transform.position;
        //Vector2 targetPos = new Vector2(targetCamPos.x, targetCamPos.y);
        //Vector2 targetCamDir = new Vector2(targetCamPos.x, targetCamPos.y) - new Vector2(curPos.x, curPos.y);

        // Fuck it for now.... stupid lerp jitter...
        // Come back to this after sorting out Execution order and data flow in rest of program...
        this.transform.position = Vector3.Lerp(this.transform.position, new Vector3(targetCamPos.x, targetCamPos.y, -50f), lerpSpeed * Time.deltaTime);

    }
    private void UpdateCameraTestBaseline() {
        //float orbitRadius = 35f;
        //orbitSpeed
        //curOrbitAngle

        Vector3 currentCamPos = this.transform.position;

        curOrbitAngle += orbitSpeed * Time.deltaTime;
        float xDir = Mathf.Cos(curOrbitAngle);
        float yDir = Mathf.Sin(curOrbitAngle);

        //Vector3 newCameraPosition = new Vector3(xDir * orbitRadius, yDir * orbitRadius, -50f);
        Vector3 newCameraPosition = new Vector3(xDir * orbitRadius, 0f, -50f);

        this.transform.position = newCameraPosition;

    }
    // Update is called once per frame
    void FixedUpdate () {

        //UpdateCameraTestBaseline();
        //UpdateCameraOld();

        //targetCamPos = new Vector3(playerAgent.transform.position.x, playerAgent.transform.position.y, -10f);
        //mainCam.transform.position = Vector3.Lerp(mainCam.transform.position, camPos, 0.08f);
        /*switch (curMode) {
            case GameMode.ModeA:
                //
                targetCamPos = Vector3.Lerp(targetCamPos, targetTransform.position, 0.1f);
                lerpSpeed = lerpSpeedA;
                break;
            case GameMode.ModeB:
                //
                targetCamPos = Vector3.Lerp(targetCamPos, targetTransform.position, 0.1f);
                lerpSpeed = lerpSpeedB;
                break;
            case GameMode.ModeC:
                targetCamPos = Vector3.Lerp(targetCamPos, Vector3.zero, 0.1f);
                lerpSpeed = lerpSpeedC;
                //
                break;
            default:
                //
                break;
        }

        camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, targetZoomValue, 0.5f * Time.deltaTime);
        //Vector3 curPos = transform.position;
        //Vector2 targetPos = new Vector2(targetCamPos.x, targetCamPos.y);
        //Vector2 targetCamDir = new Vector2(targetCamPos.x, targetCamPos.y) - new Vector2(curPos.x, curPos.y);

        // Fuck it for now.... stupid lerp jitter...
        // Come back to this after sorting out Execution order and data flow in rest of program...
        this.transform.position = Vector3.Lerp(this.transform.position, new Vector3(targetCamPos.x, targetCamPos.y, -50f), 2.5f * Time.deltaTime);

        //Vector2 
        */
        //transform.position = Vector3.Lerp(transform.position, targetPos, Mathf.Clamp01(lerpSpeed * Time.deltaTime)).normalized * camMaxSpeed;


        // move target //
        //targetTransform.position += Vector3.right * Time.deltaTime * 200;

        // move follower //
        //Vector2 newCamPosition = SmoothApproach(prevCameraPosition, prevTargetPosition, targetPos, 10f);
        //this.transform.position = new Vector3(newCamPosition.x, newCamPosition.y, -10f);
        //prevCameraPosition = new Vector2(transform.position.x, transform.position.y);
        //prevTargetPosition = targetPos;

        // move camera along side the target //
        //camTransform.position = new Vector3(targetTransform.position.x, targetTransform.position.y, targetTransform.position.z - 15);
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
