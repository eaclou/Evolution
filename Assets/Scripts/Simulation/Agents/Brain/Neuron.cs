using UnityEngine;

public class Neuron 
{
    public NeuronGenome.NeuronType neuronType;
    public string name;
    public float inputTotal;
    public float[] currentValue;
    public float previousValue;

    public Neuron() { }
    
    public Neuron(int index, int inputCount)
    {
        neuronType = index < inputCount ? NeuronGenome.NeuronType.In : NeuronGenome.NeuronType.Out;
        currentValue = new float[1];
        currentValue[0] = Random.Range(-2f, 2f);
    }
}
