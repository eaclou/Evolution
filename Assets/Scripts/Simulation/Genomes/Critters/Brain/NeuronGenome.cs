using System;

[Serializable]
public class NeuronGenome 
{
    public NeuronType neuronType;
    public string name = "none";
    public NID nid; // Neuron ID

    public NeuronGenome() { }

    public NeuronGenome(string name, NeuronType type, int moduleID, int neuronID) {
        neuronType = type;
        this.name = name;
        nid = new NID(moduleID, neuronID);
    }

    public NeuronGenome(string name, NeuronType type, NID nid) {
        neuronType = type;
        this.name = name;
        this.nid = nid;
    }

    public NeuronGenome(NeuronGenome template) {
        neuronType = template.neuronType;
        name = template.name;
        nid = template.nid;
    }
}

public enum NeuronType {
    In,
    Hid,
    Out
}