using System.Collections.Generic;
using UnityEngine;

public class FindNearestAgent : MonoBehaviour
{
    [SerializeField] Agent self;
    [SerializeField] TrackNearbyAgents agentTracker;
    
    List<Agent> nearbyAgents => agentTracker.nearbyAgents;
    Vector3 ownPosition => self.transform.position;
    CritterModuleCore core => self.coreModule;

    float nearestFriendDistance;
    [Tooltip("Exposed for debugging")]
    public Agent nearestFriend;
    
    float nearestEnemyDistance;
    [Tooltip("Exposed for debugging")]
    public Agent nearestEnemy;
    
    bool _isFriend;
    float _neighborDistance;
    
    // Replace with InvokeRepeating to expose interval
    private void Update() { Refresh(); }
    
    private void Refresh()
    {
        nearestFriend = null;
        nearestEnemy = null;
        nearestFriendDistance = Mathf.Infinity;
        nearestEnemyDistance = Mathf.Infinity;
    
        foreach (var neighbor in nearbyAgents)
        {
            if(!neighbor.isMature) 
                continue;
                
            _isFriend = neighbor.speciesIndex == self.speciesIndex;
            _neighborDistance = Vector3.Distance(ownPosition, neighbor.transform.position);
            
            if (_isFriend && _neighborDistance < nearestFriendDistance)
            {
                nearestFriend = neighbor;
                nearestFriendDistance = _neighborDistance;
            }
            else if (!_isFriend && _neighborDistance < nearestEnemyDistance)
            {
                nearestEnemy = neighbor;
                nearestEnemyDistance = _neighborDistance;
            }
        }
        
        core.nearestFriendAgent = nearestFriend;
        core.nearestEnemyAgent = nearestEnemy;   
    }
}
