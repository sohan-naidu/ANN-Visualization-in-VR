using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;

public class NNDiff : MonoBehaviour {
    public GameObject NeuralNetworkSpawner;

    // takes a set of weights of two neural networks and generates their diff
    // TODO: currently very barebones, needs updation to handle changes in layers
    public List<bool[]> Generate_Diff(List<float[]> previousWeights, List<float[]> currentWeights)
    {
        //List<float[]> previousWeights = NeuralNetworkSpawner.GetComponent<NeuronInstantiator>().Generate_Weights(previous);
        //List<float[]> currentWeights = NeuralNetworkSpawner.GetComponent<NeuronInstantiator>().Generate_Weights(current);
        List<bool[]> diff = new List<bool[]>();
        for (int i = 0; i < currentWeights.Count; i++) {
            diff.Add(new bool[currentWeights[i].Length]);
            for (int j = 0; j < currentWeights[i].Length; j++) {
                if (previousWeights.Count <= i || previousWeights[i].Length <= j) {
                    diff[i][j] = true;
                    continue;
                }
                if (previousWeights[i][j] != currentWeights[i][j]) {
                    diff[i][j] = true;
                }
                else {
                    diff[i][j] = false;
                }
            }
        }
        // TODO: comment the following line out
        diff[0][0] = true;
        return diff;
    }
}
