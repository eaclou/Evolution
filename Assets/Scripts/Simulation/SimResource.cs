using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimResource
{
    public string name;
    public List<ResourceDataPoint> resourceDataPointList;
    private float minDisplayValue;
    private float maxDisplayValue;
    private int maxNumDataPointEntries;
    private Color displayColor;

    public Color GetColor() {
        return displayColor;
    }
    public float GetMinValue() {
        return minDisplayValue;
    }
    public float GetMaxValue() {
        return maxDisplayValue;
    }
    public SimResource(string n, int maxNum, Color col) {
        // constructor
        this.name = n;
        resourceDataPointList = new List<ResourceDataPoint>();
        maxNumDataPointEntries = maxNum;
        minDisplayValue = float.PositiveInfinity;
        maxDisplayValue = float.NegativeInfinity;
        displayColor = col;
    }

    public void AddNewResourceDataEntry(int timestep, float val) {
        
        ResourceDataPoint point = new ResourceDataPoint();
        point.timestep = timestep;
        point.value = val;
        if(resourceDataPointList == null) {
            resourceDataPointList = new List<ResourceDataPoint>();
        }
        if(val < minDisplayValue) {
            minDisplayValue = val;
        }
        if(val > maxDisplayValue) {
            maxDisplayValue = val;
        }
        resourceDataPointList.Add(point);
        if(resourceDataPointList.Count > maxNumDataPointEntries) {
            MergeDataPoints();
        }
        // how to handle bounds? does each resource need its own min/max?

        maxDisplayValue *= 0.9999f;
    }

    private void MergeDataPoints() {
        float closestPairDistance = float.PositiveInfinity;
        int closestPairStartIndex = 1;
        for(int i = 1; i < resourceDataPointList.Count - 2; i++) { // don't include first or last point
            float distFront = resourceDataPointList[i + 1].timestep - resourceDataPointList[i].timestep;
            float distBack = resourceDataPointList[i].timestep - resourceDataPointList[i - 1].timestep;

            float multiplier = 25f;
            float bonusDist = (multiplier - (float)i * multiplier / (float)(resourceDataPointList.Count - 1)) * 1f;
            float dist = (distFront + distBack) / bonusDist;
            if(dist < closestPairDistance) {
                closestPairDistance = dist;
                closestPairStartIndex = i;
            }
        }
        ResourceDataPoint avgData = new ResourceDataPoint();
        avgData.timestep = (resourceDataPointList[closestPairStartIndex].timestep + resourceDataPointList[closestPairStartIndex + 1].timestep) / 2f;
        avgData.value = (resourceDataPointList[closestPairStartIndex].value + resourceDataPointList[closestPairStartIndex + 1].value) / 2f;
                
        resourceDataPointList[closestPairStartIndex + 1] = avgData;
        resourceDataPointList.RemoveAt(closestPairStartIndex);
        
    }
}
