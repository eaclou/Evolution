using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class Neuron 
{
    UIManager ui => UIManager.instance;
    PlaceNeuronsAtUI placement => ui.placeNeuronsAtUI;

    // WPP: moved to constructors
    /*NeuronGenome _genome;
    public NeuronGenome genome
    {
        get => _genome;
        set
        {
            _genome = value;
            data = new NeuronData(this);
            data.iconPosition = placement.GetNeuronPosition(this);
        }
    }*/
    
    /// Static data
    [Header("Static Data")] 
    [ReadOnly] public NeuronData data;
    public MetaNeuron template => data.template;
    public NeuronType io => data.io; 
    public string name => data.name;
    public int index => data.index;
    
    [Header("Dynamic Data")]
    public float inputTotal;
    
    public float[] currentValues = new float[1];
    /// Interface to currentValues array
    public float currentValue
    {
        get
        {
            if (currentValues == null) currentValues = new float[1];
            return currentValues[0];
        }
        set
        {
            if (currentValues == null) currentValues = new float[1];
            currentValues[0] = value;
            //Debug.Log(value); // NG: always zero
        }
    }
    
    /// Deep copy
    public Neuron(Neuron original) { GenerateStaticData(original.template, original.index); }

    //public Neuron(NeuronGenome genome) { this.genome = genome; }
    public Neuron(MetaNeuron template, int index) { GenerateStaticData(template, index); }
    
    void GenerateStaticData(MetaNeuron template, int index)
    {
        data = new NeuronData(template, index);
        data.iconPosition = placement.GetNeuronPosition(this);        
    }
    
    //public bool IsMe(NeuronGenome other) { return data.template == other.data && genome.index == other.index; }
    public bool IsMe(Neuron other) { return template == other.template && index == other.index; }
}

/// Static data associated with a neuron
[Serializable]
public class NeuronData
{
    public MetaNeuron template;
    public NeuronType io; 
    public string name;
    public int index;
    public Vector3 iconPosition;
    public int randomID;  // Debug: tests if reference to neuron is shared (it is not)
    
    public NeuronData(MetaNeuron template, int index)
    {
        this.template = template;
        name = template.name;
        this.index = index;
        io = template.io;
        iconPosition = Vector3.zero;
        randomID = Random.Range(int.MinValue, int.MaxValue);
    }
}


/// Categorizes neuron groups into lists, keeping them synced.  
/// Allows rapid access of group properties
[Serializable]
public class NeuronList
{
    public List<Neuron> all = new List<Neuron>();
    public List<Neuron> inOut = new List<Neuron>();
    public List<Neuron> hidden = new List<Neuron>();
    public List<Neuron> input = new List<Neuron>();
    public List<Neuron> output = new List<Neuron>();
    
    public int inCount => input.Count;
    public int outCount => output.Count;
    public int inOutCount => inOut.Count;
    public int hiddenCount => hidden.Count;
    public int allCount => all.Count;
    
    public void Add(Neuron neuron)
    {
        if (all.Contains(neuron)) return;
            
        all.Add(neuron);
            
        switch (neuron.io)
        {
            case NeuronType.In: 
                input.Add(neuron); 
                inOut.Add(neuron);
                break;
            case NeuronType.Out: 
                output.Add(neuron); 
                inOut.Add(neuron);
                break;
            case NeuronType.Hidden: hidden.Add(neuron); break;
        }
    }
        
    public void Remove(Neuron neuron)
    {
        if (!all.Contains(neuron)) return;

        all.Remove(neuron);
            
        switch (neuron.io)
        {
            case NeuronType.In: 
                input.Remove(neuron); 
                inOut.Remove(neuron);
                break;
            case NeuronType.Out: 
                output.Remove(neuron); 
                inOut.Remove(neuron);
                break;
            case NeuronType.Hidden: hidden.Remove(neuron); break;
        }        
    }
        
    public void Clear()
    {
        all.Clear();
        inOut.Clear();
        input.Clear();
        output.Clear();
        hidden.Clear();
    }
        
    public void Sync(List<Neuron> oldList, List<Neuron> newList)
    {
        foreach (var item in newList)
            if (!oldList.Contains(item))
                Add(item);
                    
        foreach (var item in oldList)
            if (!newList.Contains(item))
                Remove(item);
    }
        
    public void ClearSublist(List<Neuron> list)
    {
        foreach (var item in list)
            Remove(item);
    }
    
    public Neuron RandomNeuron(List<Neuron> list) { return list[Random.Range(0, list.Count)]; }
    
    public void PrintCounts() 
    { 
        Debug.Log($"Neurons: {inCount} input, {hiddenCount} hidden, " +
                  $"{outCount} output, {allCount} total."); 
    }
}