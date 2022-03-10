using UnityEngine;
using UnityEngine.UI;

public class DebugPanelUI : MonoBehaviour 
{
    SimulationManager simulation => SimulationManager.instance;
    SimResourceManager simResourceManager => simulation.simResourceManager;
    SettingsManager settings => SettingsManager.instance;
    SettingsEnvironment environmentSettings => settings.environmentSettings;
    VegetationManager vegetationManager => simulation.vegetationManager;
    ZooplanktonManager zooplanktonManager => simulation.zooplanktonManager;
    MasterGenomePool masterGenomePool => simulation.masterGenomePool;
    
    TheRenderKing theRenderKing => TheRenderKing.instance;
    TheCursorCzar theCursorCzar => TheCursorCzar.instance;
    CameraManager cameraManager => CameraManager.instance;
    Agent agent => SelectionManager.instance.currentSelection.agent;
    UIManager uiManager => UIManager.instance;
    EnvironmentFluidManager fluidManager => EnvironmentFluidManager.instance;
    
    public bool isOpen;
    public GameObject panelDebug;

    public Material debugTextureViewerMat;
    
    private Vector4 _Zoom = new Vector4(1f, 1f, 1f, 1f);
    private float _Amplitude = 1f;
    private Vector4 _ChannelMask = new Vector4(1f, 1f, 1f, 1f);
    private int _ChannelSoloIndex = 0;
    private float _IsChannelSolo = 0f;
    private float _Gamma = 1f;
    private int _DebugTextureIndex = 0;
    //private string _DebugTextureString = "-";
        
    public Button buttonDebugTexturePrev;
    public Button buttonDebugTextureNext;
    public Slider sliderDebugTextureZoomX;
    public Slider sliderDebugTextureZoomY;
    public Slider sliderDebugTextureAmplitude;
    public Slider sliderDebugTextureSoloChannelIndex;
    public Toggle toggleDebugTextureIsSolo;
    public Slider sliderDebugTextureGamma;
    public Text textDebugTextureName;
    public Text textDebugTextureZoomX;
    public Text textDebugTextureZoomY;
    public Text textDebugTextureAmplitude;
    public Text textDebugTextureSoloChannelIndex;
    public Text textDebugTextureGamma;
    public Image imageDebugTexture;
        
    
    public Text textDebugTrainingInfo1;
    public Text textDebugTrainingInfo2;
    public Text textDebugTrainingInfo3;
    //public Text textDebugSimSettings;    

    //public Button buttonToggleDebug;

    private RenderTexture[] debugTextureViewerArray;
    
    int agentIndex;
    CandidateAgentData candidate;
    AgentGenome candidateGenome;
    CritterModuleCore coreModule;
    Brain brain;
    BodyGenome bodyGenome;
    BrainGenome brainGenome;
    CritterModuleCoreGenome coreGenome;
    CritterModuleFood foodModule;
    CritterModuleMovement movementModule;
    //CritterModuleFoodSensorsGenome foodGenome;

    // * WPP: break code sections into methods, call from here
    private void UpdateUI() {
        if (!agent) return;
     
        agentIndex = agent.index;
        candidate = agent.candidateRef;
        coreModule = agent.coreModule;
        brain = agent.brain;
        candidateGenome = candidate.candidateGenome;
        brainGenome = candidateGenome.brainGenome;
        bodyGenome = candidateGenome.bodyGenome;
        coreGenome = bodyGenome.coreGenome;
        foodModule = agent.foodModule;
        movementModule = agent.movementModule;
        //foodGenome = bodyGenome.foodGenome;
        
        if (!agent.isInert) {
            // DebugTxt1 : use this for selected creature stats:
            int curCount = 0;
            int maxCount = 1;
            if (agent.isEgg) {
                curCount = agent.lifeStageTransitionTimeStepCounter;
                maxCount = agent.gestationDurationTimeSteps;
            }
            if (agent.isMature) {
                curCount = agent.ageCounter;
                maxCount = agent.maxAgeTimeSteps;
            }
            if (agent.isDead) {
                curCount = agent.lifeStageTransitionTimeStepCounter;
                maxCount = curCount; // agentRef._DecayDurationTimeSteps;
            }
            int progressPercent = Mathf.RoundToInt((float)curCount / (float)maxCount * 100f);
            string lifeStageProgressTxt = " " + agent.curLifeStage + " " + curCount + "/" + maxCount + "  " + progressPercent + "% ";

            // &&&& INDIVIDUAL AGENT: &&&&
            string debugTxtAgent = "";            
            debugTxtAgent += "CRITTER# [" + agentIndex + "]     SPECIES# [" + agent.speciesIndex + "]\n\n";
            // Init Attributes:
            // Body:
            debugTxtAgent += "Base Size: " + coreGenome.creatureBaseLength.ToString("F2") + ",  Aspect: " + coreGenome.creatureAspectRatio.ToString("F2") + "\n"; 
            debugTxtAgent += "Fullsize Dimensions: ( " + agent.fullSizeBoundingBox.x.ToString("F2") + ", " + agent.fullSizeBoundingBox.y.ToString("F2") + ", " + agent.fullSizeBoundingBox.z.ToString("F2") + " )\n";
            debugTxtAgent += "BONUS - Damage: " + coreModule.damageBonus.ToString("F2") + ", Speed: " + coreModule.speedBonus.ToString("F2") + ", Health: " + coreModule.healthBonus.ToString("F2") + ", Energy: " + coreModule.energyBonus.ToString("F2") + "\n";
            debugTxtAgent += "DIET - Decay: " + coreModule.digestEfficiencyDecay.ToString("F2") + ", Plant: " + coreModule.digestEfficiencyPlant.ToString("F2") + ", Meat: " + coreModule.digestEfficiencyMeat.ToString("F2") + "\n";
            //string mouthType = "Active";
            //if (agentRef.mouthRef.isPassive) { mouthType = "Passive"; }
            //debugTxtAgent += "Mouth: [" + mouthType + "]\n";
            debugTxtAgent += "# Neurons: " + brain.neurons.Count + ", # Axons: " + brain.axons.Count + "\n";
            debugTxtAgent += "# In/Out Nodes: " + brainGenome.inOutNeurons.Count + ", # Hidden Nodes: " + brainGenome.hiddenNeurons.Count + ", # Links: " + brainGenome.links.Count + "\n";

            debugTxtAgent += "\nSENSORS:\n";
            debugTxtAgent += "Comms= " + bodyGenome.data.hasComms + "\n";
            debugTxtAgent += "Enviro: WaterStats: " + bodyGenome.data.useWaterStats + ", Cardinals= " + bodyGenome.data.useCardinals + ", Diagonals= " + bodyGenome.data.useDiagonals + "\n";
            //debugTxtAgent += "Food: Nutrients= " + foodGenome.useNutrients + ", Pos= " + foodGenome.usePos + ",  Dir= " + foodGenome.useDir + ",  Stats= " + foodGenome.useStats + ", useEggs: " + foodGenome.useEggs + ", useCorpse: " + foodGenome.useCorpse + "\n";
            //debugTxtAgent += "Friend: Pos= " + bodyGenome.friendGenome.usePos + ",  Dir= " + bodyGenome.friendGenome.useDir + ",  Vel= " + bodyGenome.friendGenome.useVel + "\n";
            //debugTxtAgent += "Threat: Pos= " + bodyGenome.threatGenome.usePos + ",  Dir= " + bodyGenome.threatGenome.useDir + ",  Vel= " + bodyGenome.threatGenome.useVel + ",  Stats= " + bodyGenome.threatGenome.useStats + "\n";
            // Realtime Values:
            debugTxtAgent += "\nREALTIME DATA:";
            //debugTxtAgent += "\nExp: " + agentRef.totalExperience.ToString("F2") + ",  fitnessScore: " + agentRef.masterFitnessScore.ToString("F2") + ", LVL: " + agentRef.curLevel.ToString();
            debugTxtAgent += "\n(" + lifeStageProgressTxt + ") Growth: " + (agent.sizePercentage * 100f).ToString("F0") + "%, Age: " + agent.ageCounter + " Frames\n\n";
                        
            debugTxtAgent += "Nearest Food: [" + foodModule.nearestFoodParticleIndex +
                        "] Amount: " + foodModule.nearestFoodParticleAmount.ToString("F4") +
                        "\nPos: ( " + foodModule.nearestFoodParticlePos.x.ToString("F2") +
                        ", " + foodModule.nearestFoodParticlePos.y.ToString("F2") +
                        " ), Dir: ( " + foodModule.foodPlantDirX[0].ToString("F2") +
                        ", " + foodModule.foodPlantDirY[0].ToString("F2") + " )" +
                        "\n";
            debugTxtAgent += "\nNutrients: " + foodModule.nutrientDensity[0].ToString("F4") + ", Stamina: " + coreModule.stamina[0].ToString("F3") + "\n";
            debugTxtAgent += "Gradient Dir: (" + foodModule.nutrientGradX[0].ToString("F2") + ", " + foodModule.nutrientGradY[0].ToString("F2") + ")\n";
            //debugTxtAgent += "Total Food Eaten -- Decay: n/a, Plant: " + agentRef.totalFoodEatenPlant.ToString("F2") + ", Meat: " + agentRef.totalFoodEatenZoop.ToString("F2") + "\nFood Stored: " + agentRef.coreModule.foodStored[0].ToString() + ", Corpse Food Amount: " + agentRef.currentBiomass.ToString("F3") + "\n";

            //debugTxtAgent += "\nFullSize: " + agentRef.fullSizeBoundingBox.ToString() + ", Volume: " + agentRef.fullSizeBodyVolume.ToString() + "\n";
            //debugTxtAgent += "( " + (agentRef.sizePercentage * 100f).ToString("F0") + "% )\n";

            debugTxtAgent += "\nCurVel: " + agent.curVel.ToString("F3") + ", CurAccel: " + agent.curAccel.ToString("F3") + ", AvgVel: " + agent.avgVel.ToString("F3") + "\n";

            debugTxtAgent += "\nWater Depth: " + agent.waterDepth.ToString("F3") + ", Vel: " + (agent.avgFluidVel * 10f).ToString("F3") + "\n";
            debugTxtAgent += "Throttle: [ " + movementModule.throttleX[0].ToString("F3") + ", " + movementModule.throttleY[0].ToString("F3") + " ]\n";
            debugTxtAgent += "FeedEffector: " + coreModule.mouthFeedEffector[0].ToString("F2") + "\n";
            debugTxtAgent += "AttackEffector: " + coreModule.mouthAttackEffector[0].ToString("F2") + "\n";
            debugTxtAgent += "DefendEffector: " + coreModule.defendEffector[0].ToString("F2") + "\n";
            debugTxtAgent += "DashEffector: " + coreModule.dashEffector[0].ToString("F2") + "\n";
            debugTxtAgent += "HealEffector: " + coreModule.healEffector[0].ToString("F2") + "\n";
            
            //+++++++++++++++++++++++++++++++++++++ CRITTER: ++++++++++++++++++++++++++++++++++++++++++++
            string debugTxtGlobalSim = "";
            debugTxtGlobalSim += "\n\nNumChildrenBorn: " + simulation.numAgentsBorn + ", numDied: " + simulation.numAgentsDied + ", ~Gen: " + ((float)simulation.numAgentsBorn / (float)simulation.numAgents);
            debugTxtGlobalSim += "\nSimulation Age: " + simulation.simAgeTimeSteps;
            debugTxtGlobalSim += "\nYear " + simulation.curSimYear + "\n\n";
            int numActiveSpecies = masterGenomePool.currentlyActiveSpeciesIDList.Count;
            debugTxtGlobalSim += numActiveSpecies + " Active Species:\n";
            
            for (int s = 0; s < numActiveSpecies; s++) {
                int speciesID = masterGenomePool.currentlyActiveSpeciesIDList[s];
                //int parentSpeciesID = simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].parentSpeciesID;
                int numCandidates = masterGenomePool.completeSpeciesPoolsList[speciesID].candidateGenomesList.Count;
                int numLeaders = masterGenomePool.completeSpeciesPoolsList[speciesID].leaderboardGenomesList.Count;
                //int numBorn = simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].numAgentsEvaluated;
                int speciesPopSize = 0;
                //float avgFitness = simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgPerformanceData.totalTicksAlive;
                for (int a = 0; a < simulation.numAgents; a++) {
                    if (simulation.agents[a].speciesIndex == speciesID) {
                        speciesPopSize++;
                    }
                }
                if(masterGenomePool.completeSpeciesPoolsList[speciesID].isFlaggedForExtinction) {
                    debugTxtGlobalSim += "xxx ";
                }
                /*debugTxtGlobalSim += "Species[" + speciesID.ToString() + "] p(" + parentSpeciesID.ToString() + "), size: " + speciesPopSize.ToString() + ", #cands: " + numCandidates.ToString() + ", numEvals: " + numBorn.ToString() + 
                             ",   avgFitness: " + avgFitness.ToString("F2") + 
                             ",   avgConsumption: (" + simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgFoodEatenCorpse.ToString("F4") + ", " + simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgFoodEatenPlant.ToString("F4") + ", " + simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgFoodEatenZoop.ToString("F4") + ", " + simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgFoodEatenEgg.ToString("F4") + ", " + simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgFoodEatenCreature.ToString("F4") +
                             "),   avgBodySize: " + simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgBodySize.ToString("F3") +
                             ",   avgTalentSpec: (" + simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgSpecAttack.ToString("F2") + ", " + simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgSpecDefend.ToString("F2") + ", " + simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgSpecSpeed.ToString("F2") + ", " + simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgSpecUtility.ToString("F2") +
                             "),   avgDiet: (" + simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgFoodSpecDecay.ToString("F2") + ", " + simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgFoodSpecPlant.ToString("F2") + ", " + simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgFoodSpecMeat.ToString("F2") +
                             "),   avgNumNeurons: " + simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgNumNeurons.ToString("F1") +
                             ",   avgNumAxons: " + simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgNumAxons.ToString("F1") +
                             ", total: " + simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgFitnessScore.ToString("F2") +
                             ", avgExp: " + simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgExperience.ToString() + "\n\n";*/
            }

            /*debugTxtGlobalSim += "\n\nAll-Time Species List:\n";
            for (int p = 0; p < simManager.masterGenomePool.completeSpeciesPoolsList.Count; p++) {
                string extString = "Active!";
                if (simManager.masterGenomePool.completeSpeciesPoolsList[p].isExtinct) {
                    extString = "Extinct!";
                }
                debugTxtGlobalSim += "Species[" + p.ToString() + "] p(" + simManager.masterGenomePool.completeSpeciesPoolsList[p].parentSpeciesID.ToString() + ") " + extString + "\n";
            }*/
            
            textDebugTrainingInfo1.text = debugTxtAgent;
            textDebugTrainingInfo3.text = debugTxtGlobalSim;
        } 
        
        string debugTxtResources = "";
        debugTxtResources += "GLOBAL RESOURCES:\n";
        debugTxtResources += "\nSunlight: " + environmentSettings._BaseSolarEnergy;
        debugTxtResources += "\nOxygen: " + simResourceManager.curGlobalOxygen;
        debugTxtResources += "\n     + " + simResourceManager.oxygenProducedByAlgaeReservoirLastFrame + " ( algae reservoir )";
        debugTxtResources += "\n     + " + simResourceManager.oxygenProducedByPlantParticlesLastFrame + " ( algae particles )";
        debugTxtResources += "\n     - " + simResourceManager.oxygenUsedByDecomposersLastFrame + " ( decomposers )";
        debugTxtResources += "\n     - " + simResourceManager.oxygenUsedByAnimalParticlesLastFrame + " ( zooplankton )";
        debugTxtResources += "\n     - " + simResourceManager.oxygenUsedByAgentsLastFrame + " ( agents )";
        debugTxtResources += "\nNutrients: " + simResourceManager.curGlobalNutrients;
        debugTxtResources += "\n     + " + simResourceManager.nutrientsProducedByDecomposersLastFrame + " ( decomposers )";
        debugTxtResources += "\n     - " + simResourceManager.nutrientsUsedByAlgaeReservoirLastFrame + " ( algae reservoir )";
        debugTxtResources += "\n     - " + simResourceManager.nutrientsUsedByPlantParticlesLastFrame + " ( algae particles )";
        debugTxtResources += "\nDetritus: " + simResourceManager.curGlobalDetritus;
        debugTxtResources += "\n     + " + simResourceManager.wasteProducedByAlgaeReservoirLastFrame + " ( algae reservoir )";
        debugTxtResources += "\n     + " + simResourceManager.wasteProducedByPlantParticlesLastFrame + " ( algae particles )";
        debugTxtResources += "\n     + " + simResourceManager.wasteProducedByAnimalParticlesLastFrame + " ( zooplankton )";
        debugTxtResources += "\n     + " + simResourceManager.wasteProducedByAgentsLastFrame + " ( agents )";
        debugTxtResources += "\n     - " + simResourceManager.detritusRemovedByDecomposersLastFrame + " ( decomposers )";
        debugTxtResources += "\nDecomposers: " + simResourceManager.curGlobalDecomposers;
        debugTxtResources += "\nAlgae (Reservoir): " + simResourceManager.curGlobalAlgaeReservoir;
        debugTxtResources += "\nAlgae (Particles): " + simResourceManager.curGlobalPlantParticles;
        debugTxtResources += "\nZooplankton: " + simResourceManager.curGlobalAnimalParticles;
        debugTxtResources += "\nLive Agents: " + simResourceManager.curGlobalAgentBiomass;
        debugTxtResources += "\nDead Agents: " + simResourceManager.curGlobalCarrionVolume;
        debugTxtResources += "\nEggSacks: " + simResourceManager.curGlobalEggSackVolume;
        debugTxtResources += "\nGlobal Mass: " + simResourceManager.curTotalMass;
        Vector4 resourceGridSample = simulation.SampleTexture(vegetationManager.resourceGridRT1, theCursorCzar.curMousePositionOnWaterPlane / SimulationManager._MapSize);
        Vector4 simTansferSample = simulation.SampleTexture(vegetationManager.resourceSimTransferRT, theCursorCzar.curMousePositionOnWaterPlane / SimulationManager._MapSize) * 100f;
        //Debug.Log("curMousePositionOnWaterPlane: " + curMousePositionOnWaterPlane.ToString());
        debugTxtResources += "\nresourceGridSample: (" + resourceGridSample.x.ToString("F4") + ", " + resourceGridSample.y.ToString("F4") + ", " + resourceGridSample.z.ToString("F4") + ", " + resourceGridSample.w.ToString("F4") + ")";
        debugTxtResources += "\nsimTansferSample: (" + simTansferSample.x.ToString("F4") + ", " + simTansferSample.y.ToString("F4") + ", " + simTansferSample.z.ToString("F4") + ", " + simTansferSample.w.ToString("F4") + ")";

        textDebugTrainingInfo2.text = debugTxtResources;

        if(debugTextureViewerArray == null) {
            CreateDebugRenderViewerArray();
        }
        
        debugTextureViewerMat.SetPass(0);
        debugTextureViewerMat.SetVector("_Zoom", _Zoom);
        debugTextureViewerMat.SetFloat("_Amplitude", _Amplitude);
        debugTextureViewerMat.SetVector("_ChannelMask", _ChannelMask);
        debugTextureViewerMat.SetInt("_ChannelSoloIndex", _ChannelSoloIndex);
        debugTextureViewerMat.SetFloat("_IsChannelSolo", _IsChannelSolo);
        debugTextureViewerMat.SetFloat("_Gamma", _Gamma);        
        //debugTextureViewerMat.
        if(debugTextureViewerArray[_DebugTextureIndex]) {
            debugTextureViewerMat.SetTexture("_MainTex", debugTextureViewerArray[_DebugTextureIndex]);
            int channelID = 4;
            string[] channelLabelTxt = new string[5];            
            channelLabelTxt[0] = " (X Solo)";
            channelLabelTxt[1] = " (Y Solo)";
            channelLabelTxt[2] = " (Z Solo)";
            channelLabelTxt[3] = " (W Solo)";
            channelLabelTxt[4] = " (Color)";
            if(_IsChannelSolo > 0.5f) {
                channelID = _ChannelSoloIndex;
            }
            textDebugTextureName.text = debugTextureViewerArray[_DebugTextureIndex].name + channelLabelTxt[channelID];
        }
        
        textDebugTextureZoomX.text = _Zoom.x.ToString();
        textDebugTextureZoomY.text = _Zoom.y.ToString();
        textDebugTextureAmplitude.text = _Amplitude.ToString();
        textDebugTextureSoloChannelIndex.text = _ChannelSoloIndex.ToString();
        textDebugTextureGamma.text = _Gamma.ToString();
    }
    
    public void UpdateDebugUI() {
        panelDebug.SetActive(isOpen);
        if(isOpen) {            
            UpdateUI();
        }        
    }    
    
    private void CreateDebugRenderViewerArray() {
        debugTextureViewerArray = new RenderTexture[13];
        debugTextureViewerArray[0] = theRenderKing.baronVonTerrain.terrainHeightDataRT;
        debugTextureViewerArray[0].name = "Terrain Height Data";
        //if (gameManager.theRenderKing.baronVonTerrain.terrainColorRT0 != null) {
        debugTextureViewerArray[1] = theRenderKing.baronVonTerrain.terrainColorRT0;
        debugTextureViewerArray[1].name = "Terrain Color";

        debugTextureViewerArray[2] = theRenderKing.baronVonWater.waterSurfaceDataRT0;
        debugTextureViewerArray[2].name = "Water Surface Data";

        debugTextureViewerArray[3] = fluidManager._VelocityPressureDivergenceMain;
        debugTextureViewerArray[3].name = "_VelocityPressureDivergenceMain";

        debugTextureViewerArray[4] = fluidManager._VelocityPressureDivergenceMain;
        debugTextureViewerArray[4].name = "_VelocityPressureDivergenceMain";

        debugTextureViewerArray[5] = fluidManager._VelocityPressureDivergenceMain;
        debugTextureViewerArray[5].name = "_VelocityPressureDivergenceMain";

        debugTextureViewerArray[6] = fluidManager._VelocityPressureDivergenceMain;
        debugTextureViewerArray[6].name = "_VelocityPressureDivergenceMain";

        debugTextureViewerArray[7] = fluidManager._ObstaclesRT;
        debugTextureViewerArray[7].name = "Solid Obstacles Render";

        debugTextureViewerArray[8] = vegetationManager.critterNearestPlants32;
        debugTextureViewerArray[8].name = "critterNearestPlants32";        
        
        debugTextureViewerArray[9] = zooplanktonManager.critterNearestZooplankton32;
        debugTextureViewerArray[9].name = "critterNearestZooplankton32";

        debugTextureViewerArray[10] = vegetationManager.resourceGridRT1;
        debugTextureViewerArray[10].name = "Resources Grid";

        //}
        //if(gameManager.theRenderKing.spiritBrushRT != null) {
        debugTextureViewerArray[11] = theRenderKing.spiritBrushRT;
        debugTextureViewerArray[11].name = "Spirit Brush";
        //}        
        //if(gameManager.simulationManager.environmentFluidManager._DensityA != null) {
        //debugTextureViewerArray[3] = gameManager.simulationManager.environmentFluidManager._DensityA;
        //debugTextureViewerArray[3].name = "Water DensityA";
        //}        
        
        debugTextureViewerArray[12] = simulation.vegetationManager.resourceSimTransferRT;
        debugTextureViewerArray[12].name = "Resource Sim Transfer";
    }
    
    public void ClickDebugTexturePrev() {
        if(debugTextureViewerArray == null) {
            CreateDebugRenderViewerArray();
        }

        _DebugTextureIndex -= 1;
        if(_DebugTextureIndex < 0) {
            _DebugTextureIndex = debugTextureViewerArray.Length - 1;
        }
        
        imageDebugTexture.enabled = false;
        imageDebugTexture.enabled = true;
        debugTextureViewerMat.SetTexture("_MainTex", debugTextureViewerArray[_DebugTextureIndex]);
    }
    
    public void ClickDebugTextureNext() {
        if(debugTextureViewerArray == null) {
            CreateDebugRenderViewerArray();
        }

        _DebugTextureIndex += 1;
        if(_DebugTextureIndex > debugTextureViewerArray.Length - 1) {
            _DebugTextureIndex = 0;
        }
        
        imageDebugTexture.enabled = false;
        imageDebugTexture.enabled = true;
        debugTextureViewerMat.SetTexture("_MainTex", debugTextureViewerArray[_DebugTextureIndex]);
    }
    
    public void SliderDebugTextureZoomX(float val) {
        _Zoom.x = val;
    }
    
    public void SliderDebugTextureZoomY(float val) {
        _Zoom.y = val;
    }
    
    public void SliderDebugTextureAmplitude(float val) {
        _Amplitude = val;
    }
    
    public void SliderDebugTextureChannelSoloIndex(float val) {
        _ChannelSoloIndex = Mathf.RoundToInt(val);
        debugTextureViewerMat.SetInt("_ChannelSoloIndex", _ChannelSoloIndex);
        imageDebugTexture.enabled = false;
        imageDebugTexture.enabled = true;
    }
    
    public void ToggleDebugTextureIsSolo(bool val) {
        _IsChannelSolo = _IsChannelSolo > 0.5f ? 0f : 1f;

        debugTextureViewerMat.SetFloat("_IsChannelSolo", (float)_IsChannelSolo);
        imageDebugTexture.enabled = false;
        imageDebugTexture.enabled = true;
    }
    
    public void SliderDebugTextureGamma(float val) {
        _Gamma = val;
    }

    public void ClickButtonToggleDebug() {
        isOpen = !isOpen;
        panelDebug.SetActive(isOpen);
    }
}
