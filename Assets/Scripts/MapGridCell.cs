using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGridCell {

    public Vector2 bottomLeft;
    public Vector2 topRight;
    public List<int> foodIndicesList;
    //public List<int> deadAnimalIndicesList;
    public List<int> friendIndicesList;
    public List<int> predatorIndicesList;

    public MapGridCell(Vector2 bottomLeft, Vector2 topRight) {
        this.bottomLeft = bottomLeft;
        this.topRight = topRight;
        foodIndicesList = new List<int>();
        //deadAnimalIndicesList = new List<int>();
        friendIndicesList = new List<int>();
        predatorIndicesList = new List<int>();
    }
    
}
