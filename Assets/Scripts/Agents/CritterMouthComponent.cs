using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CritterMouthComponent : MonoBehaviour {

    public int agentIndex = -1;

    public bool isPassive = false;

    public bool isBiting = false;
    public int bitingFrameCounter = 0;
    public int biteCooldown = 30;
    
    public Vector2 mouthSize;  // relative to head size?
    public Vector2 biteZoneDimensions;
    public float biteZoneOffset;
    //public float mouthOffset;  // also relative?
    public int biteChargeUpDuration = 4;
    public int biteCooldownDuration = 26;
    public float biteStrength;
    public float biteSharpness;
        
    //public float feedingRate = 0.4f;

    public Agent agentRef;

    public CircleCollider2D triggerCollider;

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
    private void BiteFood(FoodModule foodModule) {
        
        if (isBiting) {
            // animation playing:
            if(bitingFrameCounter == biteChargeUpDuration) {
                // BITE!!
            }
        }
        else { // Not in Cooldown!

            //Debug.Log("BiteFood");
            // CONSUME FOOD!
            float flow = 0.4f; // / colliderCount;

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
        float baseDamage = 0.4f;

        float mouthSize = triggerCollider.radius * triggerCollider.radius;
        float targetSize = segment.agentRef.growthPercentage * segment.agentRef.coreModule.coreLength * segment.agentRef.coreModule.coreWidth;

        float sizeRatio = mouthSize / targetSize; // for now clamped to 10x

        float damage = baseDamage * sizeRatio;
        damage = Mathf.Clamp(damage, 0.01f, 10f);

        //Debug.Log("BiteAnimal! ownSize: " + mouthSize.ToString() + ", targetSize: " + targetSize.ToString() + ", sizeRatio: " + damage.ToString());

        segment.agentRef.coreModule.hitPoints[0] -= damage;
        // currently no distinctionbetween regions:
        segment.agentRef.coreModule.healthHead -= damage;
        segment.agentRef.coreModule.healthBody -= damage;
        segment.agentRef.coreModule.healthExternal -= damage;

    }

    */

    public void InitiateActiveBite() {
        if (isBiting) {

        }
        else {
            isBiting = true;
            triggerCollider.enabled = true;
            bitingFrameCounter = 0;
        }
    }

    private void ActiveBiteCheck(Collider2D collider) {
        // own Bite Capacity:
        float ownBiteArea = triggerCollider.radius * triggerCollider.radius * 4f;
        float targetArea = 1f;

        CritterSegment collidingSegment = collider.gameObject.GetComponent<CritterSegment>();
        if(collidingSegment != null) {
            if(agentIndex != collidingSegment.agentIndex) {
                // ANIMAL:
                // Compare sizes:
                targetArea = collidingSegment.agentRef.growthPercentage * collidingSegment.agentRef.coreModule.coreWidth * collidingSegment.agentRef.coreModule.coreLength;
                //targetArea = 0.2f; // TEMP TEST!! ***
                if(ownBiteArea > targetArea) {
                    // Swallow!:::
                    SwallowAnimalWhole(collidingSegment.agentRef);
                }
                else {
                    // Toothy Attack Bite GO!!!
                    BiteDamageAnimal(collidingSegment.agentRef, ownBiteArea, targetArea);        
                }
            }
            else {
                //Debug.Log("SELF");
            }
        }

        FoodChunk collidingFoodModule = collider.gameObject.GetComponent<FoodChunk>();
        if (collidingFoodModule != null) {
            // FOOD:
            // Compare sizes:
            targetArea = collidingFoodModule.curSize.x * collidingFoodModule.curSize.y;

            if(ownBiteArea > targetArea) {
                // Swallow!:::
                SwallowFoodWhole(collidingFoodModule);
            }
            else {
                // Bite off a smaller chunk -- sharper teeth better?
                BiteDamageFood(collidingFoodModule, ownBiteArea, targetArea);      
            }
        }
    }
    private void PassiveBiteCheck(Collider2D collider) {
        // own Bite Capacity:
        float ownBiteArea = triggerCollider.radius * triggerCollider.radius;
        float targetArea = 1f;

        // Check for valid edible object type:
        CritterSegment collidingSegment = collider.gameObject.GetComponent<CritterSegment>();
        if(collidingSegment != null) {
            if(agentIndex != collidingSegment.agentIndex) {
                // ANIMAL:
                // Compare sizes:
                targetArea = collidingSegment.agentRef.growthPercentage * collidingSegment.agentRef.coreModule.coreWidth * collidingSegment.agentRef.coreModule.coreLength;

                if(ownBiteArea > targetArea) {
                    // Swallow!:::
                    SwallowAnimalWhole(collidingSegment.agentRef);
                }
                else {
                    // do nothing - unless it's a spike mouth?
                }
            }
        }
        FoodChunk collidingFoodModule = collider.gameObject.GetComponent<FoodChunk>();
        if (collidingFoodModule != null) {
            // FOOD:
            // Compare sizes:
            targetArea = collidingFoodModule.curSize.x * collidingFoodModule.curSize.y;

            if(ownBiteArea > targetArea) {
                // Swallow!:::
                SwallowFoodWhole(collidingFoodModule);
            }
            else {
                // do nothing - unless it's able to nibble at edge?
            }
        }
    }

    private void SwallowAnimalWhole(Agent preyAgent) {
        //Debug.Log("SwallowAnimalWhole");
        preyAgent.curLifeStage = Agent.AgentLifeStage.SwallowedWhole;
        // Credit food:
        float flow = preyAgent.growthPercentage * preyAgent.coreModule.coreWidth * preyAgent.coreModule.coreLength + preyAgent.coreModule.stomachContents;
        agentRef.EatFood(flow * 1f); // assumes all foodAmounts are equal !! *****    
    }
    private void SwallowFoodWhole(FoodChunk foodModule) {
        //Debug.Log("SwallowFoodWhole");
        float flow = foodModule.curSize.x * foodModule.curSize.y;        
        agentRef.EatFood(flow * 1f);    
        foodModule.amountR = 0f;
        //foodModule.amountG = 0f;
        //foodModule.amountB = 0f;
    }
    private void BiteDamageAnimal(Agent preyAgent, float ownBiteArea, float targetArea) {
        //Debug.Log("BiteDamageAnimal");
        float baseDamage = 0.5f;

        //float mouthSize = triggerCollider.radius * triggerCollider.radius;
        //float targetSize = segment.agentRef.growthPercentage * segment.agentRef.coreModule.coreLength * segment.agentRef.coreModule.coreWidth;

        float sizeRatio = ownBiteArea / targetArea; // for now clamped to 10x

        float damage = baseDamage * sizeRatio;
        damage = Mathf.Clamp(damage, 0.01f, 10f);
        
        preyAgent.coreModule.hitPoints[0] -= damage;
        // currently no distinctionbetween regions:
        preyAgent.coreModule.healthHead -= damage;
        preyAgent.coreModule.healthBody -= damage;
        preyAgent.coreModule.healthExternal -= damage;
    }
    private void BiteDamageFood(FoodChunk foodModule, float ownArea, float targetArea) {
        //Debug.Log("BiteDamageFood");
        //Debug.Log("BiteFood");
        // CONSUME FOOD!
        float flow = ownArea * 2f; // / colliderCount;

        float flowR = Mathf.Min(foodModule.amountR, flow);
        //collidingAgent.testModule.foodAmountR[0] += flowR * 2f;  // make sure Agent doesn't receive food from empty dispenser

        agentRef.EatFood(flowR * 1f); // assumes all foodAmounts are equal !! *****
    
        foodModule.amountR -= flowR;
        if (foodModule.amountR < 0f) {
            foodModule.amountR = 0f;
        }
        /*float flowG = Mathf.Min(foodModule.amountG, flow);
        foodModule.amountG -= flowG;
        if (foodModule.amountG < 0f) {
            foodModule.amountG = 0f;
        }
        float flowB = Mathf.Min(foodModule.amountB, flow);
        foodModule.amountB -= flowB;
        if (foodModule.amountB < 0f) {
            foodModule.amountB = 0f;
        }*/
    }

    private void TriggerCheck(Collider2D collider) {
        if(isPassive) {
            PassiveBiteCheck(collider);
        }
        else {  // Active Bite Animation:
            // TEMP!!!
            if(isBiting) {
                if(bitingFrameCounter == biteChargeUpDuration) {  // is the current frame the Damage-Frame?
                    // if so, BITE!!
                    ActiveBiteCheck(collider);                
                }
            }
            else {
                InitiateActiveBite();
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collider) {
        TriggerCheck(collider);        
    }
    private void OnTriggerStay2D(Collider2D collider) {
        TriggerCheck(collider);       
    }
}
