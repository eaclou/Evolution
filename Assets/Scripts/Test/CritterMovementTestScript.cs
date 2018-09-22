using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CritterMovementTestScript : MonoBehaviour {

    public bool reset = false;

    public float frequency = 4f;
    public float amplitude = 0f;
    public float offsetDelay = 1.0f;
    public float headDrag = 10f;
    public float bodyDrag = 10f;
    public float headMass = 1f;
    public float bodyMass = 1f;
    public float speed = 100f;
    public float jointSpeed = 100f;
    public float jointMaxTorque = 250f;
    public float swimAnimationCycleSpeed = 0.01f;
    public float smoothedThrottleLerp = 0.1f;
    public float restingJointTorque = 11f;
    public float bendRatioHead = 0f;
    public float bendRatioTailTip = 1f;

    public int numSegments = 6;
    public float bodyLength = 6f;

    private AgentGenome testAgentGenome;
    private Agent testAgent;

	// Use this for initialization
	void Start () {
        InitializeCritter();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void FixedUpdate() {
        if(reset) {



            InitializeCritter();

            reset = false;
        }


        //testAgent.Tick();
    }

    private void InitializeCritter() {

        // Delete existing children GameObjects:
        foreach (Transform child in this.transform) {
             GameObject.Destroy(child.gameObject);
         }

        // Create dummy genome
        testAgentGenome = new AgentGenome();
        testAgentGenome.GenerateInitialRandomBodyGenome();
        testAgentGenome.InitializeRandomBrainFromCurrentBody(0f, 0);
        
        // Create container Agent:
        GameObject agentGO = new GameObject("Agent" + 0.ToString());
        agentGO.transform.parent = this.transform;
        testAgent = agentGO.AddComponent<Agent>();

        //testAgent.humanControlLerp = 1f;
        //testAgent.humanControlled = true;

        testAgentGenome.bodyGenome.coreGenome.numSegments = numSegments;
        testAgentGenome.bodyGenome.coreGenome.fullBodyLength = bodyLength;

        testAgent.speed = speed;
        /*
        testAgent.frequency = frequency;
        testAgent.amplitude = amplitude;
        testAgent.offsetDelay = offsetDelay;
        testAgent.headDrag = headDrag;
        testAgent.bodyDrag = bodyDrag;
        testAgent.headMass = headMass;
        testAgent.bodyMass = bodyMass;
        testAgent.jointSpeed = jointSpeed;
        testAgent.jointMaxTorque = jointMaxTorque;
        testAgent.swimAnimationCycleSpeed = swimAnimationCycleSpeed;
        testAgent.smoothedThrottleLerp = smoothedThrottleLerp;
        testAgent.restingJointTorque = restingJointTorque;
        testAgent.bendRatioHead = bendRatioHead;
        testAgent.bendRatioTailTip = bendRatioTailTip;
        */
        // initialize Agent based on genome:
        StartPositionGenome startPosGenome = new StartPositionGenome(Vector3.zero, Quaternion.identity);
        //testAgent.InitializeAgentFromGenome(0, testAgentGenome, startPosGenome);  // This also rebuilds GameObjects
    }
}
