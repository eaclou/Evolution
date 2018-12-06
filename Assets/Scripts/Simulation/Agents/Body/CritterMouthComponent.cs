﻿using UnityEngine;

public class CritterMouthComponent : MonoBehaviour {

    public Agent agentRef;
    public int agentIndex = -1;
    
    public bool isFeeding = false;
    public bool isAttacking = false;
    public bool isCooldown = false;

    public int feedingFrameCounter = 0;
    public int attackingFrameCounter = 0;

    public Vector2 mouthTriggerSize;
    public int feedAnimDuration = 20;
    public int feedAnimCooldown = 20;
    public int attackAnimDuration = 40;
    public int attackAnimCooldown = 80;

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

        if(isFeeding) {
            //Debug.Log("mouthTick(" + bitingFrameCounter.ToString() + ")");            
            feedingFrameCounter++;     
            if(feedingFrameCounter > feedAnimDuration) {
                isCooldown = true;
                //feedingFrameCounter = 0;
                //isFeeding = false;
            }
            if(feedingFrameCounter >= feedAnimDuration + feedAnimCooldown) {
                feedingFrameCounter = 0;
                isCooldown = false;
                isFeeding = false;
            }
        }
        if(isAttacking) {
            //Debug.Log("mouthTick(" + bitingFrameCounter.ToString() + ")");            
            attackingFrameCounter++;   
            if(attackingFrameCounter > attackAnimDuration) {
                isCooldown = true;
            }
            if(attackingFrameCounter >= attackAnimDuration + attackAnimCooldown) {
                attackingFrameCounter = 0;
                isCooldown = false;
                isAttacking = false;
            }
        }
        
        /*else
        {
            bitingFrameCounter = 0; // needed?
        }*/
    }

    public float GetIsFeeding() {
        float isConsuming = 0f;
        if(isFeeding) {
            if(feedingFrameCounter <= feedAnimDuration) {
                isConsuming = 1f;
            }
        }
        else {

        }
        return isConsuming;
    }
    public float GetIsAttacking() {
        float val = 0f;
        if(isAttacking) {
            if(attackingFrameCounter == attackAnimDuration / 2) {
                val = 1f;
            }
        }
        else {

        }
        return val;
    }

    public void Disable() {
        triggerCollider.enabled = false;
        isCooldown = false;
        isFeeding = false;
        isAttacking = false;
        //isBiting = false;        
    }
    public void Enable() {
        triggerCollider.enabled = true;
        isCooldown = false;
        if(!isFeeding) {
            feedingFrameCounter = 0;
        }
        if(!isAttacking) {
            attackingFrameCounter = 0;
        }
    }

    public void Initialize(CritterModuleCoreGenome genome, Agent agent) {
        // Set Values from Genome:

        //mouthTriggerSize;
        float baseFeedDuration = 24f;
        float baseFeedCooldown = 24f;
        float baseAttackDuration = 40f;
        float baseAttackCooldown = 80f;

        if(agent.coreModule.dietSpecDecayNorm > 0.5f) {
            baseFeedDuration = 36f;
            baseFeedCooldown = 12f;

            if(agent.coreModule.dietSpecDecayNorm > 0.75f) {
                baseFeedDuration = 64f;
                baseFeedCooldown = 6f;
            }
        }
        if(agent.coreModule.dietSpecPlantNorm > 0.5f) {

            if(agent.coreModule.dietSpecPlantNorm > 0.75f) {

            }
        }
        if(agent.coreModule.dietSpecMeatNorm > 0.5f) {
            baseFeedDuration = 36f;
            baseFeedCooldown = 48f;
            baseAttackDuration = 32f;
            baseAttackCooldown = 64f;

            if(agent.coreModule.dietSpecMeatNorm > 0.75f) {
                baseFeedDuration = 60f;
                baseFeedCooldown = 80f;
                baseAttackDuration = 16f;
                baseAttackCooldown = 48f;
            }
        }

        feedAnimDuration = Mathf.RoundToInt(baseFeedDuration * genome.mouthFeedFrequency);
        feedAnimCooldown = Mathf.RoundToInt(baseFeedCooldown * genome.mouthFeedFrequency);
        attackAnimDuration = Mathf.RoundToInt(baseAttackDuration / genome.mouthAttackAmplitude);
        attackAnimCooldown = Mathf.RoundToInt(baseAttackCooldown / genome.mouthAttackAmplitude);
        
    }

    public void AttemptInitiateActiveFeedBite() {
        if (isFeeding) {
            // this shouldn't happen?
        }
        else {
            // Check if able to bite:
            bool biteReqsMet = true;

            if(agentRef.coreModule.stamina[0] <= 0.05f) {
                biteReqsMet = false;
            }
            if(isAttacking) {
                biteReqsMet = false;
            }
            if(isCooldown) {
                biteReqsMet = false;
            }
            if(biteReqsMet) {
                isFeeding = true;
                triggerCollider.enabled = true;
                feedingFrameCounter = 0;
                agentRef.coreModule.stamina[0] -= 0.05f;
                //Debug.Log("FEED!");
            }           
        }
    }

    public void AttemptInitiateActiveAttackBite() {
        if (isAttacking) {
            // this shouldn't happen?
        }
        else {
            // Check if able to bite:
            bool biteReqsMet = true;
            if(agentRef.coreModule.stamina[0] <= 0.25f) {
                biteReqsMet = false;
            }
            if (isFeeding) {
                biteReqsMet = false;
            }
            if(isCooldown) {
                biteReqsMet = false;
            }
            if(biteReqsMet) {
                isAttacking = true;
                triggerCollider.enabled = true;
                attackingFrameCounter = 0;
                agentRef.coreModule.stamina[0] -= 0.25f;
                //Debug.Log("ATTACK!");
            }                                  
        }
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

        /*EggSack collidingEggSack = collider.gameObject.GetComponent<EggSack>();
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
        }*/
    }

    private void SwallowAnimalWhole(Agent preyAgent) {
        //Debug.Log("SwallowAnimalWhole [" + agentRef.index.ToString() + "] ---> [" + preyAgent.index.ToString() + "]");
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
        float baseDamage = 0.5f;

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
            float flow = ownArea * 1f; // bonus for predators?
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
        float flow = ownBiteArea * 1f; // / colliderCount;

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
        if(collider.gameObject.CompareTag("HazardCollider")) {

        }
        else {                
            agentRef.coreModule.isMouthTrigger[0] = 1f;  // there is an object in range of mouth trigger:
        } 

        if(isFeeding) {
            if(feedingFrameCounter == feedAnimDuration / 2) {  // is the current frame the Damage-Frame?
                // if so, BITE!!
                if(agentRef.curLifeStage != Agent.AgentLifeStage.Dead)
                {
                    ActiveFeedBiteCheck(collider);
                }               
            }
        }
        else {
            if(collider.gameObject.CompareTag("HazardCollider")) {

            }
            else {                
                //AttemptInitiateActiveFeedBite();
            }                
        }

        if(isAttacking) {
            if(attackingFrameCounter == attackAnimDuration / 2) {  // is the current frame the Damage-Frame?
                // if so, BITE!!
                if(agentRef.curLifeStage != Agent.AgentLifeStage.Dead)
                {
                    ActiveAttackBiteCheck(collider);
                }               
            }
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
