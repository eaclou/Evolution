using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlaceNeuronsAtUI : MonoBehaviour
{
    [SerializeField] UIPlacement[] uiPlacements;

    #region WPP: condensed repetition
    /// INCOMPLETE: visualization is finicky about placement
    /// ui placement may need to be mapped to a valid area
    //***EAC IF TWO OR MORE NEURONS SHARE SAME EXACT POSITION IT CREATES A DIVIDE BY ZERO ERROR!!!!!!!!!***
    /*public Vector3 GetInputNeuronPosition(Neuron neuron)
    {
        var data = neuron.genome.data;

        foreach (var placement in uiPlacements) 
        {
            if (placement.id == data.iconID) 
            {
                float radialDistance = ((neuron.index % 3) + 0.4f) * -16f;
                Vector2 clockHand = placement.location.localPosition.normalized;
                Vector3 newPos = placement.location.localPosition;
                Vector2 offset = clockHand * radialDistance;
                newPos.x += offset.x;
                newPos.y += offset.y;
                return (newPos / 160f) + Random.insideUnitSphere * 0.01f;
            }
        }        
        
        Debug.LogError($"Unable to find placement for {neuron.moduleID} {data}");
        return Vector3.zero + Random.insideUnitSphere * 0.01f;
    }*/

    /*public Vector3 GetOutputNeuronPosition(Neuron neuron)
    {
            var data = neuron.genome.data;
    
            foreach (var placement in uiPlacements) 
            {
                if (placement.id == data.iconID) 
                {
                    // WPP: Moved to GetOffset
                    float radialDistance = ((neuron.index % 3) - 0.2f) * -16f;
                    Vector2 clockHand = placement.location.localPosition.normalized;
                    Vector3 newPos = placement.location.localPosition;
                    Vector2 offset = clockHand * radialDistance;
                    newPos.x += offset.x;
                    newPos.y += offset.y;
                    return (newPos / 160f) + Random.insideUnitSphere * 0.01f;*/
                
                /*Vector3 localPos = Vector3.zero; // Random.insideUnitSphere * 0.001f;
                float frac = Mathf.Clamp01((float)placement.id / (float)uiPlacements.Length);
                float radius = 0.35f;
                float angleRadians = Mathf.PI * frac;
                float x = Mathf.Cos(angleRadians) * radius;
                float y = Mathf.Sin(angleRadians) * radius;
                localPos.x = x;
                localPos.y = y - 0.8f;
                return localPos + Random.insideUnitSphere * 0.001f;
            }
        }
    }*/
    #endregion
    
    public Vector3 GetNeuronPosition(Neuron neuron)
    {
        var placement = GetPlacement(neuron);
        return placement != null ? 
            GetRadialOffsetPosition(neuron, placement) : 
            //GetPlacementPosition(placement) : 
            Vector3.zero + Random.insideUnitSphere * 0.01f;        
    }
    
    UIPlacement GetPlacement(Neuron neuron)
    {
        var data = neuron.genome.data;

        foreach (var placement in uiPlacements)
            if (placement.id == data.iconID)
                return placement;
        
        return null;
    }
    
    /// Simple calculation for debugging
    Vector3 GetPlacementPosition(UIPlacement placement)
    {
        // ERROR: moves neuron after one frame
        var anchor = placement.location.anchoredPosition;
        return new Vector3(anchor.x, anchor.y, 0f);
    }

    Vector3 GetRadialOffsetPosition(Neuron neuron, UIPlacement icon)
    {
        //Vector2 anchor = icon.location.anchoredPosition;
        //Vector3 iconPosition = new Vector3(anchor.x, anchor.y, 0f);
        Vector3 iconPosition = icon.location.localPosition;
        float radialDistance = (neuron.index % 3 - GetModuloOffset(neuron.neuronType)) * -16f;
        Vector2 clockHand = iconPosition.normalized;
        Vector3 newPos = iconPosition;
        Vector2 offset = clockHand * radialDistance;
        newPos.x += offset.x;
        newPos.y += offset.y;
        return newPos/160f + Random.insideUnitSphere * 0.01f;
    }
    
    float GetModuloOffset(NeuronType io)
    {
        switch (io)
        {
            case NeuronType.In: return 0.4f;
            case NeuronType.Out: return 0.2f;
            default: return 0.1f;
        }
    } 
    
    /// INCOMPLETE
    public Vector3 GetHiddenNeuronPosition(Neuron neuron)
    {   
        // Get input location
        // Get output location
        // Calculate & return intermediate location
        Vector3 localPos = Vector3.zero; // Random.insideUnitSphere * 0.001f;
        float frac = (float)neuron.index / 5f;
        float radius = 0.25f;
        float angleRadians = Mathf.PI * 2f * frac;
        float x = Mathf.Cos(angleRadians) * radius;
        float y = Mathf.Sin(angleRadians) * radius;
        localPos.x = x;
        localPos.y = y + 0.1f;
        return localPos + Random.insideUnitSphere * 0.01f;
    }

    [Serializable]
    public class UIPlacement
    {
        public BrainIconID id;
        public RectTransform location;
    }
}
