using System.Collections.Generic;
using UnityEngine;

public class SimulationUI : MonoBehaviour
{
    public UIManager manager;
    public WildSpirit wildSpirit;
    public WatcherUI watcherUI;
    public BrushesUI brushesUI;
    public KnowledgeUI knowledgeUI;
    public MutationUI mutationUI;
    public GlobalResourcesUI globalResourcesUI;
    public FeatsUI featsUI;
    public DebugPanelUI debugPanelUI;
    public Animator animatorSpiritUnlock;
    public SpeciesOverviewUI speciesOverviewUI;
    public GenomeViewerUI genomeViewerUI;
    public ClockPanelUI clockPanelUI;
    public ObserverModeUI observerModeUI;
    public BigBangPanelUI bigBangPanelUI;
    
    SimulationManager simulation => SimulationManager.instance;
    TheCursorCzar theCursorCzar => TheCursorCzar.instance;
    MasterGenomePool genomePool => simulation.masterGenomePool;
    List<SpeciesGenomePool> speciesPools => genomePool.completeSpeciesPoolsList;
    
    const string ANIM_FINISHED = "_AnimFinished";
    
    void OnEnable()
    {
        manager.InitialUnlocks();
    }
    
    SpeciesGenomePool pool;

    void Update() {
        bigBangPanelUI.Tick();
        observerModeUI.Tick();  
        theCursorCzar.UpdateCursorCzar();  // this will assume a larger role
        brushesUI.UpdateBrushesUI();
        watcherUI.UpdateWatcherPanelUI(simulation.trophicLayersManager);
        knowledgeUI.UpdateKnowledgePanelUI(simulation.trophicLayersManager);
        mutationUI.UpdateMutationPanelUI(simulation.trophicLayersManager);
        //worldSpiritHubUI.UpdateWorldSpiritHubUI();
        globalResourcesUI.UpdateGlobalResourcesPanelUpdate();
        featsUI.UpdateFeatsPanelUI(simulation.featsList);
        clockPanelUI.Tick();

        // * Only reassign this when it changes
        pool = speciesPools[manager.selectedSpeciesID];
        
        if(manager.focusedCandidate != null) {
            genomeViewerUI.UpdateUI(pool, manager.focusedCandidate);

            if(simulation.simAgeTimeSteps % 111 == 1) {
                speciesOverviewUI.RebuildGenomeButtons();  
            }
        }
        
        // REFACTOR: delegate to WildSpirit        
        if(wildSpirit.isClickableSpiritRoaming) {
            wildSpirit.UpdateWildSpiritProto();
        }
        else {
            wildSpirit.framesSinceLastClickableSpirit++;
            wildSpirit.protoSpiritClickColliderGO.SetActive(false);
        }

        if(animatorSpiritUnlock.GetBool(ANIM_FINISHED)) {
            manager.SpiritUnlockComplete();
            animatorSpiritUnlock.SetBool(ANIM_FINISHED, false);
        }
        
        debugPanelUI.UpdateDebugUI();              
    }
}