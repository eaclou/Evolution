using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FakeDataGenerator {

    //public static int numSetsAvoidWalls = 64;

	public static float GetRandomValue() {
        return Gaussian.GetRandomGaussian(0f, 1f);
        //return UnityEngine.Random.Range(-1f, 1f);
    }
    public static DataSample GetRandomSampleInputs() {
        DataSample sample = new DataSample(44, 7);

        sample.inputDataArray[0] = 1f; // bias
        sample.inputDataArray[1] = GetRandomValue();
        sample.inputDataArray[2] = GetRandomValue();
        sample.inputDataArray[3] = GetRandomValue();
        sample.inputDataArray[4] = GetRandomValue();
        sample.inputDataArray[5] = Mathf.Abs(GetRandomValue());
        sample.inputDataArray[6] = Mathf.Abs(GetRandomValue());
        sample.inputDataArray[7] = Mathf.Abs(GetRandomValue());
        sample.inputDataArray[8] = GetRandomValue();
        sample.inputDataArray[9] = GetRandomValue();
        sample.inputDataArray[10] = GetRandomValue();
        sample.inputDataArray[11] = GetRandomValue();
        sample.inputDataArray[12] = GetRandomValue();
        sample.inputDataArray[13] = GetRandomValue();
        sample.inputDataArray[14] = GetRandomValue();
        sample.inputDataArray[15] = GetRandomValue();
        sample.inputDataArray[16] = GetRandomValue();
        sample.inputDataArray[17] = GetRandomValue();
        sample.inputDataArray[18] = GetRandomValue();
        sample.inputDataArray[19] = GetRandomValue();
        sample.inputDataArray[20] = GetRandomValue();
        sample.inputDataArray[21] = GetRandomValue();
        sample.inputDataArray[22] = 0f;
        sample.inputDataArray[23] = 0f;
        sample.inputDataArray[24] = 0f;
        sample.inputDataArray[25] = 0f;
        sample.inputDataArray[26] = 0f;
        sample.inputDataArray[27] = Mathf.Abs(GetRandomValue());
        sample.inputDataArray[28] = Mathf.Abs(GetRandomValue());
        sample.inputDataArray[29] = Mathf.Abs(GetRandomValue());
        sample.inputDataArray[30] = Mathf.Abs(GetRandomValue());
        sample.inputDataArray[31] = Mathf.Abs(GetRandomValue());

        sample.inputDataArray[32] = 0f; //GetRandomValue();//testModule.distUp[0];
        sample.inputDataArray[33] = 0f; //GetRandomValue()*0.5f;//testModule.distTopRight[0];
        sample.inputDataArray[34] = 0f; //GetRandomValue();//testModule.distRight[0];
        sample.inputDataArray[35] = 0f; //GetRandomValue() * 0.5f;//testModule.distBottomRight[0];
        sample.inputDataArray[36] = 0f; //GetRandomValue();//testModule.distDown[0];
        sample.inputDataArray[37] = 0f; //GetRandomValue() * 0.5f;//testModule.distBottomLeft[0];
        sample.inputDataArray[38] = 0f; //GetRandomValue();//testModule.distLeft[0];
        sample.inputDataArray[39] = 0f; //GetRandomValue() * 0.5f;//testModule.distTopLeft[0];

        // NOTE!! Disconnect InOutComms for simplification (also in HookupModules())
        sample.inputDataArray[40] = Mathf.Abs(GetRandomValue());
        sample.inputDataArray[41] = Mathf.Abs(GetRandomValue());
        sample.inputDataArray[42] = Mathf.Abs(GetRandomValue());
        sample.inputDataArray[43] = Mathf.Abs(GetRandomValue());

        sample.outputDataArray[0] = 0f;
        sample.outputDataArray[1] = 0f;
        sample.outputDataArray[2] = 0f; //dash;
        sample.outputDataArray[3] = Mathf.Abs(GetRandomValue()); //outComm0;
        sample.outputDataArray[4] = Mathf.Abs(GetRandomValue()); //outComm1;
        sample.outputDataArray[5] = Mathf.Abs(GetRandomValue()); //outComm2;
        sample.outputDataArray[6] = Mathf.Abs(GetRandomValue()); //outComm3;

        return sample;
    }
    public static List<DataSample> GenerateDataRepelWalls(int numRandomSets) {
        List<DataSample> dataList = new List<DataSample>();
        //int numRandomSets = 64;
        for(int i = 0; i < numRandomSets; i++) {
            for (int x = -1; x < 2; x++) {
                for (int y = -1; y < 2; y++) {
                    if(x != 0 & y != 0) {
                        DataSample sample = GetRandomSampleInputs();

                        sample.inputDataArray[32] = Mathf.Max(y, 0f); //testModule.distUp[0];
                        sample.inputDataArray[33] = Mathf.Max(y, 0f) * Mathf.Max(x, 0f);//testModule.distTopRight[0];
                        sample.inputDataArray[34] = Mathf.Max(x, 0f);//testModule.distRight[0];
                        sample.inputDataArray[35] = Mathf.Abs(Mathf.Min(y, 0f)) * Mathf.Max(x, 0f);//testModule.distBottomRight[0];
                        sample.inputDataArray[36] = Mathf.Abs(Mathf.Min(y, 0f));//testModule.distDown[0];
                        sample.inputDataArray[37] = Mathf.Abs(Mathf.Min(y, 0f)) * Mathf.Abs(Mathf.Min(x, 0f));//testModule.distBottomLeft[0];
                        sample.inputDataArray[38] = Mathf.Abs(Mathf.Min(x, 0f));//testModule.distLeft[0];
                        sample.inputDataArray[39] = Mathf.Max(y, 0f) * Mathf.Abs(Mathf.Min(x, 0f));//testModule.distTopLeft[0];

                        sample.outputDataArray[0] = Mathf.Clamp(-sample.inputDataArray[34] + sample.inputDataArray[38]
                                                    - Mathf.Max(sample.inputDataArray[33], 0f) * 0.5f - Mathf.Max(sample.inputDataArray[35], 0f) * 0.5f  // TopRight and BottomRight
                                                    + Mathf.Abs(Mathf.Min(sample.inputDataArray[39], 0f)) * 0.5f + Mathf.Abs(Mathf.Min(sample.inputDataArray[37], 0f)) * 0.5f, -1f, 1f);  // TopLeft & BottomLeft
                        sample.outputDataArray[1] = Mathf.Clamp(-sample.inputDataArray[32] + sample.inputDataArray[36]
                                                    - Mathf.Max(sample.inputDataArray[33], 0f) * 0.5f - Mathf.Max(sample.inputDataArray[39], 0f) * 0.5f  // TopRight and TopLeft
                                                    + Mathf.Abs(Mathf.Min(sample.inputDataArray[35], 0f)) * 0.5f + Mathf.Abs(Mathf.Min(sample.inputDataArray[37], 0f)) * 0.5f, -1f, 1f);  // BottomRight & BottomLeft;

                        dataList.Add(sample);

                        //sample.Print();
                    }                    
                }
            }
        }
        return dataList;
    }
    public static List<DataSample> GenerateDataRepelPreds(int numRandomSets) {
        List<DataSample> dataList = new List<DataSample>();
        //int numRandomSets = 64;
        for (int i = 0; i < numRandomSets; i++) {
            for (int x = -1; x < 2; x++) {
                for (int y = -1; y < 2; y++) {
                    if (x != 0 & y != 0) {
                        DataSample sample = GetRandomSampleInputs();
                        
                        Vector2 enemyPos = new Vector2((float)x, (float)y);
                        Vector2 enemyDir = enemyPos.normalized;

                        sample.inputDataArray[14] = enemyPos.x;  // enemyPos
                        sample.inputDataArray[15] = enemyPos.y;  // enemyPos
                        sample.inputDataArray[18] = enemyDir.x;  // enemyDir
                        sample.inputDataArray[19] = enemyDir.y;  // enemyDir
                        
                        sample.outputDataArray[0] = -sample.inputDataArray[14] - sample.inputDataArray[18] * 0.5f;  // Opposite of enemyPos/Dir
                        sample.outputDataArray[1] = -sample.inputDataArray[15] - sample.inputDataArray[19] * 0.5f;  // Opposite of enemyPos/Dir

                        dataList.Add(sample);

                        //sample.Print();
                    }
                }
            }
        }
        return dataList;
    }
    public static List<DataSample> GenerateDataRepelFriends(int numRandomSets) {
        List<DataSample> dataList = new List<DataSample>();
        //int numRandomSets = 64;
        for (int i = 0; i < numRandomSets; i++) {
            for (int x = -1; x < 2; x++) {
                for (int y = -1; y < 2; y++) {
                    if (x != 0 & y != 0) {
                        DataSample sample = GetRandomSampleInputs();

                        Vector2 friendPos = new Vector2((float)x, (float)y);
                        Vector2 friendDir = friendPos.normalized;

                        sample.inputDataArray[8] = friendPos.x;  // friendPos
                        sample.inputDataArray[9] = friendPos.y;  // friendPos
                        sample.inputDataArray[12] = friendDir.x;  // friendDir
                        sample.inputDataArray[13] = friendDir.y;  // friendDir

                        sample.outputDataArray[0] = -sample.inputDataArray[8] - sample.inputDataArray[12] * 0.5f;  // Opposite of enemyPos/Dir
                        sample.outputDataArray[1] = -sample.inputDataArray[9] - sample.inputDataArray[13] * 0.5f;  // Opposite of enemyPos/Dir

                        dataList.Add(sample);

                        //sample.Print();
                    }
                }
            }
        }
        return dataList;
    }
    public static List<DataSample> GenerateDataAttractPreds(int numRandomSets) {
        List<DataSample> dataList = new List<DataSample>();
        //int numRandomSets = 64;
        for (int i = 0; i < numRandomSets; i++) {
            for (int x = -1; x < 2; x++) {
                for (int y = -1; y < 2; y++) {
                    if (x != 0 & y != 0) {
                        DataSample sample = GetRandomSampleInputs();

                        Vector2 enemyPos = new Vector2((float)x, (float)y);
                        Vector2 enemyDir = enemyPos.normalized;

                        sample.inputDataArray[14] = enemyPos.x;  // enemyPos
                        sample.inputDataArray[15] = enemyPos.y;  // enemyPos
                        sample.inputDataArray[18] = enemyDir.x;  // enemyDir
                        sample.inputDataArray[19] = enemyDir.y;  // enemyDir

                        sample.outputDataArray[0] = sample.inputDataArray[14] + sample.inputDataArray[18];  // Same as enemyPos/Dir
                        sample.outputDataArray[1] = sample.inputDataArray[15] + sample.inputDataArray[19];  // Same as enemyPos/Dir

                        dataList.Add(sample);

                        //sample.Print();
                    }
                }
            }
        }
        return dataList;
    }
    public static List<DataSample> GenerateDataAttractFood(int numRandomSets) {
        List<DataSample> dataList = new List<DataSample>();
        //int numRandomSets = 64;
        for (int i = 0; i < numRandomSets; i++) {
            for (int x = -1; x < 2; x++) {
                for (int y = -1; y < 2; y++) {
                    if (x != 0 & y != 0) {
                        DataSample sample = GetRandomSampleInputs();

                        Vector2 foodPos = new Vector2((float)x, (float)y);
                        Vector2 foodDir = foodPos.normalized;

                        sample.inputDataArray[1] = foodPos.x;  // foodPos
                        sample.inputDataArray[2] = foodPos.y;  // foodPos
                        sample.inputDataArray[3] = foodDir.x;  // foodDir
                        sample.inputDataArray[4] = foodDir.y;  // foodDir

                        sample.outputDataArray[0] = sample.inputDataArray[1] + sample.inputDataArray[3];  // Same as foodPos/Dir
                        sample.outputDataArray[1] = sample.inputDataArray[2] + sample.inputDataArray[4];  // Same as foodPos/Dir

                        dataList.Add(sample);

                        //sample.Print();
                    }
                }
            }
        }
        return dataList;
    }
    public static List<DataSample> GenerateDataAttractFriends(int numRandomSets) {
        List<DataSample> dataList = new List<DataSample>();
        //int numRandomSets = 64;
        for (int i = 0; i < numRandomSets; i++) {
            for (int x = -1; x < 2; x++) {
                for (int y = -1; y < 2; y++) {
                    if (x != 0 & y != 0) {
                        DataSample sample = GetRandomSampleInputs();

                        Vector2 friendPos = new Vector2((float)x, (float)y);
                        Vector2 friendDir = friendPos.normalized;

                        sample.inputDataArray[8] = friendPos.x;  // friendPos
                        sample.inputDataArray[9] = friendPos.y;  // friendPos
                        sample.inputDataArray[12] = friendDir.x;  // friendDir
                        sample.inputDataArray[13] = friendDir.y;  // friendDir

                        sample.outputDataArray[0] = sample.inputDataArray[8] + sample.inputDataArray[12];  // Opposite of enemyPos/Dir
                        sample.outputDataArray[1] = sample.inputDataArray[9] + sample.inputDataArray[13];  // Opposite of enemyPos/Dir

                        dataList.Add(sample);

                        //sample.Print();
                    }
                }
            }
        }
        return dataList;
    }
    public static List<DataSample> GenerateDataStandardMix(int numRandomSets) {
        List<DataSample> mainDataList = new List<DataSample>();

        List<DataSample> repelPredsDataList = FakeDataGenerator.GenerateDataRepelPreds(Mathf.RoundToInt((float)numRandomSets * 1.0f));
        List<DataSample> repelWallsDataList = FakeDataGenerator.GenerateDataRepelWalls(Mathf.RoundToInt((float)numRandomSets * 0.25f));
        List<DataSample> repelFriendsDataList = FakeDataGenerator.GenerateDataRepelFriends(Mathf.RoundToInt((float)numRandomSets * 0.75f));
        List<DataSample> attractFoodDataList = FakeDataGenerator.GenerateDataAttractFood(Mathf.RoundToInt((float)numRandomSets * 2f));

        for(int i = 0; i < repelPredsDataList.Count; i++) {
            mainDataList.Add(repelPredsDataList[i]);
        }
        for (int i = 0; i < repelWallsDataList.Count; i++) {
            mainDataList.Add(repelWallsDataList[i]);
        }
        for (int i = 0; i < repelFriendsDataList.Count; i++) {
            mainDataList.Add(repelFriendsDataList[i]);
        }
        for (int i = 0; i < attractFoodDataList.Count; i++) {
            mainDataList.Add(attractFoodDataList[i]);
        }

        return mainDataList;
    }
    //public DataSample GetDataSample() {
    //
    //}
}
