using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GridSearchResults {

    public float highestScoreAchieved;
    public float lowestScoreAchieved;
    public List<string> readoutList;
    public List<float> trainingRunsScoreList;
    public List<GridSearchRunData> dataList;

    public List<GenePool> genePoolList;

    public GridSearchResults() {
        readoutList = new List<string>();
        trainingRunsScoreList = new List<float>();
        dataList = new List<GridSearchRunData>();
        genePoolList = new List<GenePool>();
    }
}
