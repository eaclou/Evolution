using UnityEngine;
using Playcraft;

public class SpawnZone : MonoBehaviour 
{
    public float radius = 10f;
    public bool active = true;
    public int refactoryCounter = 0;
    [Range(0, 1)] public float resetChance = .002f;

    void FixedUpdate() {
        if (!active) {
            refactoryCounter++;
        }
        else if (RandomStatics.CoinToss(resetChance)) {
            active = false;
            refactoryCounter = 0;
        }
    }
}