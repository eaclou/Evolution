using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BigBangPanelUI : MonoBehaviour
{
    UIManager manager => UIManager.instance;
    ToolType curActiveTool { set => manager.curActiveTool = value; }

    [SerializeField] int totalFrameDuration = 70;
    [SerializeField] BrushStroke[] strokes;
    
    [HideInInspector] public bool isRunning;
    
    public void Begin() 
    {
        gameObject.SetActive(true);
        StartCoroutine(Process()); 
    }
    
    IEnumerator Process()
    {
        isRunning = true;
        int frameCounter = 0;
        
        manager.InitialUnlocks();
        curActiveTool = ToolType.Stir;
        
        foreach (var stroke in strokes)
            stroke.image.enabled = true;

        while (frameCounter <= totalFrameDuration)
        {
            foreach (var stroke in strokes)
                if (stroke.disableOnFrame == frameCounter)
                    stroke.image.enabled = false;

            yield return null;
            frameCounter += 1;
        }
        
        gameObject.SetActive(false);
        curActiveTool = ToolType.None;
        isRunning = false;
    }
    
    [Serializable]
    public struct BrushStroke
    {
        public Image image;
        public int disableOnFrame;
    }
}
