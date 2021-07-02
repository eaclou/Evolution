using System.Collections.Generic;
using UnityEngine;

public class FindNearestNeighbors : MonoBehaviour
{
    [SerializeField] Agent self;
    [SerializeField] TrackNearbyAgents agentTracker;
    
    List<Agent> nearbyAgents => agentTracker.nearbyAgents;
    Vector3 ownPosition => self.transform.position;
    CritterModuleCore core => self.coreModule;

    float nearestFriendDistance;
    Agent nearestFriend;
    
    float nearestEnemyDistance;
    Agent nearestEnemy;
    
    bool _isFriend;
    float _neighborDistance;
    
    public void Refresh()
    {
        SetNearestFriendAndEnemy();
        SetNearestEggSack();
    }
    
    void SetNearestFriendAndEnemy()
    {
        nearestFriend = self;
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
    
    // In progress
    // * Remove: use separate component
    void SetNearestEggSack()
    {
        
    }
}
