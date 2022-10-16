using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;

public class NNDiff : MonoBehaviour
{
    public GameObject NeuralNetworkSpawner;
    // takes a set of weights of two neural networks and generates their diff
    // TODO: currently very barebones, needs updation to handle changes in layers
    public List<bool[]> Generate_Diff(Model previous, Model current)
    {
        List<float[]> previousWeights = NeuralNetworkSpawner.GetComponent<NeuronInstantiator>().Generate_Weights(previous);
        List<float[]> currentWeights = NeuralNetworkSpawner.GetComponent<NeuronInstantiator>().Generate_Weights(current);
        List<bool[]> diff = new List<bool[]>();
        for (int i = 0; i < previousWeights.Count; i++)
        {
            diff.Add(new bool[currentWeights[i].Length]);
            for (int j = 0; j < previousWeights[i].Length; j++) { 
                if (previousWeights[i][j] != currentWeights[i][j])
                {
                    diff[i][j] = true;
                } 
                else
                {
                    diff[i][j] = false;
                }
            }
        }
        return diff;
    }
}
