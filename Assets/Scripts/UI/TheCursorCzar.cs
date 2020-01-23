using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TheCursorCzar : MonoBehaviour {
    public UIManager uiManagerRef;

    public bool _IsHoverClickableSpirit;

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

    public Texture2D cursorTexBrush;
    public Texture2D cursorTexWatcher;
    public Texture2D cursorTexWorld;

    public Vector3 cursorParticlesWorldPos;
    public Ray cursorRay;

	// Use this for initialization
	void Start () {
		
	}


    
    private void MouseRaycastWaterPlane(Vector3 screenPos) {

        Vector2 cursorScreenPosNormalized = new Vector2(Input.mousePosition.x / uiManagerRef.cameraManager.cameraRef.pixelWidth, Input.mousePosition.y / uiManagerRef.cameraManager.cameraRef.pixelHeight);
        Vector3 bottomMidpoint = Vector3.Lerp(uiManagerRef.cameraManager.worldSpaceBottomLeft, uiManagerRef.cameraManager.worldSpaceBottomRight, cursorScreenPosNormalized.x);
        Vector3 topMidpoint = Vector3.Lerp(uiManagerRef.cameraManager.worldSpaceTopLeft, uiManagerRef.cameraManager.worldSpaceTopRight, cursorScreenPosNormalized.x);
        Vector3 midMidpoint = Vector3.Lerp(bottomMidpoint, topMidpoint, cursorScreenPosNormalized.y);
        

        mouseRaycastWaterPlane.SetActive(true);

        //float scale = SimulationManager._MapSize * 0.1f;
        Vector3 targetPosition = mouseRaycastWaterPlane.gameObject.transform.position; //
        targetPosition.z = (uiManagerRef.gameManager.simulationManager.theRenderKing.baronVonWater._GlobalWaterLevel - 0.5f) * -20f;

        mouseRaycastWaterPlane.gameObject.transform.position = Vector3.zero; // targetPosition;
        //new Vector3(SimulationManager._MapSize * 0.5f, gameManager.simulationManager.theRenderKing.baronVonWater._GlobalWaterLevel, SimulationManager._MapSize * 0.5f);
        //mouseRaycastWaterPlane.gameObject.transform.position = targetPosition;
        //mouseRaycastWaterPlane.gameObject.transform.localScale = Vector3.one * scale;
        //Vector3 camPos = cameraManager.gameObject.transform.position;                

        Ray ray = uiManagerRef.cameraManager.gameObject.GetComponent<Camera>().ScreenPointToRay(screenPos);
        cursorRay = ray;
        //Ray ray = new Ray(uiManagerRef.cameraManager.gameObject.transform.position, midMidpoint - uiManagerRef.cameraManager.gameObject.transform.position);
        RaycastHit hit = new RaycastHit();
        int layerMask = 1 << 12;  // UtilityRaycast???
        Physics.Raycast(ray, out hit, layerMask);

        if (hit.collider != null) {
            prevMousePositionOnWaterPlane = curMousePositionOnWaterPlane;
            curMousePositionOnWaterPlane = hit.point;            

            cursorParticlesWorldPos = hit.point - ray.direction * 5f;
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
        int layerMask = ~(1 << LayerMask.NameToLayer("UtilityRaycast"));  // *** THIS ISN'T WORKING!!! *** might be inverted?

        Physics.Raycast(ray, out hit, 1000f, layerMask);  // *** USE DEDICATED LAYER FOR THIS CHECK!!!! *********

        uiManagerRef.cameraManager.isMouseHoverAgent = false;
        uiManagerRef.cameraManager.mouseHoverAgentIndex = 0;
        uiManagerRef.cameraManager.mouseHoverAgentRef = null;

        _IsHoverClickableSpirit = false;

        //Debug.Log("MouseRaycastCheckAgents");
        if(hit.collider != null) {
            
            // CHECK FOR AGENT COLLISION:
            Agent agentRef = hit.collider.gameObject.GetComponentInParent<Agent>();
            if(agentRef != null) {
                //Debug.Log("AGENT: [ " + agentRef.gameObject.name + " ] #" + agentRef.index.ToString());
                    
                if(clicked) {
                    if (uiManagerRef.panelFocus == UIManager.PanelFocus.Watcher) {
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
                if(clicked) {
                    Debug.Log("CLICKED ON A SPIRIT!!!! ? " + uiManagerRef.curClickableSpiritType.ToString());
                    uiManagerRef.CapturedClickableSpirit();

                }
                else {
                    _IsHoverClickableSpirit = true;
                    Debug.Log("_IsHoverClickableSpirit ON: [ ");
                }
            }
            //Debug.Log("CLICKED ON: [ " + hit.collider.gameObject.name + " ] Ray= " + ray.ToString() + ", hit= " + hit.point.ToString());
        }
        else {
            //Debug.Log("hit.collider == null");
        }
    }
    
	
	public void UpdateCursorCzar () {
        
        if(uiManagerRef.panelFocus == UIManager.PanelFocus.Brushes) {
            Cursor.SetCursor(cursorTexBrush, Vector2.zero, CursorMode.Auto);
        }
        else if(uiManagerRef.panelFocus == UIManager.PanelFocus.Watcher) {
            Cursor.SetCursor(cursorTexWatcher, Vector2.zero, CursorMode.Auto);
        }
        else {
            Cursor.SetCursor(cursorTexWorld, Vector2.zero, CursorMode.Auto);
        }
        

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
