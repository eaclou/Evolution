﻿using UnityEngine;

public class BrainGenomeImage : MonoBehaviour
{
    [SerializeField] Material mat;
    [SerializeField] string mainTexture = "_MainTex";

    // Barcode
    Texture2D texture;
    
    const int WIDTH = 256;

    void Start() 
    {
        texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        mat.SetTexture(mainTexture, texture);
    }
    
    // * WPP: expose values in editor
    public void SetTexture(BrainGenome brain) 
    {
        texture.Resize(WIDTH, 1);

        for(int x = 0; x < WIDTH; x++) 
        {                              
            Color testColor;

            if (brain.linkList.Count > x) 
            {
                float weightVal = brain.linkList[x].weight;
                testColor = new Color(weightVal * 0.5f + 0.5f, weightVal * 0.5f + 0.5f, weightVal * 0.5f + 0.5f);
                
                if(weightVal < -0.25f) 
                {
                    testColor = Color.Lerp(testColor, Color.black, 0.15f);
                }
                else if(weightVal > 0.25f) 
                {
                    testColor = Color.Lerp(testColor, Color.white, 0.15f);
                }
                else 
                {
                    testColor = Color.Lerp(testColor, Color.gray, 0.15f);
                }
            }
            else 
            {
                testColor = Color.black; // CLEAR
            }
                
            texture.SetPixel(x, 0, testColor);
        }
        
        texture.Apply();
    } 

}
