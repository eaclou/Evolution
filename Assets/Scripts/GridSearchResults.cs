using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GridSearchResults {

    public List<GridSearchRunData> dataList;

    public GridSearchResults() {
        dataList = new List<GridSearchRunData>();
    }
}
