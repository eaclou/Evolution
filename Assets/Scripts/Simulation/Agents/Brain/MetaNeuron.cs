using UnityEngine;

[CreateAssetMenu(menuName = "Pond Water/Brain/Meta Neuron", fileName = "Meta Neuron")]
public class MetaNeuron : ScriptableObject
{
    public new string name;
    public int id;  // * Obsolete
    public NeuronType io;
    public BrainModuleID moduleID;
    public BrainIconID iconID;
}
