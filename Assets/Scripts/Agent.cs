using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour {
   
    public Brain brain;

    public TestModule testModule;
    private Rigidbody2D rigidBody2D;
    //public bool isVisible = false;
    //public float horizontalMovementInput = 0f;
    //public float verticalMovementInput = 0f;
    public float speed = 150f;
    public bool humanControlled = false;

    // Use this for initialization
    void Start() {
        rigidBody2D = GetComponent<Rigidbody2D>();
    }

    public void MapNeuronToModule(NID nid, Neuron neuron) {
        testModule.MapNeuron(nid, neuron);
        // Hidden nodes!
        if (nid.moduleID == -1) {
            neuron.currentValue = new float[1];
            neuron.neuronType = NeuronGenome.NeuronType.Hid;
            neuron.previousValue = 0f;
        }
    }

    public void TickBrain() {
        brain.BrainMasterFunction();
    }
    public void TickModules() {
        testModule.Tick();
        //rangefinderModule.Tick();
        //enemyModule.Tick();
    }

    public void Tick() {
        // Any external inputs updated by simManager just before this
        TickModules(); // update inputs for Brain
        TickBrain(); // Tick Brain
        TickActions(); // Execute Actions
    }

    public void TickActions() {
        float horizontalMovementInput = 0f;
        float verticalMovementInput = 0f;

        if (humanControlled) {
            horizontalMovementInput = 0f;
            if (Input.GetKey("left") || Input.GetKey("a")) {
                horizontalMovementInput -= 1f;
            }
            if (Input.GetKey("right") || Input.GetKey("d")) {
                horizontalMovementInput += 1f;
            }
            verticalMovementInput = 0f;
            if (Input.GetKey("up") || Input.GetKey("w")) {
                verticalMovementInput += 1f;
            }
            if (Input.GetKey("down") || Input.GetKey("s")) {
                verticalMovementInput -= 1f;
            }
        }
        else {
            horizontalMovementInput = Mathf.Round(testModule.throttleX[0] * 3f / 2f);
            verticalMovementInput = Mathf.Round(testModule.throttleY[0] * 3f / 2f);
        }

        // MOVEMENT HERE:
        this.GetComponent<Rigidbody2D>().AddForce(new Vector2(speed * horizontalMovementInput * Time.deltaTime, 0f), ForceMode2D.Impulse);
        this.GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, speed * verticalMovementInput * Time.deltaTime), ForceMode2D.Impulse);
    }
    
    public void InitializeModules(AgentGenome genome, Agent agent, StartPositionGenome startPos) {
        testModule = new TestModule();
        testModule.Initialize(genome.bodyGenome.testModuleGenome, agent, startPos);        
    }

    public void InitializeAgentFromGenome(AgentGenome genome, StartPositionGenome startPos) {
        this.transform.localPosition = startPos.agentStartPosition;
        InitializeModules(genome, this, startPos);      // Modules need to be created first so that Brain can map its neurons to existing modules  
        brain = new Brain(genome.brainGenome, this);
    }
}
