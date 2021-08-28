using UnityEngine;
using UnityEngine.UI;

public class BigBangPanelUI : MonoBehaviour
{
    UIManager manager => UIManager.instance;
    
    public GameObject panelBigBang;
    public Image imageBigBangStrokes01;
    public Image imageBigBangStrokes02;
    public Image imageBigBangStrokes03;

    SimulationManager simulationManager => SimulationManager.instance;
    
    ToolType curActiveTool { set => manager.curActiveTool = value; }
    //PanelFocus panelFocus { set => manager.panelFocus = value; }  //*EC no longer used
    
    public int bigBangFramesCounter = 0;

    public bool Tick() {
        //Debug.Log("IS THIS RUNNING!?!?!? " + bigBangFramesCounter.ToString());

        if (!simulationManager._BigBangOn) 
            return false;

        panelBigBang.SetActive(true);
        bigBangFramesCounter += 1;
        
        if(bigBangFramesCounter == 1) {
            manager.InitialUnlocks();    
        }   
        if(bigBangFramesCounter > 70) {
            bigBangFramesCounter = 0;
            simulationManager._BigBangOn = false;
            panelBigBang.SetActive(false);
            curActiveTool = ToolType.None;
        }
        else if(bigBangFramesCounter > 40) {
            imageBigBangStrokes01.gameObject.SetActive(true);
            imageBigBangStrokes02.gameObject.SetActive(false);
            imageBigBangStrokes03.gameObject.SetActive(false);
            //worldSpiritHubUI.PlayBigBangSpawnAnim();
            simulationManager.vegetationManager.isBrushActive = true;
            //panelFocus = PanelFocus.Watcher;
        }
        else if(bigBangFramesCounter > 20) {
            imageBigBangStrokes01.gameObject.SetActive(true);
            imageBigBangStrokes02.gameObject.SetActive(true);
            imageBigBangStrokes03.gameObject.SetActive(false);
            //simulationManager.zooplanktonManager. = true;
        }
        else if(bigBangFramesCounter > 0) {
            imageBigBangStrokes01.gameObject.SetActive(true);
            imageBigBangStrokes02.gameObject.SetActive(true);
            imageBigBangStrokes03.gameObject.SetActive(true);
            curActiveTool = ToolType.Stir;
        }
        
        return true;
    }
}
