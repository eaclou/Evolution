using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WildSpirit : MonoBehaviour {
    public UIManager uiManagerRef;

    public GameObject protoSpiritClickColliderGO;
        
    public Vector3 creationSpiritClickableStartPos = new Vector3(120f, 124f, -5f);
    public Vector3 curRoamingSpiritPosition;
    public Vector3 prevRoamingSpiritPosition;
    
    public float roamingSpiritScale = 2.0f;
    public bool isClickableSpiritRoaming;
    public Color roamingSpiritColor;
    public int framesSinceLastClickableSpirit;
    public ClickableSpiritType curClickableSpiritType = ClickableSpiritType.CreationBrush;
    public enum ClickableSpiritType {
        CreationBrush,
        KnowledgeSpirit,
        MutationSpirit,
        WatcherSpirit,
        Pebbles,
        Sand,
        Air,
        Water,
        Minerals,
        Decomposers,
        Algae,
        Zooplankton,
        Plants,
        VertA,
        VertB,
        VertC,
        VertD
    }

    private Vector3 velocity;

    private float spiritSpeed = 0.115f;

    private float threatLevel = 0f;

    public bool isFleeing = false;
    private int fleeingFrameCounter = 0;



    public void SpawnWildSpirit(Color col) {
        
        uiManagerRef.wildSpirit.isClickableSpiritRoaming = true;
        uiManagerRef.wildSpirit.curRoamingSpiritPosition = uiManagerRef.wildSpirit.creationSpiritClickableStartPos;
        uiManagerRef.wildSpirit.roamingSpiritColor = col;
        isFleeing = false;
        fleeingFrameCounter = 0;
        threatLevel = 0f;
    }
    public void UpdateWildSpiritProto() {


        protoSpiritClickColliderGO.SetActive(true);
        float orbitSpeed = 0.5f;
        float orbitRadius = 20f;
        float spinAngle = Time.realtimeSinceStartup * orbitSpeed;
        float zPhase = spinAngle * 9.5f;
        float zBounceMag = 1f;
        
        
        Vector3 cursorToSpiritVec = (curRoamingSpiritPosition - uiManagerRef.theCursorCzar.cursorParticlesWorldPos);
        cursorToSpiritVec.z = 0f;
        float sqrDistanceToCursor = cursorToSpiritVec.sqrMagnitude;

        //float relativeVel = uiManagerRef.theCursorCzar.
        if(sqrDistanceToCursor < 150f) {
            isFleeing = true;
            fleeingFrameCounter = 0;

            
        }

        if(isFleeing) {
            // random noise vel:
            if(framesSinceLastClickableSpirit % 35 == 0) {
                velocity = Vector3.Lerp(velocity, UnityEngine.Random.insideUnitSphere * spiritSpeed, 0.1f);
            }
            velocity.z = Mathf.Lerp(velocity.z, 0f, 0.25f);  // stay near zero altitude?

            velocity = Vector3.Lerp(velocity, cursorToSpiritVec.normalized * spiritSpeed, 0.24f);

            fleeingFrameCounter++;
            if(fleeingFrameCounter > 70) {
                isFleeing = false;
                fleeingFrameCounter = 0;
            }
        }
        else {
            // random noise vel:
            if(framesSinceLastClickableSpirit % 735 == 0) {
                velocity = Vector3.Lerp(velocity, UnityEngine.Random.insideUnitSphere * spiritSpeed * 0.14f, 0.15f);
            }

            // guide back to center:
            if(framesSinceLastClickableSpirit % 831 == 0) {
                
                velocity = Vector3.Lerp(velocity, (new Vector3(128f, 128f, 0f) - curRoamingSpiritPosition).normalized * spiritSpeed * 0.2f, 0.18f);
            }

            velocity.z = Mathf.Lerp(velocity.z, 0f, 0.05f);  // stay near zero altitude?

        }
        velocity = Vector3.Lerp(velocity, Vector3.zero, 0.033f); // drag

        curRoamingSpiritPosition = curRoamingSpiritPosition + velocity;


        protoSpiritClickColliderGO.transform.position = curRoamingSpiritPosition;
        framesSinceLastClickableSpirit = 0;
    }

    
    public void CapturedClickableSpirit() {
        uiManagerRef.animatorSpiritUnlock.SetTrigger("_TriggerClicked");
        isClickableSpiritRoaming = false;

        
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
