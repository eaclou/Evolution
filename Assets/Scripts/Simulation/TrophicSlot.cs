using UnityEngine;

public class TrophicSlot {
    public TrophicLayerSO data;
    public KnowledgeMapId id => data.id;
    public KingdomId kingdomID => data.kingdom;
    
    // Obsolete identifier -> remove
    //public int tierID => data.listIndex;
    
    public int layerIndex => data.layerIndex;
    public Sprite icon => data.icon;
    public Color color => data.color;
    
    public TrophicSlotStatus status;
    
    // For animals
    public int linkedSpeciesID; // * WPP: what is the purpose of this?
    public int slotID;
    public string speciesName;


    public TrophicSlot(TrophicLayerSO data) {
        this.data = data;
        status = data.initialStatus;
        speciesName = data.defaultSpeciesName;
    }
}

public enum TrophicSlotStatus {
    Off,       // doesn't exist
    Locked,    // exists but unavailable
    Unlocked,  // ready to be added
    On,        // active, occupied
}
