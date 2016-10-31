
#if _MSC_VER // this is defined when compiling with Visual Studio
#define EXPORT_API __declspec(dllexport) // Visual Studio needs annotating exported functions with this
#else
#define EXPORT_API // XCode does not need annotating exported functions, so define is empty
#endif

// ------------------------------------------------------------------------
// Plugin itself

#include <vector>
#include <memory>
#include "regression.h"


std::vector < std::unique_ptr <regression> > regression_models;
std::vector < std::unique_ptr <std::vector<trainingExample>> > training_sets;

// Link following functions C-style (required for plugins)
extern "C"
{

EXPORT_API regression * createRegressionModel() {
	//regression_models.push_back(std::unique_ptr<regression>(new regression(3, 4)));
	//return regression_models.back().get();
	return new regression();
}

EXPORT_API void destroyModel(regression *model) {
	delete model;
}

EXPORT_API int getNumInputs(regression *model) {
	return  model->getNumInputs();
}


EXPORT_API std::vector<trainingExample > * createTrainingSet() {
	//training_sets.push_back(std::unique_ptr<std::vector<trainingExample >>(new std::vector<trainingExample >()));
	//return training_sets.back().get();
	return new std::vector<trainingExample >();
}

EXPORT_API void destroyTrainingSet(std::vector<trainingExample> *trainingSet) {
	delete trainingSet;
}

EXPORT_API void addTrainingExample(std::vector<trainingExample> *trainingSet, double *inputs, int numInputs, double *outputs, int numOutputs) {
	trainingExample tempExample;
	//tempExample.input = { 0.2, 0.7 };
	for (int i = 0; i < numInputs; i++) {
		tempExample.input.push_back(inputs[i]);
	}
	for (int i = 0; i < numOutputs; i++) {
		tempExample.output.push_back(outputs[i]);
	}
	//tempExample.input.insert(tempExample.input.begin(), inputs, inputs+numInputs);
	//tempExample.output = { 3.0 };
	//tempExample.output.insert(tempExample.input.begin(), outputs, outputs + numOutputs);
	trainingSet->push_back(tempExample);
}

EXPORT_API int getNumTrainingExamples(std::vector<trainingExample> *trainingSet) {
	return trainingSet->size();
}


EXPORT_API double getInput(std::vector<trainingExample> *trainingSet, int i, int j) {
	if (i < trainingSet->size() && j < (*trainingSet)[i].input.size()) {
		return (*trainingSet)[i].input[j];
	}
	return 0.0;
}

EXPORT_API double getOutput(std::vector<trainingExample> *trainingSet, int i, int j) {
	if (i < trainingSet->size() && j < (*trainingSet)[i].output.size()) {
		return (*trainingSet)[i].output[j];
	}
	return 0.0;
}

EXPORT_API bool train(regression *model, std::vector<trainingExample> *trainingSet) {
	model->initialize();
	return  model->train(*trainingSet);
}

EXPORT_API int process(regression *model, double *input, int numInputs, double *output, int numOutputs) {
	std::vector<double> inputVector;
	for (int i = 0; i < numInputs; i++) {
		inputVector.push_back(input[i]);
	}
	std::vector<double> outputVector = model->process(inputVector);
	
	if (numOutputs > outputVector.size()) {
		numOutputs = outputVector.size();
	}
	
	for (int i = 0; i < numOutputs; i++) {
		output[i] = outputVector[i];
	}
	return numOutputs;
}

// The functions we will call from Unity.
//
//const EXPORT_API char*  PrintHello(){
//	return "Hello";
//}
//
//int EXPORT_API PrintANumber(){
//	return 5;
//}
//
//int EXPORT_API AddTwoIntegers(int a, int b) {
//	return a + b;
//}
//
//float EXPORT_API AddTwoFloats(float a, float b) {
//	return a + b;
//}

} // end of export C block
