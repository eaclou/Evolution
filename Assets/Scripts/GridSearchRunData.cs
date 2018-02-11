using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GridSearchRunData {

    public float dimensionA;
    public float dimensionB;
    public float dimensionC;
    public float dimensionD;

    public float bestScore;
    public List<float> scoresList;

    public GridSearchRunData() {
        scoresList = new List<float>();
    }
}
