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

    public bool classification = false;

	public float startDelay = 0.0f;
	public float captureRate = 10.0f;
	public float recordTime = -1.0f;
	float timeToNextCapture = 0.0f;
	float timeToStopCapture = 0.0f;

    public Transform[] inputs;

    public double[] outputs;

    //public double[] TrainingOutputs;

    public TrainingExample[] trainingExamples;

    public bool run = false;

    public bool collectData = false;

    public string jsonString = "";

    //Lets make our calls from the Plugin

    [DllImport("RapidLibPlugin")]
    private static extern IntPtr createRegressionModel();

    [DllImport("RapidLibPlugin")]
    private static extern IntPtr createClassificationModel();

    [DllImport("RapidLibPlugin")]
    private static extern void destroyModel(IntPtr model);

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
    

    void Start () {
        //model = (IntPtr)0;
        //Train();
        //jsonString = "";
        if ((int)model != 0)
        {
            destroyModel(model);
        }
        model = (IntPtr)0;

        if (classification)
        {
            Train();
        }
        else
        {
            model = createRegressionModel();

            putJSON(model, jsonString);
        }
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

        if (trainingExamples.Length <= 0) return;

        if ((int)model != 0)
        {
            destroyModel(model);
        }
        model = (IntPtr)0;

        if (classification)
        {
            model = createClassificationModel();
        } else
        {
            model = createRegressionModel();
        }
        
        Debug.Log("created model");
        
        IntPtr trainingSet = createTrainingSet();
        for(int i = 0; i < trainingExamples.Length; i++)
        {
            addTrainingExample(trainingSet, trainingExamples[i].input, trainingExamples[i].input.Length, trainingExamples[i].output, trainingExamples[i].output.Length);
        }

        Debug.Log("created training set");

        if (classification)
        {
            if (!trainClassification(model, trainingSet))
            {
                Debug.Log("training failed");
            }
        }
        else
        {
            if (!trainRegression(model, trainingSet))
            {
                Debug.Log("training failed");
            }
        }

        Debug.Log("finished training");

        destroyTrainingSet(trainingSet);
        
        Debug.Log("about to save");

        //jsonString = getJSON(model);
        if (!classification)
        {
            jsonString = Marshal.PtrToStringAnsi(getJSON(model));
        }

        Debug.Log("saved");

        Debug.Log(jsonString);
    }

	public void ToggleRecording(){
		collectData = !collectData;
		if(collectData){
			Debug.Log("starting recording in " + startDelay + " seconds");
			timeToNextCapture = Time.time + startDelay;
			if(recordTime > 0){
				timeToStopCapture = Time.time + startDelay + recordTime;
			}else {
				timeToStopCapture = -1;
			}
		}
	}

    void Update()
    {
		Debug.Log ("update " + Time.time);
        if (run && (int)model != 0) { 
            double [] input = new double[3 * inputs.Length];

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
			process(model, input, input.Length, outputs, outputs.Length);
			for (int i = 0; i < outputs.Length; i++)
			{
				Debug.Log(outputs[i]);
			}
       } else if (collectData) {
			if (Application.isPlaying && Time.time >= timeToStopCapture) {
				collectData = false;
				Debug.Log ("end recording");
			} else if (!Application.isPlaying || Time.time >= timeToNextCapture) {
				Debug.Log ("recording");
				AddTrainingExample ();
				timeToNextCapture = Time.time + 1.0f / captureRate;
			}

       }

#if UNITY_EDITOR

        if (Input.GetKeyDown("space"))
        {
			ToggleRecording();
        }

#endif

    }

	void OnGUI(){
		if (collectData) {
			GUI.Label (new Rect (20, 20, 100, 100), "time to capture " + (timeToNextCapture - Time.time));
		}
	}
}
