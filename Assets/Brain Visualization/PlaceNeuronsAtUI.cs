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
        return neuron.io == NeuronType.Hidden ? 
            GetHiddenNeuronPosition(neuron) : 
            GetIONeuronPosition(neuron);
    }

    Vector3 GetIONeuronPosition(Neuron neuron)
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

    const float offsetDistance = -16f;

    Vector3 GetRadialOffsetPosition(Neuron neuron, UIPlacement icon)
    {
        Vector3 iconPosition = icon.location.localPosition;

        // * WPP: where do 3 and -16 come from?  If settings, expose values in inspector; 
        // if mathematical constants, declare as constants, if based on something else, include calculation.
        float minOffsetDist = 1.5f;
        float radialDistance = (minOffsetDist + (neuron.index % 2)) * offsetDistance;
        
        Vector2 clockHand = iconPosition.normalized;
        Vector3 newPos = iconPosition;
        Vector2 offset = clockHand * radialDistance;
        newPos.x += offset.x;
        newPos.y += offset.y;
        return newPos/halfPanelSize + Random.insideUnitSphere * randomOffset;
    }
    
    
    const float hiddenRadius = 0.36f;
    const float hiddenVariance = 0.001f;

    Vector3 GetHiddenNeuronPosition(Neuron neuron)
    {
        Vector3 localPos = Vector3.zero;
        float pointOnCircle = (float)neuron.index / 7f;
        float angleRadians = Mathf.PI * 2 * pointOnCircle;
        localPos.x = Mathf.Cos(angleRadians) * hiddenRadius;
        localPos.y = Mathf.Sin(angleRadians) * hiddenRadius;
        return localPos + Random.insideUnitSphere * hiddenVariance;
    }

    [Serializable]
    public class UIPlacement
    {
        public BrainIconID id;
        public RectTransform location;
    }
}
