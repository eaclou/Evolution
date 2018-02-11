using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSearchManager {

    public int resolution = 2;  // how to split each dimension
    public int duration = 4;  // number of generations for each Run

    // Start out hardcoded to 4 dimensions, expand functionality later:
    public int coordA = 0;
    public int coordB = 0;
    public int coordC = 0;
    public int coordD = 0;

    public float minA = 0.01f;
    public float maxA = 0.1f;
    public float minB = 0.01f;
    public float maxB = 0.1f;
    public float minC = 0.01f;
    public float maxC = 0.1f;
    public float minD = 0.01f;
    public float maxD = 0.1f;

    public GridSearchResults storedResults;

    public bool isComplete = false;

    public GridSearchManager() {
        
    }

    public void InitializeGridSearch() {
        storedResults = new GridSearchResults();
        coordA = 0;
        coordB = 0;
        coordC = 0;
        coordD = 0;

        GridSearchRunData GridSearchRunData = new GridSearchRunData();
        storedResults.dataList.Add(GridSearchRunData);
    }

    public void StartNewRun() {
        
        // Next Coordinates:
        coordD += 1;
        if(coordD >= resolution) {
            coordD = 0;

            coordC += 1;
            if (coordC >= resolution) {
                coordC = 0;

                coordB += 1;
                if (coordB >= resolution) {
                    coordB = 0;

                    coordA += 1;
                    if(coordA >= resolution) {
                        // DONE!!

                        Debug.Log("GridSearch COMPLETE!!!");
                        isComplete = true;
                        return;
                    }
                }
            }
        }
        Debug.Log("GridSearch RUN || A: " + coordA.ToString() + ", B: " + coordB.ToString() + ", C: " + coordC.ToString() + ", D: " + coordD.ToString());
        GridSearchRunData GridSearchRunData = new GridSearchRunData();

        GridSearchRunData.dimensionA = Mathf.Lerp(minA, maxA, (float)coordA / (float)(resolution - 1));
        GridSearchRunData.dimensionB = Mathf.Lerp(minB, maxB, (float)coordB / (float)(resolution - 1));
        GridSearchRunData.dimensionC = Mathf.Lerp(minC, maxC, (float)coordC / (float)(resolution - 1));
        GridSearchRunData.dimensionD = Mathf.Lerp(minD, maxD, (float)coordD / (float)(resolution - 1));

        //storedResults.dataList.Add(GridSearchRunData);
    }

    public void UpdateGridSearch() {

    }

    public void DataEntry(int curGen, float score) {
        storedResults.dataList[storedResults.dataList.Count - 1].scoresList.Add(score);
        if(score > storedResults.dataList[storedResults.dataList.Count - 1].bestScore) {
            storedResults.dataList[storedResults.dataList.Count - 1].bestScore = score;
        }
    }
}
