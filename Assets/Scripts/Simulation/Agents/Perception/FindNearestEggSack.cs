using System.Collections.Generic;
using UnityEngine;

public class FindNearestEggSack : MonoBehaviour
{
    [SerializeField] Agent self;
    [SerializeField] TrackNearbyEggSacks eggTracker;
    
    List<EggSack> nearbyEggSacks => eggTracker.nearbyEggSacks;
    Vector3 ownPosition => self.transform.position;
    CritterModuleCore core => self.coreModule;

    float nearestEggSackDistance;
    [Tooltip("Exposed for debugging")]
    public EggSack nearestEggSack;

    float _eggSackDistance;
    
    // Replace with InvokeRepeating to expose interval
    private void Update() { Refresh(); }
    
    private void Refresh()
    {
        nearestEggSack = null;
        nearestEggSackDistance = Mathf.Infinity;
    
        foreach (var egg in nearbyEggSacks)
        {
            if(egg.isNull || egg.isProtectedByParent) 
                continue;
                
            _eggSackDistance = Vector3.Distance(ownPosition, egg.transform.position);
            
            if (_eggSackDistance >= nearestEggSackDistance)
                continue;
                
            nearestEggSack = egg;
            nearestEggSackDistance = _eggSackDistance;
        }
        
        if (nearestEggSack)
            core.nearestEggSackModule = nearestEggSack;
    }
}
