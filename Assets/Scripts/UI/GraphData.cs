using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphData {

    public static int historicalGraphsResolution = 64;

    public bool isActive = false;
    public float[] dataArray;
    public Texture2D dataTex;
    public Material targetDisplayMat;
    public float minValue = 0f;
    public float maxValue = 1f;
    public int nextWriteIndex = 0;

	public GraphData(Material targetMat) {
        if (dataTex == null) {
            dataTex = new Texture2D(historicalGraphsResolution, 1, TextureFormat.RGBAFloat, false);
            dataTex.filterMode = FilterMode.Bilinear;
            dataTex.wrapMode = TextureWrapMode.Clamp;
        }

        dataArray = new float[historicalGraphsResolution];

        targetDisplayMat = targetMat;
        targetDisplayMat.SetTexture("_DataTex", dataTex);
        
        nextWriteIndex = 0;
        isActive = true;
    }

    public void AddNewEntry(float value) {
        
        if(nextWriteIndex >= historicalGraphsResolution) {
            RebuildDataArray(); 
            
            for(int i = 0; i < historicalGraphsResolution; i++) {
                dataTex.SetPixel(i, 0, new Color(dataArray[i], 0f, 0f)); 
            }
        }
        int index = nextWriteIndex;
        // Add
        dataArray[index] = value;        
        if(value < minValue) {
            minValue = value;
        }

        if(value > maxValue) {
            maxValue = value;
        }
        dataTex.SetPixel(index, 0, new Color(value, 0f, 0f));

        dataTex.Apply();        

        RefreshShaderProperties();

        nextWriteIndex++;

        Debug.Log("AddNewEntry[" + index.ToString() + "] (" + value.ToString() + ")  (" + minValue.ToString() + ",  " + maxValue.ToString() + ")");
    }

    private void RefreshShaderProperties() {
        targetDisplayMat.SetFloat("_MinValue", minValue);
        targetDisplayMat.SetFloat("_MaxValue", maxValue);

        targetDisplayMat.SetFloat("_SampleCoordMax", (float)nextWriteIndex / (float)historicalGraphsResolution);
    }
    
    private void RebuildDataArray() {
        // take full buffer, compress temporally by half. 

        // Build new value array? --> build texture
        

        for(int i = 0; i < historicalGraphsResolution / 2; i++) {
            float avgValue = (dataArray[i * 2] + dataArray[i * 2 + 1]) / 2f;
            
            dataArray[i] = avgValue;                   
        }
        //dataTex.Apply(); 

        // Zero out secondHalf
        for (int i = historicalGraphsResolution / 2; i < historicalGraphsResolution; i++) {            
            dataArray[i] = 0f; // do I even have to zero these out? or simply overwrite (robust pointer logic)
            //dataTex.SetPixel(i, 0, new Color(dataArray[i], 0f, 0f));     
        }
        // update pointer to start writing at midpoint
        
        nextWriteIndex = historicalGraphsResolution / 2;
        Debug.Log("RebuildDataArray() " + nextWriteIndex.ToString());
    }
}
