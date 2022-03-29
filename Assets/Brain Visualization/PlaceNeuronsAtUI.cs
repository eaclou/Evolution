using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlaceNeuronsAtUI : MonoBehaviour
{
    [SerializeField] float randomOffset = 0.001f;
    [SerializeField] RectTransform panel;
    [SerializeField] UIPlacement[] uiPlacements;
    
    // WPP: replaced arbitrary constant with calculated value
    // in OnValidate for efficiency;
    [ReadOnly] [SerializeField] 
    float halfPanelSize;
    
    void OnValidate()
    {
        if (panel) halfPanelSize = panel.rect.width / 2f;
    }

    public Vector3 GetNeuronPosition(Neuron neuron)
    {
        var placement = GetPlacement(neuron);
        return placement != null ? 
            GetRadialOffsetPosition(neuron, placement) : 
            //GetPlacementPosition(placement) : 
            Vector3.zero + Random.insideUnitSphere * randomOffset;     
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
        Vector3 iconPosition = icon.location.localPosition;
        
        // * WPP: where do 3 and -16 come from?  If settings, expose values in inspector; 
        // if mathematical constants, declare as constants, if based on something else, include calculation.
        float radialDistance = (neuron.index % 3 - GetModuloOffset(neuron.neuronType)) * -16f;
        
        Vector2 clockHand = iconPosition.normalized;
        Vector3 newPos = iconPosition;
        Vector2 offset = clockHand * radialDistance;
        newPos.x += offset.x;
        newPos.y += offset.y;
        return newPos/halfPanelSize + Random.insideUnitSphere * randomOffset;
    }
    
    // * WPP: magic numbers
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
