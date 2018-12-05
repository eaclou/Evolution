using UnityEngine;

public class CritterMouthComponent : MonoBehaviour {

    public int agentIndex = -1;

    //public bool isPassive = false;

    public enum MouthType {
        None,
        Simple,
        Jaw
    }

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
    public int biteCooldownOversaturationPenalty = 1;
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

    public void Tick() {
        if(isBiting) {
            //Debug.Log("mouthTick(" + bitingFrameCounter.ToString() + ")");
            // Already biting
            bitingFrameCounter++;
            
            if(bitingFrameCounter >= biteHalfCycleDuration * 2 + biteCooldownDuration) {
                bitingFrameCounter = 0;
                isBiting = false;
            }
        }
        else
        {
            bitingFrameCounter = 0; // needed?
        }
    }

    public float GetIsConsuming() {
        float isConsuming = 0f;
        if(isBiting) {
            if(bitingFrameCounter <= biteHalfCycleDuration * 2) {
                isConsuming = 1f;
            }
        }
        else {

        }
        return isConsuming;
    }

    public void Disable() {
        triggerCollider.enabled = false;
        //isBiting = false;        
    }
    public void Enable() {
        if(isBiting) {

        }
        else {
            triggerCollider.enabled = true;
            bitingFrameCounter = 0;
        }        
    }

    public void AttemptInitiateActiveBite() {
        if (isBiting) {
            // this shouldn't happen?
        }
        else {
            // Check if able to bite:
            bool biteReqsMet = true;

            if(agentRef.coreModule.stamina[0] < 0.25f) {
                biteReqsMet = false;
            }
            //if(cooldown) { // handled by isBiting? since that is on for the whole time?
            //    biteReqsMet = false;
            //}

            if(biteReqsMet) {
                isBiting = true;
                triggerCollider.enabled = true;
                bitingFrameCounter = 0;
                agentRef.coreModule.stamina[0] -= 0.2f;
            }
            else {

            }            
        }
    }
    /*public void InitiatePassiveBite() {
        if (isBiting) {

        }
        else {
            isBiting = true;            
            bitingFrameCounter = 0;
        }
    }*/
    

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
        agentRef.EatFoodMeat(flow); // assumes all foodAmounts are equal !! *****  

        //if (agentRef.coreModule.foodEfficiencyMeat > 0.5f) {
        //    agentRef.GainExperience(1f); // swallow bonus
        //}  
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
        agentRef.EatFoodMeat(flow);

        eggSack.ConsumedByPredatorAgent();

        //if (agentRef.coreModule.foodEfficiencyMeat > 0.5f) {
        //    agentRef.GainExperience(1f); // swallow bonus
        //}        
        
        //eggSack.foodAmount = 0f;
    }
    private void BiteDamageAnimal(Agent preyAgent, float ownBiteArea, float targetArea) {
        
        //Debug.Log("BiteDamageAnimal");
        float baseDamage = 0.55f;

        float sizeRatio = ownBiteArea / targetArea; // for now clamped to 10x

        float damage = baseDamage * sizeRatio * agentRef.coreModule.damageBonus;
        damage = Mathf.Clamp01(damage);

        preyAgent.ProcessDamageReceived(damage);

        //if(agentRef.coreModule.foodEfficiencyMeat > 0.5f) { // ** // damage bonus -- provided has the required specialization level:::::
        //    agentRef.GainExperience(damage * 0.5f);  
        //}
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
        
        agentRef.EatFoodMeat(flowR * 1f); // assumes all foodAmounts are equal !! *****
    
        //if(agentRef.coreModule.foodEfficiencyMeat > 0.5f) { // ** // damage bonus -- provided has the required specialization level:::::
        //    agentRef.GainExperience((flowR / agentRef.coreModule.stomachCapacity) * 0.5f);  
        //}
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
        
        agentRef.EatFoodMeat(flowR * 1f); // assumes all foodAmounts are equal !! *****

        //if(agentRef.coreModule.foodEfficiencyMeat > 0.5f) { // ** // damage bonus -- provided has the required specialization level:::::
        //    agentRef.GainExperience((flowR / agentRef.coreModule.stomachCapacity) * 0.5f);  
        //}        

        corpseAgent.ProcessBeingEaten(flowR);
    }

    private void TriggerCheck(Collider2D collider) {
        // Creature OutputNeuron controls mouthEffector[0] = (intention to feed) --> enables collider trigger
        // when collider enabled, OnTriggerEnter/Stay --> attempt to start a bite
        // when bite reaches execution frame, process bite action & consequences

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
                AttemptInitiateActiveBite();
            }                
        }

        /*if(isPassive) {
            PassiveBiteCheck(collider);
        }
        else {  // Active Bite Animation:
            // TEMP!!!
            
        }*/
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
