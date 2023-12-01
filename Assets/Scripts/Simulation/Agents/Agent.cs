using UnityEngine;

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

public enum AgentLifeStage {
    AwaitingRespawn,  // can i merge this in with null?
    Egg,
    Mature,
    Dead,
    Null
}

public class Agent : MonoBehaviour {
    Lookup lookup => Lookup.instance;
    SettingsManager settingsRef => SettingsManager.instance;
    SimulationManager simManager => SimulationManager.instance;
    AudioManager audioManager => AudioManager.instance;
    UIManager uiManager => UIManager.instance;
    CreaturePanelUI creaturePanel => uiManager.creaturePanelUI;
    //private PerformanceData performanceData;
    //public float totalFoodEatenDecay = 0f;
    
    //private bool isFeedingPlant = false;
    //private bool isFeedingZooplankton = false;
    
    public AgentInfo info;
    [HideInInspector] 
    public AgentData data;
    
    // WPP: separates data from process for saving purposes
    // get/set avoids having to use data. everywhere else in the script
    public float speed { get => data.speed; set => data.speed = value; }
    public float smoothedThrottleLerp { get => data.smoothedThrottleLerp; set => data.smoothedThrottleLerp = value; } 
    public float animationCycle { get => data.animationCycle; set => data.animationCycle = value; } 
    public float turningAmount { get => data.turningAmount; set => data.turningAmount = value; } 
    public float swimAnimationCycleSpeed { get => data.swimAnimationCycleSpeed; set => data.swimAnimationCycleSpeed = value; } 
    
    // *** REFACTOR!!! SYNC WITH EGGS!!!
    public float spawnStartingScale { get => data.spawnStartingScale; set => data.spawnStartingScale = value; }

    /// Disables colliders when true;
    public bool isInert { get => data.isInert; set => data.isInert = value; }
    /// biting, defending, dashing, etc -- exclusive actions  
    public bool isActing { get => data.isActing; set => data.isActing = value; }  
    public bool isDecaying { get => data.isDecaying; set => data.isDecaying = value; }
    public bool isFeeding => feed.inProcess;
    public bool isAttacking => attack.inProcess;
    
    // Flag for intention to eat gpu food particle (plant-type) 
    public bool isFreeToEat => isFeeding && !isCooldown && feedingFrameCounter == 4; //***EAC HACKY
    
    //***EC eventually move these into creature genome, make variable
    public int feedAnimDuration { get => data.feedAnimDuration; set => data.feedAnimDuration = value; }
    public int feedAnimCooldown { get => data.feedAnimCooldown; set => data.feedAnimCooldown = value; }
    public int attackAnimDuration { get => data.attackAnimDuration; set => data.attackAnimDuration = value; }
    public int attackAnimCooldown { get => data.attackAnimCooldown; set => data.attackAnimCooldown = value; }
    public int feedingFrameCounter => feed.frameCount;
    public int attackingFrameCounter => attack.frameCount;
    public bool isDashing => dash.inProcess;
    public int dashFrameCounter => dash.frameCount;
    public int dashDuration { get => data.dashDuration; set => data.dashDuration = value; }
    public int dashCooldown { get => data.dashCooldown; set => data.dashCooldown = value; }
    public bool isDefending => defend.inProcess;
    public int defendFrameCounter => defend.frameCount;
    public int defendDuration { get => data.defendDuration; set => data.defendDuration = value; }
    public int defendCooldown { get => data.defendCooldown; set => data.defendCooldown = value; }
    
    // * WPP: same functionality as isFreeToAct -> pick one to use and delete other
    public bool isResting => curActionState == AgentActionState.Resting && isFreeToAct;
    
    public bool isCooldown => cooldown.inProcess;
    public int cooldownFrameCounter => cooldown.frameCount;
    public int cooldownDuration { get => data.cooldownDuration; set => data.cooldownDuration = value; }
    public bool isMarkedForDeathByUser { get => data.isMarkedForDeathByUser; set => data.isMarkedForDeathByUser = value; }
    public int index { get => data.index; set => data.index = value;  }  
    public int speciesIndex { get => data.speciesIndex; set => data.speciesIndex = value; }
    public AgentLifeStage curLifeStage { get => data.curLifeStage; set => data.curLifeStage = value; }
    
    public CandidateAgentData candidateRef;
    
    public bool isYoung => curLifeStage == AgentLifeStage.Mature && !isSexuallyMature;
    
    public int gestationDurationTimeSteps = 90;
    //public int _GestationDurationTimeSteps => gestationDurationTimeSteps;
    
    public int maxAgeTimeSteps = 1000000;
    
    private int growthScalingSkipFrames = 32;

    public float sizePercentage = 0f;
    
    [ReadOnly] public Brain brain;
    /// Primary root segment child object
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
    /// Used to attach to EggSack Object while still in Egg stage
    public SpringJoint2D springJoint;   
    public CapsuleCollider mouseClickCollider;
    
    /// z = length, y = height, x = width
    public Vector3 fullSizeBoundingBox;  
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
    public float fullsizeBiomass = 1f;
    public float biomassAtDeath = 1f;
    
    private Vector3 prevPos;  // use this instead of sampling rigidbody

    public float prevVel;
    public float curVel;
    public float curAccel;

    public Vector2 ownPos;
    public Vector2 ownVel;

    public Vector2 throttle;
    public Vector2 smoothedThrottle;
    /// based on throttle history
    public Vector2 facingDirection;  

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
    public Agent predatorAgentRef;  // * Not used
    public Agent preyAgentRef;      // * Not used

    public EggSack parentEggSackRef;  // instead of using own fixed embryo development duration - look up parentEggSack and use its counter?
    public bool isAttachedToParentEggSack = false;

    public EggSack childEggSackRef;
    public bool isPregnantAndCarryingEggs = false;
    public int pregnancyRefactoryDuration = 420;
    
    IAgentAbility attack;
    IAgentAbility dash;
    IAgentAbility defend;
    IAgentAbility feed;
    Cooldown cooldown;
        
    public float totalEaten => candidateRef.performanceData.totalEaten;
    public Vector3 position { get => bodyGO.transform.position; set => bodyGO.transform.position = value; }
        
    private void Awake() {  
        data = info.GetData();
        
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
            Debug.LogError($"I'm not dead yet! {index} {curLifeStage}");
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
        RegisterAgentEvent(simManager.simAgeTimeSteps, "Ate Vertebrate! (" + (foodAmount * 100f).ToString("F0") + ") candID: " + preyAgent.candidateRef.candidateID, 1f, 2);
        preyAgent.ProcessBeingEaten(preyAgent.currentBiomass);
        
        colliderBody.enabled = false;
        springJoint.enabled = false;
        springJoint.connectedBody = null;

        audioManager.PlayCritterBite(ownPos);
    }

    IBrainModule GetModule(BrainModuleID id)
    {
        switch (id)
        {
            case BrainModuleID.Core: return coreModule;
            case BrainModuleID.Communication: return communicationModule;
            case BrainModuleID.EnvironmentSensors: return environmentModule;
            case BrainModuleID.FoodSensors: return foodModule;
            case BrainModuleID.FriendSensors: return friendModule;
            case BrainModuleID.ThreatSensors: return threatsModule;
            case BrainModuleID.Movement: return movementModule;
            default: return null;
        }
    }

    public void ResetBrainState() {
        brain.ResetBrainState();
    }
    
    public void TickBrain() {
        foreach (var neuron in brain.genome.neurons.inOut)
            SetNeuronValue(neuron);
    
        brain.TickAxons();
    }
    
    public void SetNeuronValue(Neuron neuron)
    {
        if (neuron.template.moduleID == BrainModuleID.Undefined)
            neuron.currentValue = 0f;
        else
            GetModule(neuron.template.moduleID)?.SetNeuralValue(neuron);
    }
    
    // Updates internal state of body - i.e health, energy etc. -- updates input Neuron values!!!
    // Update Stocks & Flows ::: new health, energy, stamina
    // This should have happened during last frame's Internal PhysX Update
    // HOWEVER, some updates only happen once per frame and can be handled here, like converting food into energy automatically
    // Turns out that most of these updates are updating input neurons which tend to be sensors
    // These values are sometimes raw attributes & sometimes processed data
    // Should I break them up into individual sensor types -- like Ears, Collider Rangefind, etc.?
    // Separate sensors for each target type or add multiple data types to rangefinder raycasts?
    public void TickModules() 
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
    
    /// STARVATION
    private void CheckForDeathStarvation() {
        if (coreModule.energy > 0f)
            return;
        
        if (coreModule.stomachEmpty)
            InitializeDeath(lookup.GetCauseOfDeath(CauseOfDeathId.Starved));
        else
            InitializeDeath("Suffocated", "Suffocated! stomachContentsNorm: " + coreModule.stomachContentsPercent);
    }
    
    /// HEALTH FAILURE:
    public void CheckForDeathHealth() {
        if (coreModule.health > 0f)
            return;
        
        coreModule.SetAllHealth(0f);
        //Debug.LogError("CheckForDeathHealth" + currentBiomass.ToString());

        InitializeDeath(lookup.GetCauseOfDeath(CauseOfDeathId.Injuries));    
    }
    
    private void CheckForDeathOldAge() {
        if (ageCounter > maxAgeTimeSteps)
            InitializeDeath(lookup.GetCauseOfDeath(CauseOfDeathId.OldAge));    
    }
    
    private void CheckForDeathDivineJudgment() {
        if (isMarkedForDeathByUser)   
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
            RegisterAgentEvent(simManager.simAgeTimeSteps, deathEvent, 0f, 10);

        if(isPregnantAndCarryingEggs) {
            AbortPregnancy();
        }

        brain.ResetBrainState();

        //colliderBody.enabled = false;
        //bodyRigidbody.simulated = false;
        //bodyRigidbody.isKinematic = true;

        isSexuallyMature = false;
        isSwallowingPrey = false;
        swallowingPreyFrameCounter = 0;

        masterFitnessScore = totalExperience; // update this???
        candidateRef.performanceData.totalTicksAlive = ageCounter;
        candidateRef.performanceData.timeStepDied = simManager.simAgeTimeSteps;
        biomassAtDeath = currentBiomass;
        mouthRef.Disable();
        
        audioManager.PlayCritterDeath(ownPos);
        //Debug.Log("wtf?" + candidateRef.performanceData.p0x);
        //candidateRef.performanceData.p0x = candidateRef.performanceData.timeStepDied;//
    }
    
    // *** WPP: trigger state changes & processes when conditions met
    // rather than polling in the update loop -> efficiency and natural flow
    // (would also eliminate conditional)
    private void CheckForLifeStageTransition() {
        

        switch (curLifeStage) {
            case AgentLifeStage.AwaitingRespawn:
                break;
            case AgentLifeStage.Egg:
                if (lifeStageTransitionTimeStepCounter >= gestationDurationTimeSteps) {
                    BeginHatching();
                    
                }
                else {
                    CheckForDeathHealth();
                }
                //candidateRef.performanceData.p0y = SimulationManager.instance.masterGenomePool.completeSpeciesPoolsList[speciesIndex].avgLifespan;
                break;
            case AgentLifeStage.Mature:
                // Check for Death:
                CheckForDeathStarvation();
                CheckForDeathHealth();
                CheckForDeathOldAge();
                CheckForDeathDivineJudgment();
                colliderBody.enabled = true;
                CheckForMaturity();
                candidateRef.performanceData.totalTicksAlive = ageCounter;
                //candidateRef.performanceData.max
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
                Debug.LogError($"NO SUCH ENUM ENTRY IMPLEMENTED, YOU FOOL!!! ({curLifeStage})");
                break;
        }
    }
    
    private void RegisterAgentEvent(AgentActionState actionState)
    {
        switch (actionState)
        {
            case AgentActionState.Attacking: RegisterAgentEvent(simManager.simAgeTimeSteps, "Attacked!", 1f, 6); break;
            case AgentActionState.Defending: RegisterAgentEvent(simManager.simAgeTimeSteps, "Defended!", 1f, 7); break;
            //case AgentActionState.Dashing: RegisterAgentEvent(simManager.simAgeTimeSteps, "Dashed!", 1f, 8); break;//handled elsewhere?
        }
    }
    
    public void RegisterAgentEvent(int frame, string textString, float goodness, int type) {
        candidateRef.RegisterCandidateEvent(frame, textString, goodness, type);        
    }
    
    public void EatFoodPlant(float amount) {           
        amount = Mathf.Min(amount * 100f, coreModule.stomachSpace);
        
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
        if(this.feedingFrameCounter == this.feedAnimDuration / 2) {
            RegisterAgentEvent(simManager.simAgeTimeSteps, "Ate Plant! (+" + (amount).ToString("F2") + " food)", 1f, 0);

        }
        
        audioManager.PlayCritterBite(ownPos);
    }
    
    // * WPP: combine below two methods (were extracted from CritterMouthComponent)
    public void EatEggs(float amount)
    {
        candidateRef.performanceData.totalFoodEatenEgg += amount;
        EatFoodMeat(amount); // assumes all foodAmounts are equal
        RegisterAgentEvent(simManager.simAgeTimeSteps, "Ate Egg Bit! (" + (amount).ToString("F2") + ")", 1f, 3);

        audioManager.PlayCritterBite(ownPos);
    }
    
    public void EatEggsWhole(float amount)
    {
        candidateRef.performanceData.totalFoodEatenEgg += amount;
        EatFoodDecay(amount);
        RegisterAgentEvent(simManager.simAgeTimeSteps, "Ate Egg! (" + (amount).ToString("F2") + ")", 1f, 3);

        audioManager.PlayCritterBite(ownPos);
    }
    
    public void EatCorpse(float amount, float biteSize)
    {
        candidateRef.performanceData.totalFoodEatenCorpse += biteSize;
        EatFoodDecay(amount); // assumes all foodAmounts are equal !! *****
        RegisterAgentEvent(simManager.simAgeTimeSteps, "Ate Carrion! (" + (amount).ToString("F2") + ")", 1f, 4);
        //if(coreModule.foodEfficiencyMeat > 0.5f) { // ** // damage bonus -- provided has the required specialization level:::::
        //    GainExperience((flowR / coreModule.stomachCapacity) * 0.5f);  
        //}  
        audioManager.PlayCritterBite(ownPos);
    }
    
    public void EatFoodMeat(float amount) {
        //totalFoodEatenZoop += amount; 
        amount *= 1f;
        amount = Mathf.Min(amount, coreModule.stomachSpace);
                
        
        //coreModule.stomachContentsTotal01 += (amount / coreModule.stomachCapacity);
        
        if (coreModule.stomachContentsPercent > 1f) {
            return;
            //float overStuffAmount = coreModule.stomachContentsNorm - 1f;
            //ProcessDamageReceived(overStuffAmount);
            //coreModule.stomachContentsPercent = 1f;
        }

        coreModule.stomachContentsMeat += amount;
        
        GainExperience((amount / coreModule.stomachCapacity) * coreModule.digestEfficiencyMeat * 1f); // Exp for appropriate food
        
        RegisterAgentEvent(simManager.simAgeTimeSteps, "Ate Microbe! (" + amount.ToString("F2") + ")", 1f, 1);
                
        audioManager.PlayCritterBite(ownPos);
    }
    
    public void EatFoodDecay(float amount) {
        
        amount = Mathf.Min(amount, coreModule.stomachSpace);

        //coreModule.stomachContentsTotal01 += (amount / coreModule.stomachCapacity);
        
        if (coreModule.stomachContentsPercent > 1f) {
            return;
            //float overStuffAmount = coreModule.stomachContentsNorm - 1f;            
            //coreModule.stomachContentsPercent = 1f;
        }

        coreModule.stomachContentsDecay += amount;

        GainExperience((amount / coreModule.stomachCapacity) * coreModule.digestEfficiencyDecay * 1f); // Exp for appropriate food

        RegisterAgentEvent(simManager.simAgeTimeSteps, "Ate Corpse! (" + (amount).ToString("F2") + ")", 1f, 4);

        audioManager.PlayCritterBite(ownPos);
    }
    
    public void TakeDamage(float damage) {
        coreModule.DirectDamage(damage);        
        candidateRef.performanceData.totalDamageTaken += damage;
        RegisterAgentEvent(simManager.simAgeTimeSteps, "Took Damage! (" + (damage * 100f).ToString("F0") + ")", 0f, 10);
        audioManager.PlayCritterDamage(ownPos);
        CheckForDeathHealth();
    }
    
    public void ProcessBiteDamageReceived(float damage, Agent predatorAgentRef) {
        //damage /= coreModule.healthBonus;

        float defendBonus = 1f;
        if (isDefending && defendFrameCounter < defendDuration) {
            RegisterAgentEvent(simManager.simAgeTimeSteps, "Blocked Bite! from #" + predatorAgentRef.index, 0.75f, 11);
        }
        else {
            damage *= defendBonus;
            predatorAgentRef.candidateRef.performanceData.totalDamageDealt += damage;
            TakeDamage(damage);
            RegisterAgentEvent(simManager.simAgeTimeSteps, "Bitten! (" + (damage * 100f).ToString("F0") + ") by #" + predatorAgentRef.index, 0f, 10);
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
        
        RegisterAgentEvent(simManager.simAgeTimeSteps, "Devoured!", 0f, 10);  
    }

    public void Tick() {
        // Resources:
        wasteProducedLastFrame = 0f;
        oxygenUsedLastFrame = 0f;

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

        if (isBeingSwallowed) {
            //Debug.Log("TickEgg() isBeingSwallowed! " + index.ToString() + " --> " + predatorAgentRef.index.ToString());
        }

        sizePercentage = Mathf.Clamp01(((float)lifeStageTransitionTimeStepCounter / (float)gestationDurationTimeSteps) * spawnStartingScale);

        // Scaling Test:
        int frameNum = lifeStageTransitionTimeStepCounter % growthScalingSkipFrames;
        bool resizeCollider = false;
        
        if (frameNum == 1) {
            resizeCollider = true;
            // *** WPP: magic number -> what does 250 represent?
            bodyRigidbody.AddForce(250f * bodyRigidbody.mass * Time.deltaTime * Random.insideUnitCircle, ForceMode2D.Impulse);
        }
        ScaleBody(sizePercentage, resizeCollider);  
        
        if (!isAttachedToParentEggSack || !parentEggSackRef) 
            return;
            
        float embryoPercentage = (float)lifeStageTransitionTimeStepCounter / (float)gestationDurationTimeSteps;
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
        //this.candidateRef.candidateGenome.bodyGenome.appearanceGenome.bodyStrokeBrushTypeX = SimulationManager.instance.masterGenomePool.completeSpeciesPoolsList[this.speciesIndex].foundingCandidate.bodyStrokeBrushTypeX;
        //this.candidateRef.candidateGenome.bodyGenome.appearanceGenome.bodyStrokeBrushTypeY = SimulationManager.instance.masterGenomePool.completeSpeciesPoolsList[this.speciesIndex].foundingCandidate.bodyStrokeBrushTypeY;
        //this.candidateRef.candidateGenome.bodyGenome.appearanceGenome.BlendHue(SimulationManager.instance.masterGenomePool.completeSpeciesPoolsList[this.speciesIndex].foundingCandidate.primaryHue, 0.5f);
        //this.candidateRef.candidateGenome.bodyGenome.appearanceGenome.hueSecondary = SimulationManager.instance.masterGenomePool.completeSpeciesPoolsList[this.speciesIndex].foundingCandidate.secondaryHue;
                
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
        coreModule.energy = currentBiomass * 256f;  // should be proportional to body size??
        
        mouthRef.Enable();
        //isCooldown = false;
        RegisterAgentEvent(simManager.simAgeTimeSteps, "Was Born!", 1f, 11);

        candidateRef.performanceData.timeStepHatched = simManager.simAgeTimeSteps;

        audioManager.PlayCritterSpawn(ownPos);

        //candidateRef.performanceData.minScoreValue = candidateRef.performanceData.timeStepHatched;//SetCurvePointStart(UIManager.instance.historyPanelUI.graphBoundsMinX, UIManager.instance.historyPanelUI.graphBoundsMinY);
        //candidateRef.performanceData.p0x = candidateRef.performanceData.timeStepHatched;
        //candidateRef.performanceData.p0y = simManager.masterGenomePool.completeSpeciesPoolsList[speciesIndex].avgLifespan;// + ((candidateRef.candidateID % 47) / 47) * 0.5f;
        //candidateRef.SetCurvePointEnd(UIManager.instance.historyPanelUI.graphBoundsMaxX, UIManager.instance.historyPanelUI.graphBoundsMaxY);
        //Debug.Log(candidateRef.performanceData.bezierCurve.points[0]);
        //candidateRef.performanceData.speciesAvgLifespanAtTimeOfBirth = simManager.masterGenomePool.completeSpeciesPoolsList[speciesIndex].avgLifespan;
    }
    
    private void TickMature() {
        mouthRef.isFeeding = isFeeding;
        mouthRef.isAttacking = isAttacking;
        //ProcessSwallowing();
        
        // *************************************************************************************
        candidateRef.performanceData.timeStepDied = SimulationManager.instance.simAgeTimeSteps;
        //candidateRef.performanceData.p1x = candidateRef.performanceData.timeStepDied;


        if (isSwallowingPrey) {
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

        TickModules(); // Update inputs for Brain        
        TickBrain();  
        TickActions(); // Execute Actions  -- Also Updates Resources!!! ***
        TickMetabolism();

        lifeStageTransitionTimeStepCounter++;
        ageCounter++;
                
        if(!isPregnantAndCarryingEggs) {
            pregnancyRefactoryTimeStepCounter++;
        }    
    }
    
    public void BeginPregnancy(EggSack developingEggSackRef) {
        //Debug.Log("BeginPregnancy! [" + developingEggSackRef.index.ToString() + "] agent# " + this.index);

        isPregnantAndCarryingEggs = true;
        childEggSackRef = developingEggSackRef;

        // energy transfer within EggSack class
        float starterMass = settingsRef.agentSettings._BaseInitMass * settingsRef.agentSettings._MinPregnancyFactor;
        float curProportion = currentBiomass * settingsRef.agentSettings._MaxPregnancyProportion;
        // probably 0.05 * 2 = 0.1   for now
        // Good to go!
        if (curProportion > starterMass) { 
            //Debug.Log("Pregnancy! " + " curMass: " + currentBiomass + ", reqMass: " + starterMass + ", curProp: " + curProportion);
            currentBiomass -= settingsRef.agentSettings._BaseInitMass * 3;
            childEggSackRef.currentBiomass = starterMass * 2;     // * TROUBLE!!!
            RegisterAgentEvent(simManager.simAgeTimeSteps, "Pregnant! " + starterMass, 0.5f, 10);
        }
        else {
            Debug.LogError("Something went wrong!! " + " curMass: " + currentBiomass + ", reqMass: " + starterMass + ", curProp: " + curProportion );
        }
    }
    
    public void AbortPregnancy() {
        //childEggSackRef
        if (childEggSackRef) {
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
            //Debug.Log("Agent " + this.index + " has reached sexual maturity!");
        }
    }
    
    public void CompletedPregnancy() {
        childEggSackRef = null;
        candidateRef.performanceData.totalTimesPregnant++;
        isPregnantAndCarryingEggs = false;
        pregnancyRefactoryTimeStepCounter = 0;

        RegisterAgentEvent(simManager.simAgeTimeSteps, "Pregnancy Complete!", 0.95f, 10);
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
    
    [SerializeField] [Range(0,1)] float restingBonusWhenResting = 0.75f;
    float restingBonus => isResting ? restingBonusWhenResting : 1f;

    public void TickActions() {
        //AgentActionState currentState = AgentActionState.Default;
        
        // Facing Direction:
        throttle = movementModule.throttle; //new Vector2(horizontalMovementInput, verticalMovementInput);        
        smoothedThrottle = Vector2.Lerp(smoothedThrottle, throttle, smoothedThrottleLerp);

        if (isDead || isEgg) {
            throttle = Vector2.zero;
            smoothedThrottle = Vector2.zero;
        }
        else 
        {
            // Food calc before energy/healing/etc? **************
            //float sizeValue = BodyGenome.GetBodySizeScore01(candidateRef.candidateGenome.bodyGenome);
            // FOOD PARTICLES: Either mouth type for now:
            float foodParticleEatAmount = simManager.vegetationManager.plantParticlesEatAmountsArray[index]; // **************** PLANT BONUS!! HACKY
            float animalParticleEatAmount = simManager.zooplanktonManager.animalParticlesEatAmountsArray[index];
            //Debug.Log(index + ", " + animalParticleEatAmount);
            bool isEatingPlant = foodParticleEatAmount > 0f;
            bool isEatingAnimal = animalParticleEatAmount > 0f;
            
            if (isEatingPlant) {
                //mouthRef.InitiatePassiveBite();
                //float sizeEfficiencyPlant = Mathf.Lerp(settings.minSizeFeedingEfficiencyDecay, settings.maxSizeFeedingEfficiencyDecay, sizeValue);
                //Debug.Log("Agent[" + index.ToString() + "], Ate Plant: " + foodParticleEatAmount.ToString());
                candidateRef.performanceData.totalFoodEatenPlant += foodParticleEatAmount; 
                EatFoodPlant(foodParticleEatAmount);                
            }

            if (isEatingAnimal) {
                //float sizeEfficiencyPlant = Mathf.Lerp(settings.minSizeFeedingEfficiencyDecay, settings.maxSizeFeedingEfficiencyDecay, sizeValue);
                candidateRef.performanceData.totalFoodEatenZoop += animalParticleEatAmount;
                //animalParticleEatAmount *= 0.98f;
                
                //Debug.Log("Agent[" + index.ToString() + "], Ate Zooplankton: " + animalParticleEatAmount.ToString());
                EatFoodMeat(animalParticleEatAmount); // * sizeEfficiencyPlant);    
            }

            mouthRef.lastBiteFoodAmount += foodParticleEatAmount + animalParticleEatAmount;

            //if(isEatingPlant || isEatingAnimal) {
            //    UseAbility(feed);
            //}
           
            SelectAction();                  
        }
        
        MovementScalingTest(smoothedThrottle);

        float rotationInRadians = (bodyRigidbody.transform.localRotation.eulerAngles.z + 90f) * Mathf.Deg2Rad;
        facingDirection = new Vector2(Mathf.Cos(rotationInRadians), Mathf.Sin(rotationInRadians));
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
        
        coreModule.TickDigestion(digestedAmountTotal);
      
        coreModule.Regenerate(healRate, energyToHealth);

        //float oxygenMask = Mathf.Clamp01(simManager.simResourceManager.curGlobalOxygen * settingsRef.agentSettings._OxygenEnergyMask);

        //ENERGY:
        float energyCostMult = settingsRef.agentSettings._BaseEnergyCost; // Mathf.Lerp(settingsRef.agentSettings._BaseEnergyCost, settingsRef.agentSettings._BaseEnergyCost * 0.25f, sizePercentage);
        float restingEnergyCost = Mathf.Pow(currentBiomass, 1.1f) * energyCostMult * restingBonus; // * SimulationManager.energyDifficultyMultiplier; // / coreModule.energyBonus;
        //float throttleMag = smoothedThrottle.magnitude;
        
        // ENERGY DRAIN::::        
        coreModule.energy -= restingEnergyCost;        
        
        // STAMINA:
        float staminaRefillRate = 0.00025f;
        float energyToStaminaConversionRate = 5f * coreModule.healthBonus;
        coreModule.stamina[0] += staminaRefillRate;// * energyToStaminaConversionRate;
        coreModule.stamina[0] = Mathf.Clamp01(coreModule.stamina[0]);
        //coreModule.energy -= staminaRefillRate; // / energyToStaminaConversionRate;
        /*
        if(coreModule.stamina[0] < 0.1f) {
            staminaRefillRate *= 0.5f;
        }
        if(coreModule.stamina[0] > 0.75f) {
            staminaRefillRate *= 1.5f;
        }
        if(isResting) {
            staminaRefillRate *= 5f;
        }*/
        /*if(coreModule.stamina[0] < 1f) {
            coreModule.stamina[0] += staminaRefillRate;
            coreModule.energy -= staminaRefillRate / energyToStaminaConversionRate;
        }
        else {
            coreModule.stamina[0] = 1f;
        }*/
        
    }
    
    public AgentActionState curActionState;

    private void SelectAction() 
    {
        if (!isFreeToAct) 
        {
            creaturePanel.UpdateAgentActionStateData(candidateRef.candidateID, curActionState);
            return;
        }
        
        curActionState = GetCurrentActionState();
        UseAbility(curActionState);
        
        creaturePanel.UpdateAgentActionStateData(candidateRef.candidateID, curActionState);
    }

    public float feedEffector => coreModule.feed[0];
    public float dashEffector => coreModule.dash[0];
    public float attackEffector => coreModule.attack[0];
    public float defendEffector => coreModule.defend[0];
    public float healEffector => coreModule.heal[0];
    
    public float GetMostActiveEffectorValue(float minimum) {
        return Mathf.Max(GetMostActiveEffectorValue(), minimum);
    }
    
    public float GetMostActiveEffectorValue() {
        float[] effectorValues = { feedEffector, attackEffector, defendEffector, dashEffector, healEffector };
        return FloatMath.GetHighest(effectorValues);
    }
    
    private AgentActionState GetCurrentActionState()
    {
        //float[] effectorValues = { 0.001f, feedEffector, attackEffector, defendEffector, dashEffector, healEffector };
        //float mostActiveEffectorValue = FloatMath.GetHighest(effectorValues);
        var mostActiveEffectorValue = GetMostActiveEffectorValue(0.001f);

        if (healEffector >= mostActiveEffectorValue) 
            return AgentActionState.Resting;
        if (feedEffector >= mostActiveEffectorValue)
            return AgentActionState.Feeding;
        if (attackEffector >= mostActiveEffectorValue) 
            return AgentActionState.Attacking;
        if (dashEffector >= mostActiveEffectorValue) 
            return AgentActionState.Dashing;
        if (defendEffector >= mostActiveEffectorValue) 
            return AgentActionState.Defending;

        return AgentActionState.Default;
    }

    private void UseAbility(AgentActionState actionState)
    {
        //Debug.Log($"Agent [{index}] {actionState}");
        if (actionState == AgentActionState.Resting) {
            candidateRef.performanceData.totalTicksRested++;
            return;
        }

        var ability = GetAbilityFromActionState(actionState);
        if (ability != null) {
            audioManager.PlayCritterAction(ownPos, actionState);
            RegisterAgentEvent(actionState);
            UseAbility(ability);
        }
        
        if (actionState == AgentActionState.Feeding) {
            return;
        }

        
    }
    
    private IAgentAbility GetAbilityFromActionState(AgentActionState actionState)
    {
        switch (actionState)
        {
            case AgentActionState.Attacking: return attack;
            case AgentActionState.Dashing: return dash;
            case AgentActionState.Defending: return defend;
            case AgentActionState.Feeding: return feed;
            default: return null;
        }
    }
    
    private void UseAbility(IAgentAbility ability)
    {
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

        float fatigueMultiplier = Mathf.Clamp01(coreModule.energy * 5f + 0.25f); // * Mathf.Clamp01(coreModule.stamina[0] * 4f + 0.05f);
        float lowHealthPenalty = Mathf.Clamp01(coreModule.health * 5f) * 0.5f + 0.5f;
        fatigueMultiplier += lowHealthPenalty;
        
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

        float swimSpeed = 12f;// * coreModule.speedBonus; // Mathf.Lerp(movementModule.smallestCreatureBaseSpeed, movementModule.largestCreatureBaseSpeed, 0.5f); // sizeValue);
        float turnRate = 7f;// * coreModule.speedBonus; //10 // Mathf.Lerp(movementModule.smallestCreatureBaseTurnRate, movementModule.largestCreatureBaseTurnRate, 0.5f) * 0.1f; // sizeValue);
        
        /*float dashBonus = 1f;
        if(isDashing) {                
            dashBonus = 4.3f;                
        }
        if(isCooldown) {
            dashBonus = 0.33f;
        }*/

        speed = swimSpeed * dashBonus * forcePenalty; // * movementModule.speedBonus ; // * restingPenalty;
        Vector2 segmentForwardDir = new Vector2(bodyRigidbody.transform.up.x, bodyRigidbody.transform.up.y).normalized;
        Vector2 forwardThrustDir = Vector2.Lerp(segmentForwardDir, throttleDir, 0.2f).normalized;
        bodyRigidbody.AddForce(forwardThrustDir * (1f - turnSharpness * 0.25f) * speed * bodyRigidbody.mass * Time.deltaTime * fatigueMultiplier * bitingPenalty, ForceMode2D.Impulse);

        // modify turning rate based on body proportions:
        //float turnRatePenalty = Mathf.Lerp(0.25f, 1f, 1f - sizeValue);

        // Head turn:
        float torqueForce = Mathf.Lerp(headTurn, headTurnSign, 0.05f) * forcePenalty * turnRate * bodyRigidbody.mass * fatigueMultiplier * bitingPenalty * Time.deltaTime;
        torqueForce = Mathf.Min(torqueForce, 50000.55f);
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
        communicationModule.Initialize(genome.bodyGenome.data.hasComms);

        coreModule = new CritterModuleCore(genome.bodyGenome.coreGenome);

        mouthRef.Initialize(genome.bodyGenome.coreGenome);

        environmentModule = new CritterModuleEnvironment(genome.bodyGenome.data);
        foodModule = new CritterModuleFood(genome.bodyGenome.data);
        friendModule = new CritterModuleFriends();
        movementModule = new CritterModuleMovement(genome);
        threatsModule = new CritterModuleThreats();
        
        unlockedTech = genome.bodyGenome.unlockedTech;
    }
    
    [ReadOnly] public UnlockedTech unlockedTech;

    public void FirstTimeInitialize() { 
        curLifeStage = AgentLifeStage.AwaitingRespawn;
        //InitializeAgentWidths(genome);
        //InitializeGameObjectsAndComponents();
        //InitializeModules(genome);  //  This breaks MapGridCell update, because coreModule doesn't exist?
    }

    // Colliders Footprint???  *****************************************************************************************

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
        bodyRigidbody.drag = 10;// 3.75f; // bodyDrag;
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
        //isResting = false;

        //mouthRef.Disable();
        //mouseclickcollider MCC
        mouseClickCollider.direction = 1; // Y-Axis ???
        mouseClickCollider.center = new Vector3(0f, 0f, 0f); // (waterLevel * 2f - 1f) * -10f); //Vector3.zero; // new Vector3(0f, 0f, 1f);
        mouseClickCollider.radius = fullSizeBoundingBox.x / 2f * sizePercentage;
        mouseClickCollider.radius *= 1.1514f; // ** TEMP
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

        ResetStartingValues();
        
        // Modules need to be created first so that Brain can map its neurons to existing modules 
        InitializeModules(genome);       
        
        ReconstructAgentGameObjects(genome, null, spawnWorldPos, true, globalWaterLevel);

        brain = new Brain(genome.brainGenome); 
        isInert = false;
    }
    
    public void InitializeSpawnAgentFromEggSack(int agentIndex, CandidateAgentData candidate, EggSack parentEgg, float globalWaterLevel) 
    {        
        index = agentIndex;
        speciesIndex = candidate.speciesID;
        candidateRef = candidate;
        AgentGenome genome = candidateRef.candidateGenome;
                
        curLifeStage = AgentLifeStage.Egg;
        parentEggSackRef = parentEgg;
                
        ResetStartingValues();    
        
        // Modules need to be created first so that Brain can map its neurons to existing modules    
        InitializeModules(genome);       

        Vector3 spawnOffset = 0.167f * parentEgg.curSize.magnitude * Random.insideUnitSphere;
        spawnOffset.z = 0f;
        
        ReconstructAgentGameObjects(genome, parentEgg, parentEgg.gameObject.transform.position + spawnOffset, false, globalWaterLevel);

        brain = new Brain(genome.brainGenome);   
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
    
    SimulationStateData simStateData => simManager.simStateData;
    
    // REVISIT CONVERSION btw fluid/scene coords and Force Amounts
    // Expose magic numbers
    public void ApplyFluidForces(int index)
    {
        Vector4 depthSample = simStateData.depthAtAgentPositionsArray[index];
        waterDepth = SimulationManager._GlobalWaterLevel - depthSample.x;
        
        bool depthSampleInitialized = depthSample.y != 0f && depthSample.z != 0f;
        depthGradient = depthSampleInitialized ? 
            new Vector2(depthSample.y, depthSample.z).normalized :
            Vector2.zero;
                                //***** world boundary *****
        if (depthSample.x > SimulationManager._GlobalWaterLevel || depthSample.w < 0.1f) //(floorDepth < agentSize)
        {
            float wallForce = 12.0f; // Mathf.Clamp01(agentSize - floorDepth) / agentSize;
            Vector2 gradient = depthGradient; // new Vector2(depthSample.y, depthSample.z); //.normalized;
            bodyRigidbody.AddForce(bodyRigidbody.mass * wallForce * -gradient, ForceMode2D.Impulse);

            float damage = wallForce * 0.015f;  
            
            if (depthSample.w < 0.51f) {
                damage *= 0.33f;
            }
            
            if (coreModule != null && isMature) 
            {
                float defendBonus = isDefending ? 0f : 1.5f;
                damage *= defendBonus;
                
                candidateRef.performanceData.totalDamageTaken += damage;
                coreModule.requestContactEvent(gradient.x, gradient.y);
                //coreModule.isContact[0] = 1f;
                //coreModule.contactForceX[0] = gradient.x;
                //coreModule.contactForceY[0] = gradient.y;
                TakeDamage(damage);
            }
        }
        
        bodyRigidbody.AddForce(simStateData.fluidVelocitiesAtAgentPositionsArray[index] * 48f * bodyRigidbody.mass, ForceMode2D.Impulse);
        avgFluidVel = Vector2.Lerp(avgFluidVel, simStateData.fluidVelocitiesAtAgentPositionsArray[index], 0.25f);
    }
}