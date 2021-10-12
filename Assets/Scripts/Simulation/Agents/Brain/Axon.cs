
public class Axon 
{
    //public int fromID;
    //public int toID;
    public Neuron from;
    public Neuron to;
    public float weight;

    //public Axon(int fromID, int toID, float weight) 
    public Axon(Neuron from, Neuron to, float weight)
    {
        //this.fromID = fromID;
        //this.toID = toID;
        this.from = from;
        this.to = to;
        this.weight = weight;
        
        if (from == null || to == null)
            UnityEngine.Debug.LogError("Initializing axon to null neuron");
    }
}
