using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;

[Serializable]
public class TrainingExample
{
    public double[] input;
    public double[] output;
}

[ExecuteInEditMode]
public class RapidLib: MonoBehaviour {

    IntPtr model = (IntPtr)0;

    public Transform[] inputs;

    public double[] outputs;

    //public double[] TrainingOutputs;

    public TrainingExample[] trainingExamples;

    public bool run = false;

    public bool collectData = false;

    //Lets make our calls from the Plugin

    [DllImport("RapidLibPlugin")]
    private static extern IntPtr createRegressionModel();

    [DllImport("RapidLibPlugin")]
    private static extern void destroyModel(IntPtr model);

    [DllImport("RapidLibPlugin")]
    private static extern IntPtr createTrainingSet();

    [DllImport("RapidLibPlugin")]
    private static extern void destroyTrainingSet(IntPtr trainingSet);

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

    [DllImport("RapidLibPlugin")]
    private static extern int process(IntPtr model, double [] input, int numInputs, double [] output, int numOutputs);
    

    void Start () {
        //model = (IntPtr)0;
        Train();
    }

    void OnDestroy()
    {
        if ((int)model != 0)
        {
            destroyModel(model);
        }
        model = (IntPtr)0;
    }

    public void AddTrainingExample()
    {
        TrainingExample newExample = new TrainingExample();
        newExample.input = new double[3 * inputs.Length];

        for(int i = 0; i < inputs.Length; i++)
        {
            newExample.input[3 * i] = inputs[i].position.x;
            newExample.input[3 * i + 1] = inputs[i].position.y;
            newExample.input[3 * i + 2] = inputs[i].position.z;
        }

        newExample.output = new double[outputs.Length];
        for (int i = 0; i < outputs.Length; i++)
        {
            newExample.output[i] = outputs[i];
        }

        Array.Resize<TrainingExample>(ref trainingExamples, trainingExamples.Length + 1);
        trainingExamples[trainingExamples.Length - 1] = newExample;

    }

    public void Train()
    {
        Debug.Log("training");

        if ((int)model != 0)
        {
            destroyModel(model);
        }
        model = (IntPtr)0;

        model = createRegressionModel();
        IntPtr trainingSet = createTrainingSet();
        for(int i = 0; i < trainingExamples.Length; i++)
        {
            addTrainingExample(trainingSet, trainingExamples[i].input, trainingExamples[i].input.Length, trainingExamples[i].output, trainingExamples[i].output.Length);
        }
        if(!train(model, trainingSet))
        {
            Debug.Log("training failed");
        }
        destroyTrainingSet(trainingSet);
    }

    void Update()
    {
        if (run && (int)model != 0) { 
            double [] input = new double[3 * inputs.Length];

            for (int i = 0; i < inputs.Length; i++)
            {
                input[3 * i] = inputs[i].position.x;
                input[3 * i + 1] = inputs[i].position.y;
                input[3 * i + 2] = inputs[i].position.z;
            }

            Debug.Log(input);
            Debug.Log(input.Length);
            for (int i = 0; i < input.Length; i++)
            {
                Debug.Log(input[i]);
            }

            Debug.Log(outputs);
            Debug.Log(outputs.Length);
            for (int i = 0; i < outputs.Length; i++)
            {
                Debug.Log(outputs[i]);
            }
            Debug.Log(process(model, input, input.Length, outputs, outputs.Length));
       } else if (collectData) {
            AddTrainingExample();
       }
    }
}
