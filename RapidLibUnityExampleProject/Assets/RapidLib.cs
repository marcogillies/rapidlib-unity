using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;

public class RapidLib: MonoBehaviour {
    //Lets make our calls from the Plugin

    [DllImport("RapidLibPlugin")]
    private static extern IntPtr createRegressionModel();

    [DllImport("RapidLibPlugin")]
    private static extern int getNumInputs(IntPtr model);

    [DllImport("RapidLibPlugin")]
    private static extern IntPtr createTrainingSet();

    [DllImport("RapidLibPlugin")]
    private static extern void addTrainingExample(IntPtr trainingSet, double[] inputs, int numInputs, double[] outputs, int numOutputs);

    [DllImport("RapidLibPlugin")]
    private static extern int getNumTrainingExamples(IntPtr trainingSet);

    [DllImport("RapidLibPlugin")]
    private static extern double getInput(IntPtr trainingSet, int i, int j);

    [DllImport("RapidLibPlugin")]
    private static extern double getOutput(IntPtr trainingSet, int i, int j);

    [DllImport("RapidLibPlugin")]
    private static extern bool train(IntPtr model, IntPtr trainingSet);

    //   [DllImport ("RapidLibPlugin")]
    //private static extern int PrintANumber();

    //[DllImport ("RapidLibPlugin")]
    //private static extern IntPtr PrintHello();

    //[DllImport ("RapidLibPlugin")]
    //private static extern int AddTwoIntegers(int i1,int i2);

    //[DllImport ("RapidLibPlugin")]
    //private static extern float AddTwoFloats(float f1,float f2);	

    void Start () {
		//Debug.Log(PrintANumber());
		//Debug.Log(Marshal.PtrToStringAuto (PrintHello()));
		//Debug.Log(AddTwoIntegers(2,2));
		//Debug.Log(AddTwoFloats(2.5F,4F));
        IntPtr model = createRegressionModel();
        Debug.Log(model);
        Debug.Log(getNumInputs(model));
        Debug.Log("new version");

        IntPtr trainingSet = createTrainingSet();
        double[] inputs = { 1.0, 2.0 };
        double[] outputs = { 1.0, 2.0, 3.0 };
        addTrainingExample(trainingSet, inputs, inputs.Length, outputs, outputs.Length);
        //addTrainingExample(trainingSet);
        //addTrainingExample(trainingSet);
        Debug.Log(getNumTrainingExamples(trainingSet));
        Debug.Log(getInput(trainingSet, 0, 1));
        Debug.Log(getOutput(trainingSet, 0, 0));

        Debug.Log(train(model, trainingSet));
        Debug.Log(getNumInputs(model));

    }
}
