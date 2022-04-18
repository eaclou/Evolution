using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlaceNeuronsAtUI : MonoBehaviour
{
    [SerializeField] float randomOffset = 0.001f;
    [SerializeField] RectTransform panel;
    [SerializeField] UIPlacement[] uiPlacements;
    [ReadOnly] [SerializeField] float halfPanelSize;
    
    [SerializeField] float hiddenRadius = 0.36f;
    [SerializeField] float hiddenVariance = 0.001f;
    [SerializeField] int hiddenSlotCount = 7;
    
    void OnValidate()
    {
        if (panel) halfPanelSize = panel.rect.width / 2f;
    }
    
    public Vector3 GetNeuronPosition(NeuronData neuron)
    {
        return neuron.io == NeuronType.Hidden ? 
            GetHiddenNeuronPosition(neuron.index) : 
            GetIONeuronPosition(neuron);
    }

    Vector3 GetIONeuronPosition(NeuronData neuron)
    {
        var placement = GetPlacement(neuron);
        return placement != null ? 
            GetRadialOffsetPosition(neuron, placement) : 
            Vector3.zero + Random.insideUnitSphere * randomOffset;     
    }
    
    UIPlacement GetPlacement(NeuronData neuron)
    {
        var data = neuron.template;

        foreach (var placement in uiPlacements)
            if (placement.id == data.iconID)
                return placement;
        
        return null;
    }

    const float offsetDistance = -16f;

    Vector3 GetRadialOffsetPosition(NeuronData neuron, UIPlacement icon)
    {
        Vector3 iconPosition = icon.location.localPosition;

        float minOffsetDist = 1.5f;
        float radialDistance = (minOffsetDist + (neuron.index % 2)) * offsetDistance;
        
        Vector2 clockHand = iconPosition.normalized;
        Vector3 newPos = iconPosition;
        Vector2 offset = clockHand * radialDistance;
        newPos.x += offset.x;
        newPos.y += offset.y;
        return newPos/halfPanelSize + Random.insideUnitSphere * randomOffset;
    }

    Vector3 GetHiddenNeuronPosition(int index)
    {
        Vector3 localPosition = Vector3.zero;
        int slot = index % hiddenSlotCount;
        float pointOnCircle = (float)slot / hiddenSlotCount;
        float angleRadians = Mathf.PI * 2 * pointOnCircle;
        localPosition.x = Mathf.Cos(angleRadians) * hiddenRadius;
        localPosition.y = Mathf.Sin(angleRadians) * hiddenRadius;
        return localPosition + Random.insideUnitSphere * hiddenVariance;
    }

    [Serializable]
    public class UIPlacement
    {
        public BrainIconID id;
        public RectTransform location;
    }
}
