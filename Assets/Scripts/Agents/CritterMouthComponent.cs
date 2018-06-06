using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CritterMouthComponent : MonoBehaviour {

    public int agentIndex = -1;

    //public bool foodInRange = false;

    public bool isBiting = false;
    public int bitingFrameCounter = 0;
    public int biteCooldown = 30;

    public float feedingRate = 0.4f;

    public Agent agentRef;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    /*public void Bite() {
        // BITING:
        // *** TEMP: ***
        // RESET trigger flags before OnTriggerEnter2D runs later in frame: must do this AFTER checking for it from previous frame
        if(coreModule.mouthRef.foodInRange) {
            Debug.Log("foodInRange agent: " + index.ToString());

            // Initiate BITE!
            if(coreModule.mouthRef.isBiting) {
                
            }
            else {
                coreModule.mouthRef.isBiting = true;

                // ACTUALLY BITE:
                coreModule.mouthRef.Bite();
            }
            
            // RESET FOR NEXT FRAME:
            coreModule.mouthRef.foodInRange = false;
        }
        else {
            
        }

        if(coreModule.mouthRef.isBiting) {
            // in cooldown
            coreModule.mouthRef.bitingFrameCounter++;

            if(coreModule.mouthRef.bitingFrameCounter >= coreModule.mouthRef.biteCooldown) {
                coreModule.mouthRef.bitingFrameCounter = 0;
                coreModule.mouthRef.isBiting = false;
            }
        }
    }
    */


    private void BiteFood(FoodModule foodModule) {
        if(isBiting) {
                        
        }
        else { // Not in Cooldown!

            //Debug.Log("BiteFood");
            // CONSUME FOOD!
            float flow = feedingRate; // / colliderCount;

            float flowR = Mathf.Min(foodModule.amountR, flow);
            //collidingAgent.testModule.foodAmountR[0] += flowR * 2f;  // make sure Agent doesn't receive food from empty dispenser

            agentRef.EatFood(flowR * 1f); // assumes all foodAmounts are equal !! *****
    
            foodModule.amountR -= flowR;
            if (foodModule.amountR < 0f) {
                foodModule.amountR = 0f;
            }
            float flowG = Mathf.Min(foodModule.amountG, flow);
            foodModule.amountG -= flowG;
            if (foodModule.amountG < 0f) {
                foodModule.amountG = 0f;
            }
            float flowB = Mathf.Min(foodModule.amountB, flow);
            foodModule.amountB -= flowB;
            if (foodModule.amountB < 0f) {
                foodModule.amountB = 0f;
            } 

            //SET:
            isBiting = true;
        }
    }
    private void BiteAnimal(CritterSegment segment) {
        float damage = 0.4f;
        segment.agentRef.coreModule.hitPoints[0] -= damage;

        // currently no distinctionbetween regions:
        segment.agentRef.coreModule.healthHead -= damage;
        segment.agentRef.coreModule.healthBody -= damage;
        segment.agentRef.coreModule.healthExternal -= damage;

    }

    private void OnTriggerEnter2D(Collider2D collider) {
        CritterSegment collidingSegment = collider.gameObject.GetComponent<CritterSegment>();
        if(collidingSegment != null) {
            if(agentIndex != collidingSegment.agentIndex) {
                //Debug.Log("LiveAnimal OTHER CRITTER");
                BiteAnimal(collidingSegment);
            }
            else {
                //Debug.Log("SELF");
            }
        }

        FoodModule collidingFoodModule = collider.gameObject.GetComponent<FoodModule>();
        if (collidingFoodModule != null) {
            //Debug.Log("Food");
            //foodInRange = true;

            BiteFood(collidingFoodModule);
        }

        /*if(collider.gameObject.CompareTag("LiveAnimal")) {

            
            Debug.Log("LiveAnimal");


        }*/
        //Debug.Log(this.gameObject.name + ", OnTriggerEnter2D: " + collider.name);
    }

    private void OnTriggerStay2D(Collider2D collider) {
        CritterSegment collidingSegment = collider.gameObject.GetComponent<CritterSegment>();
        if(collidingSegment != null) {
            if(agentIndex != collidingSegment.agentIndex) {
                //Debug.Log("LiveAnimal OTHER CRITTER");
            }
            else {
                //Debug.Log("SELF");
            }
        }

        FoodModule collidingFoodModule = collider.gameObject.GetComponent<FoodModule>();
        if (collidingFoodModule != null) {
            //Debug.Log("Food");
            //foodInRange = true;

            BiteFood(collidingFoodModule);
        }
    }
}
