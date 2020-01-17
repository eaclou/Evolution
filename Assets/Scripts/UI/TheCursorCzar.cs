using System.Collections;
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

    public Texture2D cursorTexBrush;
    public Texture2D cursorTexWatcher;

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
        int layerMask = ~(1 << LayerMask.NameToLayer("UtilityRaycast"));  // *** THIS ISN'T WORKING!!! *** might be inverted?

        Physics.Raycast(ray, out hit, 1000f, layerMask);  // *** USE DEDICATED LAYER FOR THIS CHECK!!!! *********

        uiManagerRef.cameraManager.isMouseHoverAgent = false;
        uiManagerRef.cameraManager.mouseHoverAgentIndex = 0;
        uiManagerRef.cameraManager.mouseHoverAgentRef = null;
        //Debug.Log("MouseRaycastCheckAgents");
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
                if(clicked) {
                    Debug.Log("CLICKED ON A SPIRIT!!!! ?");

                    uiManagerRef.isClickableSpiritRoaming = false;

                    switch(uiManagerRef.curClickableSpiritType) {
                        case UIManager.ClickableSpiritType.CreationBrush:
                            uiManagerRef.brushesUI.Unlock();
                            uiManagerRef.brushesUI.SetTargetFromWorldTree();
                            uiManagerRef.AnnounceUnlockBrushes();
                            //uiManagerRef.isClickableSpiritRoaming = false;
                            uiManagerRef.curClickableSpiritType = UIManager.ClickableSpiritType.Decomposers;
                            break;
                        case UIManager.ClickableSpiritType.Decomposers:
                            uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomDecomposers.trophicTiersList[0].trophicSlots[0].status = TrophicSlot.SlotStatus.On;
                            Debug.Log("DECOMPOSERS UNLOCKED!!! " + uiManagerRef.unlockCooldownCounter.ToString());

                            uiManagerRef.AnnounceUnlockDecomposers();
                            uiManagerRef.isUnlockCooldown = true;
                            uiManagerRef.unlockedAnnouncementSlotRef = uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomDecomposers.trophicTiersList[0].trophicSlots[0];
                            //simManager.uiManager.buttonToolbarExpandOn.GetComponent<Animator>().enabled = true;
                            uiManagerRef.worldSpiritHubUI.ClickWorldCreateNewSpecies(uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomDecomposers.trophicTiersList[0].trophicSlots[0]);

                            uiManagerRef.worldSpiritHubUI.selectedWorldSpiritSlot = uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomDecomposers.trophicTiersList[0].trophicSlots[0];
                            uiManagerRef.brushesUI.selectedEssenceSlot = uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomDecomposers.trophicTiersList[0].trophicSlots[0];
                            
                            //uiManagerRef.isClickableSpiritRoaming = false;
                            uiManagerRef.curClickableSpiritType = UIManager.ClickableSpiritType.KnowledgeSpirit;
                            break;
                        case UIManager.ClickableSpiritType.KnowledgeSpirit:

                            uiManagerRef.knowledgeUI.isUnlocked = true;
                            uiManagerRef.AnnounceUnlockKnowledgeSpirit();
                            uiManagerRef.knowledgeUI.OpenKnowledgePanel();
                            uiManagerRef.worldSpiritHubUI.OpenWorldTreeSelect();
                            //uiManagerRef.isClickableSpiritRoaming = false;
                            uiManagerRef.curClickableSpiritType = UIManager.ClickableSpiritType.Algae;
                            break;
                        case UIManager.ClickableSpiritType.WatcherSpirit:
                            uiManagerRef.watcherUI.isUnlocked = true;
                            uiManagerRef.watcherUI.ClickToolButton();
                            uiManagerRef.AnnounceUnlockWatcherSpirit();
                            uiManagerRef.curClickableSpiritType = UIManager.ClickableSpiritType.VertA;
                            break;
                        case UIManager.ClickableSpiritType.MutationSpirit:
                            uiManagerRef.mutationUI.isUnlocked = true;
                            uiManagerRef.AnnounceUnlockMutationSpirit();
                            //uiManagerRef.curClickableSpiritType = UIManager.ClickableSpiritType.Zooplankton; // *** Last unlock?
                            uiManagerRef.curClickableSpiritType = UIManager.ClickableSpiritType.VertB;
                            break;
                        case UIManager.ClickableSpiritType.Minerals:
                            TrophicSlot mineralSlot = uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomOther.trophicTiersList[0].trophicSlots[0];
                            mineralSlot.status = TrophicSlot.SlotStatus.On;
                            Debug.Log("MINERALS UNLOCKED!!! " + uiManagerRef.unlockCooldownCounter.ToString());
                            uiManagerRef.AnnounceUnlockMinerals();
                            uiManagerRef.isUnlockCooldown = true;
                            uiManagerRef.unlockedAnnouncementSlotRef = mineralSlot;
                            //simManager.uiManager.buttonToolbarExpandOn.GetComponent<Animator>().enabled = true;
                            //uiManagerRef.worldSpiritHubUI.ClickWorldCreateNewSpecies(uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomDecomposers.trophicTiersList[0].trophicSlots[0]);
                            uiManagerRef.worldSpiritHubUI.selectedWorldSpiritSlot = mineralSlot;
                            uiManagerRef.brushesUI.selectedEssenceSlot = mineralSlot;
                            
                            uiManagerRef.curClickableSpiritType = UIManager.ClickableSpiritType.Pebbles;
                            break;
                        case UIManager.ClickableSpiritType.Air:
                            TrophicSlot airSlot = uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomOther.trophicTiersList[0].trophicSlots[2];
                            airSlot.status = TrophicSlot.SlotStatus.On;
                            Debug.Log("AIR SPIRIT UNLOCKED!!! " + uiManagerRef.unlockCooldownCounter.ToString());
                            uiManagerRef.AnnounceUnlockAir();
                            uiManagerRef.isUnlockCooldown = true;
                            uiManagerRef.unlockedAnnouncementSlotRef = airSlot;
                            uiManagerRef.worldSpiritHubUI.selectedWorldSpiritSlot = airSlot;
                            uiManagerRef.brushesUI.selectedEssenceSlot = airSlot;
                            
                            uiManagerRef.curClickableSpiritType = UIManager.ClickableSpiritType.Plants;
                            break;
                        case UIManager.ClickableSpiritType.Pebbles:
                            TrophicSlot pebblesSlot = uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomTerrain.trophicTiersList[0].trophicSlots[2];
                            pebblesSlot.status = TrophicSlot.SlotStatus.On;
                            Debug.Log("PEBBLES SPIRIT UNLOCKED!!! " + uiManagerRef.unlockCooldownCounter.ToString());
                            uiManagerRef.AnnounceUnlockPebbles();
                            uiManagerRef.isUnlockCooldown = true;
                            uiManagerRef.unlockedAnnouncementSlotRef = pebblesSlot;
                            uiManagerRef.worldSpiritHubUI.selectedWorldSpiritSlot = pebblesSlot;
                            uiManagerRef.brushesUI.selectedEssenceSlot = pebblesSlot;
                            uiManagerRef.brushesUI.selectedBrushLinkedSpiritTerrainLayer = 2; // uiManagerRef.worldSpiritHubUI.selectedToolbarTerrainLayer; 
                            uiManagerRef.brushesUI.ClickButtonBrushPaletteTerrain(2);

                            uiManagerRef.curClickableSpiritType = UIManager.ClickableSpiritType.Zooplankton;
                            break;
                        case UIManager.ClickableSpiritType.Sand:
                            TrophicSlot sandSlot = uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomTerrain.trophicTiersList[0].trophicSlots[3];
                            sandSlot.status = TrophicSlot.SlotStatus.On;
                            Debug.Log("SAND SPIRIT UNLOCKED!!! " + uiManagerRef.unlockCooldownCounter.ToString());
                            uiManagerRef.AnnounceUnlockSand();
                            uiManagerRef.isUnlockCooldown = true;
                            uiManagerRef.unlockedAnnouncementSlotRef = sandSlot;
                            uiManagerRef.worldSpiritHubUI.selectedWorldSpiritSlot = sandSlot;
                            uiManagerRef.brushesUI.selectedEssenceSlot = sandSlot;
                            uiManagerRef.brushesUI.selectedBrushLinkedSpiritTerrainLayer = 3; 
                            uiManagerRef.brushesUI.ClickButtonBrushPaletteTerrain(3);
                            
                            uiManagerRef.curClickableSpiritType = UIManager.ClickableSpiritType.Air;
                            break;
                        case UIManager.ClickableSpiritType.Algae:
                            uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomPlants.trophicTiersList[0].trophicSlots[0].status = TrophicSlot.SlotStatus.On;
                            Debug.Log("ALGAE UNLOCKED!!! " + uiManagerRef.unlockCooldownCounter.ToString());

                            uiManagerRef.AnnounceUnlockAlgae();
                            uiManagerRef.isUnlockCooldown = true;
                            uiManagerRef.unlockedAnnouncementSlotRef = uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomPlants.trophicTiersList[0].trophicSlots[0];   
                
                            uiManagerRef.worldSpiritHubUI.ClickWorldCreateNewSpecies(uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomPlants.trophicTiersList[0].trophicSlots[0]);
                            
                            uiManagerRef.brushesUI.selectedEssenceSlot = uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomPlants.trophicTiersList[0].trophicSlots[0];
                            uiManagerRef.worldSpiritHubUI.selectedWorldSpiritSlot = uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomPlants.trophicTiersList[0].trophicSlots[0];
                            //uiManagerRef.isClickableSpiritRoaming = false;
                            uiManagerRef.curClickableSpiritType = UIManager.ClickableSpiritType.Minerals;
                            break;
                        case UIManager.ClickableSpiritType.Plants:
                            uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomPlants.trophicTiersList[1].trophicSlots[0].status = TrophicSlot.SlotStatus.On;
                            Debug.Log("PLANTS UNLOCKED!!! " + uiManagerRef.unlockCooldownCounter.ToString());
                            uiManagerRef.AnnounceUnlockPlants();
                            uiManagerRef.isUnlockCooldown = true;
                            uiManagerRef.unlockedAnnouncementSlotRef = uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomPlants.trophicTiersList[1].trophicSlots[0];                
                            uiManagerRef.worldSpiritHubUI.ClickWorldCreateNewSpecies(uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomPlants.trophicTiersList[1].trophicSlots[0]);

                            uiManagerRef.worldSpiritHubUI.selectedWorldSpiritSlot = uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomPlants.trophicTiersList[1].trophicSlots[0];
                            uiManagerRef.brushesUI.selectedEssenceSlot = uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomPlants.trophicTiersList[1].trophicSlots[0];

                            uiManagerRef.curClickableSpiritType = UIManager.ClickableSpiritType.WatcherSpirit;
                            break;
                        case UIManager.ClickableSpiritType.Zooplankton:
                            uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[0].trophicSlots[0].status = TrophicSlot.SlotStatus.On;
                            Debug.Log("ZOOPLANKTON UNLOCKED!!! " + uiManagerRef.unlockCooldownCounter.ToString());
                            uiManagerRef.AnnounceUnlockZooplankton();
                            uiManagerRef.isUnlockCooldown = true;
                            uiManagerRef.unlockedAnnouncementSlotRef = uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[0].trophicSlots[0];
                            uiManagerRef.worldSpiritHubUI.ClickWorldCreateNewSpecies(uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[0].trophicSlots[0]);
                            
                            uiManagerRef.worldSpiritHubUI.selectedWorldSpiritSlot = uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[0].trophicSlots[0];
                            uiManagerRef.brushesUI.selectedEssenceSlot = uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[0].trophicSlots[0];
                            
                            uiManagerRef.curClickableSpiritType = UIManager.ClickableSpiritType.Sand;
                            break;
                        case UIManager.ClickableSpiritType.VertA:                            
                            uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[1].unlocked = true;
                            uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[0].status = TrophicSlot.SlotStatus.On;                
                            Debug.Log("CREATURE A UNLOCKED!!! " + uiManagerRef.unlockCooldownCounter.ToString());
                            uiManagerRef.AnnounceUnlockVertebrates();
                            uiManagerRef.isUnlockCooldown = true;
                            uiManagerRef.unlockedAnnouncementSlotRef = uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[0];                
                            uiManagerRef.worldSpiritHubUI.ClickWorldCreateNewSpecies(uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[0]);

                            uiManagerRef.worldSpiritHubUI.selectedWorldSpiritSlot = uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[0];
                            uiManagerRef.brushesUI.selectedEssenceSlot = uiManagerRef.gameManager.simulationManager.trophicLayersManager.kingdomAnimals.trophicTiersList[1].trophicSlots[0];

                            
                            uiManagerRef.curClickableSpiritType = UIManager.ClickableSpiritType.MutationSpirit;
                            break;
                        default:
                            Debug.LogError("No Enum Type Found! (");
                            break;

                    }
                    
                }                
            }
            //Debug.Log("CLICKED ON: [ " + hit.collider.gameObject.name + " ] Ray= " + ray.ToString() + ", hit= " + hit.point.ToString());
        }
        else {
            //Debug.Log("hit.collider == null");
        }
    }
    
	
	public void UpdateCursorCzar () {
        /*
        if(uiManagerRef.isBrushModeON_snoopingOFF) {
            Cursor.SetCursor(cursorTexBrush, Vector2.one * 64f, CursorMode.Auto);
        }
        else {
            Cursor.SetCursor(cursorTexWatcher, Vector2.one * 64f, CursorMode.Auto);
        }
        */

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
