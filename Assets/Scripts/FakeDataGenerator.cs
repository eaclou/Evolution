using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FakeDataGenerator {

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
        sample.inputDataArray[40] = 0f;
        sample.inputDataArray[41] = 0f;
        sample.inputDataArray[42] = 0f;
        sample.inputDataArray[43] = 0f;

        sample.outputDataArray[0] = 0f;
        sample.outputDataArray[1] = 0f;
        sample.outputDataArray[2] = 0f; //dash;
        sample.outputDataArray[3] = 0f; //outComm0;
        sample.outputDataArray[4] = 0f; //outComm1;
        sample.outputDataArray[5] = 0f; //outComm2;
        sample.outputDataArray[6] = 0f; //outComm3;

        return sample;
    }
    public static List<DataSample> GenerateDataAvoidWalls() {
        List<DataSample> dataList = new List<DataSample>();
        int numRandomSets = 64;
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

                        sample.Print();
                    }                    
                }
            }
        }
        return dataList;
    }

    //public DataSample GetDataSample() {
    //
    //}
}
