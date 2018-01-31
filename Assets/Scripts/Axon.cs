using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Axon {

    public int fromID;
    public int toID;
    public float weight;

    public Axon(int from, int to, float w) {
        fromID = from;
        toID = to;
        weight = w;
    }
}
