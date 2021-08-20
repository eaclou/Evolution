using UnityEngine;

public class Agent : MonoBehaviour {
    Lookup lookup => Lookup.instance;
    SettingsManager settingsRef => SettingsManager.instance;
    SimulationManager simManager => SimulationManager.instance;
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

    public bool isInert = true;  // when inert, colliders disabled // is this still used??
    // Refactor??
    public bool isActing = false;  // biting, defending, dashing, etc -- exclusive actions    
    public bool isDecaying = false;

    public bool isFeeding => feed.inProcess;
    public bool isAttacking => attack.inProcess;
    
    // Flag for intention to eat gpu food particle (plant-type) 
    public bool isFreeToEat => isFeeding && !isDefending && !isCooldown && feedingFrameCounter <= 4;

    //***EC eventually move these into creature genome, make variable
    public int feedAnimDuration = 30;  
    public int feedAnimCooldown = 30;
    public int attackAnimDuration = 40;
    public int attackAnimCooldown = 75;
    public int feedingFrameCounter => feed.frameCount;
    public int attackingFrameCounter => attack.frameCount;
    
    public bool isDashing => dash.inProcess;
    public int dashFrameCounter => dash.frameCount;
    public int dashDuration = 40;
    public int dashCooldown = 120;

    public bool isDefending => defend.inProcess;
    public int defendFrameCounter => defend.frameCount;
    public int defendDuration = 60;
    public int defendCooldown = 60;

    public bool isResting = false;  // * WPP: same functionality as isFreeToAct -> pick one to use and delete other
    public bool isCooldown => cooldown.inProcess;

    public int cooldownFrameCounter => cooldown.frameCount;
    public int cooldownDuration = 60;  // arbitrary default

    public bool isMarkedForDeathByUser = false;

    public int index;    
    public int speciesIndex = -1;  // Set these at birth!
    public CandidateAgentData candidateRef;

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
    public GameObject bodyGO;   // * WPP: Remove, just move the agent
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
    
    public float xyBoundArea => currentBoundingBoxSize.x * currentBoundingBoxSize.y;
    
    // * Combine thse stats into a serializable class for cleanliness?
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
    
    public bool pregnancyRefactoryComplete => pregnancyRefactoryTimeStepCounter > pregnancyRefactoryDuration;

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
    public int swallowDuration = 60; // * how does this mix with mouseComponent???? 
    public Agent predatorAgentRef;
    public Agent preyAgentRef;

    public EggSack parentEggSackRef;  // instead of using own fixed embry development duration - look up parentEggSack and use its counter?
    public bool isAttachedToParentEggSack = false;

    public EggSack childEggSackRef;
    public bool isPregnantAndCarryingEggs = false;
    public int pregnancyRefactoryDuration = 2400;
    
    IAgentAbility activeAbility;
    IAgentAbility attack;
    IAgentAbility dash;
    IAgentAbility defend;
    IAgentAbility feed;
    Cooldown cooldown;
    
    public float totalEaten => candidateRef.performanceData.totalEaten;
    //public float overflowFoodAmount = 0f;
        
    private void Awake() {        
        // temp fix for delayed spawning of Agents (leading to nullReferenceExceptions)
        //agentWidthsArray = new float[widthsTexResolution];
        isInert = true;
    }
    
    void Start()
    {
        attack = new AttackOverTime(this, attackAnimDuration, attackAnimCooldown, EnterCooldown); 
        dash = new Dash(this, dashDuration, dashCooldown, EnterCooldown);
        defend = new Defend(this, defendDuration, defendCooldown, EnterCooldown);
        feed = new Feed(this, feedAnimDuration, feedAnimCooldown, EnterCooldown);
        cooldown = new Cooldown();
    }
    
    void EnterCooldown(int duration) 
    {
        cooldown.duration = duration; 
        UseAbility(cooldown);
    }
    
    public bool isDead => curLifeStage == AgentLifeStage.Dead;
    public bool isEgg => curLifeStage == AgentLifeStage.Egg;
    public bool isMature => curLifeStage == AgentLifeStage.Mature;
    public bool isNull => curLifeStage == AgentLifeStage.Null;
    public bool isAwaitingRespawn => curLifeStage == AgentLifeStage.AwaitingRespawn;

    public float GetDecayPercentage() {
        if (biomassAtDeath == 0f) {
            //Debug.LogError("Biomass at death zero for " + index);
            return 0f;
        }
        if (!isDead) {
            Debug.LogError("I'm not dead yet! " + index + " " + curLifeStage);
            return 0f;
        }
    
        float percentage = 1f - currentBiomass / biomassAtDeath;
        
        return Mathf.Clamp01(percentage);
    }
    
    public bool isAttackBiteFrame => attackingFrameCounter == attackAnimDuration / 2;
    public float attackAnimCycle => Mathf.Clamp01((float)attackingFrameCounter / attackAnimDuration);
    
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

        InitializeDeath(lookup.GetCauseOfDeath(CauseOfDeathId.SwallowedWhole));

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

        float foodAmount = preyAgent.currentBiomass * 1000f * coreModule.digestEfficiencyMeat;  // experiment!
        candidateRef.performanceData.totalFoodEatenCreature += foodAmount;

        EatFoodMeat(foodAmount);
        RegisterAgentEvent(SimulationManager.instance.simAgeTimeSteps, "Ate Vertebrate! (" + (foodAmount * 100f).ToString("F0") + ") candID: " + preyAgent.candidateRef.candidateID, 1f, 0);
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
    
    // Updates internal state of body - i.e health, energy etc. -- updates input Neuron values!!!
    // Update Stocks & Flows ::: new health, energy, stamina
    // This should have happened during last frame's Internal PhysX Update
    // HOWEVER, some updates only happen once per frame and can be handled here, like converting food into energy automatically
    // Turns out that most of these updates are updating input neurons which tend to be sensors
    // These values are sometimes raw attributes & sometimes processed data
    // Should I break them up into individual sensor types -- like Ears, Collider Rangefind, etc.?
    // Separate sensors for each target type or add multiple data types to rangefinder raycasts?
    public void TickModules(SimulationManager simManager) 
    {
        //UpdateInternalResources();  // update energy, stamina, food -- or do this during TickActions?
               
        // * WPP: iterate through an array of interfaces
        coreModule.Tick();
        communicationModule.Tick(this);
        environmentModule.Tick(this);
        foodModule.Tick(this);
        friendModule.Tick(this);
        movementModule.Tick(this);
        threatsModule.Tick(this);
        // Add more sensor Modules later:

        mouthRef.Tick();
    }
    
    // STARVATION
    private void CheckForDeathStarvation() {
        if (coreModule.energy > 0f)
            return;
        
        if (coreModule.stomachEmpty)
            InitializeDeath(lookup.GetCauseOfDeath(CauseOfDeathId.Starved));
        else
            InitializeDeath("Suffocated", "Suffocated! stomachContentsNorm: " + coreModule.stomachContentsPercent);
    }
    
    // HEALTH FAILURE:
    public void CheckForDeathHealth() {
        if (coreModule.health > 0f)
            return;
        
        coreModule.SetAllHealth(0f);

        //Debug.LogError("CheckForDeathHealth" + currentBiomass.ToString());

        InitializeDeath(lookup.GetCauseOfDeath(CauseOfDeathId.Injuries));    
    }
    
    private void CheckForDeathOldAge() {
        if(ageCounter > maxAgeTimeSteps)
            InitializeDeath(lookup.GetCauseOfDeath(CauseOfDeathId.OldAge));    
    }
    
    private void CheckForDeathDivineJudgment() {
        if(isMarkedForDeathByUser)   
            InitializeDeath(lookup.GetCauseOfDeath(CauseOfDeathId.DivineJudgment));    
    }
    
    private void InitializeDeath(CauseOfDeathSO data) {
        InitializeDeath(data.causeOfDeath, data.eventMessage);
    }
    
    private void InitializeDeath(string causeOfDeath, string deathEvent) 
    {
        curLifeStage = AgentLifeStage.Dead;
        lifeStageTransitionTimeStepCounter = 0;
        candidateRef.causeOfDeath = causeOfDeath;
        
        // * Swallowed Whole did not call RegisterAgentEvent -> mistake?
        if (deathEvent != "")
            RegisterAgentEvent(SimulationManager.instance.simAgeTimeSteps, deathEvent, 0f, 4);

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
        candidateRef.performanceData.timeStepDied = SimulationManager.instance.simAgeTimeSteps;
        biomassAtDeath = currentBiomass;
        mouthRef.Disable();
    }
    
    // *** WPP: trigger state changes & processes when conditions met
    // rather than polling in the update loop -> efficiency and natural flow
    // (would also eliminate conditional)
    private void CheckForLifeStageTransition() {
        switch(curLifeStage) {
            case AgentLifeStage.AwaitingRespawn:
                break;
            case AgentLifeStage.Egg:
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

                CheckForMaturity();
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
                Debug.LogError("NO SUCH ENUM ENTRY IMPLEMENTED, YOU FOOL!!! (" + curLifeStage + ")");
                break;
        }
    }
    
    public void RegisterAgentEvent(int frame, string textString, float goodness, int type) {
        candidateRef.RegisterCandidateEvent(frame, textString, goodness, type);        
    }
    
    public void EatFoodPlant(float amount) {           
        amount = Mathf.Min(amount, coreModule.stomachSpace);
        
        //amount *= coreModule.foodEfficiencyPlant;
        //coreModule.stomachContentsTotal01 += (amount / coreModule.stomachCapacity);
        
        // *** delegate
        // Put food in stomach
        // unusual that a different variable would increment than the one being checked
        // Consider clamping
        if(coreModule.isFull) {
            return;
            //float overStuffAmount = coreModule.stomachContentsNorm - 1f;
            //ProcessDamageReceived(overStuffAmount);
            //coreModule.stomachContentsPercent = 1f;            
        }

        coreModule.stomachContentsPlant += amount;

        // Exp for appropriate food
        GainExperience((amount / coreModule.stomachCapacity) * coreModule.digestEfficiencyPlant * 1f);     

        //Debug.Log("EatFoodPlant " + amount.ToString());
        RegisterAgentEvent(SimulationManager.instance.simAgeTimeSteps, "Ate Plant! (+" + (amount * 100).ToString("F0") + " food)", 1f, 0);
    }
    
    // * WPP: combine below two methods (were extracted from CritterMouthComponent)
    public void EatEggs(float amount)
    {
        candidateRef.performanceData.totalFoodEatenEgg += amount;
        EatFoodMeat(amount); // assumes all foodAmounts are equal
        RegisterAgentEvent(SimulationManager.instance.simAgeTimeSteps, "Ate Egg Bit! (" + (amount * 100f).ToString("F0") + ")", 1f, 0);
    }
    
    public void EatEggsWhole(float amount)
    {
        candidateRef.performanceData.totalFoodEatenEgg += amount;
        EatFoodDecay(amount);
        RegisterAgentEvent(SimulationManager.instance.simAgeTimeSteps, "Ate Egg! (" + (amount * 100f).ToString("F0") + ")", 1f, 0);
    }
    
    public void EatCorpse(float amount, float biteSize)
    {
        candidateRef.performanceData.totalFoodEatenCorpse += biteSize;
        EatFoodDecay(amount); // assumes all foodAmounts are equal !! *****
        RegisterAgentEvent(SimulationManager.instance.simAgeTimeSteps, "Ate Carrion! (" + (amount * 100f).ToString("F0") + ")", 1f, 0);
        //if(coreModule.foodEfficiencyMeat > 0.5f) { // ** // damage bonus -- provided has the required specialization level:::::
        //    GainExperience((flowR / coreModule.stomachCapacity) * 0.5f);  
        //}  
    }
    
    public void EatFoodMeat(float amount) {
        //totalFoodEatenZoop += amount; 
        
        amount = Mathf.Min(amount, coreModule.stomachSpace);
                
        //amount *= coreModule.foodEfficiencyMeat;
        //coreModule.stomachContentsTotal01 += (amount / coreModule.stomachCapacity);
        
        if(coreModule.stomachContentsPercent > 1f) {
            return;
            //float overStuffAmount = coreModule.stomachContentsNorm - 1f;
            //ProcessDamageReceived(overStuffAmount);
            //coreModule.stomachContentsPercent = 1f;
        }

        coreModule.stomachContentsMeat += amount;
        
        GainExperience((amount / coreModule.stomachCapacity) * coreModule.digestEfficiencyMeat * 1f); // Exp for appropriate food

        RegisterAgentEvent(SimulationManager.instance.simAgeTimeSteps, "Ate Microbe! (" + (amount * 100f).ToString("F0") + ")", 1f, 0);
    }
    
    public void EatFoodDecay(float amount) {
        
        amount = Mathf.Min(amount, coreModule.stomachSpace);

        //coreModule.stomachContentsTotal01 += (amount / coreModule.stomachCapacity);
        
        if(coreModule.stomachContentsPercent > 1f) {
            return;
            //float overStuffAmount = coreModule.stomachContentsNorm - 1f;            
            //coreModule.stomachContentsPercent = 1f;
        }

        coreModule.stomachContentsDecay += amount;

        GainExperience((amount / coreModule.stomachCapacity) * coreModule.digestEfficiencyDecay * 1f); // Exp for appropriate food

        RegisterAgentEvent(SimulationManager.instance.simAgeTimeSteps, "Ate Corpse! (" + (amount * 100f).ToString("F0") + ")", 1f, 0);
    }
    
    public void TakeDamage(float damage) {
        coreModule.DirectDamage(damage);        
        candidateRef.performanceData.totalDamageTaken += damage;
        RegisterAgentEvent(SimulationManager.instance.simAgeTimeSteps, "Took Damage! (" + (damage * 100f).ToString("F0") + ")", 0f, 1);
        CheckForDeathHealth();
    }
    
    public void ProcessBiteDamageReceived(float damage, Agent predatorAgentRef) {
        damage /= coreModule.healthBonus;

        float defendBonus = 1f;
        if(isDefending && defendFrameCounter < defendDuration) {
            RegisterAgentEvent(SimulationManager.instance.simAgeTimeSteps, "Blocked Bite! from #" + predatorAgentRef.index, 0.75f, 2);
        }
        else {
            damage *= defendBonus;
            predatorAgentRef.candidateRef.performanceData.totalDamageDealt += damage;
            TakeDamage(damage);
            RegisterAgentEvent(SimulationManager.instance.simAgeTimeSteps, "Bitten! (" + (damage * 100f).ToString("F0") + ") by #" + predatorAgentRef.index.ToString(), 0f, 1);
        }
        
        //coreModule.energy *= 0.5f; 
    }
    
    // If this agent is dead, it acts as food.
    public void ProcessBeingEaten(float amount) {
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
        
        RegisterAgentEvent(SimulationManager.instance.simAgeTimeSteps, "Devoured!", 0f, 1);  
    }

    public void Tick() {
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
                bodyRigidbody.simulated = false;
                bodyRigidbody.isKinematic = false;
                colliderBody.enabled = false;
                break;
            case AgentLifeStage.Egg:
                TickEgg();
                break;            
            case AgentLifeStage.Mature:
                TickMature();
                break;
            case AgentLifeStage.Dead:
                TickDead();
                break;
            case AgentLifeStage.Null:
                //Debug.Log("agent is null - probably shouldn't have gotten to this point...;");
                break;
            default:
                Debug.LogError("NO SUCH ENUM ENTRY IMPLEMENTED, YOU FOOL!!! (" + curLifeStage + ")");
                break;
        }
        
        Vector3 curPos = bodyRigidbody.transform.position;

        // *** WPP: ownVel and curVel express the same concept
        // but are calculated in subtly different ways -> extremely error prone.
        // (1) Pick one calculation and condense to one variable, or
        // (2) create more distinct names and add a comment explaining unique purpose of each
        ownPos = new Vector2(bodyRigidbody.transform.localPosition.x, bodyRigidbody.transform.localPosition.y);
        ownVel = new Vector2(bodyRigidbody.velocity.x, bodyRigidbody.velocity.y); // * change this to ownPos - prevPos
        
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
            // *** WPP: magic number -> what does 250 represent?
            bodyRigidbody.AddForce(250f * bodyRigidbody.mass * Time.deltaTime * Random.insideUnitCircle, ForceMode2D.Impulse);
        }
        ScaleBody(sizePercentage, resizeCollider);  
        
        if (!isAttachedToParentEggSack || !parentEggSackRef) 
            return;
            
        float embryoPercentage = (float)lifeStageTransitionTimeStepCounter / (float)_GestationDurationTimeSteps;
        float targetDist = Mathf.Lerp(0.01f, parentEggSackRef.fullSize.magnitude * 0.15f, embryoPercentage);
        springJoint.distance = targetDist;

        if(parentEggSackRef.curLifeStage != EggSack.EggLifeStage.Null) 
            return;
                               
        isAttachedToParentEggSack = false;
        springJoint.connectedBody = null;
        springJoint.enabled = false;
        colliderBody.enabled = true;
        parentEggSackRef = null;                 
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
        //isCooldown = false;
        RegisterAgentEvent(SimulationManager.instance.simAgeTimeSteps, "Was Born!", 1f, 3);

        candidateRef.performanceData.timeStepHatched = simManager.simAgeTimeSteps;
    }
    
    private void TickMature() {
        mouthRef.isFeeding = isFeeding;
        mouthRef.isAttacking = isAttacking;
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
        TickMetabolism();

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
            childEggSackRef.currentBiomass = starterMass;     // * TROUBLE!!!

            RegisterAgentEvent(SimulationManager.instance.simAgeTimeSteps, "Pregnant! " + starterMass, 0.5f, 3);
        }
        else {
            Debug.LogError("Something went wrong!! " + " curMass: " + currentBiomass + ", reqMass: " + starterMass.ToString() + ", curProp: " + curProportion.ToString() );
        }
    }
    
    public void AbortPregnancy() {
        //childEggSackRef
        if(childEggSackRef) {
            childEggSackRef.ParentDiedWhilePregnant();
            childEggSackRef = null;
        }        

        isPregnantAndCarryingEggs = false;
        pregnancyRefactoryTimeStepCounter = 0;
    }
    private void CheckForMaturity() {
        float starterMass = settingsRef.agentSettings._BaseInitMass * settingsRef.agentSettings._MinPregnancyFactor;
        float curProportion = currentBiomass * settingsRef.agentSettings._MaxPregnancyProportion;
        // probably 0.05 * 2 = 0.1   for now
        if (curProportion > starterMass) { // Good to go!
            isSexuallyMature = true;
        }
    }
    public void CompletedPregnancy() {
        childEggSackRef = null;
        candidateRef.performanceData.totalTimesPregnant++;
        isPregnantAndCarryingEggs = false;
        pregnancyRefactoryTimeStepCounter = 0;

        RegisterAgentEvent(SimulationManager.instance.simAgeTimeSteps, "Pregnancy Complete!", 0.95f, 3);
    }
    
    private void TickDead() {
        lifeStageTransitionTimeStepCounter++;

        // DECAY HERE!
        // Should shrink as well as lose foodContent
        float decayAmount = settingsRef.agentSettings._BaseDecompositionRate;

        // include reproductive stockpile energy???

        currentBiomass = Mathf.Max(currentBiomass - decayAmount, 0f);
        //currentBiomass -= decayAmount;
        
        wasteProducedLastFrame += decayAmount;// * wasteProducedMult;
    }

    private void ScaleBody(float sizePercentage, bool resizeColliders) {
        //segmentFullSizeArray
        //float minScale = 0.2f;
        float scale = Mathf.Lerp(spawnStartingScale, 1f, sizePercentage); // Minimum size = 0.1 ???  // SYNC WITH EGG SIZE!!!
        currentBoundingBoxSize = fullSizeBoundingBox * scale;
        float currentBodyVolume = currentBoundingBoxSize.y * (currentBoundingBoxSize.x + currentBoundingBoxSize.z) * 0.5f; // coreModule.currentBodySize.x * coreModule.currentBodySize.y;

        // Revisit this::::
        //currentBiomass = currentBodyVolume;  // ??? **** DOESN'T LOOK CORRECT -- FIX!!

        coreModule.stomachCapacity = 1f; // currentBodyVolume;
        
        if(resizeColliders) {
            ResizeColliders(currentBodyVolume);
        }               
    }
    
    private void ResizeColliders(float currentBodyVolume)
    {
        colliderBody.size = new Vector2(currentBoundingBoxSize.x, currentBoundingBoxSize.y); // coreModule.currentBodySize;
        bodyRigidbody.mass = currentBodyVolume; 

        // MOUTH:
        mouthRef.triggerCollider.radius = currentBoundingBoxSize.x * 0.5f;  // ***** REVISIT THIS!!! USE GENOME FOR MOUTH!!! *****
        mouthRef.triggerCollider.offset = new Vector2(0f, currentBoundingBoxSize.y * 0.5f);
        
        // THIS IS HOT GARBAGE !!! REFACTOR!! *****
        mouseClickCollider.radius = currentBoundingBoxSize.x * 0.5f + 2f;        
        mouseClickCollider.height = currentBoundingBoxSize.y + 2f;
        mouseClickCollider.center = new Vector3(0f, 0f, -SimulationManager._GlobalWaterLevel * SimulationManager._MaxAltitude);
        //mouseClickCollider.radius += 2f; // ** TEMP -- should be based on camera distance also
        //mouseClickCollider.height += 2f;        
    }
    
    [SerializeField] float healRate = 0.0005f;
    [SerializeField] float baseEnergyToHealth = 5f;
    
    float energyToHealth => baseEnergyToHealth * coreModule.healthBonus;
    
    [SerializeField] [Range(0,1)] float restingBonusWhenResting = 0.65f;
    float restingBonus => isResting ? restingBonusWhenResting : 1f;

    public void TickActions(SimulationManager simManager) {
        //AgentActionState currentState = AgentActionState.Default;
        
        float horizontalMovementInput = movementModule.throttleX[0]; // Mathf.Lerp(horAI, horHuman, humanControlLerp);
        float verticalMovementInput = movementModule.throttleY[0]; // Mathf.Lerp(verAI, verHuman, humanControlLerp);
        
        // Facing Direction:
        throttle = new Vector2(horizontalMovementInput, verticalMovementInput);        
        smoothedThrottle = Vector2.Lerp(smoothedThrottle, throttle, smoothedThrottleLerp);
        Vector2 throttleForwardDir = throttle.normalized;

        if(isDead || isEgg) {
            throttle = Vector2.zero;
            smoothedThrottle = Vector2.zero;
        }
        else 
        {
            // Food calc before energy/healing/etc? **************
            //float sizeValue = BodyGenome.GetBodySizeScore01(candidateRef.candidateGenome.bodyGenome);
            // FOOD PARTICLES: Either mouth type for now:
            float foodParticleEatAmount = simManager.vegetationManager.plantParticlesEatAmountsArray[index] * coreModule.digestEfficiencyPlant; // **************** PLANT BONUS!! HACKY
            float animalParticleEatAmount = simManager.zooplanktonManager.animalParticlesEatAmountsArray[index] * coreModule.digestEfficiencyMeat;
            
            bool isEatingPlant = foodParticleEatAmount > 0f;
            bool isEatingAnimal = animalParticleEatAmount > 0f;
            
            if(isEatingPlant) {
                //mouthRef.InitiatePassiveBite();
                //float sizeEfficiencyPlant = Mathf.Lerp(settings.minSizeFeedingEfficiencyDecay, settings.maxSizeFeedingEfficiencyDecay, sizeValue);
                //Debug.Log("Agent[" + index.ToString() + "], Ate Plant: " + foodParticleEatAmount.ToString());
                candidateRef.performanceData.totalFoodEatenPlant += foodParticleEatAmount; 
                EatFoodPlant(foodParticleEatAmount);                
            }

            if(isEatingAnimal) {
                //float sizeEfficiencyPlant = Mathf.Lerp(settings.minSizeFeedingEfficiencyDecay, settings.maxSizeFeedingEfficiencyDecay, sizeValue);
                candidateRef.performanceData.totalFoodEatenZoop += animalParticleEatAmount;
                //animalParticleEatAmount *= 0.98f;
                
                //Debug.Log("Agent[" + index.ToString() + "], Ate Zooplankton: " + animalParticleEatAmount.ToString());
                EatFoodMeat(animalParticleEatAmount); // * sizeEfficiencyPlant);    
                RegisterAgentEvent(SimulationManager.instance.simAgeTimeSteps, "Ate Zooplankton! (+" + (animalParticleEatAmount * 100).ToString("F0") + " food)", 1f, 0);
            }

            mouthRef.lastBiteFoodAmount += foodParticleEatAmount + animalParticleEatAmount;

            if(isEatingPlant || isEatingAnimal) {
                UseAbility(feed);
            }
           
            SelectAction();                  
        }
        
        MovementScalingTest(smoothedThrottle);

        float rotationInRadians = (bodyRigidbody.transform.localRotation.eulerAngles.z + 90f) * Mathf.Deg2Rad;
        facingDirection = new Vector2(Mathf.Cos(rotationInRadians), Mathf.Sin(rotationInRadians));
                
        //curActionState = currentState;
    }
    
    public void TickMetabolism() {
        // Digestion:
        float maxDigestionRate = settingsRef.agentSettings._BaseDigestionRate * currentBiomass; // proportional to biomass?        
        float totalStomachContents = coreModule.totalStomachContents;        
        float digestedAmountTotal = Mathf.Min(totalStomachContents, maxDigestionRate);

        wasteProducedLastFrame += digestedAmountTotal * settingsRef.agentSettings._DigestionWasteEfficiency;
        oxygenUsedLastFrame = currentBiomass * settingsRef.agentSettings._BaseOxygenUsage;
        currentBiomass += digestedAmountTotal * settingsRef.agentSettings._GrowthEfficiency;  // **** <-- Reconsider
        
        if(currentBiomass > fullsizeBiomass) {
            wasteProducedLastFrame += (currentBiomass - fullsizeBiomass);
            currentBiomass = fullsizeBiomass;
        }
        
        // WPP: delegated to CritterModuleCore
        coreModule.TickDigestion(digestedAmountTotal);
        /*float digestedPlantMass = digestedAmountTotal * coreModule.plantEatenPercent;
        float digestedMeatMass = digestedAmountTotal * coreModule.meatEatenPercent;
        float digestedDecayMass = digestedAmountTotal * coreModule.decayEatenPercent; 
        
        float createdEnergyTotal = coreModule.GetEnergyCreatedFromDigestion(digestedPlantMass, digestedMeatMass, digestedDecayMass) * settingsRef.agentSettings._DigestionEnergyEfficiency;
        coreModule.stomachContentsPlant -= digestedPlantMass;        
        coreModule.stomachContentsMeat -= digestedMeatMass;
        coreModule.stomachContentsDecay -= digestedDecayMass;
        //coreModule.energy += createdEnergyTotal; */
        
        if(index == 1) {
            //Debug.Log(createdEnergyTotal + "  GetEnergyCreatedFromDigestion: " + coreModule.stomachContentsPlant + ", " + coreModule.stomachContentsMeat + ", " + coreModule.stomachContentsDecay);
        }
        
        //float spentEnergyTotal = 
        //(plantToEnergyAmount * coreModule.dietSpecPlantNorm + meatToEnergyAmount * coreModule.dietSpecMeatNorm + decayToEnergyAmount * coreModule.dietSpecDecayNorm)

        coreModule.Regenerate(healRate, energyToHealth);

        //float oxygenMask = Mathf.Clamp01(simManager.simResourceManager.curGlobalOxygen * settingsRef.agentSettings._OxygenEnergyMask);

        //ENERGY:
        float energyCostMult = settingsRef.agentSettings._BaseEnergyCost; // Mathf.Lerp(settingsRef.agentSettings._BaseEnergyCost, settingsRef.agentSettings._BaseEnergyCost * 0.25f, sizePercentage);
        float restingEnergyCost = Mathf.Sqrt(currentBiomass) * energyCostMult * restingBonus; // * SimulationManager.energyDifficultyMultiplier; // / coreModule.energyBonus;
        //float throttleMag = smoothedThrottle.magnitude;
        
        // ENERGY DRAIN::::        
        coreModule.energy -= restingEnergyCost;
        
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
    }

    private void SelectAction() {
        curActionState = AgentActionState.Default;

        float[] effectorValues = { 0f, coreModule.mouthFeedEffector[0], 
            coreModule.mouthAttackEffector[0], coreModule.defendEffector[0],
            coreModule.dashEffector[0], coreModule.healEffector[0] };
            
        float mostActiveEffectorValue = FloatMath.GetHighest(effectorValues);
        
        if(coreModule.healEffector[0] >= mostActiveEffectorValue) {
            isResting = isFreeToAct;
                
            if(isFreeToAct) {
                candidateRef.performanceData.totalTicksRested++;
            }
            curActionState = AgentActionState.Resting;
        }

        if(coreModule.mouthFeedEffector[0] >= mostActiveEffectorValue) {
            curActionState = AgentActionState.Feeding;
            UseAbility(feed);
        }
        
        if (!isFreeToAct) {
            curActionState = AgentActionState.Cooldown;
            return;
        }  

        if(coreModule.mouthAttackEffector[0] >= mostActiveEffectorValue) {
            curActionState = AgentActionState.Attacking;
            UseAbility(attack);
        }
        if(coreModule.dashEffector[0] >= mostActiveEffectorValue) {
            curActionState = AgentActionState.Dashing;
            UseAbility(dash);
        }
        if(coreModule.defendEffector[0] >= mostActiveEffectorValue) {
            curActionState = AgentActionState.Defending;
            UseAbility(defend);
        }    
    }
    
    private void UseAbility(IAgentAbility ability)
    {
        activeAbility = ability;
        ability.Begin();        
    }
    
    bool isFreeToAct => !isCooldown && !isDashing && !isDefending && !isFeeding && !isAttacking &&
                        curLifeStage == AgentLifeStage.Mature;
                        
    bool outOfStamina => coreModule.stamina[0] < 0.1f;
    
    [SerializeField] [Range(0,1)] float bitingPenaltyWhenFeeding = 0.5f;
    float bitingPenalty => isFeeding ? bitingPenaltyWhenFeeding : 1f;
    
    [SerializeField] [Range(0,1)] float forcePenaltyWhenResting = 0.1f;
    float forcePenalty => isResting ? forcePenaltyWhenResting : 1f;
    
    [SerializeField] float dashBonusInCooldown = 0.33f;
    [SerializeField] float dashBonusWhenDashing = 4.3f;
    float dashBonus
    {
        get
        {
            if (isCooldown)
                return dashBonusInCooldown;
                
            return isDashing ?  dashBonusWhenDashing : 1f;
        }
    }
    
    private void MovementScalingTest(Vector2 throttle) {
        // Save current joint angles:
        //for(int j = 0; j < numSegments - 1; j++) {
        //    jointAnglesArray[j] = hingeJointsArray[j].jointAngle;            
        //}
        
        /*if(coreModule.mouthFeedEffector[0] > 0f)  // Clean up code for State-machine-esque behaviors/abilities
        {
            bitingPenalty = 0.5f;
        }*/

        float fatigueMultiplier = Mathf.Clamp01(coreModule.energy * 5f + 0.05f); // * Mathf.Clamp01(coreModule.stamina[0] * 4f + 0.05f);
        float lowHealthPenalty = Mathf.Clamp01(coreModule.health * 5f) * 0.5f + 0.5f;
        fatigueMultiplier *= lowHealthPenalty;
        
        turningAmount = Mathf.Lerp(turningAmount, bodyRigidbody.angularVelocity * Mathf.Deg2Rad * 0.03f, 0.28f);

        animationCycle += smoothedThrottle.magnitude * swimAnimationCycleSpeed * fatigueMultiplier * forcePenalty; // (Mathf.Lerp(fullSizeBoundingBox.y, 1f, 1f)

        if (throttle.sqrMagnitude <= 0.000001f)
            return;
            
        Vector2 headForwardDir = new Vector2(bodyRigidbody.transform.up.x, bodyRigidbody.transform.up.y).normalized;
        Vector2 headRightDir =  new Vector2(bodyRigidbody.transform.right.x, bodyRigidbody.transform.right.y).normalized;
        Vector2 throttleDir = throttle.normalized;

        float turnSharpness = -Vector2.Dot(throttleDir, headForwardDir) * 0.5f + 0.5f;
        float headTurn = Vector2.Dot(throttleDir, headRightDir) * -1f * turnSharpness;
        float headTurnSign = Mathf.Clamp(Vector2.Dot(throttleDir, headRightDir) * -10000f, -1f, 1f);
          
        // get size in 0-1 range from minSize to maxSize: // **** NOT ACCURATE!!!!
        //float sizeValue = Mathf.Clamp01(coreModule.speedBonus * (candidateRef.candidateGenome.bodyGenome.coreGenome.creatureBaseLength - 0.2f) / 2f);  // Mathf.Clamp01((fullSizeBoundingBox.x - 0.1f) / 2.5f); // ** Hardcoded assuming size ranges from 0.1 --> 2.5 !!! ********

        float swimSpeed = 48f * coreModule.speedBonus; // Mathf.Lerp(movementModule.smallestCreatureBaseSpeed, movementModule.largestCreatureBaseSpeed, 0.5f); // sizeValue);
        float turnRate = 6f * coreModule.speedBonus; //10 // Mathf.Lerp(movementModule.smallestCreatureBaseTurnRate, movementModule.largestCreatureBaseTurnRate, 0.5f) * 0.1f; // sizeValue);
        
        /*float dashBonus = 1f;
        if(isDashing) {                
            dashBonus = 4.3f;                
        }
        if(isCooldown) {
            dashBonus = 0.33f;
        }*/

        speed = swimSpeed * dashBonus * forcePenalty; // * movementModule.speedBonus ; // * restingPenalty;
        Vector2 segmentForwardDir = new Vector2(bodyRigidbody.transform.up.x, bodyRigidbody.transform.up.y).normalized;
        Vector2 forwardThrustDir = Vector2.Lerp(segmentForwardDir, throttleDir, 0.1f).normalized;
        bodyRigidbody.AddForce(forwardThrustDir * (1f - turnSharpness * 0.25f) * speed * bodyRigidbody.mass * Time.deltaTime * fatigueMultiplier * bitingPenalty, ForceMode2D.Impulse);

        // modify turning rate based on body proportions:
        //float turnRatePenalty = Mathf.Lerp(0.25f, 1f, 1f - sizeValue);

        // Head turn:
        float torqueForce = Mathf.Lerp(headTurn, headTurnSign, 0.35f) * forcePenalty * turnRate * this.bodyRigidbody.mass * fatigueMultiplier * bitingPenalty * Time.deltaTime;
        torqueForce = Mathf.Min(torqueForce, 50000.55f) * 3f;
        bodyRigidbody.AddTorque(torqueForce, ForceMode2D.Impulse); 
    }

    public void GainExperience(float exp) {
        totalExperience += exp;
        if(totalExperience >= experienceForNextLevel) {
            GainLevel();
        }
    }
    
    public void GainLevel() {
        curLevel++;
        experienceForNextLevel *= 2f;
    }

    public void InitializeModules(AgentGenome genome) {
        communicationModule = new CritterModuleCommunication();
        communicationModule.Initialize(genome.bodyGenome.communicationGenome);

        coreModule = new CritterModuleCore(genome.bodyGenome.coreGenome);

        mouthRef.Initialize(genome.bodyGenome.coreGenome);

        environmentModule = new CritterModuleEnvironment(genome.bodyGenome.environmentalGenome, this);
        foodModule = new CritterModuleFood(genome.bodyGenome.foodGenome, this);
        friendModule = new CritterModuleFriends(genome.bodyGenome.friendGenome, this);
        movementModule = new CritterModuleMovement(genome, genome.bodyGenome.movementGenome);
        threatsModule = new CritterModuleThreats(genome.bodyGenome.threatGenome, this);
    }
    
    public void FirstTimeInitialize() { 
        curLifeStage = AgentLifeStage.AwaitingRespawn;
        //InitializeAgentWidths(genome);
        //InitializeGameObjectsAndComponents();
        //InitializeModules(genome);  //  This breaks MapGridCell update, because coreModule doesn't exist?
    }

    // Colliders Footprint???  *************************************************************************************************************

    // * WPP: expose magic numbers
    public void ReconstructAgentGameObjects(AgentGenome genome, EggSack parentEggSack, Vector3 startPos, bool isImmaculate, float waterLevel) {
        //float corpseLerp = (float)settings.curTierFoodCorpse / 10f;
        //decayDurationTimeSteps = 480; // Mathf.RoundToInt(Mathf.Lerp(360f, 3600f, corpseLerp));
        //float eggLerp = (float)settings.curTierFoodEgg / 10f;
        //gestationDurationTimeSteps = Mathf.RoundToInt(Mathf.Lerp(360f, 1080f, eggLerp));
        //pregnancyRefactoryDuration = Mathf.RoundToInt(Mathf.Lerp(3600f, 800f, eggLerp));

        //InitializeAgentWidths(genome);
        //InitializeGameObjectsAndComponents();  // Not needed??? ***

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
        
        isAttachedToParentEggSack = !isImmaculate;
        colliderBody.enabled = isImmaculate;

        if(isImmaculate) {            
            springJoint.connectedBody = null; // parentEggSack.rigidbodyRef;
            springJoint.enabled = false;
        }
        //else {
            //bodyGO.transform.localPosition = parentEggSack.gameObject.transform.position; // startPos.startPosition;        
            //springJoint.connectedBody = parentEggSack.rigidbodyRef;
            //springJoint.enabled = true;
        //}                
            
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
        
        mouthRef.agentIndex = index;
        //mouthRef.agent = this;
        isResting = false;

        //mouthRef.Disable();
        //mouseclickcollider MCC
        mouseClickCollider.direction = 1; // Y-Axis ???
        mouseClickCollider.center = new Vector3(0f, 0f, (waterLevel * 2f - 1f) * -10f); //Vector3.zero; // new Vector3(0f, 0f, 1f);
        mouseClickCollider.radius = fullSizeBoundingBox.x / 2f * sizePercentage;
        mouseClickCollider.radius *= 5.14f; // ** TEMP
        mouseClickCollider.height = fullSizeBoundingBox.y / 2f * sizePercentage;
    }

    private void ResetStartingValues() 
    {
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
        experienceForNextLevel = 2f;
        curLevel = 0;
        
        turningAmount = 5f; // temporary for zygote animation
        facingDirection = new Vector2(0f, 1f);
        throttle = Vector2.zero;
        smoothedThrottle = new Vector2(0f, 0.01f); 
    }

    // When should biomass be transferred from EggSack?
    // If spawnImmaculate, where does the biomass come from? -- should it be free?
    public void InitializeSpawnAgentImmaculate(int agentIndex, CandidateAgentData candidateData, Vector3 spawnWorldPos, float globalWaterLevel) {        
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
        
        ReconstructAgentGameObjects(genome, null, spawnWorldPos, true, globalWaterLevel);

        brain = new Brain(genome.brainGenome, this); 
        isInert = false;
    }
    
    public void InitializeSpawnAgentFromEggSack(int agentIndex, CandidateAgentData candidateData, EggSack parentEggSack, float globalWaterLevel) {        
        index = agentIndex;
        speciesIndex = candidateData.speciesID;
        candidateRef = candidateData;
        AgentGenome genome = candidateRef.candidateGenome;
                
        curLifeStage = AgentLifeStage.Egg;
        parentEggSackRef = parentEggSack;
                
        ResetStartingValues();       
        InitializeModules(genome);      // Modules need to be created first so that Brain can map its neurons to existing modules  

        Vector3 spawnOffset = 0.167f * parentEggSack.curSize.magnitude * Random.insideUnitSphere;
        spawnOffset.z = 0f;
        
        ReconstructAgentGameObjects(genome, parentEggSack, parentEggSack.gameObject.transform.position + spawnOffset, false, globalWaterLevel);

        brain = new Brain(genome.brainGenome, this);   
        isInert = false;
    }

    // For setting activity material property
    public int GetActivityID()
    {
        if (!isMature) return 0;
        if (isCooldown) return 7;
        if (isResting) return 5;
        if (isDefending) return 4;
        if (isDashing) return 3;
        if (isAttacking) return 2;
        if (isFeeding) return 1;
        if (isPregnantAndCarryingEggs) return 6;

        return 0;
    }
}