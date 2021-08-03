using System.Collections.Generic;
using UnityEngine;

public class TrackNearbyAgents : MonoBehaviour
{
    public List<Agent> nearbyAgents;
    
    void OnTriggerEnter2D(Collider2D other)
    {
        var mouth = other.GetComponent<CritterMouthComponent>();
        if (!mouth) return;
        nearbyAgents.Add(mouth.agent);
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        var mouth = other.GetComponent<CritterMouthComponent>();
        if (!mouth) return;
        nearbyAgents.Remove(mouth.agent);
    }
    
    void OnDisable() 
    { 
        nearbyAgents.Clear(); 
    }
}
