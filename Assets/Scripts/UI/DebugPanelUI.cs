using UnityEngine;
using UnityEngine.UI;

public class DebugPanelUI : MonoBehaviour {
    public UIManager uiManagerRef;
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

    TheCursorCzar theCursorCzar => TheCursorCzar.instance;
	
    private void UpdateUI() {
        // DISABLED!!!! -- Need to establish good method for grabbing data from SimulationManager!
        SimulationManager simManager = uiManagerRef.gameManager.simulationManager;

        Agent agentRef = uiManagerRef.cameraManager.targetAgent;  

        // WPP: exit early if no target agent
        if (!agentRef) return;
     
        int agentIndex = agentRef.index;

        if (!agentRef.isInert) {
            // DebugTxt1 : use this for selected creature stats:
            int curCount = 0;
            int maxCount = 1;
            if (agentRef.curLifeStage == Agent.AgentLifeStage.Egg) {
                curCount = agentRef.lifeStageTransitionTimeStepCounter;
                maxCount = agentRef._GestationDurationTimeSteps;
            }
            if (agentRef.curLifeStage == Agent.AgentLifeStage.Mature) {
                curCount = agentRef.ageCounter;
                maxCount = agentRef.maxAgeTimeSteps;
            }
            if (agentRef.curLifeStage == Agent.AgentLifeStage.Dead) {
                curCount = agentRef.lifeStageTransitionTimeStepCounter;
                maxCount = curCount; // agentRef._DecayDurationTimeSteps;
            }
            int progressPercent = Mathf.RoundToInt((float)curCount / (float)maxCount * 100f);
            string lifeStageProgressTxt = " " + agentRef.curLifeStage.ToString() + " " + curCount.ToString() + "/" + maxCount.ToString() + "  " + progressPercent.ToString() + "% ";

            // &&&& INDIVIDUAL AGENT: &&&&
            string debugTxtAgent = "";            
            debugTxtAgent += "CRITTER# [" + agentIndex.ToString() + "]     SPECIES# [" + agentRef.speciesIndex.ToString() + "]\n\n";
            // Init Attributes:
            // Body:
            debugTxtAgent += "Base Size: " + agentRef.candidateRef.candidateGenome.bodyGenome.coreGenome.creatureBaseLength.ToString("F2") + ",  Aspect: " + agentRef.candidateRef.candidateGenome.bodyGenome.coreGenome.creatureAspectRatio.ToString("F2") + "\n"; 
            debugTxtAgent += "Fullsize Dimensions: ( " + agentRef.fullSizeBoundingBox.x.ToString("F2") + ", " + agentRef.fullSizeBoundingBox.y.ToString("F2") + ", " + agentRef.fullSizeBoundingBox.z.ToString("F2") + " )\n";
            debugTxtAgent += "BONUS - Damage: " + agentRef.coreModule.damageBonus.ToString("F2") + ", Speed: " + agentRef.coreModule.speedBonus.ToString("F2") + ", Health: " + agentRef.coreModule.healthBonus.ToString("F2") + ", Energy: " + agentRef.coreModule.energyBonus.ToString("F2") + "\n";
            debugTxtAgent += "DIET - Decay: " + agentRef.coreModule.foodEfficiencyDecay.ToString("F2") + ", Plant: " + agentRef.coreModule.foodEfficiencyPlant.ToString("F2") + ", Meat: " + agentRef.coreModule.foodEfficiencyMeat.ToString("F2") + "\n";
            //string mouthType = "Active";
            //if (agentRef.mouthRef.isPassive) { mouthType = "Passive"; }
            //debugTxtAgent += "Mouth: [" + mouthType + "]\n";
            debugTxtAgent += "# Neurons: " + agentRef.brain.neuronList.Count.ToString() + ", # Axons: " + agentRef.brain.axonList.Count.ToString() + "\n";
            debugTxtAgent += "# In/Out Nodes: " + agentRef.candidateRef.candidateGenome.brainGenome.bodyNeuronList.Count.ToString() + ", # Hidden Nodes: " + agentRef.candidateRef.candidateGenome.brainGenome.hiddenNeuronList.Count.ToString() + ", # Links: " + agentRef.candidateRef.candidateGenome.brainGenome.linkList.Count.ToString() + "\n";

            debugTxtAgent += "\nSENSORS:\n";
            debugTxtAgent += "Comms= " + agentRef.candidateRef.candidateGenome.bodyGenome.communicationGenome.useComms.ToString() + "\n";
            debugTxtAgent += "Enviro: WaterStats: " + agentRef.candidateRef.candidateGenome.bodyGenome.environmentalGenome.useWaterStats.ToString() + ", Cardinals= " + agentRef.candidateRef.candidateGenome.bodyGenome.environmentalGenome.useCardinals.ToString() + ", Diagonals= " + agentRef.candidateRef.candidateGenome.bodyGenome.environmentalGenome.useDiagonals.ToString() + "\n";
            CritterModuleFoodSensorsGenome foodGenome = agentRef.candidateRef.candidateGenome.bodyGenome.foodGenome;
            debugTxtAgent += "Food: Nutrients= " + foodGenome.useNutrients.ToString() + ", Pos= " + foodGenome.usePos.ToString() + ",  Dir= " + foodGenome.useDir.ToString() + ",  Stats= " + foodGenome.useStats.ToString() + ", useEggs: " + foodGenome.useEggs.ToString() + ", useCorpse: " + foodGenome.useCorpse.ToString() + "\n";
            debugTxtAgent += "Friend: Pos= " + agentRef.candidateRef.candidateGenome.bodyGenome.friendGenome.usePos.ToString() + ",  Dir= " + agentRef.candidateRef.candidateGenome.bodyGenome.friendGenome.useDir.ToString() + ",  Vel= " + agentRef.candidateRef.candidateGenome.bodyGenome.friendGenome.useVel.ToString() + "\n";
            debugTxtAgent += "Threat: Pos= " + agentRef.candidateRef.candidateGenome.bodyGenome.threatGenome.usePos.ToString() + ",  Dir= " + agentRef.candidateRef.candidateGenome.bodyGenome.threatGenome.useDir.ToString() + ",  Vel= " + agentRef.candidateRef.candidateGenome.bodyGenome.threatGenome.useVel.ToString() + ",  Stats= " + agentRef.candidateRef.candidateGenome.bodyGenome.threatGenome.useStats.ToString() + "\n";
            // Realtime Values:
            debugTxtAgent += "\nREALTIME DATA:";
            //debugTxtAgent += "\nExp: " + agentRef.totalExperience.ToString("F2") + ",  fitnessScore: " + agentRef.masterFitnessScore.ToString("F2") + ", LVL: " + agentRef.curLevel.ToString();
            debugTxtAgent += "\n(" + lifeStageProgressTxt + ") Growth: " + (agentRef.sizePercentage * 100f).ToString("F0") + "%, Age: " + agentRef.ageCounter.ToString() + " Frames\n\n";
                        
            debugTxtAgent += "Nearest Food: [" + agentRef.foodModule.nearestFoodParticleIndex.ToString() +
                        "] Amount: " + agentRef.foodModule.nearestFoodParticleAmount.ToString("F4") +
                        "\nPos: ( " + agentRef.foodModule.nearestFoodParticlePos.x.ToString("F2") +
                        ", " + agentRef.foodModule.nearestFoodParticlePos.y.ToString("F2") +
                        " ), Dir: ( " + agentRef.foodModule.foodPlantDirX[0].ToString("F2") +
                        ", " + agentRef.foodModule.foodPlantDirY[0].ToString("F2") + " )" +
                        "\n";
            debugTxtAgent += "\nNutrients: " + agentRef.foodModule.nutrientDensity[0].ToString("F4") + ", Stamina: " + agentRef.coreModule.stamina[0].ToString("F3") + "\n";
            debugTxtAgent += "Gradient Dir: (" + agentRef.foodModule.nutrientGradX[0].ToString("F2") + ", " + agentRef.foodModule.nutrientGradY[0].ToString("F2") + ")\n";
            //debugTxtAgent += "Total Food Eaten -- Decay: n/a, Plant: " + agentRef.totalFoodEatenPlant.ToString("F2") + ", Meat: " + agentRef.totalFoodEatenZoop.ToString("F2") + "\nFood Stored: " + agentRef.coreModule.foodStored[0].ToString() + ", Corpse Food Amount: " + agentRef.currentBiomass.ToString("F3") + "\n";

            //debugTxtAgent += "\nFullSize: " + agentRef.fullSizeBoundingBox.ToString() + ", Volume: " + agentRef.fullSizeBodyVolume.ToString() + "\n";
            //debugTxtAgent += "( " + (agentRef.sizePercentage * 100f).ToString("F0") + "% )\n";

            debugTxtAgent += "\nCurVel: " + agentRef.curVel.ToString("F3") + ", CurAccel: " + agentRef.curAccel.ToString("F3") + ", AvgVel: " + agentRef.avgVel.ToString("F3") + "\n";

            debugTxtAgent += "\nWater Depth: " + agentRef.waterDepth.ToString("F3") + ", Vel: " + (agentRef.avgFluidVel * 10f).ToString("F3") + "\n";
            debugTxtAgent += "Throttle: [ " + agentRef.movementModule.throttleX[0].ToString("F3") + ", " + agentRef.movementModule.throttleY[0].ToString("F3") + " ]\n";
            debugTxtAgent += "FeedEffector: " + agentRef.coreModule.mouthFeedEffector[0].ToString("F2") + "\n";
            debugTxtAgent += "AttackEffector: " + agentRef.coreModule.mouthAttackEffector[0].ToString("F2") + "\n";
            debugTxtAgent += "DefendEffector: " + agentRef.coreModule.defendEffector[0].ToString("F2") + "\n";
            debugTxtAgent += "DashEffector: " + agentRef.coreModule.dashEffector[0].ToString("F2") + "\n";
            debugTxtAgent += "HealEffector: " + agentRef.coreModule.healEffector[0].ToString("F2") + "\n";
            
            //+++++++++++++++++++++++++++++++++++++ CRITTER: ++++++++++++++++++++++++++++++++++++++++++++
            string debugTxtGlobalSim = "";
            debugTxtGlobalSim += "\n\nNumChildrenBorn: " + simManager.numAgentsBorn.ToString() + ", numDied: " + simManager.numAgentsDied.ToString() + ", ~Gen: " + ((float)simManager.numAgentsBorn / (float)simManager._NumAgents).ToString();
            debugTxtGlobalSim += "\nSimulation Age: " + simManager.simAgeTimeSteps.ToString();
            debugTxtGlobalSim += "\nYear " + simManager.curSimYear.ToString() + "\n\n";
            int numActiveSpecies = simManager.masterGenomePool.currentlyActiveSpeciesIDList.Count;
            debugTxtGlobalSim += numActiveSpecies.ToString() + " Active Species:\n";
            for (int s = 0; s < numActiveSpecies; s++) {
                int speciesID = simManager.masterGenomePool.currentlyActiveSpeciesIDList[s];
                //int parentSpeciesID = simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].parentSpeciesID;
                int numCandidates = simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].candidateGenomesList.Count;
                int numLeaders = simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].leaderboardGenomesList.Count;
                //int numBorn = simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].numAgentsEvaluated;
                int speciesPopSize = 0;
                //float avgFitness = simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].avgPerformanceData.totalTicksAlive;
                for (int a = 0; a < simManager._NumAgents; a++) {
                    if (simManager.agentsArray[a].speciesIndex == speciesID) {
                        speciesPopSize++;
                    }
                }
                if(simManager.masterGenomePool.completeSpeciesPoolsList[speciesID].isFlaggedForExtinction) {
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
        debugTxtResources += "\nSunlight: " + simManager.settingsManager.environmentSettings._BaseSolarEnergy.ToString();
        debugTxtResources += "\nOxygen: " + simManager.simResourceManager.curGlobalOxygen.ToString();
        debugTxtResources += "\n     + " + simManager.simResourceManager.oxygenProducedByAlgaeReservoirLastFrame.ToString() + " ( algae reservoir )";
        debugTxtResources += "\n     + " + simManager.simResourceManager.oxygenProducedByPlantParticlesLastFrame.ToString() + " ( algae particles )";
        debugTxtResources += "\n     - " + simManager.simResourceManager.oxygenUsedByDecomposersLastFrame.ToString() + " ( decomposers )";
        debugTxtResources += "\n     - " + simManager.simResourceManager.oxygenUsedByAnimalParticlesLastFrame.ToString() + " ( zooplankton )";
        debugTxtResources += "\n     - " + simManager.simResourceManager.oxygenUsedByAgentsLastFrame.ToString() + " ( agents )";
        debugTxtResources += "\nNutrients: " + simManager.simResourceManager.curGlobalNutrients.ToString();
        debugTxtResources += "\n     + " + simManager.simResourceManager.nutrientsProducedByDecomposersLastFrame.ToString() + " ( decomposers )";
        debugTxtResources += "\n     - " + simManager.simResourceManager.nutrientsUsedByAlgaeReservoirLastFrame.ToString() + " ( algae reservoir )";
        debugTxtResources += "\n     - " + simManager.simResourceManager.nutrientsUsedByPlantParticlesLastFrame.ToString() + " ( algae particles )";
        debugTxtResources += "\nDetritus: " + simManager.simResourceManager.curGlobalDetritus.ToString();
        debugTxtResources += "\n     + " + simManager.simResourceManager.wasteProducedByAlgaeReservoirLastFrame.ToString() + " ( algae reservoir )";
        debugTxtResources += "\n     + " + simManager.simResourceManager.wasteProducedByPlantParticlesLastFrame.ToString() + " ( algae particles )";
        debugTxtResources += "\n     + " + simManager.simResourceManager.wasteProducedByAnimalParticlesLastFrame.ToString() + " ( zooplankton )";
        debugTxtResources += "\n     + " + simManager.simResourceManager.wasteProducedByAgentsLastFrame.ToString() + " ( agents )";
        debugTxtResources += "\n     - " + simManager.simResourceManager.detritusRemovedByDecomposersLastFrame.ToString() + " ( decomposers )";
        debugTxtResources += "\nDecomposers: " + simManager.simResourceManager.curGlobalDecomposers.ToString();
        debugTxtResources += "\nAlgae (Reservoir): " + simManager.simResourceManager.curGlobalAlgaeReservoir.ToString();
        debugTxtResources += "\nAlgae (Particles): " + simManager.simResourceManager.curGlobalPlantParticles.ToString();
        debugTxtResources += "\nZooplankton: " + simManager.simResourceManager.curGlobalAnimalParticles.ToString();
        debugTxtResources += "\nLive Agents: " + simManager.simResourceManager.curGlobalAgentBiomass.ToString();
        debugTxtResources += "\nDead Agents: " + simManager.simResourceManager.curGlobalCarrionVolume.ToString();
        debugTxtResources += "\nEggSacks: " + simManager.simResourceManager.curGlobalEggSackVolume.ToString();
        debugTxtResources += "\nGlobal Mass: " + simManager.simResourceManager.curTotalMass.ToString();
        Vector4 resourceGridSample = uiManagerRef.SampleTexture(simManager.vegetationManager.resourceGridRT1, theCursorCzar.curMousePositionOnWaterPlane / SimulationManager._MapSize);
        Vector4 simTansferSample = uiManagerRef.SampleTexture(simManager.vegetationManager.resourceSimTransferRT, theCursorCzar.curMousePositionOnWaterPlane / SimulationManager._MapSize) * 100f;
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
        debugTextureViewerMat.SetFloat("_IsChannelSolo", (float)_IsChannelSolo);
        debugTextureViewerMat.SetFloat("_Gamma", _Gamma);        
        //debugTextureViewerMat.
        if(debugTextureViewerArray[_DebugTextureIndex] != null) {
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
        debugTextureViewerArray[0] = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.terrainHeightDataRT;
        debugTextureViewerArray[0].name = "Terrain Height Data";
        //if (gameManager.theRenderKing.baronVonTerrain.terrainColorRT0 != null) {
        debugTextureViewerArray[1] = uiManagerRef.gameManager.theRenderKing.baronVonTerrain.terrainColorRT0;
        debugTextureViewerArray[1].name = "Terrain Color";

        debugTextureViewerArray[2] = uiManagerRef.gameManager.theRenderKing.baronVonWater.waterSurfaceDataRT0;
        debugTextureViewerArray[2].name = "Water Surface Data";

        debugTextureViewerArray[3] = uiManagerRef.gameManager.theRenderKing.fluidManager._VelocityPressureDivergenceMain;
        debugTextureViewerArray[3].name = "_VelocityPressureDivergenceMain";

        debugTextureViewerArray[4] = uiManagerRef.gameManager.theRenderKing.fluidManager._VelocityPressureDivergenceMain;
        debugTextureViewerArray[4].name = "_VelocityPressureDivergenceMain";

        debugTextureViewerArray[5] = uiManagerRef.gameManager.theRenderKing.fluidManager._VelocityPressureDivergenceMain;
        debugTextureViewerArray[5].name = "_VelocityPressureDivergenceMain";

        debugTextureViewerArray[6] = uiManagerRef.gameManager.theRenderKing.fluidManager._VelocityPressureDivergenceMain;
        debugTextureViewerArray[6].name = "_VelocityPressureDivergenceMain";

        debugTextureViewerArray[7] = uiManagerRef.gameManager.theRenderKing.fluidManager._ObstaclesRT;
        debugTextureViewerArray[7].name = "Solid Obstacles Render";

        debugTextureViewerArray[8] = uiManagerRef.gameManager.simulationManager.vegetationManager.critterNearestPlants32;
        debugTextureViewerArray[8].name = "critterNearestPlants32";        
        
        debugTextureViewerArray[9] = uiManagerRef.gameManager.simulationManager.zooplanktonManager.critterNearestZooplankton32;
        debugTextureViewerArray[9].name = "critterNearestZooplankton32";

        debugTextureViewerArray[10] = uiManagerRef.gameManager.simulationManager.vegetationManager.resourceGridRT1;
        debugTextureViewerArray[10].name = "Resources Grid";

        
        //}
        //if(gameManager.theRenderKing.spiritBrushRT != null) {
        debugTextureViewerArray[11] = uiManagerRef.gameManager.theRenderKing.spiritBrushRT;
        debugTextureViewerArray[11].name = "Spirit Brush";
        //}        
        //if(gameManager.simulationManager.environmentFluidManager._DensityA != null) {
        //debugTextureViewerArray[3] = gameManager.simulationManager.environmentFluidManager._DensityA;
        //debugTextureViewerArray[3].name = "Water DensityA";
        //}        
        
        debugTextureViewerArray[12] = uiManagerRef.gameManager.simulationManager.vegetationManager.resourceSimTransferRT;
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
        if(_IsChannelSolo > 0.5f) {
            _IsChannelSolo = 0f;
        }
        else {
            _IsChannelSolo = 1f;
        }
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
