using UnityEngine;

public class Neuron 
{
    public NeuronType neuronType;
    public string name;
    public float inputTotal;
    public float[] currentValue;
    public float previousValue;
    
    public BrainModuleID moduleID;

    public Neuron(string name, BrainModuleID moduleID) 
    {
        this.name = name; 
        this.moduleID = moduleID;
    }
    
    public Neuron(int index, int inputCount)
    {
        neuronType = index < inputCount ? NeuronType.In : NeuronType.Out;
        currentValue = new float[1];
        currentValue[0] = Random.Range(-2f, 2f);
    }
}
