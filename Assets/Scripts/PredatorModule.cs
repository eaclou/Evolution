﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PredatorModule : MonoBehaviour {

    public Rigidbody2D rigidBody;
    private float speed = 1000f;

    private float damage = 0.14f;
    private int counter = 0;

    private float randX;
    private float randY;

    private float minScale = 4f;
    private float maxScale = 5.5f;
    public float curScale = 4f;

    private int framesPerDirChange = 64;

    // Use this for initialization
    void Awake () {
        rigidBody = GetComponent<Rigidbody2D>();

        float curScale = UnityEngine.Random.Range(minScale, maxScale);
        Vector3 scale = new Vector3(curScale, curScale, curScale);
        transform.localScale = scale;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void FixedUpdate() {
        counter = (counter + 1) % framesPerDirChange;
        if(counter == 0) {
            randX = UnityEngine.Random.Range(-1f, 1f);
            randY = UnityEngine.Random.Range(-1f, 1f);
        }
        // MOVEMENT HERE:
        this.GetComponent<Rigidbody2D>().AddForce(new Vector2(speed * randX * Time.deltaTime, speed * randY * Time.deltaTime), ForceMode2D.Impulse);
    }

    private void AttackAgent(Agent agent) {
        agent.testModule.hitPoints[0] -= damage;
        //Debug.Log("AttackAgent!");
        if (agent.testModule.hitPoints[0] <= 0f) {
            agent.isDead = true;
            //Debug.Log("Agent DEAD!");
        }
    }
    
    private void OnCollisionStay2D(Collision2D coll) {
        //Debug.Log("OnCollisionStay2D! " + coll.collider.gameObject.name);

        Agent collidingAgent = coll.collider.gameObject.GetComponent<Agent>();
        if (collidingAgent != null) {
            AttackAgent(collidingAgent);
        }
    }
    
}