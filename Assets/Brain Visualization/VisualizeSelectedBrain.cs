using System.Collections.Generic;
using UnityEngine;

public class VisualizeSelectedBrain : MonoBehaviour
{
    UIManager ui => UIManager.instance;
    
    [SerializeField] GenerateBrainVisualization visualization;
    [SerializeField] PlaceNeuronsAtUI placement;

    void Start()
    {
        ui.OnAgentSelected += RefreshAgent;
    }

    void OnDestroy()
    {
        if (UIManager.exists) ui.OnAgentSelected -= RefreshAgent;
    }
    
    void RefreshAgent(Agent agent)
    {
        neurons = agent.brain.neurons;
        axons = agent.brain.axons;
        
        //Debug.Log($"Selected agent brain has {neurons.Count} neurons and {axons.Count} axons");
        var sockets = CreateSockets();
        /*
        string neuronString = "RefreshAgentneuronData: [" + neurons.Count + "] ";
        foreach(var socket in sockets) {
            neuronString += socket.position + ", ";
        }
        Debug.Log(neuronString);
        */
        visualization.Initialize(neurons, axons, ref sockets, inputNeurons.Count, outputNeurons.Count);
    }
    
    List<Neuron> neurons = new List<Neuron>();
    List<Neuron> inputNeurons = new List<Neuron>();
    List<Neuron> outputNeurons = new List<Neuron>();
    List<Neuron> hiddenNeurons = new List<Neuron>();
    List<Axon> axons = new List<Axon>();
    
    SocketInitData[] CreateSockets()
    {
        SortNeuronsByIO(neurons);
        
        SocketInitData[] sockets = new SocketInitData[neurons.Count];
        //Debug.Log($"sockets: {sockets.Length}, input: {inputNeurons.Count}, output: {outputNeurons.Count}, " +
        //          $"hidden: {hiddenNeurons.Count}, total: {inputNeurons.Count + outputNeurons.Count + hiddenNeurons.Count}");
        
        // Place input and output neurons before hidden neurons
        // because hidden neurons are between connected input and output neurons.
        /*int offset = 0;
        for (int i = 0; i < inputNeurons.Count; i++) 
            sockets[i + offset].position = placement.GetNeuronPosition(inputNeurons[i]);
            
        offset = inputNeurons.Count;
        for (int i = 0; i < outputNeurons.Count; i++)
            sockets[i + offset].position = placement.GetNeuronPosition(outputNeurons[i]);
        
        offset = inputNeurons.Count + outputNeurons.Count;
        for (int i = 0; i < hiddenNeurons.Count; i++) {
            sockets[i + offset].position = placement.GetHiddenNeuronPosition(hiddenNeurons[i]);
            //Debug.Log("HID " + sockets[i + offset].position);
        }*/
        // WPP: delegated branching to placement to simplify interface
        // + placement calculated during neuron creation
        for (int i = 0; i < sockets.Length; i++)
            sockets[i].position = neurons[i].data.iconPosition;

        return sockets;
    }
    
    void SortNeuronsByIO(List<Neuron> neurons)
    {
        inputNeurons.Clear();
        outputNeurons.Clear();
        hiddenNeurons.Clear();
    
        foreach (var neuron in neurons) 
        {
            switch (neuron.data.io)
            {
                case NeuronType.In: inputNeurons.Add(neuron); break;
                case NeuronType.Out: outputNeurons.Add(neuron); break;
                case NeuronType.Hidden: hiddenNeurons.Add(neuron); break;
            }
        }
    }
}
