﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TheCursorCzar : MonoBehaviour {
    public UIManager uiManagerRef;

    public GameObject mouseRaycastWaterPlane;
    private Vector3 prevMousePositionOnWaterPlane;
    public Vector3 curMousePositionOnWaterPlane;

    public float stirStickDepth = 0f;

    public Vector2 smoothedMouseVel;
    private Vector2 prevMousePos;

    public Vector2 smoothedCtrlCursorVel;
    private Vector2 prevCtrlCursorPos;
    private Vector3 prevCtrlCursorPositionOnWaterPlane;
    public Vector3 curCtrlCursorPositionOnWaterPlane;
    private bool rightTriggerOn = false;

    public bool stirGizmoVisible = false;

    public bool leftClickThisFrame = false;
    public bool rightClickThisFrame = false;
    public bool letGoThisFrameLeft = false;
    public bool letGoThisFrameRight = false;
    public bool isDraggingMouseLeft = false;
    public bool isDraggingMouseRight = false;
    public bool isDraggingSpeciesNode = false;


	// Use this for initialization
	void Start () {
		
	}


    
    private void MouseRaycastWaterPlane(Vector3 screenPos) {
        mouseRaycastWaterPlane.SetActive(true);

        //float scale = SimulationManager._MapSize * 0.1f;
        Vector3 targetPosition = mouseRaycastWaterPlane.gameObject.transform.position; //
        targetPosition.z = (uiManagerRef.gameManager.simulationManager.theRenderKing.baronVonWater._GlobalWaterLevel - 0.5f) * -20f;
        mouseRaycastWaterPlane.gameObject.transform.position = targetPosition;
        //new Vector3(SimulationManager._MapSize * 0.5f, gameManager.simulationManager.theRenderKing.baronVonWater._GlobalWaterLevel, SimulationManager._MapSize * 0.5f);
        //mouseRaycastWaterPlane.gameObject.transform.position = targetPosition;
        //mouseRaycastWaterPlane.gameObject.transform.localScale = Vector3.one * scale;
        //Vector3 camPos = cameraManager.gameObject.transform.position;                
        Ray ray = uiManagerRef.cameraManager.gameObject.GetComponent<Camera>().ScreenPointToRay(screenPos);
        RaycastHit hit = new RaycastHit();
        int layerMask = 1 << 12;  // UtilityRaycast???
        Physics.Raycast(ray, out hit, layerMask);

        if (hit.collider != null) {
            prevMousePositionOnWaterPlane = curMousePositionOnWaterPlane;
            curMousePositionOnWaterPlane = hit.point;            

            //Debug.Log("curMousePositionOnWaterPlane:" + curMousePositionOnWaterPlane.ToString() + ", " + screenPos.ToString() + ", hit: " + hit.point.ToString());
        }
        else {
            Debug.Log("NULL: " + ray.ToString());
        }
    }    
    private void MouseRaycastCheckAgents(bool clicked) {
        
        Vector3 camPos = uiManagerRef.cameraManager.gameObject.transform.position;
        
        Ray ray = uiManagerRef.cameraManager.gameObject.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        RaycastHit hit = new RaycastHit();
        int layerMask = ~(1 << LayerMask.NameToLayer("UtilityRaycast")); 

        Physics.Raycast(ray, out hit, 1000f, layerMask);  // *** USE DEDICATED LAYER FOR THIS CHECK!!!! *********

        uiManagerRef.cameraManager.isMouseHoverAgent = false;
        uiManagerRef.cameraManager.mouseHoverAgentIndex = 0;
        uiManagerRef.cameraManager.mouseHoverAgentRef = null;

        if(hit.collider != null) {
            
            // CHECK FOR AGENT COLLISION:
            Agent agentRef = hit.collider.gameObject.GetComponentInParent<Agent>();
            if(agentRef != null) {
                //Debug.Log("AGENT: [ " + agentRef.gameObject.name + " ] #" + agentRef.index.ToString());
                    
                if(clicked) {
                    if (uiManagerRef.watcherUI.isOpen && !uiManagerRef.isBrushModeON_snoopingOFF) {
                        uiManagerRef.cameraManager.SetTargetAgent(agentRef, agentRef.index);
                        uiManagerRef.cameraManager.isFollowingAgent = true;
                        uiManagerRef.watcherUI.StopFollowingPlantParticle();
                        uiManagerRef.watcherUI.StopFollowingAnimalParticle();
                        
                        uiManagerRef.watcherUI.StartFollowingAgent();
                        //uiManagerRef.gameManager.simulationManager.trophicLayersManager.selectedTrophicSlotRef = uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[agentRef.speciesIndex];
                        uiManagerRef.watcherUI.watcherSelectedTrophicSlotRef = uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[agentRef.speciesIndex];
                    }
                }
                else {
                    // HOVER:
                    //hoverAgentID = agentRef.index;
                    //Debug.Log("HOVER AGENT: " + agentRef.index.ToString() + ", species: " + agentRef.speciesIndex.ToString());
                }

                uiManagerRef.cameraManager.isMouseHoverAgent = true;
                uiManagerRef.cameraManager.mouseHoverAgentIndex = agentRef.index;
                uiManagerRef.cameraManager.mouseHoverAgentRef = agentRef;                    
            }
            else {
                
            }
            //Debug.Log("CLICKED ON: [ " + hit.collider.gameObject.name + " ] Ray= " + ray.ToString() + ", hit= " + hit.point.ToString());
        }
        else {
        }
    }
    
	
	public void UpdateCursorCzar () {

        // &&&&&&&&&&&&&&&&& MOUSE: &&&&&&&&&&&&&&&
        leftClickThisFrame = Input.GetMouseButtonDown(0);
        rightClickThisFrame = Input.GetMouseButtonDown(1);
        letGoThisFrameLeft = Input.GetMouseButtonUp(0);
        letGoThisFrameRight = Input.GetMouseButtonUp(1);
        isDraggingMouseLeft = Input.GetMouseButton(0);
        isDraggingMouseRight = Input.GetMouseButton(1);
        if (letGoThisFrameLeft) {
            isDraggingMouseLeft = false;
            isDraggingSpeciesNode = false;
            //gameManager.simulationManager.isBrushingAgents = false;
        }
        if (letGoThisFrameRight) {
            isDraggingMouseRight = false;
            //gameManager.simulationManager.isBrushingAgents = false;
        }
    
        // check for player clicking on an animal in the world
        MouseRaycastCheckAgents(leftClickThisFrame);

        // Get position of mouse on water plane:
        MouseRaycastWaterPlane(Input.mousePosition);
        Vector4[] dataArray = new Vector4[1];
        Vector4 gizmoPos = new Vector4(curMousePositionOnWaterPlane.x, curMousePositionOnWaterPlane.y, 0f, 0f);
        dataArray[0] = gizmoPos;
        uiManagerRef.gameManager.theRenderKing.gizmoCursorPosCBuffer.SetData(dataArray);

        stirGizmoVisible = false;

        //-----------------------------------------------------------------------------------------

        Vector2 curMousePos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        Vector2 instantMouseVel = curMousePos - prevMousePos;
        smoothedMouseVel = Vector2.Lerp(smoothedMouseVel, instantMouseVel, 0.16f);
        prevMousePos = curMousePos;
	}
}
