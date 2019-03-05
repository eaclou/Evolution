using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrophicTier {

    public bool nextTierUnlocked = false;
    public TrophicSlot[] trophicSlots;

	public TrophicTier() {
        trophicSlots = new TrophicSlot[4];  // max size 4 for now?
        for(int i = 0; i < 4; i++) {
            trophicSlots[i] = new TrophicSlot();
        }
    }
}
