using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class GridSearchManager {

    private ComputeShader graphImageComputeShader;
    public MutationSettings settings;
    public bool biggerIsBetter;

    //public int resolution = 2;  // how to split each dimension    
    public int numGens = 32;  // number of generations for each Run
    public int numRunsPerSetting = 1;

    public int resolutionA = 4;  // how to split each dimension
    public int resolutionB = 3;  // how to split each dimension
    public int resolutionC = 3;  // how to split each dimension
    public int resolutionD = 1;  // how to split each dimension

    // Start out hardcoded to 4 dimensions, expand functionality later:
    public int coordA = 0;  // mutationChance
    public int coordB = 0;  // stepSize
    public int coordC = 0;  // initialConnectionChance
    public int coordD = 0;  // weight decay
    public int curIter = 0;

    public float minA = 0.005f;
    public float maxA = 0.1f;
    public float minB = 0.25f;
    public float maxB = 1f;
    public float minC = 0.0f;
    public float maxC = 1.0f;
    public float minD = 1f;
    public float maxD = 1f;

    public GridSearchResults storedResults;

    public bool isComplete = false;

    public GridSearchManager() {
        
    }

    public void InitializeGridSearch(ComputeShader graphImageComputeShader, MutationSettings settings, bool biggerIsBetter) {
        this.biggerIsBetter = biggerIsBetter;
        this.graphImageComputeShader = graphImageComputeShader;
        this.settings = settings;

        storedResults = new GridSearchResults();
        coordA = 0;
        coordB = 0;
        coordC = 0;
        coordD = 0;
        curIter = 0;

        CreateDataContainer();        
    }

    private void CreateDataContainer() {
        GridSearchRunData gridSearchRunData = new GridSearchRunData();

        if(biggerIsBetter) {

        }
        else {
            //gridSearchRunData.highestScore = float.PositiveInfinity;
        }

        gridSearchRunData.iteration = curIter;
        gridSearchRunData.dimensionA = Mathf.Lerp(minA, maxA, (float)coordA / Mathf.Max(1f, (float)(resolutionA - 1)));
        gridSearchRunData.dimensionB = Mathf.Lerp(minB, maxB, (float)coordB / Mathf.Max(1f, (float)(resolutionB - 1)));
        gridSearchRunData.dimensionC = Mathf.Lerp(minC, maxC, (float)coordC / Mathf.Max(1f, (float)(resolutionC - 1)));
        gridSearchRunData.dimensionD = Mathf.Lerp(minD, maxD, (float)coordD / Mathf.Max(1f, (float)(resolutionD - 1)));

        settings.mutationChance = gridSearchRunData.dimensionA;
        settings.mutationStepSize = gridSearchRunData.dimensionB;
        settings.initialConnectionChance = gridSearchRunData.dimensionC;
        settings.weightDecayAmount = gridSearchRunData.dimensionD;

        //Debug.Log("gridSearchRunData.dimensionA " + gridSearchRunData.dimensionA.ToString());

        storedResults.dataList.Add(gridSearchRunData);
    }

    private void SaveResultsDataToFile() {
        Debug.Log("storedResults.dataList.Count" + storedResults.dataList.Count.ToString());
        // Choose Folder Name:
        string monthVar = System.DateTime.Now.Month.ToString();
        string dayVar = System.DateTime.Now.Day.ToString();
        string yearVar = System.DateTime.Now.Year.ToString();
        string hourVar = System.DateTime.Now.Hour.ToString();
        string minuteVar = System.DateTime.Now.Minute.ToString();
        // Make containing folder:
        string folderName = yearVar + "_" + monthVar + "_" + dayVar + "_" + hourVar + "_" + minuteVar;
        string directory = Application.dataPath + "/GridSearchSaves/" + folderName;
        Directory.CreateDirectory(directory);


        // Process Data:::::
        float[] maxScoreEachGenArray = new float[storedResults.dataList[0].scoresList.Count];
        float[] minScoreEachGenArray = new float[storedResults.dataList[0].scoresList.Count];
        storedResults.highestScoreAchieved = 0f;
        storedResults.lowestScoreAchieved = float.PositiveInfinity;
        for (int g = 0; g < maxScoreEachGenArray.Length; g++) {
            float recordHighScore = 0f;
            float recordLowScore = float.PositiveInfinity;

            for(int d = 0; d < storedResults.dataList.Count; d++) {
                if(storedResults.dataList[d].scoresList[g] > recordHighScore) {
                    recordHighScore = storedResults.dataList[d].scoresList[g];
                }
                if (storedResults.dataList[d].scoresList[g] < recordLowScore) {
                    recordLowScore = storedResults.dataList[d].scoresList[g];
                }
            }

            maxScoreEachGenArray[g] = recordHighScore;
            minScoreEachGenArray[g] = recordLowScore;

            //if(biggerIsBetter) {
            if (maxScoreEachGenArray[g] > storedResults.highestScoreAchieved) {
                storedResults.highestScoreAchieved = maxScoreEachGenArray[g];
            }
            //}
            //else {
            if (minScoreEachGenArray[g] < storedResults.lowestScoreAchieved) {
                storedResults.lowestScoreAchieved = minScoreEachGenArray[g];
            }
            //}            
        }



        // SAVE JSON::::
        string saveNameJson = "GS_" + "RawScores";
        string jsonString = JsonUtility.ToJson(storedResults);
        string pathJson = directory + "/" + saveNameJson + ".json";        
        Debug.Log(pathJson);
        if (File.Exists(pathJson)) {
            File.Delete(pathJson);
        }
        System.IO.File.WriteAllText(pathJson, jsonString);

        // Save TXT:::
        string saveNameResults = "GS_" + "Results";
        float bestScore = storedResults.highestScoreAchieved;
        if(!biggerIsBetter) {
            bestScore = storedResults.lowestScoreAchieved;
        }
        string resultsString = "Grid Search Results! best individual score: " + bestScore.ToString() + "\n";
        for(int i = 0; i < storedResults.readoutList.Count; i++) {
            resultsString += storedResults.readoutList[i] + "\n";
        }
        string pathResults = directory + "/" + saveNameResults + ".txt";
        Debug.Log(pathResults);
        if (File.Exists(pathResults)) {
            File.Delete(pathResults);
        }
        System.IO.File.WriteAllText(pathResults, resultsString);



        // Save Textures:
        // Texture Size:
        int pixels = 128;
        Texture2D tex = new Texture2D(pixels, pixels, TextureFormat.ARGB32, false);
        // will be re-used ^ ^ ^
        for (int i = 0; i < storedResults.dataList.Count; i++) {
            
            for(int x = 0; x < pixels; x++) {
                for(int y = 0; y < pixels; y++) {
                    Color color = Color.black;
                    //float val = 0f; // = 1f;
                    float yPos = (float)y / (float)pixels;
                    int xIndex = Mathf.RoundToInt(((float)x / (float)pixels) * (float)(storedResults.dataList[i].scoresList.Count - 1));
                    float rawScore = storedResults.dataList[i].scoresList[xIndex];
                    float score01 = rawScore / storedResults.dataList[i].highestScore;

                    if (yPos < score01) {
                        float range = maxScoreEachGenArray[xIndex] - minScoreEachGenArray[xIndex];
                        if(range == 0f) {
                            range = 1f; // divide by 0
                        }

                        float lerp = (rawScore - minScoreEachGenArray[xIndex]) / range;
                        color.r = 1f - lerp;
                        color.g = lerp;
                        color.b = rawScore / storedResults.highestScoreAchieved;

                        if(!biggerIsBetter) {
                            color.r = 1f - color.r;
                            color.g = 1f - color.g;
                            color.b = 1f - color.b;
                        }
                    }
                    tex.SetPixel(x, y, color);
                }
            }
            tex.Apply();
            string imageFilename = directory + "/" + "Graph" + i.ToString() + ".jpg";
            System.IO.File.WriteAllBytes(imageFilename, tex.EncodeToJPG());            
        }
    }

    public void StartNewRun() {
        
        curIter++;
        if(curIter >= numRunsPerSetting) {
            curIter = 0;

            // Calculate Average Score:
            float totalRawScore = 0f;
            for(int i = 0; i < numRunsPerSetting; i++) {
                int index = ((storedResults.dataList.Count - 1) - i);
                if(biggerIsBetter) {
                    totalRawScore += storedResults.dataList[index].highestScore;
                }
                else {
                    totalRawScore += storedResults.dataList[index].lowestScore;
                }
                
            }
            float avgRawScore = totalRawScore / numRunsPerSetting;
            // trainingRunsScores:
            storedResults.trainingRunsScoreList.Add(avgRawScore);
            // Readout:
            string txt = "Settings[" + (storedResults.trainingRunsScoreList.Count - 1).ToString() + "] Score: " + avgRawScore.ToString() +
                         " A[" + coordA.ToString() + "]: " + storedResults.dataList[storedResults.dataList.Count - 1].dimensionA.ToString() +
                         " B[" + coordB.ToString() + "]: " + storedResults.dataList[storedResults.dataList.Count - 1].dimensionB.ToString() +
                         " C[" + coordC.ToString() + "]: " + storedResults.dataList[storedResults.dataList.Count - 1].dimensionC.ToString() +
                         " D[" + coordD.ToString() + "]: " + storedResults.dataList[storedResults.dataList.Count - 1].dimensionD.ToString() + "";
            storedResults.readoutList.Add(txt);
            // Next Coordinates:

            coordD += 1;
            if (coordD >= resolutionD) {
                coordD = 0;

                coordC += 1;
                if (coordC >= resolutionC) {
                    coordC = 0;

                    coordB += 1;
                    if (coordB >= resolutionB) {
                        coordB = 0;

                        coordA += 1;
                        if (coordA >= resolutionA) {
                            // DONE!!

                            Debug.Log("GridSearch COMPLETE!!! Saving...");
                            // Save out Results of all runs:
                            SaveResultsDataToFile();

                            isComplete = true;
                            return;
                        }
                    }
                }
            }
        }
        
        Debug.Log("GridSearch RUN |" + curIter.ToString() + "| A: " + coordA.ToString() + ", B: " + coordB.ToString() + ", C: " + coordC.ToString() + ", D: " + coordD.ToString());

        CreateDataContainer();

        //GridSearchRunData GridSearchRunData = new GridSearchRunData();

        //GridSearchRunData.dimensionA = Mathf.Lerp(minA, maxA, (float)coordA / (float)(resolution - 1));
        //GridSearchRunData.dimensionB = Mathf.Lerp(minB, maxB, (float)coordB / (float)(resolution - 1));
        //GridSearchRunData.dimensionC = Mathf.Lerp(minC, maxC, (float)coordC / (float)(resolution - 1));
        //GridSearchRunData.dimensionD = Mathf.Lerp(minD, maxD, (float)coordD / (float)(resolution - 1));

        //storedResults.dataList.Add(GridSearchRunData);
    }

    public void UpdateGridSearch() {

    }

    public void DataEntry(int curGen, float score) {
        //Debug.Log("DataEntry! " + curGen.ToString() + ", score: " + score.ToString());
        storedResults.dataList[storedResults.dataList.Count - 1].scoresList.Add(score);
        //if(biggerIsBetter) {
        if (score > storedResults.dataList[storedResults.dataList.Count - 1].highestScore) {
            storedResults.dataList[storedResults.dataList.Count - 1].highestScore = score;
        }
        //}
        //else {
        if (score < storedResults.dataList[storedResults.dataList.Count - 1].lowestScore) {
            storedResults.dataList[storedResults.dataList.Count - 1].lowestScore = score;
        }
        //}
        
    }
}
