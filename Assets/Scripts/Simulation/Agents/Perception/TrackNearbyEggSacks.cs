using System.Collections.Generic;
using UnityEngine;

public class TrackNearbyEggSacks : MonoBehaviour
{
    public List<EggSack> nearbyEggSacks;
    
    void OnTriggerEnter2D(Collider2D other)
    {
        var eggSack = other.GetComponent<EggSack>();
        if (!eggSack) return;
        nearbyEggSacks.Add(eggSack);
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        var eggSack = other.GetComponent<EggSack>();
        if (!eggSack) return;
        nearbyEggSacks.Remove(eggSack);
    }
    
    void OnDisable() 
    { 
        nearbyEggSacks.Clear(); 
    }
}
