using UnityEngine;

public class SimulationUI : MonoBehaviour
{
    SimulationManager simulation => SimulationManager.instance;
    TheCursorCzar theCursorCzar => TheCursorCzar.instance;

    public UIManager manager;
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
    
    void OnEnable()
    {
        manager.InitialUnlocks();
    }
    
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
        manager.SetFocus();
        debugPanelUI.UpdateDebugUI();              
    }
}