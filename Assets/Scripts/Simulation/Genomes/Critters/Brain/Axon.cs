using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class Axon 
{
    public Neuron from;
    public Neuron to;
    public float weight;    // multiplier on signal
    
    public int index;
    public int uniqueID;
    
    public float normalizedWeight => weight * 0.5f + 0.5f;
    
    public Axon(Neuron from, Neuron to, float weight)
    {
        this.from = from;
        this.to = to;
        this.weight = weight;
        uniqueID = Random.Range(int.MinValue, int.MaxValue);
        
        // NG on mutate path only. Downstream error of BrainGenome.SetToMutatedCopyOfParentGenome
        //if (from.io == to.io && from.io != NeuronType.Hidden)  
        //    Debug.LogError($"Invalid axon on construction: matching IO {from.io}");
    }
    
    public bool IsInList(List<Neuron> list)
    {
        foreach (var item in list)
        {
            if (item == from || item == to)
            {
                Debug.Log("Connection already in list");
                return true;
            }
        }
                
        return false;
    }
}

[Serializable]
public class AxonList
{
    public List<Axon> all = new List<Axon>();
    public List<Axon> inOut = new List<Axon>();
    public List<Axon> inHidden = new List<Axon>();
    public List<Axon> hiddenOut = new List<Axon>();
    public List<Axon> hiddenHidden = new List<Axon>();
    
    public void Add(Axon axon)
    {
        if (all.Contains(axon)) return;
        axon.index = all.Count;
        all.Add(axon);
        GetSublist(axon)?.Add(axon);
    }
    
    public void Remove(Axon axon)
    {
        if (!all.Contains(axon)) return;
        all.Remove(axon);
        GetSublist(axon)?.Remove(axon);
    }
    
    public void Remove(int index) { Remove(all[index]); }
    
    public void Clear()
    {
        all.Clear();
        inOut.Clear();
        inHidden.Clear();
        hiddenOut.Clear();
    }
    
    public void Reset(List<Axon> list)
    {
        Clear();
        foreach (var axon in list)
            Add(axon);
    }
    
    List<Axon> GetSublist(Axon axon)
    {
        var from = axon.from.io;
        var to = axon.to.io;    
    
        if (from == NeuronType.In && to == NeuronType.Out)
            return inOut;
        if (from == NeuronType.In && to == NeuronType.Hidden)
            return inHidden;
        if (from == NeuronType.Hidden && to == NeuronType.Out)
            return hiddenOut;
        if (from == NeuronType.Hidden && to == NeuronType.Hidden)
            return hiddenHidden;
            
        //if (!IsValid(axon))     // NG
        //    Debug.LogError($"Invalid axon: matching io {from} - {to}");
        
        return null;
    }
    
    public bool IsValid(Axon axon) { return IsValid(axon.from, axon.to); }
    
    public bool IsValid(Neuron from, Neuron to) 
    { 
        return from.io != to.io || 
        from.io == NeuronType.Hidden ||
        to.io == NeuronType.In; 
    }
    
    public void PrintCounts()
    {
        Debug.Log($"Axons: {inOut.Count} in => out, {inHidden.Count} in => hidden, " +
                  $"{hiddenOut.Count} hidden => out, {all.Count} total.");
    }
}