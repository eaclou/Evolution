using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Feat {

    public string name;
    public enum FeatType {
        Decomposer,
        Algae,
        Plants,
        Zooplankton,
        Vertebrate,
        Water,
        Stone,
        Watcher,
        Mutation,
        WorldExpand
    }
    public FeatType featType;
    public int eventFrame;
    public Color color;
    public string description;

	public Feat(string name, FeatType type, int eventFrame, Color color, string description) {
        this.name = name;        
        featType = type;
        this.eventFrame = eventFrame;
        this.color = color;
        this.description = description;
    }


}
