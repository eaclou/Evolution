using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour {

    SettingsManager settingsRef;
    //private PerformanceData performanceData;
    //public float totalFoodEatenDecay = 0f;
    
    //private bool isFeedingPlant = false;
    //private bool isFeedingZooplankton = false;

    public float speed = 500f;
    public float smoothedThrottleLerp = 0.1f;
    public float animationCycle = 0f;
    public float turningAmount = 0f;
    public float swimAnimationCycleSpeed = 0.06f;

    //public float globalWaterLevel;

    public float spawnStartingScale = 0.1f; // *** REFACTOR!!! SYNC WITH EGGS!!!

    public bool isInert = true;  // when inert, colliders disabled // is this still used?? ****
    // Refactor??
    public bool isActing = false;  // biting, defending, dashing, etc -- exclusive actions    
    public bool isDecaying = false;

    public bool isFeeding = false;
    public bool isAttacking = false;    

    public int feedAnimDuration = 30;
    public int feedAnimCooldown = 30;
    public int attackAnimDuration = 40;
    public int attackAnimCooldown = 75;
    public int feedingFrameCounter = 0;
    public int attackingFrameCounter = 0;
    
    public bool isDashing = false;
    public int dashFrameCounter = 0;
    public int dashDuration = 40;
    public int dashCooldown = 120;

    public bool isDefending = false;
    public int defendFrameCounter = 0;
    public int defendDuration = 60;
    public int defendCooldown = 60;

    public bool isResting = false;
    public bool isCooldown = false;

    public int cooldownFrameCounter = 0;
    public int cooldownDuration = 60;  // arbitrary default

    public bool isMarkedForDeathByUser = false;

    public int index;    
    public int speciesIndex = -1;  // ********************** NEED to set these at birth!
    public CandidateAgentData candidateRef;

    public struct AgentEventData {
        public int eventFrame;
        public string eventText;
        public float goodness;
    }
    public string stringCauseOfDeath = "";
    
    public List<AgentEventData> agentEventDataList = new List<AgentEventData>();
    //public string lastEvent = "";
    //public int lastEventTime = 0;
    
    public AgentLifeStage curLifeStage;
    public enum AgentLifeStage {
        AwaitingRespawn,  // can i merge this in with null?
        Egg,
        Mature,
        Dead,
        Null
    }
    public AgentActionState curActionState;
    public enum AgentActionState {
        Default,  // swimming, no actions
        Resting,
        Feeding,
        Attacking,
        Dashing,
        Defending,
        Cooldown,
        Decaying
    }
    private int gestationDurationTimeSteps = 120;
    public int _GestationDurationTimeSteps => gestationDurationTimeSteps;
    
    public int maxAgeTimeSteps = 100000;
    
    private int growthScalingSkipFrames = 16;

    public float sizePercentage = 0f;
    
    public Brain brain;
    public GameObject bodyGO;
    public Rigidbody2D bodyRigidbody;

    // MODULES:::
    public CritterModuleCommunication communicationModule;
    public CritterModuleCore coreModule;
    public CritterModuleEnvironment environmentModule;
    public CritterModuleFood foodModule;
    public CritterModuleFriends friendModule;
    public CritterModuleMovement movementModule;
    public CritterModuleThreats threatsModule;

    public CritterMouthComponent mouthRef;

    public CapsuleCollider2D colliderBody;
    public SpringJoint2D springJoint;   // Used to attach to EggSack Object while still in Egg stage
    public CapsuleCollider mouseClickCollider;
    
    public Vector3 fullSizeBoundingBox;  // ASSUMES Z=LENGTH, Y=HEIGHT, X=WIDTH
    public Vector3 currentBoundingBoxSize;
    public float fullSizeBodyVolume = 1f;
    //public float centerOfMass = 0f;
    
        // *********  Combine thse stats into a serializable class for cleanliness?  *****
    public int lifeStageTransitionTimeStepCounter = 0; // keeps track of how long agent has been in its current lifeStage
    public int ageCounter = 0;
    public float consumptionScore = 0f;
    public float damageDealtScore = 0f;
    public float damageReceivedScore = 0f;
    public float energyEfficiencyScore = 0f;
    public float reproductionScore = 0f;
    public float supportScore = 0f;
    public float masterFitnessScore = 0f;
    public float totalExperience = 0f;
    public float experienceForNextLevel = 2f; // 2, 4, 8, 16, 32, 64, 128, 256?
    public int curLevel = 0;
    
    public int pregnancyRefactoryTimeStepCounter = 0;

    // *** Need resource Overhaul
    public float currentBiomass = 0f;
    public float wasteProducedLastFrame = 0f;
    public float oxygenUsedLastFrame = 0f;
    //public float currentReproductiveStockpile = 0f;
    private float fullsizeBiomass = 1f;
    public float biomassAtDeath = 1f;
    
    private Vector3 prevPos;  // use these instead of sampling rigidbody?
    public Vector3 _PrevPos => prevPos;

    public float prevVel;
    public float curVel;
    public float curAccel;

    public Vector2 ownPos;
    public Vector2 ownVel;

    public Vector2 throttle;
    public Vector2 smoothedThrottle;
    public Vector2 facingDirection;  // based on throttle history

    public float avgVel;
    public Vector2 avgFluidVel;
    public float waterDepth;
    public Vector2 depthGradient;
        
    public bool isSwallowingPrey = false;
    public bool isBeingSwallowed = false;
    public bool isSexuallyMature = false;
    public int beingSwallowedFrameCounter = 0;
    public int swallowingPreyFrameCounter = 0;
    public int swallowDuration = 60; // **** how does this mix with mouseComponent???? ***
    public Agent predatorAgentRef;
    public Agent preyAgentRef;

    public EggSack parentEggSackRef;  // instead of using own fixed embry development duration - look up parentEggSack and use its counter?
    public bool isAttachedToParentEggSack = false;

    public EggSack childEggSackRef;
    public bool isPregnantAndCarryingEggs = false;
    public int pregnancyRefactoryDuration = 2400;

    //public float overflowFoodAmount = 0f;
        
    private void Awake() {        
        // temp fix for delayed spawning of Agents (leading to nullReferenceExceptions)
        //agentWidthsArray = new float[widthsTexResolution];
        isInert = true;
        
        // WPP: initialized in declaration to eliminate possibility of race condition
        //agentEventDataList = new List<AgentEventData>();  // created once
    }
    
    // WPP 4/9: cut verbosity with declarative style (optional if not repeated often)
    bool isDead => curLifeStage == AgentLifeStage.Dead;
    bool isEgg => curLifeStage == AgentLifeStage.Egg;

    // WPP 4/9: simplified conditionals and added error condition
    public float GetDecayPercentage() {
        if (biomassAtDeath == 0f) {
            Debug.LogError("Biomass at death zero for " + index);
            return 0f;
        }
        if (!isDead) {
            Debug.LogError("I'm not dead yet! " + index + " " + curLifeStage);
            return 0f;
        }
    
        float percentage = 1f - currentBiomass / biomassAtDeath;
                
        /*if (curLifeStage == AgentLifeStage.Dead) {
            if(biomassAtDeath == 0f) {
                Debug.LogError("AAAHH" + index.ToString());
            }
            else {
                percentage = 1f - (currentBiomass / biomassAtDeath);
            }
        }*/
        
        return Mathf.Clamp01(percentage);
    }

    // WPP: early exit instead of if-else, added warning
    public void AttemptInitiateActiveFeedBite() {
        if (isFeeding) {
            Debug.LogWarning("Already feeding, no need to initiate feed bite");
            return;
        }
        
        isFeeding = true;            
        mouthRef.triggerCollider.enabled = true;
        mouthRef.lastBiteFoodAmount = 0f;
        feedingFrameCounter = 0;            
    }

    public void AttemptInitiateActiveAttackBite() {
        //Debug.Log("ATTACK");
        if (isAttacking) {
            Debug.LogWarning("Already attacking, no need to initiate attack bite");
            return;
        }
               
        isAttacking = true;
        mouthRef.triggerCollider.enabled = true;
        attackingFrameCounter = 0;    
        
        candidateRef.performanceData.totalTimesAttacked++;
    }
    
    public void SetToAwaitingRespawn() {
        curLifeStage = AgentLifeStage.AwaitingRespawn;
        isInert = true;
        isMarkedForDeathByUser = false;
    }

    public void InitiateBeingSwallowed(Agent predatorAgent)
    {
        //curLifeStage = Agent.AgentLifeStage.Dead; // ....

        //isInert = false; // *** is this still used? ... (sorta...)

        colliderBody.enabled = false;
        isBeingSwallowed = true;
        //isSexuallyMature = false;
        beingSwallowedFrameCounter = 0;
        predatorAgentRef = predatorAgent;

        //Debug.Log("Died of old age!");
        stringCauseOfDeath = "Swallowed Whole";
        InitializeDeath();

        /*
        springJoint.connectedBody = predatorAgentRef.bodyRigidbody;
        springJoint.autoConfigureConnectedAnchor = false;
        springJoint.anchor = Vector2.zero;
        springJoint.connectedAnchor = Vector2.zero;
        springJoint.autoConfigureDistance = false;
        springJoint.distance = 0.005f;
        springJoint.enableCollision = false;
        springJoint.enabled = true;
        springJoint.frequency = 15f;
        */
        //this.biomassAtDeath
        
    }
    public void InitiateSwallowingPrey(Agent preyAgent)
    {
        isSwallowingPrey = true;
        swallowingPreyFrameCounter = 0;
        preyAgentRef = preyAgent;

        float foodAmount = preyAgent.currentBiomass * 1000f * coreModule.foodEfficiencyMeat;  // experiment!
        candidateRef.performanceData.totalFoodEatenCreature += foodAmount;

        EatFoodMeat(foodAmount);
        // WPP: .ToString() redundant on argument on concatenated string
        RegisterAgentEvent(Time.frameCount, "Ate Vertebrate! (" + foodAmount + ") candID: " + preyAgent.candidateRef.candidateID, 1f);
        preyAgent.ProcessBeingEaten(preyAgent.currentBiomass);
        
        colliderBody.enabled = false;
        springJoint.enabled = false;
        springJoint.connectedBody = null;
    }

    public void MapNeuronToModule(NID nid, Neuron neuron) {
        // Hidden nodes!
        if (nid.moduleID == -1) {
            neuron.currentValue = new float[1];
            neuron.neuronType = NeuronGenome.NeuronType.Hid;
            neuron.previousValue = 0f;
        }
        else {  // In/Out nodes:::
            communicationModule.MapNeuron(nid, neuron);
            coreModule.MapNeuron(nid, neuron);
            environmentModule.MapNeuron(nid, neuron);
            foodModule.MapNeuron(nid, neuron);
            friendModule.MapNeuron(nid, neuron);
            movementModule.MapNeuron(nid, neuron);
            threatsModule.MapNeuron(nid, neuron);
        }
    }
        
    public void ResetBrainState() {
        brain.ResetBrainState();
    }
    
    public void TickBrain() {
        brain.BrainMasterFunction();
    }
    public void TickModules(SimulationManager simManager) { // Updates internal state of body - i.e health, energy etc. -- updates input Neuron values!!!
                                // Update Stocks & Flows ::: new health, energy, stamina
                                // This should have happened during last frame's Internal PhysX Update

        // HOWEVER, some updates only happen once per frame and can be handled here, like converting food into energy automatically

        // Turns out that most of these updates are updating input neurons which tend to be sensors
        // These values are sometimes raw attributes & sometimes processed data
        // Should I break them up into individual sensor types -- like Ears, Collider Rangefind, etc.?
        // Separate sensors for each target type or add multiple data types to rangefinder raycasts?
        
        //UpdateInternalResources();  // update energy, stamina, food -- or do this during TickActions?
               
        coreModule.Tick();
        communicationModule.Tick(this);
        environmentModule.Tick(this);
        foodModule.Tick(simManager, this);
        friendModule.Tick(this);
        movementModule.Tick(this, ownVel);
        threatsModule.Tick(this);
        // Add more sensor Modules later:

        // Update Mouth::::         
        mouthRef.Tick();
    }
    
    // STARVATION
    // WPP: applied early-exit, replaced branching with ternaries
    private void CheckForDeathStarvation() {
        if (coreModule.energy > 0f)
            return;
            
        bool starved = coreModule.stomachContentsNorm <= 0.01f;
        stringCauseOfDeath = starved ? "Starved" : "Suffocated";
        string eventMessage = starved ? "Starved!" : "Suffocated! stomachContentsNorm: " + coreModule.stomachContentsNorm;
        RegisterAgentEvent(Time.frameCount, eventMessage, 0f);
    
        /*if(coreModule.stomachContentsNorm > 0.01f) {
            stringCauseOfDeath = "Suffocated";
            RegisterAgentEvent(Time.frameCount, "Suffocated! stomachContentsNorm: " + coreModule.stomachContentsNorm, 0f);
        }
        else {
            stringCauseOfDeath = "Starved";
            RegisterAgentEvent(Time.frameCount, "Starved!", 0f);
        }*/
        
        lifeStageTransitionTimeStepCounter = 0;
        InitializeDeath();
    }
    
    public void CheckForDeathHealth() {
        // HEALTH FAILURE:
        if (coreModule.healthBody <= 0f) {
            curLifeStage = AgentLifeStage.Dead;
            lifeStageTransitionTimeStepCounter = 0;
            
            coreModule.hitPoints[0] = 0f;
            coreModule.healthHead = 0f;
            coreModule.healthBody = 0f;
            coreModule.healthExternal = 0f;

            //Debug.LogError("CheckForDeathHealth" + currentBiomass.ToString());

            stringCauseOfDeath = "Fatal Injuries";
            RegisterAgentEvent(Time.frameCount, "Died of Injuries!", 0f);
            InitializeDeath();
        }
    }
    
    private void CheckForDeathOldAge() {
        if(ageCounter > maxAgeTimeSteps) {
            curLifeStage = AgentLifeStage.Dead;
            lifeStageTransitionTimeStepCounter = 0;

            //Debug.Log("Died of old age!");
            stringCauseOfDeath = "Old Age";
            RegisterAgentEvent(Time.frameCount, "Died of Old Age!", 0f);
            InitializeDeath();
        }
    }
    
    private void CheckForDeathDivineJudgment() {
        if(isMarkedForDeathByUser) {
            curLifeStage = Agent.AgentLifeStage.Dead;
            lifeStageTransitionTimeStepCounter = 0;
            stringCauseOfDeath = "Divine Judgment";
            RegisterAgentEvent(UnityEngine.Time.frameCount, "Struck down by Divine Judgment!", 0f);
            InitializeDeath();
        }
    }
    
    private void InitializeDeath()   // THIS CAN BE A LOT CLEANER!!!!! *****
    {  
        curLifeStage = AgentLifeStage.Dead;

        if(isPregnantAndCarryingEggs) {
            AbortPregnancy();
        }

        //colliderBody.enabled = false;
        //bodyRigidbody.simulated = false;
        //bodyRigidbody.isKinematic = true;

        isSexuallyMature = false;
        isSwallowingPrey = false;
        swallowingPreyFrameCounter = 0;

        masterFitnessScore = totalExperience; // update this???
        candidateRef.performanceData.totalTicksAlive = ageCounter;
        
        biomassAtDeath = currentBiomass;

        mouthRef.Disable();
    }
    private void CheckForLifeStageTransition() {
        switch(curLifeStage) {
            case AgentLifeStage.AwaitingRespawn:
                //
                break;
            case AgentLifeStage.Egg:
                //
                if(lifeStageTransitionTimeStepCounter >= gestationDurationTimeSteps) {
                    BeginHatching();
                }
                else {
                    CheckForDeathHealth();
                }
                
                break;
            case AgentLifeStage.Mature:
                
                // Check for Death:
                CheckForDeathStarvation();
                CheckForDeathHealth();
                CheckForDeathOldAge();
                CheckForDeathDivineJudgment();
                break;
            case AgentLifeStage.Dead:
                if(currentBiomass <= 0f) { //// || lifeStageTransitionTimeStepCounter >= decayDurationTimeSteps) {  // Fully decayed
                    curLifeStage = AgentLifeStage.Null;                    
                    lifeStageTransitionTimeStepCounter = 0;
                    isInert = true;
                }
                break;
            case AgentLifeStage.Null:
                isInert = true;

                beingSwallowedFrameCounter = 0;
                isBeingSwallowed = false;
                isSwallowingPrey = false;

                parentEggSackRef = null;
                childEggSackRef = null;

                colliderBody.enabled = false;
                springJoint.enabled = false;
                springJoint.connectedBody = null;

                bodyRigidbody.velocity = Vector2.zero;
                bodyRigidbody.angularVelocity = 0f;

                biomassAtDeath = 1f; // ??
                              
                break;
            default:
                Debug.LogError("NO SUCH ENUM ENTRY IMPLEMENTED, YOU FOOL!!! (" + curLifeStage.ToString() + ")");
                break;
        }
    }
    
    public void RegisterAgentEvent(int frame, string textString, float goodness01) {
        //lastEvent = "Ate Plant! (" + amount.ToString() + ")";
        //lastEventTime = UnityEngine.Time.frameCount;
        //eventLogStringList
        AgentEventData newEvent = new AgentEventData();
        newEvent.eventFrame = frame;
        newEvent.eventText = textString;
        newEvent.goodness = goodness01;

        agentEventDataList.Add(newEvent);
    }
    public void EatFoodPlant(float amount) {
         
        float stomachSpace = coreModule.stomachCapacity - coreModule.stomachContentsPlant - coreModule.stomachContentsMeat - coreModule.stomachContentsDecay;  // Make food Discrete???
        if(amount > stomachSpace) {
            amount = stomachSpace; // ??
        }
        //amount *= coreModule.foodEfficiencyPlant;
        coreModule.stomachContentsNorm += (amount / coreModule.stomachCapacity);
        
        if(coreModule.stomachContentsNorm > 1f) {
            //float overStuffAmount = coreModule.stomachContentsNorm - 1f;
            //ProcessDamageReceived(overStuffAmount);
            coreModule.stomachContentsNorm = 1f;            
        }
        else {
            coreModule.stomachContentsPlant += amount;
        }
        GainExperience((amount / coreModule.stomachCapacity) * coreModule.foodEfficiencyPlant * 1f); // Exp for appropriate food    

        //Debug.Log("EatFoodPlant " + amount.ToString());
        RegisterAgentEvent(UnityEngine.Time.frameCount, "Ate Plant! (+" + (amount * 1000).ToString("F0") + " food)", 1f);
    }
    public void EatFoodMeat(float amount) {
        //totalFoodEatenZoop += amount; 
        float stomachSpace = coreModule.stomachCapacity - coreModule.stomachContentsPlant - coreModule.stomachContentsMeat - coreModule.stomachContentsDecay;
        if(amount > stomachSpace) {
            amount = stomachSpace; // ??
        }
        //amount *= coreModule.foodEfficiencyMeat;
        coreModule.stomachContentsNorm += (amount / coreModule.stomachCapacity);
        
        if(coreModule.stomachContentsNorm > 1f) {
            //float overStuffAmount = coreModule.stomachContentsNorm - 1f;
            //ProcessDamageReceived(overStuffAmount);
            coreModule.stomachContentsNorm = 1f;
        }
        else {
            coreModule.stomachContentsMeat += amount;
        }
        
        GainExperience((amount / coreModule.stomachCapacity) * coreModule.foodEfficiencyMeat * 1f); // Exp for appropriate food

        //RegisterAgentEvent(UnityEngine.Time.frameCount, "Ate Zoop! (" + amount.ToString() + ")", 1f);
    }
    public void EatFoodDecay(float amount) {
        
        float stomachSpace = coreModule.stomachCapacity - coreModule.stomachContentsPlant - coreModule.stomachContentsMeat - coreModule.stomachContentsDecay;
        if(amount > stomachSpace) {
            amount = stomachSpace; // ??
        }
        coreModule.stomachContentsNorm += (amount / coreModule.stomachCapacity);
        
        if(coreModule.stomachContentsNorm > 1f) {
            //float overStuffAmount = coreModule.stomachContentsNorm - 1f;            
            coreModule.stomachContentsNorm = 1f;
        }
        else {
            coreModule.stomachContentsDecay += amount;
        }
        
        GainExperience((amount / coreModule.stomachCapacity) * coreModule.foodEfficiencyDecay * 1f); // Exp for appropriate food

        RegisterAgentEvent(UnityEngine.Time.frameCount, "Ate Corpse! (" + amount.ToString() + ")", 1f);
    }
    public void TakeDamage(float damage) {
        int rand = UnityEngine.Random.Range(0, 3);
        if(rand == 0) {
            coreModule.healthHead -= damage; // * UnityEngine.Random.Range(0f, 1f);
        }
        else if(rand == 1) {
            coreModule.healthBody -= damage;
        }
        else {
            coreModule.healthExternal -= damage;
        }
        //coreModule.healthHead -= damage * UnityEngine.Random.Range(0f, 1f);
        //coreModule.healthBody -= damage * UnityEngine.Random.Range(0f, 1f);
        //coreModule.healthExternal -= damage * UnityEngine.Random.Range(0f, 1f);

        coreModule.hitPoints[0] = (coreModule.healthHead + coreModule.healthBody + coreModule.healthExternal) / 3f;

        candidateRef.performanceData.totalDamageTaken += damage;

        RegisterAgentEvent(UnityEngine.Time.frameCount, "Took Damage! (" + damage.ToString() + ")", 0f);

        CheckForDeathHealth();
    }
    public void ProcessBiteDamageReceived(float damage, Agent predatorAgentRef) {

        damage = damage / coreModule.healthBonus;

        float defendBonus = 1f;
        if(isDefending && defendFrameCounter < defendDuration) {
            
            RegisterAgentEvent(UnityEngine.Time.frameCount, "Blocked Bite! from #" + predatorAgentRef.index.ToString(), 0.75f);
           
        }
        else {
            damage *= defendBonus;
            predatorAgentRef.candidateRef.performanceData.totalDamageDealt += damage;
            TakeDamage(damage);
            RegisterAgentEvent(UnityEngine.Time.frameCount, "Bitten! (" + damage.ToString("F2") + ") by #" + predatorAgentRef.index.ToString(), 0f);
        }

        

        //coreModule.energy *= 0.5f; // ******

        
     
    }
    public void ProcessBeingEaten(float amount) {
        // if this agent is dead, it acts as food.
        // it was just bitten by another creature and removed material -- 
        
        currentBiomass -= amount;

        if (currentBiomass <= 0f)
        {
            currentBiomass = 0f;

            //coreModule.healthBody = 0f;
            //coreModule.healthHead = 0f;
            //coreModule.healthExternal = 0f;

            //curLifeStage = AgentLifeStage.Null;
            //lifeStageTransitionTimeStepCounter = 0;
                
            //beingSwallowedFrameCounter = 0;
            //isBeingSwallowed = false;

            colliderBody.enabled = false;
            springJoint.enabled = false;
            springJoint.connectedBody = null;
        }
        else
        {
            ScaleBody(sizePercentage, false);
        }
        
        RegisterAgentEvent(UnityEngine.Time.frameCount, "Devoured!", 0f);
        
    }

    public void Tick(SimulationManager simManager, SettingsManager settings) {
        
        // Resources:
        wasteProducedLastFrame = 0f;
        oxygenUsedLastFrame = 0f;

        //globalWaterLevel = simManager.theRenderKing.baronVonWater._GlobalWaterLevel;

        /*if(isBeingSwallowed)
        {
            beingSwallowedFrameCounter++;

            if(beingSwallowedFrameCounter >= 10)
            {
                //Debug.Log("isBeingSwallowed + swallow Complete!");
                //curLifeStage = AgentLifeStage.Null;
                //lifeStageTransitionTimeStepCounter = 0;
                
                beingSwallowedFrameCounter = 0;
                isBeingSwallowed = false;

                colliderBody.enabled = false;
                springJoint.enabled = false;
                springJoint.connectedBody = null;
            }
            else
            {
                float scale = (float)beingSwallowedFrameCounter / (float)swallowDuration;

                ScaleBody((1.0f - scale) * 1f * sizePercentage, true);
            }
        }  */      

        // Any external inputs updated by simManager just before this

        // Check for StateChange:
        CheckForLifeStageTransition();
        
        switch(curLifeStage) {
            case AgentLifeStage.AwaitingRespawn:
                //
                bodyRigidbody.simulated = false;
                bodyRigidbody.isKinematic = false;
                colliderBody.enabled = false;
                break;
            case AgentLifeStage.Egg:
                //
                TickEgg();
                break;            
            case AgentLifeStage.Mature:
                //
                TickMature(simManager);
                break;
            case AgentLifeStage.Dead:
                //
                TickDead(settings);
                break;
            case AgentLifeStage.Null:
                //
                //Debug.Log("agent is null - probably shouldn't have gotten to this point...;");
                break;
            default:
                Debug.LogError("NO SUCH ENUM ENTRY IMPLEMENTED, YOU FOOL!!! (" + curLifeStage.ToString() + ")");
                break;
        }
        
        Vector3 curPos = bodyRigidbody.transform.position;

        ownPos = new Vector2(bodyRigidbody.transform.localPosition.x, bodyRigidbody.transform.localPosition.y);
        ownVel = new Vector2(bodyRigidbody.velocity.x, bodyRigidbody.velocity.y); // change this to ownPos - prevPos *****************
        
        curVel = (curPos - prevPos).magnitude;
        curAccel = Mathf.Lerp(curAccel, (curVel - prevVel), 0.2f);

        avgVel = Mathf.Lerp(avgVel, (curPos - prevPos).magnitude, 0.25f); // OLD

        prevPos = curPos;
        prevVel = curVel;
          
    }

    // *** Condense these into ONE?
    private void TickEgg() {        
        lifeStageTransitionTimeStepCounter++;

        if(isBeingSwallowed) {
            
            //Debug.Log("TickEgg() isBeingSwallowed! " + index.ToString() + " --> " + predatorAgentRef.index.ToString());
            

        }

        sizePercentage = Mathf.Clamp01(((float)lifeStageTransitionTimeStepCounter / (float)gestationDurationTimeSteps) * spawnStartingScale);

        // Scaling Test:
        int frameNum = lifeStageTransitionTimeStepCounter % growthScalingSkipFrames;
        bool resizeCollider = false;
        if(frameNum == 1) {
            resizeCollider = true;
            this.bodyRigidbody.AddForce(UnityEngine.Random.insideUnitCircle * 250f * this.bodyRigidbody.mass * Time.deltaTime, ForceMode2D.Impulse);
        }
        ScaleBody(sizePercentage, resizeCollider);  

        if(isAttachedToParentEggSack) {
            if(parentEggSackRef == null) {
                //  *** eventually look into aborting agents who are attached to EggSacks which don't reach birth ***
            }
            else {
                float embryoPercentage = (float)lifeStageTransitionTimeStepCounter / (float)_GestationDurationTimeSteps;
                float targetDist = Mathf.Lerp(0.01f, parentEggSackRef.fullSize.magnitude * 0.15f, embryoPercentage);
                springJoint.distance = targetDist;

                if(parentEggSackRef.curLifeStage == EggSack.EggLifeStage.Null) {                    
                    isAttachedToParentEggSack = false;
                    springJoint.connectedBody = null;
                    springJoint.enabled = false;
                    colliderBody.enabled = true;
                    parentEggSackRef = null;  
                }
            }            
        }
        else {
            //springJoint.enableCollision = false;
        }
        
        
    }
    private void BeginHatching() {

        springJoint.connectedBody = null;
        springJoint.enabled = false;

        curLifeStage = AgentLifeStage.Mature;
        //Debug.Log("EGG HATCHED!");
        lifeStageTransitionTimeStepCounter = 0;
                
        // Detach from parent EggSack
        parentEggSackRef = null;
        isAttachedToParentEggSack = false;        
                
        colliderBody.enabled = true;

        currentBiomass = settingsRef.agentSettings._BaseInitMass;
        biomassAtDeath = currentBiomass; // avoid divide by 0
        fullsizeBiomass = currentBiomass * 100f;
        coreModule.energy = currentBiomass * 512f;  // should be proportional to body size??
        
        mouthRef.Enable();
        isCooldown = false;

        agentEventDataList.Clear();
        RegisterAgentEvent(UnityEngine.Time.frameCount, "Was Born!", 1f);        
    }
    
    private void TickMature(SimulationManager simManager) {
        mouthRef.isFeeding = this.isFeeding;
        mouthRef.isAttacking = this.isAttacking;
        //ProcessSwallowing();

        if(isSwallowingPrey) {
            //Debug.Log("Holy SH!T a creature was eaten! " + index.ToString() + " --> " + preyAgentRef.index.ToString());
            //mouthRef.BiteCorpseFood(preyAgentRef, currentBiomass * 0.05f);
        }

        // Check for death & stuff? Or is this handled inside OnCollisionEnter() events?
        // Refactor this eventually!!!!
        //sizePercentage = Mathf.Clamp01(((float)ageCounter / (float)maxGrowthPeriod) * (1.0f - spawnStartingScale) + spawnStartingScale);  // *** Revisit this::: ***
        sizePercentage = Mathf.Clamp01(((currentBiomass - settingsRef.agentSettings._BaseInitMass) / fullsizeBiomass) * (1.0f - spawnStartingScale) + spawnStartingScale);

        // Scaling Test:
        int frameNum = ageCounter % growthScalingSkipFrames;
        bool resizeFrame = false;
        if(frameNum == 1) {
            resizeFrame = true;
            GainExperience(0.025f); // Auto Exp for staying alive
        }
        ScaleBody(sizePercentage, resizeFrame);  // change how growth works?

        TickModules(simManager); // update inputs for Brain        
        TickBrain(); // Tick Brain
        TickActions(simManager); // Execute Actions  -- Also Updates Resources!!! ***

        lifeStageTransitionTimeStepCounter++;
        ageCounter++;
                
        if(!isPregnantAndCarryingEggs) {
            pregnancyRefactoryTimeStepCounter++;
        }
        
    }
    public void BeginPregnancy(EggSack developingEggSackRef) {
        //Debug.Log("BeginPregnancy! [" + developingEggSackRef.index.ToString() + "]");

        
        isPregnantAndCarryingEggs = true;
        childEggSackRef = developingEggSackRef;

        // energy transfer within EggSack class
        float starterMass = settingsRef.agentSettings._BaseInitMass * settingsRef.agentSettings._MinPregnancyFactor;
        float curProportion = currentBiomass * settingsRef.agentSettings._MaxPregnancyProportion;
        // probably 0.05 * 2 = 0.1   for now
        if(curProportion > starterMass) { // Good to go!
            //Debug.Log("Pregnancy! " + " curMass: " + currentBiomass.ToString() + ", reqMass: " + starterMass.ToString() + ", curProp: " + curProportion.ToString());
            currentBiomass -= settingsRef.agentSettings._BaseInitMass;
            childEggSackRef.currentBiomass = starterMass;     // *************** TROUBLE!!!

            RegisterAgentEvent(UnityEngine.Time.frameCount, "Pregnant! " + starterMass.ToString(), 0.5f);
        }
        else {
            Debug.LogError("Something went wrong!! " + " curMass: " + currentBiomass.ToString() + ", reqMass: " + starterMass.ToString() + ", curProp: " + curProportion.ToString() );
        }
    }
    public void AbortPregnancy() {
        //childEggSackRef
        if(childEggSackRef != null) {
            childEggSackRef.ParentDiedWhilePregnant();
            childEggSackRef = null;
        }        

        isPregnantAndCarryingEggs = false;
        pregnancyRefactoryTimeStepCounter = 0;
    }
    public void CompletedPregnancy() {
        childEggSackRef = null;
        candidateRef.performanceData.totalTimesPregnant++;
        isPregnantAndCarryingEggs = false;
        pregnancyRefactoryTimeStepCounter = 0;

        RegisterAgentEvent(UnityEngine.Time.frameCount, "Pregnancy Complete!", 0.95f);
    }
    private void TickDead(SettingsManager settings) {
        lifeStageTransitionTimeStepCounter++;

        // DECAY HERE!
        // Should shrink as well as lose foodContent
        float decayAmount = settings.agentSettings._BaseDecompositionRate;

        // include reproductive stockpile energy???

        currentBiomass -= decayAmount;
        float wasteProducedMult = 1f;
        wasteProducedLastFrame += decayAmount * wasteProducedMult;

        if(currentBiomass <= 0f) {
            currentBiomass = 0f;

        }
    }

    private void ScaleBody(float sizePercentage, bool resizeColliders) {
        //segmentFullSizeArray
        //float minScale = 0.2f;
        float scale = Mathf.Lerp(spawnStartingScale, 1f, sizePercentage); // Minimum size = 0.1 ???  // SYNC WITH EGG SIZE!!!
        currentBoundingBoxSize = fullSizeBoundingBox * scale;
        float currentBodyVolume = currentBoundingBoxSize.y * (currentBoundingBoxSize.x + currentBoundingBoxSize.z) * 0.5f; // coreModule.currentBodySize.x * coreModule.currentBodySize.y;

        // REvisit this::::
        //currentBiomass = currentBodyVolume;  // ??? **** DOESN'T LOOK CORRECT -- FIX!!

        coreModule.stomachCapacity = 1f; // currentBodyVolume;
        
        if(resizeColliders) {
            colliderBody.size = new Vector2(currentBoundingBoxSize.x, currentBoundingBoxSize.y); // coreModule.currentBodySize;
            bodyRigidbody.mass = currentBodyVolume; 

            // MOUTH:
            mouthRef.triggerCollider.radius = currentBoundingBoxSize.x * 0.5f;  // ***** REVISIT THIS!!! USE GENOME FOR MOUTH!!! *****
            mouthRef.triggerCollider.offset = new Vector2(0f, currentBoundingBoxSize.y * 0.5f);
        
            // THIS IS HOT GARBAGE !!! RE-FACTOR!! *****
            mouseClickCollider.radius = currentBoundingBoxSize.x * 0.5f + 2f;        
            mouseClickCollider.height = currentBoundingBoxSize.y + 2f;
            mouseClickCollider.center = new Vector3(0f, 0f, -SimulationManager._GlobalWaterLevel * SimulationManager._MaxAltitude);
            //mouseClickCollider.radius += 2f; // ** TEMP -- should be based on camera distance also
            //mouseClickCollider.height += 2f;
        }               
    }

    public void TickActions(SimulationManager simManager) {
       
        AgentActionState currentState = AgentActionState.Default;
        
        float horizontalMovementInput = movementModule.throttleX[0]; // Mathf.Lerp(horAI, horHuman, humanControlLerp);
        float verticalMovementInput = movementModule.throttleY[0]; // Mathf.Lerp(verAI, verHuman, humanControlLerp);
        
        // Facing Direction:
        throttle = new Vector2(horizontalMovementInput, verticalMovementInput);        
        smoothedThrottle = Vector2.Lerp(smoothedThrottle, throttle, smoothedThrottleLerp);
        Vector2 throttleForwardDir = throttle.normalized;
        
        // ENERGY!!!!
        // Digestion:
        float maxDigestionRate = settingsRef.agentSettings._BaseDigestionRate * currentBiomass; // proportional to biomass?
        //float foodToEnergyBaseConversion = 1f; // what should this be?
        float totalStomachContents = (coreModule.stomachContentsPlant + coreModule.stomachContentsMeat + coreModule.stomachContentsDecay);
        Vector3 foodProportionsVec = new Vector3(coreModule.stomachContentsPlant, coreModule.stomachContentsMeat, coreModule.stomachContentsDecay) / (totalStomachContents + 0.000001f);

        float digestedAmountTotal = Mathf.Min(totalStomachContents, maxDigestionRate);

        float totalStomachContentsNorm = (coreModule.stomachContentsPlant + coreModule.stomachContentsMeat + coreModule.stomachContentsDecay) / coreModule.stomachCapacity;
        coreModule.stomachContentsNorm = totalStomachContentsNorm; // ** we'll see.... ***
                                                                   //float digestedAmountTotal = Mathf.Min(totalStomachContentsNorm, maxDigestionRate);
                                                                   //float digestedProportionOfTotalContents = digestedAmountTotal / (totalStomachContentsNorm + 0.000001f);

        // *** Remember to Re-Implement dietary specialization!!! ****
        float digestedPlantMass = digestedAmountTotal * foodProportionsVec.x;
        float plantToEnergyAmount = digestedPlantMass; // * coreModule.foodEfficiencyPlant;
        float digestedMeatMass = digestedAmountTotal * foodProportionsVec.y;
        float meatToEnergyAmount = digestedMeatMass; // * coreModule.foodEfficiencyMeat;    
        float digestedDecayMass = digestedAmountTotal * foodProportionsVec.z;
        float decayToEnergyAmount = digestedDecayMass;
        
        float createdEnergyTotal = (plantToEnergyAmount * coreModule.dietSpecPlantNorm + meatToEnergyAmount * coreModule.dietSpecMeatNorm + decayToEnergyAmount * coreModule.dietSpecDecayNorm) * settingsRef.agentSettings._DigestionEnergyEfficiency;
        
        wasteProducedLastFrame += digestedAmountTotal * settingsRef.agentSettings._DigestionWasteEfficiency;
        oxygenUsedLastFrame = currentBiomass * settingsRef.agentSettings._BaseOxygenUsage;
        currentBiomass += digestedAmountTotal * settingsRef.agentSettings._GrowthEfficiency;  // **** <-- Reconsider
        if(currentBiomass > fullsizeBiomass) {
            wasteProducedLastFrame += (currentBiomass - fullsizeBiomass);
            currentBiomass = fullsizeBiomass;
        }
                
        coreModule.stomachContentsPlant -= digestedPlantMass;
        if(coreModule.stomachContentsPlant < 0f) {
            coreModule.stomachContentsPlant = 0f;
        }
        coreModule.stomachContentsMeat -= digestedMeatMass;
        if(coreModule.stomachContentsMeat < 0f) {
            coreModule.stomachContentsMeat = 0f;
        }
        coreModule.stomachContentsDecay -= digestedDecayMass;
        if(coreModule.stomachContentsDecay < 0f) {
            coreModule.stomachContentsDecay = 0f;
        }

        // Heal:  // *** Re-Implement !!! ***************
        float healRate = 0.0005f;
        float energyToHealthConversionRate = 5f * coreModule.healthBonus;
        if(coreModule.healthBody < 1f) {
            coreModule.healthBody += healRate;
            coreModule.healthHead += healRate;
            coreModule.healthExternal += healRate;

            coreModule.energy -= healRate / energyToHealthConversionRate;
        }

        float oxygenMask = Mathf.Clamp01(simManager.simResourceManager.curGlobalOxygen * settingsRef.agentSettings._OxygenEnergyMask);
        
        coreModule.energy += createdEnergyTotal * settingsRef.agentSettings._DigestionEnergyEfficiency * oxygenMask;
        /*
        // STAMINA:
        float staminaRefillRate = 0.00025f;
        float energyToStaminaConversionRate = 5f * coreModule.healthBonus;
        coreModule.stamina[0] += staminaRefillRate * energyToStaminaConversionRate;
        coreModule.energy -= staminaRefillRate; // / energyToStaminaConversionRate;

        
        if(coreModule.stamina[0] < 0.1f) {
            staminaRefillRate *= 0.5f;
        }
        if(coreModule.stamina[0] > 0.75f) {
            staminaRefillRate *= 1.5f;
        }
        if(isResting) {
            staminaRefillRate *= 5f;
        }
        if(coreModule.stamina[0] < 1f) {
            coreModule.stamina[0] += staminaRefillRate;
            coreModule.energy -= staminaRefillRate / energyToStaminaConversionRate;
        }
        else {
            coreModule.stamina[0] = 1f;
        }
        */
        //ENERGY:
        float energyCostMult = 0.1f; // Mathf.Lerp(settingsRef.agentSettings._BaseEnergyCost, settingsRef.agentSettings._BaseEnergyCost * 0.25f, sizePercentage);
        float restingBonusMult = 1f;
        if(isResting) {
            restingBonusMult = 0.65f;
        }
        
        float energyCost = Mathf.Sqrt(currentBiomass) * energyCostMult * restingBonusMult; // * SimulationManager.energyDifficultyMultiplier; // / coreModule.energyBonus;
        
        float throttleMag = smoothedThrottle.magnitude;
        
        // ENERGY DRAIN::::
        coreModule.energy -= energyCost;
        if(coreModule.energy < 0f) {
            coreModule.energy = 0f;
        }

        if(isDead || isEgg) {
            throttle = Vector2.zero;
            smoothedThrottle = Vector2.zero;
        }
        else {
            bool startBite = false;
            // Food calc before energy/healing/etc? **************
            //float sizeValue = BodyGenome.GetBodySizeScore01(candidateRef.candidateGenome.bodyGenome);
            // FOOD PARTICLES: Either mouth type for now:
            float foodParticleEatAmount = simManager.vegetationManager.plantParticlesEatAmountsArray[index] * coreModule.foodEfficiencyPlant; // **************** PLANT BONUS!! HACKY
            if(foodParticleEatAmount > 0f) {
                //mouthRef.InitiatePassiveBite();
                //float sizeEfficiencyPlant = Mathf.Lerp(settings.minSizeFeedingEfficiencyDecay, settings.maxSizeFeedingEfficiencyDecay, sizeValue);
                startBite = true;
                //Debug.Log("Agent[" + index.ToString() + "], Ate Plant: " + foodParticleEatAmount.ToString());
                candidateRef.performanceData.totalFoodEatenPlant += foodParticleEatAmount; 
                EatFoodPlant(foodParticleEatAmount);                
            }

            float animalParticleEatAmount = simManager.zooplanktonManager.animalParticlesEatAmountsArray[index] * coreModule.foodEfficiencyMeat;
            if(animalParticleEatAmount > 0f) {
                //float sizeEfficiencyPlant = Mathf.Lerp(settings.minSizeFeedingEfficiencyDecay, settings.maxSizeFeedingEfficiencyDecay, sizeValue);
                candidateRef.performanceData.totalFoodEatenZoop += animalParticleEatAmount;
                //animalParticleEatAmount *= 0.98f;
                
                //Debug.Log("Agent[" + index.ToString() + "], Ate Zooplankton: " + animalParticleEatAmount.ToString());
                EatFoodMeat(animalParticleEatAmount); // * sizeEfficiencyPlant);    
                RegisterAgentEvent(UnityEngine.Time.frameCount, "Ate Zooplankton! (+" + (animalParticleEatAmount * 1000).ToString("F0").ToString() + " food)", 1f);
                startBite = true;
            }

            mouthRef.lastBiteFoodAmount += foodParticleEatAmount + animalParticleEatAmount;

            if(startBite) {
                //if(IsFreeToAct()) {
                if(!isAttacking && !isDefending) {
                    //if (coreModule.mouthFeedEffector[0] >= 0f) {   //  needed?
                    AttemptInitiateActiveFeedBite();
                    //}
                }                
            }
           
            // *** REFACTOR THIS GARBAGE!!!!! ********
            float mostActiveEffectorVal = 0f;
            mostActiveEffectorVal = Mathf.Max(mostActiveEffectorVal, coreModule.mouthFeedEffector[0]);
            mostActiveEffectorVal = Mathf.Max(mostActiveEffectorVal, coreModule.mouthAttackEffector[0]);
            mostActiveEffectorVal = Mathf.Max(mostActiveEffectorVal, coreModule.defendEffector[0]);
            mostActiveEffectorVal = Mathf.Max(mostActiveEffectorVal, coreModule.dashEffector[0]);
            mostActiveEffectorVal = Mathf.Max(mostActiveEffectorVal, coreModule.healEffector[0]);

            if(coreModule.mouthAttackEffector[0] >= mostActiveEffectorVal) {
                if (IsFreeToAct()) {
                    AttemptInitiateActiveAttackBite();      
                }
                          
            }
            if(coreModule.dashEffector[0] >= mostActiveEffectorVal) {
                if (IsFreeToAct()) {
                    ActionDash();
                }
            }
            if(coreModule.defendEffector[0] >= mostActiveEffectorVal) {
                if(IsFreeToAct()) {
                    ActionDefend();
                }
            }            
            
            if(coreModule.healEffector[0] >= mostActiveEffectorVal) {
                if(IsFreeToAct()) {
                    isResting = true;
                    candidateRef.performanceData.totalTicksRested++;
                }
                else {
                    isResting = false;
                }
            }                    
        }
        
        ApplyPhysicsForces(smoothedThrottle);

        float rotationInRadians = (bodyRigidbody.transform.localRotation.eulerAngles.z + 90f) * Mathf.Deg2Rad;
        facingDirection = new Vector2(Mathf.Cos(rotationInRadians), Mathf.Sin(rotationInRadians));
                
        curActionState = currentState;

        
        if(isDashing) {
            dashFrameCounter++;
            if(dashFrameCounter >= dashDuration) {                
                isDashing = false;
                EnterCooldown(dashCooldown);
                dashFrameCounter = 0;
            }
        }
        
        if(isDefending) {
            defendFrameCounter++;
            if (defendFrameCounter >= defendDuration) {                                
                isDefending = false;
                EnterCooldown(defendCooldown);
                defendFrameCounter = 0;
            }
        }

        if(isFeeding) {            
            feedingFrameCounter++;
            if(feedingFrameCounter > feedAnimDuration) {                
                isFeeding = false;
                EnterCooldown(feedAnimCooldown);
                feedingFrameCounter = 0;

                // EVENT HERE!!!!!
                //if(mouthRef.lastBiteFoodAmount > 0f) {
                    //RegisterAgentEvent(UnityEngine.Time.frameCount, "Ate Plant/Meat! (" + mouthRef.lastBiteFoodAmount.ToString("F3") + ")");
                //}
            }
        }
        if(isAttacking) {            
            attackingFrameCounter++;
            if(attackingFrameCounter > attackAnimDuration) {
                isAttacking = false;
                EnterCooldown(attackAnimCooldown);
                attackingFrameCounter = 0;
            }
        }

        if(isCooldown) {
            cooldownFrameCounter++;
            if(cooldownFrameCounter >= cooldownDuration) {
                cooldownFrameCounter = 0;
                isCooldown = false;
            }
        }
        
    }

    private void EnterCooldown(int frames) {
        isCooldown = true;
        cooldownDuration = frames;
        cooldownFrameCounter = 0;
    }

    private void ActionDash() {
        if(IsFreeToAct()) {
            if(coreModule.stamina[0] >= 0.1f) {
                isDashing = true;
                coreModule.stamina[0] -= 0.1f;
                candidateRef.performanceData.totalTimesDashed++;
            }            
        } 
    }
    private void ActionDefend() {
        if(IsFreeToAct()) {
            if(coreModule.stamina[0] >= 0.1f) {
                isDefending = true;
                coreModule.stamina[0] -= 0.1f;
                candidateRef.performanceData.totalTimesDefended++;
            }            
        } 
    }
    
    private bool IsFreeToAct() {
        bool isFree = true;

        if(isCooldown) {
            isFree = false;
        }
        if(isDashing) {
            isFree = false;
        }
        if(isDefending) {
            isFree = false;
        }
        if(isFeeding) {
            isFree = false;
        }
        if(isAttacking) {
            isFree = false;
        }
        if(curLifeStage != AgentLifeStage.Mature) {
            isFree = false;
        }

        return isFree;
    }

    private void ApplyPhysicsForces(Vector2 throttle) {
        
        MovementScalingTest(throttle);
    }
    
    private void MovementScalingTest(Vector2 throttle) {
        // Save current joint angles:
        //for(int j = 0; j < numSegments - 1; j++) {
        //    jointAnglesArray[j] = hingeJointsArray[j].jointAngle;            
        //}

        float bitingPenalty = 1f;
        
        if(isFeeding)
        {
            bitingPenalty = 0.5f;
        }
        /*if(coreModule.mouthFeedEffector[0] > 0f)  // Clean up code for State-machine-esque behaviors/abilities
        {
            bitingPenalty = 0.5f;
        }*/

        float forcePenalty = 1f;
        if(isResting) {
            forcePenalty = 0.1f;
        }

        float fatigueMultiplier = Mathf.Clamp01(coreModule.energy * 5f + 0.05f); // * Mathf.Clamp01(coreModule.stamina[0] * 4f + 0.05f);
        float lowHealthPenalty = Mathf.Clamp01(coreModule.healthBody * 5f) * 0.5f + 0.5f;
        fatigueMultiplier *= lowHealthPenalty;
        
        turningAmount = Mathf.Lerp(turningAmount, this.bodyRigidbody.angularVelocity * Mathf.Deg2Rad * 0.03f, 0.28f);

        animationCycle += smoothedThrottle.magnitude * swimAnimationCycleSpeed * fatigueMultiplier * forcePenalty; // (Mathf.Lerp(fullSizeBoundingBox.y, 1f, 1f)

        if (throttle.sqrMagnitude > 0.000001f) {  // Throttle is NOT == ZERO
            
            Vector2 headForwardDir = new Vector2(this.bodyRigidbody.transform.up.x, this.bodyRigidbody.transform.up.y).normalized;
            Vector2 headRightDir =  new Vector2(this.bodyRigidbody.transform.right.x, this.bodyRigidbody.transform.right.y).normalized;
            Vector2 throttleDir = throttle.normalized;

            float turnSharpness = (-Vector2.Dot(throttleDir, headForwardDir) * 0.5f + 0.5f);
            float headTurn = Vector2.Dot(throttleDir, headRightDir) * -1f * turnSharpness;
            float headTurnSign = Mathf.Clamp(Vector2.Dot(throttleDir, headRightDir) * -10000f, -1f, 1f);
              
            // get size in 0-1 range from minSize to maxSize: // **** NOT ACCURATE!!!!
            //float sizeValue = Mathf.Clamp01(coreModule.speedBonus * (candidateRef.candidateGenome.bodyGenome.coreGenome.creatureBaseLength - 0.2f) / 2f);  // Mathf.Clamp01((fullSizeBoundingBox.x - 0.1f) / 2.5f); // ** Hardcoded assuming size ranges from 0.1 --> 2.5 !!! ********

            float swimSpeed = 48f * coreModule.speedBonus; // Mathf.Lerp(movementModule.smallestCreatureBaseSpeed, movementModule.largestCreatureBaseSpeed, 0.5f); // sizeValue);
            float turnRate = 6f * coreModule.speedBonus; //10 // Mathf.Lerp(movementModule.smallestCreatureBaseTurnRate, movementModule.largestCreatureBaseTurnRate, 0.5f) * 0.1f; // sizeValue);
            float dashBonus = 1f;
            if(isDashing) {                
                dashBonus = 4.3f;                
            }
            if(isCooldown) {
                dashBonus = 0.33f;
            }


            speed = swimSpeed * dashBonus * forcePenalty; // * movementModule.speedBonus ; // * restingPenalty;
            
            Vector2 segmentForwardDir = new Vector2(this.bodyRigidbody.transform.up.x, this.bodyRigidbody.transform.up.y).normalized;

            Vector2 forwardThrustDir = Vector2.Lerp(segmentForwardDir, throttleDir, 0.1f).normalized;

            this.bodyRigidbody.AddForce(forwardThrustDir * (1f - turnSharpness * 0.25f) * speed * this.bodyRigidbody.mass * Time.deltaTime * fatigueMultiplier * bitingPenalty, ForceMode2D.Impulse);

            // modify turning rate based on body proportions:
            //float turnRatePenalty = Mathf.Lerp(0.25f, 1f, 1f - sizeValue);

            // Head turn:
            float torqueForce = Mathf.Lerp(headTurn, headTurnSign, 0.35f) * forcePenalty * turnRate * this.bodyRigidbody.mass * fatigueMultiplier * bitingPenalty * Time.deltaTime;
            torqueForce = Mathf.Min(torqueForce, 50000.55f) * 3f;
            this.bodyRigidbody.AddTorque(torqueForce, ForceMode2D.Impulse);
            
        }
    }

    public void GainExperience(float exp) {
        totalExperience += exp;
        if(totalExperience >= experienceForNextLevel) {
            GainLevel();
        }
    }
    public void GainLevel() {
        curLevel++;
        experienceForNextLevel = experienceForNextLevel * 2f;
    }

    public void InitializeModules(AgentGenome genome) {

        communicationModule = new CritterModuleCommunication();
        communicationModule.Initialize(genome.bodyGenome.communicationGenome, this);

        coreModule = new CritterModuleCore();
        coreModule.Initialize(genome.bodyGenome.coreGenome, this);

        mouthRef.Initialize(genome.bodyGenome.coreGenome, this);

        environmentModule = new CritterModuleEnvironment();
        environmentModule.Initialize(genome.bodyGenome.environmentalGenome, this);

        foodModule = new CritterModuleFood();
        foodModule.Initialize(genome.bodyGenome.foodGenome, this);

        friendModule = new CritterModuleFriends();
        friendModule.Initialize(genome.bodyGenome.friendGenome, this);

        movementModule = new CritterModuleMovement();
        movementModule.Initialize(genome, genome.bodyGenome.movementGenome);

        threatsModule = new CritterModuleThreats();
        threatsModule.Initialize(genome.bodyGenome.threatGenome, this);
    }
    
    public void FirstTimeInitialize(SettingsManager settings) {  //AgentGenome genome) {  // ** See if I can get away with init sans Genome
        this.settingsRef = settings;
        curLifeStage = AgentLifeStage.AwaitingRespawn;
        //InitializeAgentWidths(genome);
        InitializeGameObjectsAndComponents();
        //InitializeModules(genome);  //  This breaks MapGridCell update, because coreModule doesn't exist?
    }
    private void InitializeGameObjectsAndComponents() {
        // Create Physics GameObject:
        if(bodyGO == null) {
            GameObject bodySegmentGO = new GameObject("RootSegment" + index.ToString());
            bodySegmentGO.transform.parent = this.gameObject.transform;            
            bodySegmentGO.tag = "LiveAnimal";
            bodyGO = bodySegmentGO;
            //bodyCritterSegment = bodySegmentGO.AddComponent<CritterSegment>();
            bodyRigidbody = bodySegmentGO.AddComponent<Rigidbody2D>();
            colliderBody = bodyGO.AddComponent<CapsuleCollider2D>();            
            //bodyCritterSegment.segmentCollider = colliderBody;
            bodyRigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            springJoint = bodyGO.AddComponent<SpringJoint2D>();
            springJoint.enabled = false;
            springJoint.autoConfigureDistance = false;
            springJoint.distance = 0f;
            springJoint.dampingRatio = 0.1f;
            springJoint.frequency = 15f;

            GameObject testMouthGO = new GameObject("Mouth");
            testMouthGO.transform.parent = bodyGO.transform;
            testMouthGO.transform.localPosition = Vector3.zero;
            mouthRef = testMouthGO.AddComponent<CritterMouthComponent>();
            CircleCollider2D mouthTrigger = testMouthGO.AddComponent<CircleCollider2D>();
            mouthRef.triggerCollider = mouthTrigger;

            GameObject mouseClickColliderGO = new GameObject("MouseClickCollider");
            mouseClickColliderGO.transform.parent = bodyGO.transform;
            mouseClickColliderGO.transform.localPosition = new Vector3(0f, 0f, 0f);
            mouseClickCollider = mouseClickColliderGO.AddComponent<CapsuleCollider>();
            mouseClickCollider.isTrigger = true;
        }
    }
    
    // Colliders Footprint???  *************************************************************************************************************

    public void ReconstructAgentGameObjects(SettingsManager settings, AgentGenome genome, EggSack parentEggSack, Vector3 startPos, bool isImmaculate, float waterLevel) {
        //float corpseLerp = (float)settings.curTierFoodCorpse / 10f;
        //decayDurationTimeSteps = 480; // Mathf.RoundToInt(Mathf.Lerp(360f, 3600f, corpseLerp));
        //float eggLerp = (float)settings.curTierFoodEgg / 10f;
        //gestationDurationTimeSteps = Mathf.RoundToInt(Mathf.Lerp(360f, 1080f, eggLerp));
        //pregnancyRefactoryDuration = Mathf.RoundToInt(Mathf.Lerp(3600f, 800f, eggLerp));

        //InitializeAgentWidths(genome);
        InitializeGameObjectsAndComponents();  // Not needed??? ***

        //genome.bodyGenome.CalculateFullsizeBoundingBox();
        //Debug.Log("fullSize = " + genome.bodyGenome.fullsizeBoundingBox.ToString() + ", head: " + genome.bodyGenome.coreGenome.headLength.ToString());
        fullSizeBoundingBox = genome.bodyGenome.GetFullsizeBoundingBox(); // genome.bodyGenome.GetFullsizeBoundingBox();
        fullSizeBodyVolume = (fullSizeBoundingBox.x + fullSizeBoundingBox.z) * 0.5f * fullSizeBoundingBox.y; // * fullSizeBoundingBox.z;

        sizePercentage = 0.005f;

        // Positioning and Pinning to parentEggSack HERE:
        bodyRigidbody.isKinematic = true;
        bodyRigidbody.simulated = false;

        bodyGO.transform.position = startPos;  //old//
        bodyRigidbody.MovePosition(startPos);

        bodyRigidbody.isKinematic = false;
        bodyRigidbody.simulated = true;

        //springJoint.distance = 0.005f;
        //springJoint.enableCollision = false;        
        //springJoint.frequency = 15f;

        if(isImmaculate) {            
            springJoint.connectedBody = null; // parentEggSack.rigidbodyRef;
            springJoint.enabled = false;
            isAttachedToParentEggSack = false;

            colliderBody.enabled = true;
        }
        else {
            //bodyGO.transform.localPosition = parentEggSack.gameObject.transform.position; // startPos.startPosition;        
            //springJoint.connectedBody = parentEggSack.rigidbodyRef;
            //springJoint.enabled = true;
            isAttachedToParentEggSack = true;

            colliderBody.enabled = false;
        }                
            
        bodyRigidbody.mass = 0.01f; // min mass
        bodyRigidbody.drag = 13.75f; // bodyDrag;
        bodyRigidbody.angularDrag = 15.5f;
        
        // Collision!
        colliderBody.direction = CapsuleDirection2D.Vertical;
        colliderBody.size = new Vector2(fullSizeBoundingBox.x, fullSizeBoundingBox.y) * sizePercentage;  // spawn size percentage 1/10th  

        // Mouth Trigger:
        //mouthRef.isPassive = genome.bodyGenome.coreGenome.isPassive;
        mouthRef.triggerCollider.isTrigger = true;
        mouthRef.triggerCollider.radius = fullSizeBoundingBox.x / 2f * sizePercentage;
        mouthRef.triggerCollider.offset = new Vector2(0f, fullSizeBoundingBox.y / 2f * sizePercentage);
        isFeeding = false;
        feedingFrameCounter = 0;
        isAttacking = false;
        attackingFrameCounter = 0;
        //mouthRef.isCooldown = false;
        mouthRef.agentIndex = this.index;
        mouthRef.agentRef = this;
        isResting = false;
        isDefending = false;
        isDashing = false;



        //mouthRef.Disable();

        //mouseclickcollider MCC
        mouseClickCollider.direction = 1; // Y-Axis ???
        mouseClickCollider.center = new Vector3(0f, 0f, (waterLevel * 2f - 1f) * -10f); //Vector3.zero; // new Vector3(0f, 0f, 1f);
        mouseClickCollider.radius = fullSizeBoundingBox.x / 2f * sizePercentage;
        mouseClickCollider.radius *= 5.14f; // ** TEMP
        mouseClickCollider.height = fullSizeBoundingBox.y / 2f * sizePercentage;
    }

    private void ResetStartingValues() {
        stringCauseOfDeath = "alive";

        animationCycle = 0f;
        lifeStageTransitionTimeStepCounter = 0;
        pregnancyRefactoryTimeStepCounter = 0;
        ageCounter = 0;
        sizePercentage = 0f;
        consumptionScore = 0f;
        damageDealtScore = 0f;
        damageReceivedScore = 0f;
        energyEfficiencyScore = 0f;
        reproductionScore = 0f;
        supportScore = 0f;
        masterFitnessScore = 0f;
        totalExperience = 0f;
        experienceForNextLevel = 2f; // 2, 4, 8, 16, 32, 64, 128, 256?
        curLevel = 0;
        
        turningAmount = 5f; // temporary for zygote animation
        facingDirection = new Vector2(0f, 1f);
        throttle = Vector2.zero;
        smoothedThrottle = new Vector2(0f, 0.01f); 
    }

    //  When should biomass be transferred from EggSack?
    // If spawnImmaculate, where does the biomass come from? -- should it be free?
    //  

    public void InitializeSpawnAgentImmaculate(SettingsManager settings, int agentIndex, CandidateAgentData candidateData, Vector3 spawnWorldPos, float globalWaterLevel) {        
        index = agentIndex;
        speciesIndex = candidateData.speciesID;
        candidateRef = candidateData;
        AgentGenome genome = candidateRef.candidateGenome;
        //genome.generationCount++;
        
        curLifeStage = AgentLifeStage.Egg;
        
        parentEggSackRef = null;

        // **** Separate out this code into shared function to avoid duplicate code::::
        ResetStartingValues();
        
        InitializeModules(genome);      // Modules need to be created first so that Brain can map its neurons to existing modules  
        
        // Upgrade this to proper Pooling!!!!
        ReconstructAgentGameObjects(settings, genome, null, spawnWorldPos, true, globalWaterLevel);

        brain = new Brain(genome.brainGenome, this); 
        
        isInert = false;
    }
    public void InitializeSpawnAgentFromEggSack(SettingsManager settings, int agentIndex, CandidateAgentData candidateData, EggSack parentEggSack, float globalWaterLevel) {        
        index = agentIndex;
        speciesIndex = candidateData.speciesID;
        candidateRef = candidateData;
        AgentGenome genome = candidateRef.candidateGenome;
        //genome.generationCount++;
                
        curLifeStage = AgentLifeStage.Egg;
        parentEggSackRef = parentEggSack;
        
        ResetStartingValues();
                
        InitializeModules(genome);      // Modules need to be created first so that Brain can map its neurons to existing modules  

        // Upgrade this to proper Pooling!!!!
        Vector3 spawnOffset = UnityEngine.Random.insideUnitSphere * parentEggSack.curSize.magnitude * 0.167f;
        spawnOffset.z = 0f;
        ReconstructAgentGameObjects(settings, genome, parentEggSack, parentEggSack.gameObject.transform.position + spawnOffset, false, globalWaterLevel);

        brain = new Brain(genome.brainGenome, this);   
        
        isInert = false;
    }

}
