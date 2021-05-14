
public class GlobalGraphData
{
    Lookup lookup => Lookup.instance;
    
    GraphData nutrients;
    GraphData waste;
    GraphData decomposers;
    GraphData algae;
    GraphData plants;
    GraphData zooplankton;
    GraphData vertebrates;

    // Not used
    /*
    public GraphData graphDataVertebrateLifespan0;
    public GraphData graphDataVertebratePopulation0;
    public GraphData graphDataVertebrateFoodEaten0;
    public GraphData graphDataVertebrateGenome0;

    public GraphData graphDataVertebrateLifespan1;
    public GraphData graphDataVertebratePopulation1;
    public GraphData graphDataVertebrateFoodEaten1;
    public GraphData graphDataVertebrateGenome1;

    public GraphData graphDataVertebrateLifespan2;
    public GraphData graphDataVertebratePopulation2;
    public GraphData graphDataVertebrateFoodEaten2;
    public GraphData graphDataVertebrateGenome2;

    public GraphData graphDataVertebrateLifespan3;
    public GraphData graphDataVertebratePopulation3;
    public GraphData graphDataVertebrateFoodEaten3;
    public GraphData graphDataVertebrateGenome3;
    */
    
    // testing!!!!
    public void Initialize()
    {
        nutrients = new GraphData(lookup.knowledgeGraphNutrientsMat);  
        waste = new GraphData(lookup.knowledgeGraphDetritusMat); 
        decomposers = new GraphData(lookup.knowledgeGraphDecomposersMat); 
        algae = new GraphData(lookup.knowledgeGraphAlgaeMat); 
        plants = new GraphData(lookup.knowledgeGraphPlantsMat);  
        zooplankton = new GraphData(lookup.knowledgeGraphZooplanktonMat); 
        vertebrates = new GraphData(lookup.knowledgeGraphVertebratesMat);
        
        /*
        graphDataVertebrateLifespan0 = new GraphData(ui.knowledgeUI.knowledgeGraphVertebrateLifespanMat0);
        graphDataVertebratePopulation0 = new GraphData(ui.knowledgeUI.knowledgeGraphVertebratePopulationMat0);
        graphDataVertebrateFoodEaten0 = new GraphData(ui.knowledgeUI.knowledgeGraphVertebrateFoodEatenMat0);
        graphDataVertebrateGenome0 = new GraphData(ui.knowledgeUI.knowledgeGraphVertebrateGenomeMat0);

        graphDataVertebrateLifespan1 = new GraphData(ui.knowledgeUI.knowledgeGraphVertebrateLifespanMat1);
        graphDataVertebratePopulation1 = new GraphData(ui.knowledgeUI.knowledgeGraphVertebratePopulationMat1);
        graphDataVertebrateFoodEaten1 = new GraphData(ui.knowledgeUI.knowledgeGraphVertebrateFoodEatenMat1);
        graphDataVertebrateGenome1 = new GraphData(ui.knowledgeUI.knowledgeGraphVertebrateGenomeMat1);

        graphDataVertebrateLifespan2 = new GraphData(ui.knowledgeUI.knowledgeGraphVertebrateLifespanMat2);
        graphDataVertebratePopulation2 = new GraphData(ui.knowledgeUI.knowledgeGraphVertebratePopulationMat2);
        graphDataVertebrateFoodEaten2 = new GraphData(ui.knowledgeUI.knowledgeGraphVertebrateFoodEatenMat2);
        graphDataVertebrateGenome2 = new GraphData(ui.knowledgeUI.knowledgeGraphVertebrateGenomeMat2);

        graphDataVertebrateLifespan3 = new GraphData(ui.knowledgeUI.knowledgeGraphVertebrateLifespanMat3);
        graphDataVertebratePopulation3 = new GraphData(ui.knowledgeUI.knowledgeGraphVertebratePopulationMat3);
        graphDataVertebrateFoodEaten3 = new GraphData(ui.knowledgeUI.knowledgeGraphVertebrateFoodEatenMat3);
        graphDataVertebrateGenome3 = new GraphData(ui.knowledgeUI.knowledgeGraphVertebrateGenomeMat3);
        */
    }
    
    public void AddNewEntry(SimResourceManager resources, float totalAgentBiomass)
    {
        nutrients.AddNewEntry(resources.curGlobalNutrients);
        waste.AddNewEntry(resources.curGlobalDetritus);
        decomposers.AddNewEntry(resources.curGlobalDecomposers);
        algae.AddNewEntry(resources.curGlobalAlgaeReservoir);
        plants.AddNewEntry(resources.curGlobalPlantParticles);
        zooplankton.AddNewEntry(resources.curGlobalAnimalParticles);
        vertebrates.AddNewEntry(totalAgentBiomass);
    }
}
