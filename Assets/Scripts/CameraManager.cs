using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour {

    public Camera camera;

    public float targetZoomValue = 18f;
    public float lerpSpeed = 0.25f;
    public Vector3 targetCamPos;
    public Transform targetTransform;

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
	
	// Update is called once per frame
	void Update () {
        //targetCamPos = new Vector3(playerAgent.transform.position.x, playerAgent.transform.position.y, -10f);
        //mainCam.transform.position = Vector3.Lerp(mainCam.transform.position, camPos, 0.08f);
        switch (curMode) {
            case GameMode.ModeA:
                //
                targetCamPos = targetTransform.position;
                lerpSpeed = 0.12f;
                break;
            case GameMode.ModeB:
                //
                targetCamPos = targetTransform.position;
                lerpSpeed = 0.08f;
                //targetZoomValue = 18f;
                break;
            case GameMode.ModeC:
                //targetZoomValue = 20f;
                targetCamPos = Vector3.zero;
                lerpSpeed = 0.04f;
                //
                break;
            default:
                //
                break;
        }
        camera.orthographicSize = Mathf.Lerp(camera.orthographicSize, targetZoomValue, 0.05f);
        Vector3 camPos = new Vector3(targetCamPos.x, targetCamPos.y, -10f);
        transform.position = Vector3.Lerp(transform.position, camPos, lerpSpeed);
    }

    public void ChangeGameMode(GameMode mode) {
        curMode = mode;
        Debug.Log("ChangeGameMode(" + mode.ToString() + ")");
        switch(curMode) {
            case GameMode.ModeA:
                //
                targetZoomValue = 6.5f;
                break;
            case GameMode.ModeB:
                //
                targetZoomValue = 18f;
                break;
            case GameMode.ModeC:
                targetZoomValue = 45f;
                //
                break;
            default:
                //
                break;
        }
    }
}
