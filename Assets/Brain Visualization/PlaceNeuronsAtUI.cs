using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlaceNeuronsAtUI : MonoBehaviour
{
    [SerializeField] UIPlacement[] uiPlacements;

    /// INCOMPLETE: visualization is finicky about placement
    /// ui placement may need to be mapped to a valid area
    public Vector3 GetInputNeuronPosition(Neuron neuron)
    {
        //return Random.insideUnitSphere;
        //***EAC IF TWO OR MORE NEURONS SHARE SAME EXACT POSITION IT CREATES A DIVIDE BY ZERO ERROR!!!!!!!!!***

        var data = neuron.genome.data;

        foreach (var placement in uiPlacements) {
            if (placement.id == data.iconID) {
                float ind = (float)(neuron.index % 3) + 0.4f;
                Vector2 clockHand = placement.location.localPosition.normalized;
                Vector3 newPos = placement.location.localPosition;
                Vector2 offset = clockHand * ind * -16f;
                newPos.x += offset.x;
                newPos.y += offset.y;
                return (newPos / 160f) + Random.insideUnitSphere * 0.01f;
            }
        }        

                
        Debug.LogError($"Unable to find placement for {neuron.moduleID} {data}");
        return Vector3.zero + Random.insideUnitSphere * 0.01f;
    }

    public Vector3 GetOutputNeuronPosition(Neuron neuron)
    {        
        var data = neuron.genome.data;

        foreach (var placement in uiPlacements) {
            if (placement.id == data.iconID) {
                float ind = (float)(neuron.index % 3) - 0.2f;
                Vector2 clockHand = placement.location.localPosition.normalized;

                Vector3 newPos = placement.location.localPosition;
                Vector2 offset = clockHand * ind * -16f;
                newPos.x += offset.x;
                newPos.y += offset.y;
                return (newPos / 160f) + Random.insideUnitSphere * 0.01f;
                /*Vector3 localPos = Vector3.zero; // Random.insideUnitSphere * 0.001f;
                float frac = Mathf.Clamp01((float)placement.id / (float)uiPlacements.Length);
                float radius = 0.35f;
                float angleRadians = Mathf.PI * frac;
                float x = Mathf.Cos(angleRadians) * radius;
                float y = Mathf.Sin(angleRadians) * radius;
                localPos.x = x;
                localPos.y = y - 0.8f;
                return localPos + Random.insideUnitSphere * 0.001f;*/
            }
        }
                
        Debug.LogError($"Unable to find placement for {neuron.moduleID} {data}");
        return Vector3.zero + Random.insideUnitSphere * 0.01f;
    }
    
    /// NOT IMPLEMENTED
    public Vector3 GetHiddenNeuronPosition(Neuron neuron)
    {   
        var data = neuron.genome.data;
        // Get input location
        // Get output location
        // Calculate & return intermediate location
        // return Vector3.zero;
        //foreach (var placement in uiPlacements) {
        //    if (placement.id == data.iconID) {
        Vector3 localPos = Vector3.zero; // Random.insideUnitSphere * 0.001f;
        float frac = (float)neuron.index / 5f;
        float radius = 0.25f;
        float angleRadians = Mathf.PI * 2f * frac;
        float x = Mathf.Cos(angleRadians) * radius;
        float y = Mathf.Sin(angleRadians) * radius;
        localPos.x = x;
        localPos.y = y + 0.1f;
        //Debug.Log($"Unable to find placement for {neuron.index} {data.iconID}");
        return localPos + Random.insideUnitSphere * 0.01f;
            //}
        //}
        
        //return Vector3.zero;
    }

    [Serializable]
    public struct UIPlacement
    {
        public BrainIconID id;
        public RectTransform location;
    }
}
