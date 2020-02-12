using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CritterPortraitCameraManager : MonoBehaviour {

    public float targetZoomLevel = 0.1f;
    public float targetTiltAngleDegrees = 15f;
    public float targetFOV = 20f;

    private float curZoomLevel;
    private float curTiltAngle;
    private float curFOV;

    private Vector3 targetCamPos;

    public float lerpSpeed = 0.08f;

	// Use this for initialization
	void Start () {
        curZoomLevel = targetZoomLevel;
        curTiltAngle = targetTiltAngleDegrees;
        curFOV = targetFOV;
	}
	
	// Update is called once per frame
	void Update () {
        SimCameraTransform();
	}

    public void UpdateCameraTargetValues(float targetZoom) {
        targetZoomLevel = targetZoom;
    }

    public void SimCameraTransform() {

        // update cur values:
        curZoomLevel = Mathf.Lerp(curZoomLevel, targetZoomLevel, lerpSpeed);
        curTiltAngle = Mathf.Lerp(curTiltAngle, targetTiltAngleDegrees, lerpSpeed);
        curFOV = Mathf.Lerp(curFOV, targetFOV, lerpSpeed);

        float targetDist = Mathf.Lerp(15f, 290f, curZoomLevel);

        //targetCamPos = new Vector3(1.3f, -Mathf.Sin(targetTiltAngleDegrees * Mathf.Deg2Rad) * targetDist, -Mathf.Cos(targetTiltAngleDegrees * Mathf.Deg2Rad) * targetDist);
        //Vector3 newPos = Vector3.Lerp(this.gameObject.transform.position, targetCamPos, lerpSpeed);
        //this.gameObject.transform.position = newPos;
        //this.gameObject.transform.localEulerAngles = new Vector3(-curTiltAngle, 0f, 0f);
    }
}
