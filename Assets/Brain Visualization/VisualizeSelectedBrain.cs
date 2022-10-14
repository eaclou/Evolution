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
    
    void RefreshAgent(Agent agent) // 
    {
        if(agent == null) {
            //agent = new Agent();
            //agent.brain = new Brain(genome.brainGenome);
            
        }
        else {
            this.agent = agent;
            this.curBrain = agent.brain;
            if(neurons.Count == 0) {
                Debug.LogError("NO NEURONS! RefreshAgent " + agent.index + ", #N: " + neurons.Count + ", " + agent.candidateRef.candidateID + ", " + agent.candidateRef.candidateGenome.brainGenome.neurons.allCount);
            
            }
            else {
                //Debug.Log($"Selected agent brain has {neurons.Count} neurons and {axons.Count} axons");
                var sockets = CreateSockets();
                visualization.Initialize(this.curBrain, ref sockets);
            }
            
        }        
    }
    public void RefreshCandidate(CandidateAgentData cand) // 
    {
        if (this.agent != null) {
            curBrain = new Brain(cand.candidateGenome.brainGenome);

            Debug.Log("RefreshCandidate new brain");
            var sockets = CreateSockets();
            visualization.Initialize(curBrain, ref sockets);
        }      
    }
    
    Agent agent;
    private Brain curBrain;
    List<Neuron> neurons => curBrain.allNeurons;

    SocketInitData[] CreateSockets()
    {
        SocketInitData[] sockets = new SocketInitData[neurons.Count];

        for (int i = 0; i < sockets.Length; i++)
            sockets[i].position = neurons[i].data.iconPosition;

        return sockets;
    }
}
