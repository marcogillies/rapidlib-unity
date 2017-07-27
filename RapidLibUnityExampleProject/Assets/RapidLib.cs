using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class TrainingExample
{
    public double[] input;
    public double[] output;
}

[Serializable]
public class TrainingSeries
{
    public List<TrainingExample> examples;
}

[ExecuteInEditMode]
public class RapidLib: MonoBehaviour {

    IntPtr model = (IntPtr)0;

    public enum LearningType { Classification, Regression, DTW };

    //This is what you need to show in the inspector.
    public LearningType learningType;

    //public bool classification = false;

    public Transform[] inputs;

    public double[] outputs;

    //public double[] TrainingOutputs;

    public List<TrainingExample> trainingExamples;

    public List<TrainingSeries> trainingSerieses;

    public bool run = false;

    public bool collectData = false;

    public string jsonString = "";

    //Lets make our calls from the Plugin

    [DllImport("RapidLibPlugin")]
    private static extern IntPtr createRegressionModel();

    [DllImport("RapidLibPlugin")]
    private static extern IntPtr createClassificationModel();

    [DllImport("RapidLibPlugin")]
    private static extern IntPtr createSeriesClassificationModel();

    [DllImport("RapidLibPlugin")]
    private static extern void destroyModel(IntPtr model);

    [DllImport("RapidLibPlugin")]
    private static extern void destroySeriesClassificationModel(IntPtr model);

    [DllImport("RapidLibPlugin")]
    //[return: MarshalAs(UnmanagedType.LPStr)]
    private static extern IntPtr getJSON(IntPtr model);

    [DllImport("RapidLibPlugin")]
    private static extern void putJSON(IntPtr model, string jsonString);

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
    private static extern bool trainRegression(IntPtr model, IntPtr trainingSet);

    [DllImport("RapidLibPlugin")]
    private static extern bool trainClassification(IntPtr model, IntPtr trainingSet);

    [DllImport("RapidLibPlugin")]
    private static extern int process(IntPtr model, double [] input, int numInputs, double [] output, int numOutputs);

    [DllImport("RapidLibPlugin")]
    private static extern bool resetSeriesClassification(IntPtr model);

    [DllImport("RapidLibPlugin")]
    private static extern bool addSeries(IntPtr model, IntPtr trainingSet);

    [DllImport("RapidLibPlugin")]
    private static extern int runSeriesClassification(IntPtr model, IntPtr trainingSet);

    [DllImport("RapidLibPlugin")]
    private static extern int getSeriesClassificationCosts(IntPtr model, double[] output, int numOutputs);


    void Start () {
        //model = (IntPtr)0;
        //Train();
        //jsonString = "";
        if ((int)model != 0)
        {
            destroyModel(model);
        }
        model = (IntPtr)0;

        if (learningType == LearningType.Regression)
        {
            model = createRegressionModel();

            putJSON(model, jsonString);
        }
        else
        {
            Train();
        }
    }

    void OnDestroy()
    {
        if ((int)model != 0)
        {
            if (learningType == LearningType.DTW)
            {
                destroySeriesClassificationModel(model);
            } else
            {
                destroyModel(model);
            }
                
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

        //Array.Resize<TrainingExample>(ref trainingExamples, trainingExamples.Length + 1);
        //trainingExamples[trainingExamples.Length - 1] = newExample;
        trainingExamples.Add(newExample);
        
    }

    public void Train()
    {
        Debug.Log("training");
        Debug.Log(model);

        if (learningType == LearningType.DTW) {
            if (trainingSerieses.Count <= 0) return;
        } else {
            if(trainingExamples.Count <= 0) return;
        }

        if ((int)model != 0)
        {
            destroyModel(model);
        }
        model = (IntPtr)0;

        if (learningType == LearningType.Classification)
        {
            model = createClassificationModel();
        } else if (learningType == LearningType.Regression)
        {
            model = createRegressionModel();
        } else if (learningType == LearningType.DTW)
        {
            model = createSeriesClassificationModel();
        } else
        {
            Debug.Log("Error: unknown learning type");
        }

        Debug.Log("created model");
        Debug.Log(model);

        IntPtr trainingSet = createTrainingSet();
        //for(int i = 0; i < trainingExamples.Length; i++)
        if (learningType != LearningType.DTW)
        {
            foreach (TrainingExample example in trainingExamples)
            {
                addTrainingExample(trainingSet, example.input, example.input.Length, example.output, example.output.Length);
            }
        }

        Debug.Log("created training set");

        if (learningType == LearningType.Classification)
        {
            if (!trainClassification(model, trainingSet))
            {
                Debug.Log("training failed");
            }
        } else if (learningType == LearningType.Regression)
        {
            if (!trainRegression(model, trainingSet))
            {
                Debug.Log("training failed");
            }
        } else if (learningType == LearningType.DTW)
        {
            if(trainingSerieses.Count == 0)
            {
                destroyModel(model);
                model = (IntPtr)0;
                Debug.Log("no training series, aborting learning and destroying model");
            } else {
                Debug.Log(model);
                resetSeriesClassification(model);
                foreach (TrainingSeries series in trainingSerieses)
                {
                    trainingSet = createTrainingSet();
                    //for(int i = 0; i < trainingExamples.Length; i++)
                    foreach (TrainingExample example in series.examples)
                    {
                        Debug.Log(example);
                        addTrainingExample(trainingSet, example.input, example.input.Length, example.output, example.output.Length);
                    }
                    Debug.Log(model);
                    Debug.Log(trainingSet);
                    if (!addSeries(model, trainingSet))
                    {
                        Debug.Log("training failed");
                    }
                }
            }
            
                
        } else
        {
            Debug.Log("Error: unknown learning type");
        }

        Debug.Log("finished training");

        destroyTrainingSet(trainingSet);
        
        Debug.Log("about to save");

        //jsonString = getJSON(model);
        if (learningType == LearningType.Regression)
        {
            jsonString = Marshal.PtrToStringAnsi(getJSON(model));
        }

        Debug.Log("saved");

        Debug.Log(jsonString);
    }

    public void StartCollectingData()
    {
        
        collectData = true;
    }

    public void StopCollectingData()
    {
        if (learningType == LearningType.DTW)
        {
            if (!run)
            {
                trainingSerieses.Add(new TrainingSeries());
                trainingSerieses.Last().examples = new List<TrainingExample>(trainingExamples);
            }
            trainingExamples.Clear();
        }
        collectData = false;
    }

    public void ToggleCollectingData()
    {
        if (collectData)
        {
            StopCollectingData();
        } else
        {
            StartCollectingData();
        }
    }

    public void StartRunning()
    {
        run = true;
        if (learningType == LearningType.DTW)
        {
            StartCollectingData();
        } else
        {
            StopCollectingData();
        }
    }

    public void StopRunning()
    {
        if (learningType == LearningType.DTW)
        {
            StopCollectingData();
        } 
        run = false;
    }

    public void ToggleRunning()
    {
        if (run)
        {
            StopRunning();
        }
        else
        {
            StartRunning();
        }
    }

    void Update()
    {
        //Debug.Log(model);
        if (run && (int)model != 0) {
            if (learningType == LearningType.DTW)
            {
                Debug.Log("running");
                IntPtr trainingSet = createTrainingSet();
                //for(int i = 0; i < trainingExamples.Length; i++)
                foreach (TrainingExample example in trainingExamples)
                {
                    addTrainingExample(trainingSet, example.input, example.input.Length, example.output, example.output.Length);
                }
                if(outputs.Length < 1)
                {
                    outputs = new double[1];
                }
                Debug.Log(model);
                Debug.Log(trainingSet);
                outputs[0] = runSeriesClassification(model, trainingSet);
                Debug.Log(outputs[0]);
            }
            else
            {
                double[] input = new double[3 * inputs.Length];

                for (int i = 0; i < inputs.Length; i++)
                {
                    input[3 * i] = inputs[i].position.x;
                    input[3 * i + 1] = inputs[i].position.y;
                    input[3 * i + 2] = inputs[i].position.z;
                }

                //Debug.Log(input);
                //Debug.Log(input.Length);
                //for (int i = 0; i < input.Length; i++)
                //{
                //    Debug.Log(input[i]);
                //}

                //Debug.Log(outputs);
                //Debug.Log(outputs.Length);
                for (int i = 0; i < outputs.Length; i++)
                {
                    Debug.Log(outputs[i]);
                }
                process(model, input, input.Length, outputs, outputs.Length);
            }
       }

       if (collectData) {
            AddTrainingExample();
       }

#if UNITY_EDITOR

        if (Input.GetKeyDown("space"))
        {
            StartCollectingData();
        }

#endif

    }
}
