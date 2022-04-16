using System.Collections.Generic;
using UnityEngine;

public class VisualizeSelectedBrain : MonoBehaviour
{
    UIManager ui => UIManager.instance;
    
    [SerializeField] GenerateBrainVisualization visualization;

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
        this.agent = agent;
        //Debug.Log($"Selected agent brain has {neurons.Count} neurons and {axons.Count} axons");
        var sockets = CreateSockets();
        visualization.Initialize(agent.brain, ref sockets);
    }
    
    Agent agent;
    List<Neuron> neurons => agent.brain.allNeurons;

    SocketInitData[] CreateSockets()
    {
        SocketInitData[] sockets = new SocketInitData[neurons.Count];

        for (int i = 0; i < sockets.Length; i++)
            sockets[i].position = neurons[i].data.iconPosition;

        return sockets;
    }
}
