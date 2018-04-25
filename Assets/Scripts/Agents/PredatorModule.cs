using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PredatorModule : MonoBehaviour {

    public Rigidbody2D rigidBody;
    public MeshRenderer meshRendererBeauty;
    public MeshRenderer meshRendererFluidCollider;

    private float speed = 250f;

    private float damage = 0.55f;
    private int counter = 0;

    private float randX;
    private float randY;

    private float minScale = 1.6f;
    private float maxScale = 3.2f;
    public float curScale = 2.4f;

    private int framesPerDirChange = 64;

    private Vector2 prevPos;
    public Vector3 _PrevPos
    {
        get
        {
            return prevPos;
        }
        set
        {

        }
    }

    // Use this for initialization
    void Start () {
        //rigidBody = GetComponent<Rigidbody2D>();

        //float curScale = UnityEngine.Random.Range(minScale, maxScale);
        //Vector3 scale = new Vector3(curScale, curScale, curScale);
        //transform.localScale = scale;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void InitializePredator() {
        rigidBody = GetComponent<Rigidbody2D>();
        curScale = UnityEngine.Random.Range(minScale, maxScale);        
        Vector3 scale = new Vector3(curScale, curScale, curScale);
        transform.localScale = scale;
        counter = UnityEngine.Random.Range(0, framesPerDirChange);
        //Debug.Log("curScale: " + curScale.ToString() + ", ts: " + transform.localScale.ToString());
    }

    private void FixedUpdate() {
        counter = (counter + 1) % framesPerDirChange;
        if(counter == 0) {
            randX = UnityEngine.Random.Range(-1f, 1f);
            randY = UnityEngine.Random.Range(-1f, 1f);
        }
        // MOVEMENT HERE:
        // ** DISABLED!!!
        //this.GetComponent<Rigidbody2D>().AddForce(new Vector2(speed * randX * Time.deltaTime, speed * randY * Time.deltaTime), ForceMode2D.Impulse);

        Vector3 curPos = transform.localPosition;
        prevPos = curPos;
    }

    private void AttackAgent(Agent agent) {
        agent.testModule.hitPoints[0] -= damage;        
    }
    
    // *** Eventually look into explicitly ordering these Scripts Execution Order for OnCollision Monobehaviors (like Agents & Food) *****
    private void OnCollisionStay2D(Collision2D coll) {
        //Debug.Log("OnCollisionStay2D! " + coll.collider.gameObject.name);

        Agent collidingAgent = coll.collider.gameObject.GetComponentInParent<Agent>();
        if (collidingAgent != null) {
            AttackAgent(collidingAgent);
        }
    }
    
}
