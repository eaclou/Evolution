using System.Collections.Generic;
using UnityEngine;

public class CritterMouthComponent : MonoBehaviour 
{
    [SerializeField] float predatorBonus = 1f;
    [SerializeField] float defaultTargetArea = 1f;

    public Agent agent;
    public int agentIndex = -1;
    
    public bool isFeeding = false;
    public bool isAttacking = false;
    
    List<Collider2D> edibleObjectsInRange = new List<Collider2D>();
    bool edibleIsInRange => edibleObjectsInRange.Count >= 1;

    public float lastBiteFoodAmount = 0f;   // * WPP: assigned but not used
    
    public CircleCollider2D triggerCollider;
    
    float ownBiteArea => Mathf.Pow(triggerCollider.radius, 2) * 2f;

    public void Tick() 
    {
        //agent.coreModule.objectInRangeOfMouth = false;
        agent.coreModule.objectInRangeOfMouth = edibleIsInRange;
        if (!edibleIsInRange) return;

        // is the current frame the Damage-Frame?
        if (isFeeding && agent.feedingFrameCounter == agent.feedAnimDuration / 2 && !agent.isDead)
            ActiveFeedBiteCheck();

        if(isAttacking && agent.isAttackBiteFrame && !agent.isDead)
            ActiveAttackBiteCheck();
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
    public void Initialize(CritterModuleCoreGenome genome) { }
    
    void OnTriggerEnter2D(Collider2D other) 
    {
        if (IsEdible(other))
            edibleObjectsInRange.Add(other);
    }
    
    void OnTriggerExit2D(Collider2D other) 
    {
        if (IsEdible(other))
            edibleObjectsInRange.Remove(other);
    }
    
    bool IsEdible(Collider2D other) 
    { 
        return !other.gameObject.CompareTag("HazardCollider") && agent != null; 
    }

    void ActiveFeedBiteCheck() 
    {
        for (int i = edibleObjectsInRange.Count - 1; i >= 0; i--) 
        {            
            if (i >= edibleObjectsInRange.Count) continue;
            RequestFeedBiteAgent(edibleObjectsInRange[i]);
            if (i >= edibleObjectsInRange.Count) continue;
            RequestBiteEggSack(edibleObjectsInRange[i]);
        }
    }
    
    void RequestFeedBiteAgent(Collider2D other)
    {
        Agent collidingAgent = other.GetComponentInParent<Agent>();
        
        if (!collidingAgent || agentIndex == collidingAgent.index) 
            return;
        
        // Same species = cannibalism
        if (agent.speciesIndex == collidingAgent.speciesIndex) 
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

    void RequestBiteEggSack(Collider2D other)
    {
        EggSack collidingEggSack = other.GetComponent<EggSack>();
        
        if (!collidingEggSack) return;
        
        if(ownBiteArea > collidingEggSack.area)
            SwallowEggSackWhole(collidingEggSack);
        else
            BiteEggSack(collidingEggSack, ownBiteArea);
    }

    void ActiveAttackBiteCheck() 
    {
        for (int i = edibleObjectsInRange.Count - 1; i >= 0; i--)
        {
            RequestAttackBiteAgent(edibleObjectsInRange[i]);
            RequestBiteEggSack(edibleObjectsInRange[i]); //***EAC out of index Error occasionally
        }
    }
    
    void RequestAttackBiteAgent(Collider2D other)
    {
        Agent collidingAgent = other.GetComponentInParent<Agent>();
        
        if (!collidingAgent || agentIndex == collidingAgent.index || collidingAgent.isDead) 
            return;
        
        float targetArea = collidingAgent.xyBoundArea;
        
        if (ownBiteArea > targetArea)
            SwallowAnimalWhole(collidingAgent);
        else
            BiteAnimal(collidingAgent, targetArea);
    }

    void SwallowAnimalWhole(Agent preyAgent) 
    {
        /*if(preyAgent.index == 0) 
        {
            // No idea what's going on -- game crashes if agent[0] is eaten and seems to think it's null when it is not??? ****
            Debug.LogError("preyAgent.index = 0", preyAgent.gameObject);
            return;
        }*/

        //Debug.Log("SwallowAnimalWhole [" + agentRef.index.ToString() + "] ---> [" + preyAgent.index.ToString() + "]");
        if (agent.isSwallowingPrey) return;
        
        preyAgent.InitiateBeingSwallowed(agent);            
        agent.InitiateSwallowingPrey(preyAgent);
        agent.RegisterAgentEvent(SimulationManager.instance.simAgeTimeSteps, "Swallowed Animal[" + preyAgent.index + "] Whole!", 1f, 2);
        // Credit food:
        //float flow = preyAgent.sizePercentage * (preyAgent.fullSizeBoundingBox.x + preyAgent.fullSizeBoundingBox.z) * preyAgent.fullSizeBoundingBox.y * 0.5f; // + preyAgent.coreModule.stomachContents;
        //agentRef.EatFoodMeat(flow); // assumes all foodAmounts are equal !! *****    
    }

    void SwallowEggSackWhole(EggSack eggSack) 
    {
        float foodEaten = eggSack.foodAmount * 10f * agent.coreModule.digestEfficiencyDecay;
        agent.EatEggsWhole(foodEaten);
        eggSack.ConsumedByPredatorAgent();
        agent.RegisterAgentEvent(SimulationManager.instance.simAgeTimeSteps, "Swallowed Egg Sack Whole!", 1f, 3);
    }
    
    void BiteAnimal(Agent preyAgent, float targetArea) 
    {
        //Debug.Log("BiteAnimal");
        float baseDamage = 3.14f;
        float sizeRatio = ownBiteArea / targetArea; // for now clamped to 10x
        float damage = baseDamage * sizeRatio * agent.coreModule.damageBonus;
        damage = Mathf.Clamp01(damage);

        //agentRef.coreModule.energy += 5f;

        preyAgent.ProcessBiteDamageReceived(damage, agent);
        //agent.RegisterAgentEvent(SimulationManager.instance.simAgeTimeSteps, "Bit Vertebrate! (" + (damage * 100f).ToString("F0") + ")", 1f, 2);
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
            Mathf.Min(massConsumed, ownArea * predatorBonus) * 10f * agent.coreModule.digestEfficiencyDecay : 
            0f;

        agent.EatEggs(foodEaten);
        //agent.RegisterAgentEvent(SimulationManager.instance.simAgeTimeSteps, "Bit EggSack! (" + foodEaten.ToString("F0") + ")", 1f, 3);
    }
    
    public void BiteCorpse(Agent corpseAgent)
    {  
        float biteSize = ownBiteArea * 10f * agent.coreModule.digestEfficiencyDecay;        
        float foodEaten = Mathf.Min(corpseAgent.currentBiomass, biteSize) * 100f;
        
        agent.EatCorpse(foodEaten, biteSize);
        corpseAgent.ProcessBeingEaten(foodEaten);
        //agent.RegisterAgentEvent(SimulationManager.instance.simAgeTimeSteps, "Bit Corpse! (" + foodEaten.ToString("F0") + ")", 1f, 4);
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