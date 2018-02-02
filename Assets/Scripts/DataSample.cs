using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataSample {

    public float[] inputDataArray;
    public float[] outputDataArray;

    public DataSample(int numInputs, int numOutputs) {
        inputDataArray = new float[numInputs];
        outputDataArray = new float[numOutputs];
    }

    public DataSample GetCopy() {
        DataSample copy = new DataSample(inputDataArray.Length, outputDataArray.Length);
        for(int i = 0; i < inputDataArray.Length; i++) {
            copy.inputDataArray[i] = inputDataArray[i];
        }
        for (int o = 0; o < outputDataArray.Length; o++) {
            copy.outputDataArray[o] = outputDataArray[o];
        }
        return copy;
    }

    public void Print() {
        string txt = "Inputs: ";
        for(int i = 0; i < inputDataArray.Length; i++) {
            txt += inputDataArray[i].ToString() + ", ";
        }
        txt += "\nOutputs: ";
        for (int i = 0; i < outputDataArray.Length; i++) {
            txt += outputDataArray[i].ToString() + ", ";
        }
        Debug.Log(txt);
    }
}
