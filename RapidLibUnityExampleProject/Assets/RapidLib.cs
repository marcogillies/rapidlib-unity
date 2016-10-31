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

//[ExecuteInEditMode]
public class RapidLib: MonoBehaviour {

    IntPtr model = (IntPtr)0;

    public Transform[] inputs;

    public double[] outputs;

    public double[] TrainingOutputs;

    public TrainingExample[] trainingExamples;

    public bool run = false;

    //Lets make our calls from the Plugin

    [DllImport("RapidLibPlugin")]
    private static extern IntPtr createRegressionModel();

    [DllImport("RapidLibPlugin")]
    private static extern void destroyModel(IntPtr model);

    [DllImport("RapidLibPlugin")]
    private static extern int getNumInputs(IntPtr model);

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

        //   [DllImport ("RapidLibPlugin")]
        //private static extern int PrintANumber();

        //[DllImport ("RapidLibPlugin")]
        //private static extern IntPtr PrintHello();

        //[DllImport ("RapidLibPlugin")]
        //private static extern int AddTwoIntegers(int i1,int i2);

        //[DllImport ("RapidLibPlugin")]
        //private static extern float AddTwoFloats(float f1,float f2);	

    void Start () {
        model = (IntPtr)0;
        //Train();
		//Debug.Log(PrintANumber());
		//Debug.Log(Marshal.PtrToStringAuto (PrintHello()));
		//Debug.Log(AddTwoIntegers(2,2));
		//Debug.Log(AddTwoFloats(2.5F,4F));
        model = createRegressionModel();
        Debug.Log(model);
       // Debug.Log(getNumInputs(model));
        //Debug.Log("new version");
        /*
        IntPtr trainingSet = createTrainingSet();
        double[] inputs = { 0.0, 0.0, 0.0 };
        double[] outputs = { 0.0, 0.0 };
        addTrainingExample(trainingSet, inputs, inputs.Length, outputs, outputs.Length);
        for (int i = 0; i < 5; i++)
        {
            inputs[0] = i;
            for (int j = 0; j < 5; j++)
            {
                inputs[1] = j;
                for (int k = 0; k < 5; k++)
                {
                    inputs[2] = k;
                    outputs[0] = i + j + k;
                    outputs[1] = i + j;
                    addTrainingExample(trainingSet, inputs, inputs.Length, outputs, outputs.Length);
                }
            }
        }
        
        //addTrainingExample(trainingSet, inputs, inputs.Length, outputs, outputs.Length);
        //addTrainingExample(trainingSet);
        //addTrainingExample(trainingSet);
        Debug.Log(getNumTrainingExamples(trainingSet));
        Debug.Log(getInput(trainingSet, 0, 1));
        Debug.Log(getOutput(trainingSet, 0, 0));

        Debug.Log(train(model, trainingSet));
        Debug.Log(getNumInputs(model));


        for (int i = 0; i < inputs.Length; i++)
        {
            Debug.Log(inputs[i]);
        }

        for (int i = 0; i < outputs.Length; i++) {
            outputs[i] = 0.0;
        }
        for (int i = 0; i < outputs.Length; i++)
        {
            Debug.Log(outputs[i]);
        }
        process(model, inputs, inputs.Length, outputs, outputs.Length);
        for (int i = 0; i < outputs.Length; i++)
        {
            Debug.Log(outputs[i]);
        }
        */
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

        newExample.output = new double[TrainingOutputs.Length];
        for (int i = 0; i < TrainingOutputs.Length; i++)
        {
            newExample.output[i] = TrainingOutputs[i];
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
        
        //Debug.Log(getNumTrainingExamples(trainingSet));
        //Debug.Log(getInput(trainingSet, 0, 1));
        //Debug.Log(getOutput(trainingSet, 0, 0));

        if(!train(model, trainingSet))
        {
            Debug.Log("training failed");
        }
        //Debug.Log(getNumInputs(model));
        //outputs = new double[trainingExamples[0].output.Length];
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

            Debug.Log(outputs);
            Debug.Log(outputs.Length);
            for (int i = 0; i < outputs.Length; i++)
            {
                Debug.Log(outputs[i]);
            }
            Debug.Log(process(model, input, input.Length, outputs, outputs.Length));
       }
    }
}
