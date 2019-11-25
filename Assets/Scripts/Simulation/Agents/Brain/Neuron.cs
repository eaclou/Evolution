using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Neuron {

    public NeuronGenome.NeuronType neuronType;
    public string name;
    public float inputTotal;
    public float[] currentValue;
    public float previousValue;

    public Neuron() {
        
    }
}
