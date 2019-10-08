using UnityEngine;

public class CritterMouthComponent : MonoBehaviour {

    public Agent agentRef;
    public int agentIndex = -1;
    

    public Vector2 mouthTriggerSize;
    
    public bool isFeeding = false;
    public bool isAttacking = false;  

    // efficiency? calculated elsewhere?
    

    /*public enum MouthType {
        None,
        Simple,
        Jaw
    }

    public bool isBiting = false;
    public int bitingFrameCounter = 0;    
    public Vector2 mouthSize;  // relative to head size?
    public Vector2 biteZoneDimensions;
    public float biteZoneOffset;
    public int biteHalfCycleDuration = 6;
    public int biteCooldownDuration = 48;
    public int biteCooldownOversaturationPenalty = 1;
    public float biteStrength;
    public float biteSharpness;
     */  
    public CircleCollider2D triggerCollider;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Tick() {
        agentRef.coreModule.isMouthTrigger[0] = 0f;

        
        
        /*else
        {
            bitingFrameCounter = 0; // needed?
        }*/
    }
    
    public void Disable() {
        triggerCollider.enabled = false;
        //isCooldown = false;
        //isFeeding = false;
        //isAttacking = false;
        //isBiting = false;        
    }
    public void Enable() {
        triggerCollider.enabled = true;
        /*isCooldown = false;
        if(!isFeeding) {
            feedingFrameCounter = 0;
        }
        if(!isAttacking) {
            attackingFrameCounter = 0;
        }*/
    }

    public void Initialize(CritterModuleCoreGenome genome, Agent agent) {
        // Set Values from Genome:

        //mouthTriggerSize;
        float baseFeedDuration = 16f;
        float baseFeedCooldown = 24f;
        float baseAttackDuration = 16f;
        float baseAttackCooldown = 64f;

        /*if(agent.coreModule.dietSpecDecayNorm > 0.5f) {
            baseFeedDuration = Mathf.RoundToInt((float)baseFeedDuration * 2f);
            baseFeedCooldown = Mathf.RoundToInt((float)baseFeedCooldown * 0.75f);

            if(agent.coreModule.dietSpecDecayNorm > 0.75f) {
                baseFeedDuration = Mathf.RoundToInt((float)baseFeedDuration * 4f);
                baseFeedCooldown = Mathf.RoundToInt((float)baseFeedCooldown * 0.5f);
            }
        }
        if(agent.coreModule.dietSpecPlantNorm > 0.5f) {

            if(agent.coreModule.dietSpecPlantNorm > 0.75f) {

            }
        }
        if(agent.coreModule.dietSpecMeatNorm > 0.5f) {
            baseFeedDuration = Mathf.RoundToInt((float)baseFeedDuration * 0.8f);
            baseFeedCooldown = Mathf.RoundToInt((float)baseFeedCooldown * 0.9f);
            baseAttackDuration = Mathf.RoundToInt((float)baseAttackDuration * 0.75f);
            baseAttackCooldown = Mathf.RoundToInt((float)baseAttackCooldown * 0.9f);

            if(agent.coreModule.dietSpecMeatNorm > 0.75f) {
                baseFeedDuration = Mathf.RoundToInt((float)baseFeedDuration * 0.6f);
                baseFeedCooldown = Mathf.RoundToInt((float)baseFeedCooldown * 0.7f);
                baseAttackDuration = Mathf.RoundToInt((float)baseAttackDuration * 0.75f);  // Make this dependent on Attack Specialization rather than diet?
                baseAttackCooldown = Mathf.RoundToInt((float)baseAttackCooldown * 0.4f);
            }
        }*/

        //feedAnimDuration = Mathf.RoundToInt(baseFeedDuration * genome.mouthFeedFrequency);
        //feedAnimCooldown = Mathf.RoundToInt(baseFeedCooldown * genome.mouthFeedFrequency);
        //attackAnimDuration = Mathf.RoundToInt(baseAttackDuration / genome.mouthAttackAmplitude);
        //attackAnimCooldown = Mathf.RoundToInt(baseAttackCooldown / genome.mouthAttackAmplitude);
        
    }

    
    private void ActiveFeedBiteCheck(Collider2D collider) {
        // own Bite Capacity:
        float ownBiteArea = triggerCollider.radius * triggerCollider.radius * 2f;
        float targetArea = 1f;

        Agent collidingAgent = collider.gameObject.GetComponentInParent<Agent>();
        if(collidingAgent != null) {
            if(agentIndex != collidingAgent.index) {

                if(agentRef.speciesIndex == collidingAgent.speciesIndex) {  // same species
                    if(collidingAgent.curLifeStage == Agent.AgentLifeStage.Dead)
                    {
                        if (ownBiteArea > targetArea) {
                            SwallowAnimalWhole(collidingAgent);
                        }
                        else {
                            BiteCorpseFood(collidingAgent, ownBiteArea, targetArea);
                        }
                    }
                }
                else {
                    targetArea = collidingAgent.currentBoundingBoxSize.x * collidingAgent.currentBoundingBoxSize.y;
                    
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
                            //BiteDamageAnimal(collidingAgent, ownBiteArea, targetArea);
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
                Debug.Log("SwallowEggSackWhole:    Agent [" + agentRef.index.ToString() + "] biteSize: " + ownBiteArea.ToString() + "   ---> EggSack [" + collidingEggSack.index.ToString() + "] food: " + collidingEggSack.foodAmount.ToString() + ", growthStatus: ");
                SwallowEggSackWhole(collidingEggSack);
            }
            else {
                // Bite off a smaller chunk -- sharper teeth better?
                BiteDamageEggSack(collidingEggSack, ownBiteArea, targetArea);      
            }
        }
    } 
    
    private void ActiveAttackBiteCheck(Collider2D collider) {
        // own Bite Capacity:
        float ownBiteArea = triggerCollider.radius * triggerCollider.radius * 2f;
        float targetArea = 1f;

        Agent collidingAgent = collider.gameObject.GetComponentInParent<Agent>();
        if(collidingAgent != null) {
            if(agentIndex != collidingAgent.index) {

                targetArea = collidingAgent.currentBoundingBoxSize.x * collidingAgent.currentBoundingBoxSize.y;

                if(collidingAgent.curLifeStage != Agent.AgentLifeStage.Dead)
                {
                    if (ownBiteArea > targetArea) {
                        SwallowAnimalWhole(collidingAgent);
                    }
                    else {
                        BiteDamageAnimal(collidingAgent, ownBiteArea, targetArea);
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

    private void SwallowAnimalWhole(Agent preyAgent) {
        Debug.Log("SwallowAnimalWhole [" + agentRef.index.ToString() + "] ---> [" + preyAgent.index.ToString() + "]");
        ProcessPredatorySwallowAttempt(agentRef, preyAgent);
        // Credit food:
        float flow = preyAgent.sizePercentage * (preyAgent.fullSizeBoundingBox.x + preyAgent.fullSizeBoundingBox.z) * preyAgent.fullSizeBoundingBox.y * 0.5f; // + preyAgent.coreModule.stomachContents;
        agentRef.EatFoodMeat(flow); // assumes all foodAmounts are equal !! *****  
        
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
    }
    private void BiteDamageAnimal(Agent preyAgent, float ownBiteArea, float targetArea) {
        
        //Debug.Log("BiteDamageAnimal");
        float baseDamage = 1f;

        float sizeRatio = ownBiteArea / targetArea; // for now clamped to 10x

        float damage = baseDamage * sizeRatio * agentRef.coreModule.damageBonus;
        damage = Mathf.Clamp01(damage);

        preyAgent.ProcessBiteDamageReceived(damage, agentRef);

        //if(agentRef.coreModule.foodEfficiencyMeat > 0.5f) { // ** // damage bonus -- provided has the required specialization level:::::
        //    agentRef.GainExperience(damage * 0.5f);  
        //}
        Debug.Log("BiteDamageAnimal [" + agentRef.index.ToString() + "] ---> [" + preyAgent.index.ToString() + "] damage: " + damage.ToString() + ", preyHealth: " + preyAgent.coreModule.healthHead.ToString());
    }
    private void BiteDamageEggSack(EggSack eggSack, float ownArea, float targetArea) {

        float sizeOfEachEgg = (float)eggSack.individualEggMaxSize * eggSack.growthScaleNormalized;
                
        int numEggsEaten = Mathf.FloorToInt(ownArea / sizeOfEachEgg);
        numEggsEaten = Mathf.Min(numEggsEaten, eggSack.curNumEggs);  // prevent overdraw

        float massProportionEaten = (float)numEggsEaten / (float)eggSack.curNumEggs;

        float massConsumed = eggSack.currentBiomass * massProportionEaten;

        eggSack.curNumEggs -= numEggsEaten;
        if(eggSack.curNumEggs <= 0) {
            eggSack.curNumEggs = 0;            
            eggSack.ConsumedByPredatorAgent();
        }

        eggSack.currentBiomass -= massConsumed;
        
        // Deprecate this:::
        eggSack.foodAmount = (float)eggSack.curNumEggs / (float)eggSack.maxNumEggs * eggSack.curSize.x * eggSack.curSize.y;
        
        // CONSUME FOOD!
        float flowR = 0f;
        if(numEggsEaten > 0) {
            float flow = ownArea * 1f; // bonus for predators? // maximum bite intake
            flowR = Mathf.Min(massConsumed, flow);
        }        
        
        agentRef.EatFoodMeat(flowR * 1f); // assumes all foodAmounts are equal !! *****

        Debug.Log("BiteEggsack [" + agentRef.index.ToString() + "] ---> [" + eggSack.index.ToString() + "]");
    }
    private void BiteCorpseFood(Agent corpseAgent, float ownBiteArea, float targetArea)
    {  
        Debug.Log("BiteCorpseFood [" + agentRef.index.ToString() + "] ---> [" + corpseAgent.index.ToString() + "]");
        float flow = ownBiteArea * 1f; // / colliderCount;

        float flowR = Mathf.Min(corpseAgent.currentBiomass, flow);
        
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
        if(collider.gameObject.CompareTag("HazardCollider")) {

        }
        else {
            //Debug.Log(agentRef.coreModule.ToString());
            if(agentRef != null) {
                agentRef.coreModule.isMouthTrigger[0] = 1f;  // there is an object in range of mouth trigger:
            }
        } 

        if(isFeeding) {
            //if(feedingFrameCounter == feedAnimDuration / 2) {  // is the current frame the Damage-Frame?
                // if so, BITE!!
                if(agentRef.curLifeStage != Agent.AgentLifeStage.Dead)
                {
                    ActiveFeedBiteCheck(collider);
                }               
            //}
        }
        else {
            if(collider.gameObject.CompareTag("HazardCollider")) {

            }
            else {                
                //AttemptInitiateActiveFeedBite();
            }                
        }

        if(isAttacking) {
            //if(attackingFrameCounter == attackAnimDuration / 2) {  // is the current frame the Damage-Frame?
                // if so, BITE!!
                if(agentRef.curLifeStage != Agent.AgentLifeStage.Dead)
                {
                    ActiveAttackBiteCheck(collider);
                }               
            //}
        }
        else {
            if(collider.gameObject.CompareTag("HazardCollider")) {

            }
            else {
                //AttemptInitiateActiveAttackBite();
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
