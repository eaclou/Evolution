using Playcraft;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TheCursorCzar : Singleton<TheCursorCzar> 
{
    CameraManager cameraManager => CameraManager.instance;
    UIManager uiManagerRef => UIManager.instance;
    TheRenderKing theRenderKing => TheRenderKing.instance;
    SelectionManager selectionManager => SelectionManager.instance;

    Camera cam => cameraManager.cameraRef;
    
    float mapSize => SimulationManager._MapSize;
    float halfMapSize => mapSize * 0.5f;
    float waterLevel => SimulationManager._GlobalWaterLevel;
    float maxAltitude => SimulationManager._MaxAltitude;
    
    public bool isHoverClickableSpirit;     // WPP: not used

    public GameObject mouseRaycastWaterPlane;

    public Vector3 curMousePositionOnWaterPlane;
    public Vector2 curMousePositionOnWaterPlane2D => new Vector2(curMousePositionOnWaterPlane.x, curMousePositionOnWaterPlane.y);

    public Text textTooltip;
    public GameObject panelTooltip;
    public Text textTooltipDescription;
    public Image imageTooltipIcon;

    public Vector2 smoothedMouseVel;
    private Vector2 instantMouseVel;
    public Vector2 curMousePixelPos;
    private Vector2 prevMousePixelPos;

    public bool stirGizmoVisible = false;

    public bool leftClickThisFrame = false;
    public bool rightClickThisFrame = false;
    public bool letGoThisFrameLeft = false;
    public bool letGoThisFrameRight = false;
    public bool isDraggingMouseLeft = false;
    public bool isDraggingMouseRight = false;
    public bool isDraggingSpeciesNode = false;  // Future use?
    
    public bool isDraggingMouse => isDraggingMouseLeft || isDraggingMouseRight;
    public Vector2 normalSmoothedMouseVelocity => new Vector2(smoothedMouseVel.x, smoothedMouseVel.y).normalized;

    public Texture2D cursorTexBrush;    // Future use?
    public Texture2D cursorTexWatcher;
    public Texture2D cursorTexWorld;    // Future use?

    public Vector3 cursorParticlesWorldPos;
    public Ray cursorRay;   // WPP: not used
    
    //public Vector2 screenResolution = new Vector2(1920, 1080);
    float mouseXScreenNormal => Mathf.Clamp(curMousePixelPos.x, 0f, Screen.width - 180);
    float mouseYScreenNormal => Mathf.Clamp(curMousePixelPos.y, 0f, Screen.height - 70);
    
    public Vector2 GetCursorPixelCoords() {
        return curMousePixelPos;
    }
    
    public Vector2 GetScaledCursorCoords(float inverseScale, float yOffset)
    {
        float cursorCoordsX = Mathf.Clamp01(GetCursorPixelCoords().x / inverseScale);
        float cursorCoordsY = Mathf.Clamp01((GetCursorPixelCoords().y + yOffset) / inverseScale);
        return new Vector2(cursorCoordsX, cursorCoordsY);  
    }
    
    [SerializeField] RectTransform speciesHistoryRect;

    /// Get position of mouse on water plane:
    private void MouseRaycastWaterPlane(Vector3 screenPos) {
        mouseRaycastWaterPlane.SetActive(true);
        
        Vector3 targetPosition = new Vector3(halfMapSize, halfMapSize, -waterLevel * maxAltitude);

        mouseRaycastWaterPlane.gameObject.transform.position = targetPosition;
        
        cursorRay = cam.ScreenPointToRay(screenPos);
        //cursorRay = new Ray(uiManagerRef.cameraManager.gameObject.transform.position, midMidpoint - uiManagerRef.cameraManager.gameObject.transform.position);
        int layerMask = 1 << 12;  // UtilityRaycast???
        Physics.Raycast(cursorRay, out RaycastHit hit, layerMask);

        if (hit.collider) {
            curMousePositionOnWaterPlane = hit.point;
            cursorParticlesWorldPos = hit.point - cursorRay.direction * 5f;
            //Debug.Log("curMousePositionOnWaterPlane:" + curMousePositionOnWaterPlane.ToString() + ", " + screenPos.ToString() + ", hit: " + hit.point.ToString());
        }
        else {
            Debug.Log("NULL: " + cursorRay);
        }
    }    
    
    /// Check for player clicking on an animal in the world
    private void MouseRaycastCheckAgents(bool clicked) {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        int layerMask = ~(1 << LayerMask.NameToLayer("UtilityRaycast"));  // *** THIS ISN'T WORKING!!! *** might be inverted?

        Physics.Raycast(ray, out RaycastHit hit, 1000f, layerMask);  // *** USE DEDICATED LAYER FOR THIS CHECK!!!! *********

        cameraManager.SetMouseHoverAgent(null, false);
        cameraManager.SetMouseHoverEggSack(null, false);
        isHoverClickableSpirit = false;
                
        if (!hit.collider) return;

        Agent agent = hit.collider.gameObject.GetComponentInParent<Agent>();
        if (agent) 
        {
            cameraManager.MouseOverAgent(agent, clicked);
        
            if (!clicked)
                return; 
            
            //uiManagerRef.OnAgentSelected?.Invoke(agent); //***EAC what does this do?
            selectionManager.SetSelected(agent.candidateRef);
            cameraManager.SetFollowing(KnowledgeMapId.Animals);
        }
        else if (!clicked) 
        {
            //Testing: eggsack hover detection:
        EggSack eggSack = hit.collider.gameObject.GetComponentInParent<EggSack>();
        if (eggSack) {
            if (eggSack.curLifeStage == EggSack.EggLifeStage.Null) return;
                //Debug.Log("HorizontalOrVerticalLayoutGroup EGGSACK");
                cameraManager.MouseOverEggSack(eggSack, true);
            }
            isHoverClickableSpirit = true;
            //Debug.Log("_IsHoverClickableSpirit ON: [ ");
        }
        //Debug.Log("CLICKED ON: [ " + hit.collider.gameObject.name + " ] Ray= " + ray.ToString() + ", hit= " + hit.point.ToString());

        
        
    }
    
	public void Tick() {
        SetCursorTexture();
        RefreshMouseInput();
        
        if (!EventSystem.current.IsPointerOverGameObject()) {
            MouseRaycastCheckAgents(leftClickThisFrame);
        }
        
        MouseRaycastWaterPlane(Input.mousePosition);
        
        Vector4[] dataArray = new Vector4[1];
        Vector4 gizmoPos = new Vector4(curMousePositionOnWaterPlane.x, curMousePositionOnWaterPlane.y, 0f, 0f);
        dataArray[0] = gizmoPos;
        if(theRenderKing.gizmoCursorPosCBuffer != null) {
            theRenderKing.gizmoCursorPosCBuffer.SetData(dataArray);
        }
        

        stirGizmoVisible = false;

        //-----------------------------------------------------------------------------------------
        curMousePixelPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        instantMouseVel = curMousePixelPos - prevMousePixelPos;
        smoothedMouseVel = Vector2.Lerp(smoothedMouseVel, instantMouseVel, 0.16f);
        prevMousePixelPos = curMousePixelPos;

        Vector3 newTooltipPosition = new Vector3(mouseXScreenNormal, mouseYScreenNormal, 0f);
        panelTooltip.transform.position = newTooltipPosition;
	}
	
	void RefreshMouseInput()
	{
        leftClickThisFrame = Input.GetMouseButtonDown(0);
        rightClickThisFrame = Input.GetMouseButtonDown(1);
        letGoThisFrameLeft = Input.GetMouseButtonUp(0);
        letGoThisFrameRight = Input.GetMouseButtonUp(1);
        isDraggingMouseLeft = Input.GetMouseButton(0);
        isDraggingMouseRight = Input.GetMouseButton(1);
        
        if (letGoThisFrameLeft) {
            isDraggingMouseLeft = false;
            isDraggingSpeciesNode = false;
        }
        if (letGoThisFrameRight) {
            isDraggingMouseRight = false;
        }
	}
	
	// If cursor variation desired, setup a lookup pattern based on PanelFocus
	void SetCursorTexture()
	{
        Cursor.SetCursor(cursorTexWatcher, Vector2.zero, CursorMode.Auto);
    }
}
