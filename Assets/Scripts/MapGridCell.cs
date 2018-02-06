using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGridCell {

    public Vector2 topLeft;
    public Vector2 bottomRight;
    public List<int> foodIndicesList;
    public List<int> friendIndicesList;
    public List<int> predatorIndicesList;

    public MapGridCell(Vector2 topLeft, Vector2 bottomRight) {
        this.topLeft = topLeft;
        this.bottomRight = bottomRight;
        foodIndicesList = new List<int>();
        friendIndicesList = new List<int>();
        predatorIndicesList = new List<int>();
    }
    
}
