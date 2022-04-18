using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class Neuron
{
    /// Static data
    [Header("Static Data")] 
    [ReadOnly] public NeuronData data;
    public MetaNeuron template => data.template;
    public NeuronType io => data.io;
    public string name { get => data.name; set => data.name = value; }
    public int index { get => data.index; set => data.SetIndex(value); }
    
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
        }
    }
    
    /// Copy existing neuron
    public Neuron(Neuron original) { GenerateStaticData(original.template); }

    // Create a new neuron from template data
    public Neuron(MetaNeuron template) { GenerateStaticData(template); }
    
    void GenerateStaticData(MetaNeuron template) { data = new NeuronData(template); }
    
    public bool IsMe(Neuron other) { return template == other.template && index == other.index; }
}

/// Static data associated with a neuron
[Serializable]
public class NeuronData
{
    UIManager ui => UIManager.instance;
    PlaceNeuronsAtUI placement => ui.placeNeuronsAtUI;

    public MetaNeuron template;
    public NeuronType io; 
    public string name;
    public int index;
    public Vector3 iconPosition;
    public int uniqueID;
    
    public NeuronData(MetaNeuron template)
    {
        this.template = template;
        name = template.name;
        io = template.io;
        iconPosition = Vector3.zero;
        uniqueID = Random.Range(int.MinValue, int.MaxValue);
    }
    
    public void SetIndex(int value)
    {
        index = value;
        iconPosition = placement.GetNeuronPosition(this); 
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
    public List<Neuron> inHidden = new List<Neuron>();
    public List<Neuron> hiddenOut = new List<Neuron>();
    
    public int inCount => input.Count;
    public int outCount => output.Count;
    public int inOutCount => inOut.Count;
    public int hiddenCount => hidden.Count;
    public int allCount => all.Count;
    
    public void Add(Neuron neuron)
    {
        if (all.Contains(neuron)) return;
            
        neuron.index = all.Count;
        all.Add(neuron);
        
        // Consider storing associations in a ScriptableObject if this gets more complex.
        switch (neuron.io)
        {
            case NeuronType.In: 
                input.Add(neuron); 
                inOut.Add(neuron);
                inHidden.Add(neuron);
                break;
            case NeuronType.Out: 
                output.Add(neuron); 
                inOut.Add(neuron);
                hiddenOut.Add(neuron);
                break;
            case NeuronType.Hidden: 
                AddHidden(neuron); 
                inHidden.Add(neuron);
                hiddenOut.Add(neuron);
                break;
        }
    }

    public void Remove(Neuron neuron)
    {
        if (!all.Contains(neuron)) return;

        all.Remove(neuron);
        
        // Consider storing associations in a ScriptableObject if this gets more complex.
        switch (neuron.io)
        {
            case NeuronType.In: 
                input.Remove(neuron); 
                inOut.Remove(neuron);
                inHidden.Remove(neuron);
                break;
            case NeuronType.Out: 
                output.Remove(neuron); 
                inOut.Remove(neuron);
                hiddenOut.Remove(neuron);
                break;
            case NeuronType.Hidden: 
                RemoveHidden(neuron); 
                inHidden.Remove(neuron);
                hiddenOut.Remove(neuron);
                break;
        }
        
        RefreshIndicies();       
    }
        
    public void Clear()
    {
        all.Clear();
        inOut.Clear();
        input.Clear();
        output.Clear();
        hidden.Clear();
        inHidden.Clear();
        hiddenOut.Clear();
    }
        
    public void Sync(List<Neuron> listToModify, List<Neuron> referenceList)
    {
        ClearSublist(listToModify);
        
        foreach (var neuron in referenceList)
            Add(new Neuron(neuron));
    }
        
    public void ClearSublist(List<Neuron> list)
    {
        foreach (var item in list)
            Remove(item);
    }
    
    void RefreshIndicies()
    {
        for (int i = 0; i < all.Count; i++)
            all[i].index = i;
    }
    
    MetaNeuron hiddenTemplate => Lookup.instance.hiddenTemplate;
    string hiddenName => $"{hiddenTemplate.name} {hidden.Count}";

    void AddHidden(Neuron neuron)
    {
        neuron.name = hiddenName;
        hidden.Add(neuron);
    }
    
    void RemoveHidden(Neuron neuron)
    {
        hidden.Remove(neuron);
        RefreshHidden();
    }
    
    void RefreshHidden()
    {
        for (int i = 0; i < hidden.Count; i++)
            hidden[i].name = hiddenName;
    }
    
    public Neuron RandomNeuron(List<Neuron> list) { return list[Random.Range(0, list.Count)]; }
    
    public void PrintCounts() 
    { 
        Debug.Log($"Neurons: {inCount} input, {hiddenCount} hidden, " +
                  $"{outCount} output, {allCount} total."); 
    }
}