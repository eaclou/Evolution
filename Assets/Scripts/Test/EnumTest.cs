using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnumTest : MonoBehaviour
{
    [SerializeField] SimEventData.SimEventTypeMinor minorEvent;
    [SerializeField] bool containedInList;
    [SerializeField] List<SimEventData> simEvents;

    public void SetRandomMinorEvent()
    {
        minorEvent = SimEventData.GetRandomMinorEventType();
    }
    
    public void CheckForDuplicates()
    {
        containedInList = simEvents.Any(e => e.typeMinor == minorEvent);
    }
}
