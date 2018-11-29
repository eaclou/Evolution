using UnityEngine;

public class CritterMouthComponent : MonoBehaviour {

    public int agentIndex = -1;

    public bool isPassive = false;

    public bool isBiting = false;
    public int bitingFrameCounter = 0;
    //public int biteCooldown = 100;
    
    public Vector2 mouthSize;  // relative to head size?
    public Vector2 biteZoneDimensions;
    public float biteZoneOffset;
    //public float mouthOffset;  // also relative?
    public int biteHalfCycleDuration = 6;
    //public int biteResetDuration = 12;
    public int biteCooldownDuration = 48;
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

    public void Disable() {
        triggerCollider.enabled = false;
        isBiting = false;        
    }
    public void Enable() {
        triggerCollider.enabled = true;
        bitingFrameCounter = 0;
    }

    public void InitiateActiveBite() {
        if (isBiting) {

        }
        else {
            isBiting = true;
            triggerCollider.enabled = true;
            bitingFrameCounter = 0;
        }
    }
    public void InitiatePassiveBite() {
        if (isBiting) {

        }
        else {
            isBiting = true;            
            bitingFrameCounter = 0;
        }
    }
    

    private void ActiveBiteCheck(Collider2D collider) {
        // own Bite Capacity:
        float ownBiteArea = triggerCollider.radius * triggerCollider.radius * 4f;
        float targetArea = 1f;

        Agent collidingAgent = collider.gameObject.GetComponentInParent<Agent>();
        if(collidingAgent != null) {
            if(agentIndex != collidingAgent.index) {

                if(true) { //agentRef.speciesIndex != collidingSegment.agentRef.speciesIndex) {   // *** true == CANNIBALISM ALLOWED!!!!!! *****
                    // ANIMAL:
                    // Compare sizes:
                    targetArea = collidingAgent.currentBoundingBoxSize.x * collidingAgent.currentBoundingBoxSize.y;
                    //targetArea = 0.2f; // TEMP TEST!! ***
                    if(ownBiteArea > targetArea) {
                        // Swallow!:::
                        SwallowAnimalWhole(collidingAgent);
                    }
                    else {
                        //Debug.Log("Bite Animal!");
                        // Toothy Attack Bite GO!!!
                        if(collidingAgent.curLifeStage == Agent.AgentLifeStage.Dead)
                        {
                            BiteCorpseFood(collidingAgent, ownBiteArea, targetArea);
                        }
                        else
                        {
                            BiteDamageAnimal(collidingAgent, ownBiteArea, targetArea);
                        }                               
                    }
                }                
            }
            else {
                //Debug.Log("SELF");
            }
        }

        EggSack collidingEggSack = collider.gameObject.GetComponent<EggSack>();
        if (collidingEggSack != null) {
            // FOOD:
            // Compare sizes:
            targetArea = collidingEggSack.curSize.x * collidingEggSack.curSize.y;

            if(ownBiteArea > targetArea) {
                // Swallow!:::
                //Debug.Log("SwallowEggSackWhole:    Agent [" + agentRef.index.ToString() + "] biteSize: " + ownBiteArea.ToString() + "   ---> EggSack [" + collidingEggSack.index.ToString() + "] food: " + collidingEggSack.foodAmount.ToString() + ", growthStatus: " + collidingEggSack.growthStatus.ToString());
                SwallowEggSackWhole(collidingEggSack);
            }
            else {
                // Bite off a smaller chunk -- sharper teeth better?
                BiteDamageEggSack(collidingEggSack, ownBiteArea, targetArea);      
            }
        }
    }
    private void PassiveBiteCheck(Collider2D collider) {
        // disabled -- handling this differently...
        /*
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
        */
    }

    private void SwallowAnimalWhole(Agent preyAgent) {
        //Debug.Log("SwallowAnimalWhole [" + agentRef.index.ToString() + "] ---> [" + preyAgent.index.ToString() + "]");
        ProcessPredatorySwallowAttempt(agentRef, preyAgent);
        //preyAgent.curLifeStage = Agent.AgentLifeStage.Dead;
        // Credit food:
        float flow = preyAgent.sizePercentage * (preyAgent.fullSizeBoundingBox.x + preyAgent.fullSizeBoundingBox.z) * preyAgent.fullSizeBoundingBox.y * 0.5f; // + preyAgent.coreModule.stomachContents;
        agentRef.EatFood(flow); // assumes all foodAmounts are equal !! *****  
        //Debug.Log("SwallowAnimalWhole. foodFlow: " + flow.ToString() + ", agentStomachContents: " + agentRef.coreModule.stomachContents.ToString());
        // **** Not removing Animal? --> need to attach?
    }
    private void ProcessPredatorySwallowAttempt(Agent predatorAgent, Agent preyAgent)
    {
        // add boolean check here? possibly attempting to swallow multiple things?
        if(predatorAgent.isSwallowingPrey)
        {

        }
        else
        {
            preyAgent.InitiateBeingSwallowed(predatorAgent);            
            predatorAgent.InitiateSwallowingPrey(preyAgent);

        }

        
    }
    private void SwallowEggSackWhole(EggSack eggSack) {
        
        //Debug.Log("SwallowFoodWhole");
        float flow = eggSack.foodAmount;        
        agentRef.EatFood(flow);

        eggSack.ConsumedByPredatorAgent();
        
        //eggSack.foodAmount = 0f;
    }
    private void BiteDamageAnimal(Agent preyAgent, float ownBiteArea, float targetArea) {
        
        //Debug.Log("BiteDamageAnimal");
        float baseDamage = 0.55f;

        float sizeRatio = ownBiteArea / targetArea; // for now clamped to 10x

        float damage = baseDamage * sizeRatio * agentRef.coreModule.damageBonus;
        damage = Mathf.Clamp01(damage);

        preyAgent.ProcessBeingBitten(damage);

        //Debug.Log("BiteDamageAnimal [" + agentRef.index.ToString() + "] ---> [" + preyAgent.index.ToString() + "] damage: " + damage.ToString() + ", preyHealth: " + preyAgent.coreModule.healthHead.ToString());
    }
    private void BiteDamageEggSack(EggSack eggSack, float ownArea, float targetArea) {

        float sizeOfEachEgg = (float)eggSack.individualEggMaxSize * eggSack.growthScaleNormalized;
                
        int numEggsEaten = Mathf.FloorToInt(ownArea / sizeOfEachEgg);
        numEggsEaten = Mathf.Min(numEggsEaten, eggSack.curNumEggs);  // prevent overdraw

        eggSack.curNumEggs -= numEggsEaten;
        if(eggSack.curNumEggs <= 0) {
            eggSack.curNumEggs = 0;
            
            eggSack.ConsumedByPredatorAgent();
        }
        eggSack.foodAmount = (float)eggSack.curNumEggs / (float)eggSack.maxNumEggs * eggSack.curSize.x * eggSack.curSize.y;
        //Debug.Log("BiteDamageFood");
        //Debug.Log("BiteFood");
        
        // CONSUME FOOD!
        float flowR = 0f;
        if(numEggsEaten > 0) {
            float flow = ownArea * 2f; // bonus for predators?
            flowR = Mathf.Min(eggSack.foodAmount, flow);
        }        
        
        agentRef.EatFood(flowR * 1f); // assumes all foodAmounts are equal !! *****
    
        /*eggSack.foodAmount -= flowR;
        if (eggSack.foodAmount < 0f) {
            eggSack.foodAmount = 0f;
        }*/

        //Debug.Log("BiteDamageEggSack:    Agent [" + agentRef.index.ToString() + "] ---> EggSack [" + eggSack.index.ToString() + "] ownArea: " + ownArea.ToString() + ", flow: " + flowR.ToString() + ", numEggs: " + eggSack.curNumEggs.ToString() + ", foodAmount: " + eggSack.foodAmount.ToString());
       
    }
    private void BiteCorpseFood(Agent corpseAgent, float ownBiteArea, float targetArea)
    {  
        //Debug.Log("BiteCorpseFood [" + agentRef.index.ToString() + "] ---> [" + corpseAgent.index.ToString() + "]");
        float flow = ownBiteArea * 2f; // / colliderCount;

        float flowR = Mathf.Min(corpseAgent.currentCorpseFoodAmount, flow);
        
        agentRef.EatFood(flowR * 1f); // assumes all foodAmounts are equal !! *****

        corpseAgent.ProcessBeingEaten(flowR);
    }

    private void TriggerCheck(Collider2D collider) {
        if(isPassive) {
            PassiveBiteCheck(collider);
        }
        else {  // Active Bite Animation:
            // TEMP!!!
            if(isBiting) {
                if(bitingFrameCounter == biteHalfCycleDuration) {  // is the current frame the Damage-Frame?
                    // if so, BITE!!
                    if(agentRef.curLifeStage != Agent.AgentLifeStage.Dead)
                    {
                        ActiveBiteCheck(collider);
                    }               
                }
            }
            else {
                if(collider.gameObject.CompareTag("HazardCollider")) {

                }
                else {
                    //InitiateActiveBite();
                }                
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collider) {
        TriggerCheck(collider);        
    }
    private void OnTriggerStay2D(Collider2D collider) {
        TriggerCheck(collider);       
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

}
