using UnityEngine;
using UnityEngine.UI;

public class SpeciesGraphPanelUI : MonoBehaviour
{
    SimulationManager simulationManager => SimulationManager.instance;
    MasterGenomePool masterGenomePool => simulationManager.masterGenomePool;
    UIManager uiManager => UIManager.instance;
    SelectionManager selectionManager => SelectionManager.instance;

    [SerializeField] GameObject graphPanelGO;

    public Texture2D statsSpeciesColorKey;
    private int maxDisplaySpecies = 32;  // **** WILL BECOME A PROBLEM!!!! ****
    public Texture2D[] statsTreeOfLifeSpeciesTexArray;
    public float[] maxValuesStatArray;
    public float[] minValuesStatArray;

    public float curSpeciesStatValue;
    public string curSpeciesStatName;
    public int selectedSpeciesStatsIndex;
        
    public Text textGraphCategory;
    public Text textGraphStatsLeft;
    public Text textGraphStatsCenter;
    public Text textGraphStatsRight;
    
    public Image speciesGraphImage;
    public Material speciesGraphMatLeft;
    public Material speciesGraphMatCenter;
    public Material speciesGraphMatRight;

    public GraphCategory selectedGraphCategory;
    public enum GraphCategory {
        Life,
        Body,
        Talents,
        Eaten,
        DigestSpec
    }

    


    void Start () {
        
        if (statsSpeciesColorKey == null) {
            statsSpeciesColorKey = new Texture2D(maxDisplaySpecies, 1, TextureFormat.ARGB32, false);
            statsSpeciesColorKey.filterMode = FilterMode.Point;
            statsSpeciesColorKey.wrapMode = TextureWrapMode.Clamp;
        }
        
        statsTreeOfLifeSpeciesTexArray = new Texture2D[16]; // start with 16 choosable stats

        for (int i = 0; i < statsTreeOfLifeSpeciesTexArray.Length; i++) {
            Texture2D statsTexture = new Texture2D(maxDisplaySpecies, 1, TextureFormat.RGBAFloat, false);
            statsTexture.filterMode = FilterMode.Bilinear;
            statsTexture.wrapMode = TextureWrapMode.Clamp;

            statsTreeOfLifeSpeciesTexArray[i] = statsTexture;
        }
                
        maxValuesStatArray = new float[16];
        minValuesStatArray = new float[16];
        for (int i = 0; i < maxValuesStatArray.Length; i++) {
            maxValuesStatArray[i] = 0.000001f;
            minValuesStatArray[i] = 1000000f;
        }            
	}

    public void ClickButtonGraphType(int buttonID) {
        selectedGraphCategory = (GraphCategory)buttonID;       
        UpdateSpeciesTreeDataTextures(simulationManager.curSimYear);        
    }
    public void UpdateSpeciesTreeDataTextures(int year) {  // refactor using year?
        //Debug.Log("WOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO " + year.ToString());
        int numActiveSpecies = masterGenomePool.currentlyActiveSpeciesIDList.Count;
        int numTotalSpecies = masterGenomePool.completeSpeciesPoolsList.Count;

        //int maxDisplaySpecies = 32;
        int[] displaySpeciesIndicesArray;
        displaySpeciesIndicesArray = new int[maxDisplaySpecies];
        
        TreeOfLifeSpeciesKeyData[] speciesKeyDataArray = new TreeOfLifeSpeciesKeyData[32];

         // Get Active ones first:
        for(int i = 0; i < masterGenomePool.currentlyActiveSpeciesIDList.Count; i++) {
            SpeciesGenomePool pool = masterGenomePool.completeSpeciesPoolsList[masterGenomePool.currentlyActiveSpeciesIDList[i]];
            SpeciesGenomePool parentPool;
            Vector3 parentHue = Vector3.one;
            if(pool.parentSpeciesID != -1) {
                parentPool = masterGenomePool.completeSpeciesPoolsList[pool.parentSpeciesID];
                parentHue = parentPool.representativeCandidate.candidateGenome.bodyGenome.appearanceGenome.huePrimary;
            }
            Vector3 huePrimary = pool.representativeCandidate.candidateGenome.bodyGenome.appearanceGenome.huePrimary;
            Vector3 hueSecondary = pool.representativeCandidate.candidateGenome.bodyGenome.appearanceGenome.hueSecondary;
            
            statsSpeciesColorKey.SetPixel(i, 1, new Color(huePrimary.x, huePrimary.y, huePrimary.z));            
            //Debug.Log("(" + i.ToString() + ", " + gameManager.simulationManager.masterGenomePool.currentlyActiveSpeciesIDList[i].ToString());
            displaySpeciesIndicesArray[i] = masterGenomePool.currentlyActiveSpeciesIDList[i];

            TreeOfLifeSpeciesKeyData keyData = new TreeOfLifeSpeciesKeyData();
            keyData.timeCreated = pool.timeStepCreated;  // Use TimeSteps instead of Years???
            keyData.timeExtinct = pool.timeStepExtinct;
            keyData.huePrimary = huePrimary;
            keyData.hueSecondary = hueSecondary;
            keyData.parentHue = parentHue;
            keyData.isExtinct = pool.isExtinct ? 1f : 0f;
            keyData.isOn = 1f;
            //int selectedID = treeOfLifeManager.selectedID;
            
            keyData.isSelected = selectionManager.selectedSpeciesID == masterGenomePool.currentlyActiveSpeciesIDList[i] ? 1f : 0f;

            speciesKeyDataArray[i] = keyData;
        }
        
        // Then fill with most recently extinct:
        for(int i = numTotalSpecies - 1; i > Mathf.Clamp((numTotalSpecies - maxDisplaySpecies), 0, numTotalSpecies); i--) {
            SpeciesGenomePool pool = masterGenomePool.completeSpeciesPoolsList[i];

            SpeciesGenomePool parentPool;
            if (pool.parentSpeciesID == -1) {
                parentPool = pool; // whoa man...
            }
            else {
                parentPool = masterGenomePool.completeSpeciesPoolsList[pool.parentSpeciesID];
            }            

            Vector3 huePrimary = pool.representativeCandidate.candidateGenome.bodyGenome.appearanceGenome.huePrimary;
            Vector3 hueSecondary = pool.representativeCandidate.candidateGenome.bodyGenome.appearanceGenome.hueSecondary;
            Vector3 parentHue = parentPool.representativeCandidate.candidateGenome.bodyGenome.appearanceGenome.huePrimary;
            if(masterGenomePool.completeSpeciesPoolsList[i].isExtinct) {
                huePrimary = Vector3.zero;
            }
            statsSpeciesColorKey.SetPixel(i, 1, new Color(huePrimary.x, huePrimary.y, huePrimary.z));
            
            displaySpeciesIndicesArray[i] = i;

            TreeOfLifeSpeciesKeyData keyData = new TreeOfLifeSpeciesKeyData();
            keyData.timeCreated = pool.timeStepCreated;  // Use TimeSteps instead of Years???
            keyData.timeExtinct = pool.timeStepExtinct;
            keyData.huePrimary = huePrimary;
            keyData.hueSecondary = hueSecondary;
            keyData.parentHue = parentHue;
            keyData.isExtinct = pool.isExtinct ? 1f : 0f;
            keyData.isOn = i >= masterGenomePool.completeSpeciesPoolsList.Count ||
                           pool.yearCreated == -1 || i == 0 ? 
                                0f : 1f;
            
            // WPP: applied ternary
            /*keyData.isOn = 1f;
            if(i >= masterGenomePool.completeSpeciesPoolsList.Count) {
                keyData.isOn = 0f;
            }
            if(pool.yearCreated == -1) {
                keyData.isOn = 0f;
            }
            if(i == 0) {
                keyData.isOn = 0f;
            }*/
            
            keyData.isSelected = selectionManager.selectedSpeciesID == i ? 1f : 0f;

            speciesKeyDataArray[i] = keyData;
        }
        statsSpeciesColorKey.Apply();

        //uiManagerRef.gameManager.simulationManager.theRenderKing.treeOfLifeSpeciesDataKeyCBuffer.SetData(speciesKeyDataArray);

        // ========== data: =========== //
        int years = Mathf.Min(2048, year);  // cap textures at 2k for now?
        years = Mathf.Max(1, years);
        // check for resize before doing it?
        for(int i = 0; i < statsTreeOfLifeSpeciesTexArray.Length; i++) {
            statsTreeOfLifeSpeciesTexArray[i].Resize(years, maxDisplaySpecies);
        }
        
        for(int i = 0; i < maxValuesStatArray.Length; i++) {
            maxValuesStatArray[i] = 0.0000001f;
            minValuesStatArray[i] = 1000000f;
        }
        // for each year & each species, create 2D texture with fitness scores:
        for(int s = 0; s < maxDisplaySpecies; s++) {            
            
            if(displaySpeciesIndicesArray[s] < masterGenomePool.completeSpeciesPoolsList.Count) {

                SpeciesGenomePool speciesPool = masterGenomePool.completeSpeciesPoolsList[displaySpeciesIndicesArray[s]];
                if(speciesPool == null) {
                    Debug.LogError("well, shit");
                }
                for(int t = 0; t < years; t++) {
                
                    //int index = t - speciesPool.yearCreated;
                    
                    for(int a = 0; a < statsTreeOfLifeSpeciesTexArray.Length; a++) {
                        float valStat = 0f;
                        if(speciesPool.avgCandidateDataYearList.Count > t) {
                            valStat = (float)speciesPool.avgCandidateDataYearList[t].performanceData.totalTicksAlive; //0f;
                            //Debug.Log("valStat: " + valStat.ToString());
                        }
                        
                        if(years > 15) {
                            float time01 = (float)t / (float)years;

                            if(time01 < 0.05f) {
                                // **** Don't use first 5% of history towards stat range! ***
                            }
                            else {
                                minValuesStatArray[a] = Mathf.Min(minValuesStatArray[a], valStat);                        
                                maxValuesStatArray[a] = Mathf.Max(maxValuesStatArray[a], valStat);
                            }
                        }
                        else {
                            minValuesStatArray[a] = Mathf.Min(minValuesStatArray[a], valStat);                        
                            maxValuesStatArray[a] = Mathf.Max(maxValuesStatArray[a], valStat);
                        }
                        
                        
                                                
                        statsTreeOfLifeSpeciesTexArray[a].SetPixel(t, s, new Color(valStat, valStat, valStat, 1f));
                    }                    
                }                
            }            
        }
        
        for (int b = 0; b < statsTreeOfLifeSpeciesTexArray.Length; b++) {
            statsTreeOfLifeSpeciesTexArray[b].Apply();
        }
        
        //RefreshGraphMaterial();        
    }

    private void RefreshGraphMaterial() {
        //Debug.Log("RefreshGraphMaterial " + statsTreeOfLifeSpeciesTexArray[0].width.ToString() + ", " + maxValuesStatArray[0].ToString());
        SpeciesGenomePool pool = masterGenomePool.completeSpeciesPoolsList[selectionManager.selectedSpeciesID];

        switch(selectedGraphCategory) {
            case GraphCategory.Life:
                speciesGraphMatLeft.SetTexture("_MainTex", statsTreeOfLifeSpeciesTexArray[0]);
                speciesGraphMatLeft.SetTexture("_ColorKeyTex", statsSpeciesColorKey);
                speciesGraphMatLeft.SetFloat("_NumEntries", statsTreeOfLifeSpeciesTexArray[0].width);
                speciesGraphMatLeft.SetInt("_SelectedSpeciesID", selectionManager.selectedSpeciesID);
                speciesGraphMatLeft.SetFloat("_MaximumValue", maxValuesStatArray[0]);
                speciesGraphMatLeft.SetFloat("_MinimumValue", minValuesStatArray[0]);

                speciesGraphMatCenter.SetTexture("_MainTex", statsTreeOfLifeSpeciesTexArray[14]);
                speciesGraphMatCenter.SetTexture("_ColorKeyTex", statsSpeciesColorKey);
                speciesGraphMatCenter.SetFloat("_NumEntries", statsTreeOfLifeSpeciesTexArray[14].width);
                speciesGraphMatCenter.SetInt("_SelectedSpeciesID", selectionManager.selectedSpeciesID);
                speciesGraphMatCenter.SetFloat("_MaximumValue", maxValuesStatArray[14]);
                speciesGraphMatCenter.SetFloat("_MinimumValue", minValuesStatArray[14]);

                speciesGraphMatRight.SetTexture("_MainTex", statsTreeOfLifeSpeciesTexArray[15]);
                speciesGraphMatRight.SetTexture("_ColorKeyTex", statsSpeciesColorKey);
                speciesGraphMatRight.SetFloat("_NumEntries", statsTreeOfLifeSpeciesTexArray[15].width);
                speciesGraphMatRight.SetInt("_SelectedSpeciesID", selectionManager.selectedSpeciesID);
                speciesGraphMatRight.SetFloat("_MaximumValue", maxValuesStatArray[15]);
                speciesGraphMatRight.SetFloat("_MinimumValue", minValuesStatArray[15]);

                //curSpeciesStatValue = pool.avgDamageDealt;
            //curSpeciesStatName = "Damage Dealt";
                textGraphStatsLeft.text = "LIFESPAN\n" + pool.avgCandidateData.performanceData.totalTicksAlive.ToString(); // + ", M: " + maxValuesStatArray[selectedSpeciesStatsIndex].ToString() + ", m: " + minValuesStatArray[selectedSpeciesStatsIndex].ToString();
                textGraphStatsCenter.text = "DMG DEALT\n" + pool.avgCandidateData.performanceData.totalDamageDealt.ToString();
                textGraphStatsRight.text = "DMG TAKEN\n" + pool.avgCandidateData.performanceData.totalDamageTaken.ToString();

                textGraphCategory.text = "HEALTH";
                break;
            case GraphCategory.Body:
                speciesGraphMatLeft.SetTexture("_MainTex", statsTreeOfLifeSpeciesTexArray[4]);
                speciesGraphMatLeft.SetTexture("_ColorKeyTex", statsSpeciesColorKey);
                speciesGraphMatLeft.SetFloat("_NumEntries", statsTreeOfLifeSpeciesTexArray[4].width);
                speciesGraphMatLeft.SetInt("_SelectedSpeciesID", selectionManager.selectedSpeciesID);
                speciesGraphMatLeft.SetFloat("_MaximumValue", maxValuesStatArray[4]);
                speciesGraphMatLeft.SetFloat("_MinimumValue", minValuesStatArray[4]);
                
                speciesGraphMatCenter.SetTexture("_MainTex", statsTreeOfLifeSpeciesTexArray[12]);
                speciesGraphMatCenter.SetTexture("_ColorKeyTex", statsSpeciesColorKey);
                speciesGraphMatCenter.SetFloat("_NumEntries", statsTreeOfLifeSpeciesTexArray[12].width);
                speciesGraphMatCenter.SetInt("_SelectedSpeciesID", selectionManager.selectedSpeciesID);
                speciesGraphMatCenter.SetFloat("_MaximumValue", maxValuesStatArray[12]);
                speciesGraphMatCenter.SetFloat("_MinimumValue", minValuesStatArray[12]);

                speciesGraphMatRight.SetTexture("_MainTex", statsTreeOfLifeSpeciesTexArray[13]);
                speciesGraphMatRight.SetTexture("_ColorKeyTex", statsSpeciesColorKey);
                speciesGraphMatRight.SetFloat("_NumEntries", statsTreeOfLifeSpeciesTexArray[13].width);
                speciesGraphMatRight.SetInt("_SelectedSpeciesID", selectionManager.selectedSpeciesID);
                speciesGraphMatRight.SetFloat("_MaximumValue", maxValuesStatArray[13]);
                speciesGraphMatRight.SetFloat("_MinimumValue", minValuesStatArray[13]);

                //textGraphStatsLeft.text = "BODYSIZE\n" + pool.avgBodySize.ToString();
                //textGraphStatsCenter.text = "NEURONS\n" + pool.avgNumNeurons.ToString();
                //textGraphStatsRight.text = "AXONS\n" + pool.avgNumAxons.ToString();

                textGraphCategory.text = "BODY & BRAIN";
                break;
            case GraphCategory.Talents:
                speciesGraphMatLeft.SetTexture("_MainTex", statsTreeOfLifeSpeciesTexArray[5]);
                speciesGraphMatLeft.SetTexture("_ColorKeyTex", statsSpeciesColorKey);
                speciesGraphMatLeft.SetFloat("_NumEntries", statsTreeOfLifeSpeciesTexArray[5].width);
                speciesGraphMatLeft.SetInt("_SelectedSpeciesID", selectionManager.selectedSpeciesID);
                speciesGraphMatLeft.SetFloat("_MaximumValue", maxValuesStatArray[5]);
                speciesGraphMatLeft.SetFloat("_MinimumValue", minValuesStatArray[5]);
                
                speciesGraphMatCenter.SetTexture("_MainTex", statsTreeOfLifeSpeciesTexArray[6]);
                speciesGraphMatCenter.SetTexture("_ColorKeyTex", statsSpeciesColorKey);
                speciesGraphMatCenter.SetFloat("_NumEntries", statsTreeOfLifeSpeciesTexArray[6].width);
                speciesGraphMatCenter.SetInt("_SelectedSpeciesID", selectionManager.selectedSpeciesID);
                speciesGraphMatCenter.SetFloat("_MaximumValue", maxValuesStatArray[6]);
                speciesGraphMatCenter.SetFloat("_MinimumValue", minValuesStatArray[6]);

                speciesGraphMatRight.SetTexture("_MainTex", statsTreeOfLifeSpeciesTexArray[7]);
                speciesGraphMatRight.SetTexture("_ColorKeyTex", statsSpeciesColorKey);
                speciesGraphMatRight.SetFloat("_NumEntries", statsTreeOfLifeSpeciesTexArray[7].width);
                speciesGraphMatRight.SetInt("_SelectedSpeciesID", selectionManager.selectedSpeciesID);
                speciesGraphMatRight.SetFloat("_MaximumValue", maxValuesStatArray[7]);
                speciesGraphMatRight.SetFloat("_MinimumValue", minValuesStatArray[7]);

                textGraphCategory.text = "SPECIALIZATIONS";
                break;
            case GraphCategory.Eaten:
                speciesGraphMatLeft.SetTexture("_MainTex", statsTreeOfLifeSpeciesTexArray[1]);
                speciesGraphMatLeft.SetTexture("_ColorKeyTex", statsSpeciesColorKey);
                speciesGraphMatLeft.SetFloat("_NumEntries", statsTreeOfLifeSpeciesTexArray[1].width);
                speciesGraphMatLeft.SetInt("_SelectedSpeciesID", selectionManager.selectedSpeciesID);
                speciesGraphMatLeft.SetFloat("_MaximumValue", maxValuesStatArray[1]);
                speciesGraphMatLeft.SetFloat("_MinimumValue", minValuesStatArray[1]);

                speciesGraphMatCenter.SetTexture("_MainTex", statsTreeOfLifeSpeciesTexArray[2]);
                speciesGraphMatCenter.SetTexture("_ColorKeyTex", statsSpeciesColorKey);
                speciesGraphMatCenter.SetFloat("_NumEntries", statsTreeOfLifeSpeciesTexArray[2].width);
                speciesGraphMatCenter.SetInt("_SelectedSpeciesID", selectionManager.selectedSpeciesID);
                speciesGraphMatCenter.SetFloat("_MaximumValue", maxValuesStatArray[2]);
                speciesGraphMatCenter.SetFloat("_MinimumValue", minValuesStatArray[2]);

                speciesGraphMatRight.SetTexture("_MainTex", statsTreeOfLifeSpeciesTexArray[3]);
                speciesGraphMatRight.SetTexture("_ColorKeyTex", statsSpeciesColorKey);
                speciesGraphMatRight.SetFloat("_NumEntries", statsTreeOfLifeSpeciesTexArray[3].width);
                speciesGraphMatRight.SetInt("_SelectedSpeciesID", selectionManager.selectedSpeciesID);
                speciesGraphMatRight.SetFloat("_MaximumValue", maxValuesStatArray[3]);
                speciesGraphMatRight.SetFloat("_MinimumValue", minValuesStatArray[3]);

                textGraphCategory.text = "FOOD CONSUMED";
                break;
            case GraphCategory.DigestSpec:
                speciesGraphMatLeft.SetTexture("_MainTex", statsTreeOfLifeSpeciesTexArray[10]);
                speciesGraphMatLeft.SetTexture("_ColorKeyTex", statsSpeciesColorKey);
                speciesGraphMatLeft.SetFloat("_NumEntries", statsTreeOfLifeSpeciesTexArray[10].width);
                speciesGraphMatLeft.SetInt("_SelectedSpeciesID", selectionManager.selectedSpeciesID);
                speciesGraphMatLeft.SetFloat("_MaximumValue", maxValuesStatArray[10]);
                speciesGraphMatLeft.SetFloat("_MinimumValue", minValuesStatArray[10]);
                
                speciesGraphMatCenter.SetTexture("_MainTex", statsTreeOfLifeSpeciesTexArray[11]);
                speciesGraphMatCenter.SetTexture("_ColorKeyTex", statsSpeciesColorKey);
                speciesGraphMatCenter.SetFloat("_NumEntries", statsTreeOfLifeSpeciesTexArray[11].width);
                speciesGraphMatCenter.SetInt("_SelectedSpeciesID", selectionManager.selectedSpeciesID);
                speciesGraphMatCenter.SetFloat("_MaximumValue", maxValuesStatArray[11]);
                speciesGraphMatCenter.SetFloat("_MinimumValue", minValuesStatArray[11]);

                speciesGraphMatRight.SetTexture("_MainTex", statsTreeOfLifeSpeciesTexArray[9]);
                speciesGraphMatRight.SetTexture("_ColorKeyTex", statsSpeciesColorKey);
                speciesGraphMatRight.SetFloat("_NumEntries", statsTreeOfLifeSpeciesTexArray[9].width);
                speciesGraphMatRight.SetInt("_SelectedSpeciesID", selectionManager.selectedSpeciesID);
                speciesGraphMatRight.SetFloat("_MaximumValue", maxValuesStatArray[9]);
                speciesGraphMatRight.SetFloat("_MinimumValue", minValuesStatArray[9]);

                textGraphCategory.text = "DIGESTION BONUSES";
                break;
            default:
                break;
        }

        //speciesGraphImage.material = speciesGraphMatLeft;
        //speciesGraphImage.gameObject.SetActive(false);
        //speciesGraphImage.gameObject.SetActive(true);
    }

    public void Set(bool value) {
        graphPanelGO.SetActive(value);
    }
}
