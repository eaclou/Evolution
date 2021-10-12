
public class Neuron 
{
    public NeuronGenome genome;

    public NeuronType neuronType => genome.data.io;
    public BrainModuleID moduleID => genome.data.moduleID;
    public string name => genome.data.name;
    public int index => genome.index;
    
    public float inputTotal;
    public float[] currentValue;
    public float previousValue;

    public Neuron(NeuronGenome genome) { this.genome = genome; }
    public Neuron(Neuron original) { genome = original.genome; }

    // * WPP: need to modify TestBrainVisualization to condense into above constructor
    /*public Neuron(int index, int inputCount)
    {
        neuronType = index < inputCount ? NeuronType.In : NeuronType.Out;
        currentValue = new float[1];
        currentValue[0] = Random.Range(-2f, 2f);
    }*/
    
    public void Zero()
    {
        currentValue = new float[1];
        previousValue = 0f;
    }
    
    public bool IsMe(NeuronGenome other) { return genome.data == other.data && genome.index == other.index; }
}
