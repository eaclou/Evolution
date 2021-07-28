using UnityEngine;

public class CritterMouthComponent : MonoBehaviour 
{
    [SerializeField] float predatorBonus = 1f;
    [SerializeField] float defaultTargetArea = 1f;

    public Agent agentRef;
    public int agentIndex = -1;
    
    public bool isFeeding = false;
    public bool isAttacking = false;

    public float lastBiteFoodAmount = 0f;   // * WPP: assigned but not used
    
    public CircleCollider2D triggerCollider;
    
    float ownBiteArea => Mathf.Pow(triggerCollider.radius, 2) * 2f;

    public void Tick() 
    {
        agentRef.coreModule.objectInRangeOfMouth = false;
    }
    
    public void Disable() 
    {
        triggerCollider.enabled = false;
    }
    
    public void Enable() 
    {
        triggerCollider.enabled = true;
    }

    // TBD: Set Values from Genome
    public void Initialize(CritterModuleCoreGenome genome, Agent agent) { }
    
    void OnTriggerEnter2D(Collider2D collider) { TriggerCheck(collider); }
    
    void OnTriggerStay2D(Collider2D collider) { TriggerCheck(collider); }
    
    void TriggerCheck(Collider2D collider) 
    {
        // Creature OutputNeuron controls mouthEffector[0] = (intention to feed) --> enables collider trigger
        // when collider enabled, OnTriggerEnter/Stay --> attempt to start a bite
        // when bite reaches execution frame, process bite action & consequences
        if (!collider.gameObject.CompareTag("HazardCollider") && agentRef != null)
            agentRef.coreModule.objectInRangeOfMouth = true;

        // is the current frame the Damage-Frame?
        if (isFeeding && agentRef.feedingFrameCounter == agentRef.feedAnimDuration / 2 && !agentRef.isDead)
            ActiveFeedBiteCheck(collider);

        if(isAttacking && agentRef.isAttackBiteFrame && !agentRef.isDead)
            ActiveAttackBiteCheck(collider);
    }
    
    void ActiveFeedBiteCheck(Collider2D collider) 
    {
        RequestFeedBiteAgent(collider);
        RequestBiteEggSack(collider);
    }
    
    void RequestFeedBiteAgent(Collider2D collider)
    {
        Agent collidingAgent = collider.GetComponentInParent<Agent>();
        
        if (collidingAgent == null || 
            agentIndex == collidingAgent.index) 
            return;
        
        // Same species = cannibalism
        if (agentRef.speciesIndex == collidingAgent.speciesIndex) 
        {  
            if (!collidingAgent.isDead) return;
            
            if (ownBiteArea > defaultTargetArea)
                SwallowAnimalWhole(collidingAgent);
            else
                BiteCorpse(collidingAgent);
        }
        else
        {
            if (ownBiteArea > collidingAgent.xyBoundArea)
                SwallowAnimalWhole(collidingAgent);
            else if (collidingAgent.isDead)
                BiteCorpse(collidingAgent);
        }
    }

    void RequestBiteEggSack(Collider2D collider)
    {
        EggSack collidingEggSack = collider.GetComponent<EggSack>();
        
        if (collidingEggSack == null) return;
        
        if(ownBiteArea > collidingEggSack.area)
            SwallowEggSackWhole(collidingEggSack);
        else
            BiteEggSack(collidingEggSack, ownBiteArea);
    }

    void ActiveAttackBiteCheck(Collider2D collider) 
    {
        RequestAttackBiteAgent(collider);
        RequestBiteEggSack(collider);
    }
    
    void RequestAttackBiteAgent(Collider2D collider)
    {
        Agent collidingAgent = collider.GetComponentInParent<Agent>();
        
        if (collidingAgent == null || 
            agentIndex == collidingAgent.index ||
            collidingAgent.isDead) 
            return;
        
        float targetArea = collidingAgent.xyBoundArea;
        
        if (ownBiteArea > targetArea)
            SwallowAnimalWhole(collidingAgent);
        else
            BiteAnimal(collidingAgent, targetArea);
    }

    void SwallowAnimalWhole(Agent preyAgent) 
    {
        if(preyAgent.index == 0) 
        {
            // No idea what's going on -- game crashes if agent[0] is eaten and seems to think it's null when it is not??? ****
            Debug.LogError("preyAgent.index = 0", preyAgent.gameObject);
            return;
        }

        //Debug.Log("SwallowAnimalWhole [" + agentRef.index.ToString() + "] ---> [" + preyAgent.index.ToString() + "]");
        if (agentRef.isSwallowingPrey) return;
        
        preyAgent.InitiateBeingSwallowed(agentRef);            
        agentRef.InitiateSwallowingPrey(preyAgent);
        // Credit food:
        //float flow = preyAgent.sizePercentage * (preyAgent.fullSizeBoundingBox.x + preyAgent.fullSizeBoundingBox.z) * preyAgent.fullSizeBoundingBox.y * 0.5f; // + preyAgent.coreModule.stomachContents;
        //agentRef.EatFoodMeat(flow); // assumes all foodAmounts are equal !! *****    
    }

    void SwallowEggSackWhole(EggSack eggSack) 
    {
        float foodEaten = eggSack.foodAmount * 10f * agentRef.coreModule.digestEfficiencyDecay;
        agentRef.EatEggsWhole(foodEaten);
        eggSack.ConsumedByPredatorAgent();
    }
    
    void BiteAnimal(Agent preyAgent, float targetArea) 
    {
        float baseDamage = 3.14f;
        float sizeRatio = ownBiteArea / targetArea; // for now clamped to 10x
        float damage = baseDamage * sizeRatio * agentRef.coreModule.damageBonus;
        damage = Mathf.Clamp01(damage);

        //agentRef.coreModule.energy += 5f;

        preyAgent.ProcessBiteDamageReceived(damage, agentRef);
        agentRef.RegisterAgentEvent(Time.frameCount, "Bit Vertebrate! (" + damage + ") candID: " + preyAgent.candidateRef.candidateID, 1f);
        //if(agentRef.coreModule.foodEfficiencyMeat > 0.5f) { // ** // damage bonus -- provided has the required specialization level:::::
        //    agentRef.GainExperience(damage * 0.5f);  
        //}
        //Debug.Log("BiteDamageAnimal [" + agentRef.index.ToString() + "] ---> [" + preyAgent.index.ToString() + "] damage: " + damage.ToString() + ", preyHealth: " + preyAgent.coreModule.healthHead.ToString());
    }
    
    void BiteEggSack(EggSack eggSack, float ownArea) 
    {
        int numEggsEaten = Mathf.FloorToInt(ownArea / eggSack.eggSize);
        numEggsEaten = Mathf.Min(numEggsEaten, eggSack.curNumEggs);  // prevent overdraw

        float massProportionEaten = (float)numEggsEaten / (float)eggSack.curNumEggs;
        float massConsumed = eggSack.currentBiomass * massProportionEaten;

        eggSack.BittenByAgent(numEggsEaten, massConsumed);

        float foodEaten = numEggsEaten > 0 ? 
            Mathf.Min(massConsumed, ownArea * predatorBonus) * 10f * agentRef.coreModule.digestEfficiencyDecay : 
            0f;

        agentRef.EatEggs(foodEaten);
    }
    
    public void BiteCorpse(Agent corpseAgent)
    {  
        float biteSize = ownBiteArea * 10f * agentRef.coreModule.digestEfficiencyDecay;        
        float foodEaten = Mathf.Min(corpseAgent.currentBiomass, biteSize) * 100f;
        
        agentRef.EatCorpse(foodEaten, biteSize);
        corpseAgent.ProcessBeingEaten(foodEaten);
    }
}


#region Dead code, please delete
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
#endregion