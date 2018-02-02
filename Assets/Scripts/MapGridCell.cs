using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGridCell {

    public Vector2 topLeft;
    public Vector2 bottomRight;
    public List<int> agentIndicesList;

    public MapGridCell(Vector2 topLeft, Vector2 bottomRight) {
        this.topLeft = topLeft;
        this.bottomRight = bottomRight;
        agentIndicesList = new List<int>();
    }

    
}
