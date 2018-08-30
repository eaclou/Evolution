using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGridCell {

    public Vector2 bottomLeft;
    public Vector2 topRight;
    public List<int> eggSackIndicesList;
    //public List<int> deadAnimalIndicesList;
    public List<int> agentIndicesList;
    //public List<int> predatorIndicesList;

    public MapGridCell(Vector2 bottomLeft, Vector2 topRight) {
        this.bottomLeft = bottomLeft;
        this.topRight = topRight;
        eggSackIndicesList = new List<int>();
        //deadAnimalIndicesList = new List<int>();
        agentIndicesList = new List<int>();
        //predatorIndicesList = new List<int>();
    }
    
}
