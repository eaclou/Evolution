using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Neuron {

    public NeuronGenome.NeuronType neuronType;
    public float inputTotal;
    public float[] currentValue;
    public float previousValue;

    public Neuron() {
        
    }
}
