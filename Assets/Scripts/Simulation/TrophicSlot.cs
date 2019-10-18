using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrophicSlot {

    public SlotStatus status;
    public enum SlotStatus {
        Off,  // doesn't exist
        Locked,  // exists but unavailable
        Unlocked,  // ready to be added
        On,  // active, occupied
    }
    public int kingdomID;
    public int tierID;
    public int slotID;
    public int linkedSpeciesID;
    public string speciesName;

    public TrophicSlot() {
        status = SlotStatus.Off;  // default off (hidden)
    }

    public void Initialize(string name, SlotStatus status, int kingdomID, int tierID, int slotID) {
        speciesName = name;
        this.status = status;
        this.kingdomID = kingdomID;
        this.tierID = tierID;
        this.slotID = slotID;
    }
}
