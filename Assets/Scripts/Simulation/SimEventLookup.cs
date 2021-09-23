using System;
using UnityEngine;

// RENAME
[CreateAssetMenu(menuName = "Pond Water/Sim Events", fileName = "Sim Events")]
public class SimEventLookup : ScriptableObject
{
    [SerializeField] MinorEvent[] minorEvents;
    [SerializeField] MajorEvent[] majorEvents;
    [SerializeField] ExtremeEvent[] extremeEvents;
    
    public SimEvent GetEventData(SimEventData input)
    {
        switch (input.category)
        {
            case SimEventCategories.Minor: return GetMinorEvent(input.typeMinor);
            case SimEventCategories.Major: return GetMajorEvent(input.typeMajor);
            case SimEventCategories.Extreme: return GetExtremeEvent(input.typeExtreme);
            default: return null;
        }
    }
    
    SimEvent GetMinorEvent(SimEventTypeMinor id)
    {
        foreach (var item in minorEvents)
            if (item.id == id)
                return item.data;
                
        return null;
    }
    
    [Serializable]
    public class MinorEvent
    {
        public SimEventTypeMinor id;
        public SimEvent data;
    }
        
    SimEvent GetMajorEvent(SimEventTypeMajor id)
    {
        foreach (var item in majorEvents)
            if (item.id == id)
                return item.data;
                
        return null;
    }
    
    [Serializable]
    public class MajorEvent
    {
        public SimEventTypeMajor id;
        public SimEvent data;
    }
    
    SimEvent GetExtremeEvent(SimEventTypeExtreme id)
    {
        foreach (var item in extremeEvents)
            if (item.id == id)
                return item.data;
                
        return null;
    }

    [Serializable]
    public class ExtremeEvent
    {
        public SimEventTypeExtreme id;
        public SimEvent data;
    }

    [Serializable]
    public class SimEvent
    {
        public bool setQualifier;
        public SpeciesQualifier speciesQualifier;
        public string positiveName;
        public string positiveDescription;
        public string negativeName;
        public string negativeDescription;
        
        public string GetName(bool positive = true) { return positive ? positiveName : negativeName; }
        public string GetDescription(bool positive = true) { return positive ? positiveDescription : negativeDescription; }
    }
}
