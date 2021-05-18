using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ObserverModeUI : MonoBehaviour
{
    UIManager manager => UIManager.instance;

    public new bool enabled;
    public GameObject panelObserverMode;
    public WatcherUI watcherUI;
    public DebugPanelUI debugPanelUI;
    public BrushesUI brushesUI;
    public GenomeViewerUI genomeViewerUI;
    public GameObject panelPendingClickPrompt;
    
    GameManager gameManager => GameManager.instance;
    SimulationManager simulationManager => SimulationManager.instance;
    CameraManager cameraManager => CameraManager.instance;
    TheCursorCzar theCursorCzar => TheCursorCzar.instance;
    TheRenderKing theRenderKing => TheRenderKing.instance;
    
    Text textTooltip => theCursorCzar.textTooltip;
    GameObject panelTooltip => theCursorCzar.panelTooltip;
    ZooplanktonManager zooplanktonManager => simulationManager.zooplanktonManager;
    VegetationManager vegetationManager => simulationManager.vegetationManager;
    ToolType curActiveTool => manager.curActiveTool;
    
    const string VISIBLE = "_IsVisible";
    const string STIRRING = "_IsStirring";
    const string RADIUS = "_Radius";
    
    public int timerAnnouncementTextCounter = 0;
    public bool isAnnouncementTextOn = false;
    public bool isBrushAddingAgents = false;
    public bool updateTerrainAltitude;
    public float terrainUpdateMagnitude;
        
    public PanelFocus panelFocus = PanelFocus.WorldHub;
    public enum PanelFocus {
        None,
        WorldHub,
        Brushes,
        Watcher
    }
    
    bool isKeyboardInput;

    public void StepCamera(Vector2 input)
    {    
        panelObserverMode.SetActive(enabled);
        
        if (!enabled || input == Vector2.zero) 
            return;

        watcherUI.StopFollowingAgent();
        watcherUI.StopFollowingPlantParticle();
        watcherUI.StopFollowingAnimalParticle();

        cameraManager.MoveCamera(input.normalized);
    }

    // ***WPP: break into separate functions, call from here
    // (start by commenting blocks of code)
    public void Tick()
    {
        if(vegetationManager == null) {
            return;
        }
        //Debug.Log("ObserverMode ON");
        if (isAnnouncementTextOn) {
            panelPendingClickPrompt.SetActive(true);
            timerAnnouncementTextCounter++;

            if (timerAnnouncementTextCounter > 640) {
                isAnnouncementTextOn = false;
                timerAnnouncementTextCounter = 0;
                //inspectToolUnlockedAnnounce = false;
            }
        }
        else {
            panelPendingClickPrompt.SetActive(false);
        }
                                
        // If mouse is over ANY unity canvas UI object (with raycast enabled)
        if (EventSystem.current.IsPointerOverGameObject()) {  
            //Debug.Log("MouseOverUI!!!");
            if(genomeViewerUI.isTooltipHover) {
                panelTooltip.SetActive(true);
                string tipString = genomeViewerUI.tooltipString;
                    //if()
                textTooltip.text = tipString;
                textTooltip.color = Color.cyan;
            }
            else {
                if(theCursorCzar.GetCursorPixelCoords().x <= 360 && theCursorCzar.GetCursorPixelCoords().y > 720) {
                    textTooltip.text = "Year " + (((float)simulationManager.simAgeTimeSteps / 2048f) * theCursorCzar.GetCursorPixelCoords().x / 360f).ToString("F0");
                    textTooltip.color = Color.yellow;
                    panelTooltip.SetActive(true);
                }
                else {
                    panelTooltip.SetActive(false);
                }
                
            }  
        }
        else {
            //Debug.Log(vegetationManager.ToString());
            //Debug.Log(vegetationManager.closestPlantParticleData.ToString());
            //Debug.Log(theCursorCzar.curMousePositionOnWaterPlane.ToString());
            
            //int selectedPlantID = vegetationManager.selectedPlantParticleIndex;
            //int closestPlantID = vegetationManager.closestPlantParticleData.index;
            float plantDist = (vegetationManager.closestPlantParticleData.worldPos - new Vector2(theCursorCzar.curMousePositionOnWaterPlane.x, theCursorCzar.curMousePositionOnWaterPlane.y)).magnitude;

            //int selectedZoopID = zooplanktonManager.selectedAnimalParticleIndex;
            //int closestZoopID = zooplanktonManager.closestAnimalParticleData.index;
            float zoopDist = (new Vector2(zooplanktonManager.closestAnimalParticleData.worldPos.x, zooplanktonManager.closestAnimalParticleData.worldPos.y) - new Vector2(theCursorCzar.curMousePositionOnWaterPlane.x, theCursorCzar.curMousePositionOnWaterPlane.y)).magnitude;
            
            
            //if(plantDist < zoopDist) {                       
            //    if(panelFocus == PanelFocus.Watcher && !cameraManager.isMouseHoverAgent && theCursorCzar.leftClickThisFrame) { 
            //        if(selectedPlantID != closestPlantID && plantDist < 3.3f) {
            //            vegetationManager.selectedPlantParticleIndex = closestPlantID;
            //            vegetationManager.isPlantParticleSelected = true;
            //            Debug.Log("FOLLOWING PLANT " + vegetationManager.selectedPlantParticleIndex.ToString());
                        //isSpiritBrushSelected = true;
            //            watcherUI.StartFollowingPlantParticle();
            //        }
            //    }
                
            //}
            //else {                   
            //    if (panelFocus == PanelFocus.Watcher && !cameraManager.isMouseHoverAgent && theCursorCzar.leftClickThisFrame) {
            //        if (selectedZoopID != closestZoopID && zoopDist < 3.3f) {
            //            zooplanktonManager.selectedAnimalParticleIndex = closestZoopID;
            //            zooplanktonManager.isAnimalParticleSelected = true;
            //            Debug.Log("FOLLOWING ZOOP " + zooplanktonManager.selectedAnimalParticleIndex.ToString());
                        //isSpiritBrushSelected = true;
            //            watcherUI.StartFollowingAnimalParticle();
            //        }
            //    }
            //}
            
            manager.isPlantParticleHighlight = 0f;
            manager.isZooplanktonHighlight = 0f;
            manager.isVertebrateHighlight = 0f;
            float hitboxRadius = 1f;
            if(cameraManager.isMouseHoverAgent) {  // move this to cursorCzar?
                manager.isVertebrateHighlight = 1f;

                textTooltip.text = "Critter #" + cameraManager.mouseHoverAgentRef.candidateRef.candidateID.ToString();
                textTooltip.color = Color.white;
            }
            else {
                if(plantDist < zoopDist && plantDist < hitboxRadius) {
                    manager.isPlantParticleHighlight = 1f;

                    textTooltip.text = "Algae #" + vegetationManager.closestPlantParticleData.index.ToString();
                    textTooltip.color = Color.green;
                }
                if(plantDist > zoopDist && zoopDist < hitboxRadius) {
                    manager.isZooplanktonHighlight = 1f;

                    textTooltip.text = "Microbe #" + zooplanktonManager.closestAnimalParticleData.index.ToString();
                    textTooltip.color = Color.yellow;
                }
            }

            if (manager.isPlantParticleHighlight == 0f && manager.isZooplanktonHighlight == 0f && manager.isVertebrateHighlight == 0f) {
                panelTooltip.SetActive(false);
            }
            else {
                panelTooltip.SetActive(true);
            }

            //float cursorCoordsX = Mathf.Clamp01((theCursorCzar.GetCursorPixelCoords().x) / 360f);
            //float cursorCoordsY = Mathf.Clamp01((theCursorCzar.GetCursorPixelCoords().y - 720f) / 360f);  
            if(theCursorCzar.GetCursorPixelCoords().x <= 360 && theCursorCzar.GetCursorPixelCoords().y > 720) {
                textTooltip.text = "TimeStep #" + ((float)simulationManager.simAgeTimeSteps * theCursorCzar.GetCursorPixelCoords().x / 360f).ToString("F0");
                textTooltip.color = Color.yellow;
                panelTooltip.SetActive(true);
            }
            
            //Debug.Log("plantDist: " + plantDist.ToString() + ", zoopDist: " + zoopDist.ToString() + ",  agent: " + cameraManager.isMouseHoverAgent.ToString());
            
            if (curActiveTool == ToolType.Stir) {  
                theCursorCzar.stirGizmoVisible = true;
                
                float isActing = 0f;
                
                if (theCursorCzar.isDraggingMouseLeft) { 
                    isActing = 1f;
                    
                    float mag = theCursorCzar.smoothedMouseVel.magnitude;
                    float radiusMult = Mathf.Lerp(0.075f, 1.33f, Mathf.Clamp01(theRenderKing.baronVonWater.camDistNormalized * 1.4f)); // 0.62379f; // (1f + gameManager.simulationManager.theRenderKing.baronVonWater.camDistNormalized * 1.5f);

                    if(mag > 0f) {
                        simulationManager.PlayerToolStirOn(theCursorCzar.curMousePositionOnWaterPlane, theCursorCzar.smoothedMouseVel * (0.25f + theRenderKing.baronVonWater.camDistNormalized * 1.2f), radiusMult);  
                    }
                    else {
                        simulationManager.PlayerToolStirOff();
                    }
                }
                else {
                    simulationManager.PlayerToolStirOff();                        
                }

                if(isActing > 0.5f) {
                    //theCursorCzar.stirStickDepth = Mathf.Lerp(theCursorCzar.stirStickDepth, 1f, 0.2f);
                }
                else {
                    //theCursorCzar.stirStickDepth = Mathf.Lerp(theCursorCzar.stirStickDepth, -4f, 0.2f);
                }
                theRenderKing.isStirring = theCursorCzar.isDraggingMouseLeft;
                theRenderKing.gizmoStirToolMat.SetFloat(STIRRING, isActing);
                theRenderKing.gizmoStirStickAMat.SetFloat(STIRRING, isActing);                    
                theRenderKing.gizmoStirStickAMat.SetFloat(RADIUS, 6.2f);
            }

            theRenderKing.isBrushing = false;
            theRenderKing.isSpiritBrushOn = false;
            theRenderKing.nutrientToolOn = false;
            isBrushAddingAgents = false;
            vegetationManager.isBrushActive = false;
            
            if (curActiveTool == ToolType.Add && panelFocus == PanelFocus.Brushes) {
                // What Palette Trophic Layer is selected?
                theCursorCzar.stirGizmoVisible = true;

                simulationManager.PlayerToolStirOff();

                if (theCursorCzar.isDraggingMouseLeft || theCursorCzar.isDraggingMouseRight) {
                    if(!brushesUI.isInfluencePointsCooldown) {
                        if(brushesUI.toolbarInfluencePoints >= 0.05f) {
                            brushesUI.ApplyCreationBrush();
                        }
                        else {
                            Debug.Log("NOT ENOUGH MANA!");
                            // Enter Cooldown!
                            brushesUI.isInfluencePointsCooldown = true;
                        } 
                    }                                               
                }
            }
        }
        
        if (theCursorCzar.isDraggingMouseLeft || theCursorCzar.isDraggingMouseRight) {
            theRenderKing.ClickTestTerrainUpdateMaps(updateTerrainAltitude, terrainUpdateMagnitude);
        }
        else {
            theRenderKing.ClickTestTerrainUpdateMaps(updateTerrainAltitude, terrainUpdateMagnitude);                   
        }
        
        if(theCursorCzar.stirGizmoVisible) {
            theRenderKing.gizmoStirToolMat.SetFloat(VISIBLE, 1f);
            theRenderKing.gizmoStirStickAMat.SetFloat(VISIBLE, 1f);
        }         
        else {
            theRenderKing.gizmoStirToolMat.SetFloat(VISIBLE, 0f);
            theRenderKing.gizmoStirStickAMat.SetFloat(VISIBLE, 0f);
        }
    }     
}

