using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;

public class NeuronInstantiator : MonoBehaviour
{
    public NNModel testModel;
    private Model model;
    private IWorker worker;
    public GameObject neuronPrefab;
    public Material material;
    // private LineController lineController;
    private Transform firstCenterTransform;
    private Transform secondCenterTransform;

    // Start is called before the first frame update
    void Start()
    {
        model = ModelLoader.Load(testModel);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, model);
        float[] w = model.layers[3].weights;
        var tempvariable = model.layers[3].datasets[0]; // 35!!!
        Debug.Log(tempvariable);
        List<int> layersToBeDrawn = new List<int>();
        List<Layer> allLayers = model.layers;
        Tensor tensorThree = allLayers[3].DataSetToTensor(0);
        w = tensorThree.data.Download(tensorThree.shape);
        int cols = tensorThree.shape.channels;
        int rows = tensorThree.shape.batch;
        Debug.Log(w);

        List<float[]> weights = new List<float[]>();

        // it's present in: model.layers[3].datasets[0].shape.flatHeight, model.layers[3].datasets[0].shape.flatWidth.
        // the first three layers are present to load the model????
        // get the details of the balls asap
        // List<List<float>> weights = new List<List<float>>();
        foreach (var layer in model.layers)
        {
            if (layer.type == Unity.Barracuda.Layer.Type.Dense)
            {
                // found a layer. Add this to the list of layers to be drawn
                layersToBeDrawn.Add(layer.datasets[0].shape.flatWidth);
                var currentTensor = layer.DataSetToTensor(0);
                weights.Add(currentTensor.data.Download(currentTensor.shape));
            }
        }
        // Debug.Log(layersToBeDrawn);
        Vector3 startPosition = new Vector3(-2.5f, 0.25f, 2.25f);
        Vector3 currentPosition = startPosition;
        // spawn it at an offset
        float maxDrawSpaceHeight = 5.0f;
        List<List<Vector3>> sphereCenters = new List<List<Vector3>>();
        int index = 0;
        // each sphere should spawn its own line renderer
        // then raycast a line to the previous layer
        List<List<GameObject>> sphereReferences = new List<List<GameObject>>();
        List<List<List<GameObject>>> emptyGameObjects = new List<List<List<GameObject>>>();
        foreach(int size in layersToBeDrawn)
        {
            sphereCenters.Add(new List<Vector3>());
            sphereReferences.Add(new List<GameObject>());
            emptyGameObjects.Add(new List<List<GameObject>>());
            float currentDrawSpaceHeight = maxDrawSpaceHeight;
            float drawSpaceCenter = maxDrawSpaceHeight / 2.0f + startPosition.y;
            if (size < 5) currentDrawSpaceHeight /= 2.0f;
            float jump = (float) currentDrawSpaceHeight / size;
            int toTheSide = size / 2;
            currentPosition.y = drawSpaceCenter - (toTheSide * jump);
            // spawn that many balls at a certain height
            for (int i = 0; i < size; i++)
            {
                // spawn a ball
                GameObject reference = Instantiate(neuronPrefab, currentPosition, Quaternion.identity);
                sphereReferences[index].Add(reference);
                sphereCenters[index].Add(currentPosition);
                emptyGameObjects[index].Add(new List<GameObject>());
                currentPosition.y += jump;
            }
            index++;
            currentPosition.x += 3.0f;
        }

        for (int i = 1; i < sphereReferences.Count; i++)
        {
            for (int j = 0; j < sphereReferences[i].Count; j++)
            {
                for (int k = 0; k < sphereReferences[i - 1].Count; k++)
                {
                    GameObject temp = new GameObject();
                    temp.AddComponent<LineRenderer>();
                    emptyGameObjects[i][j].Add(temp);
                }
            }
        }

        // getting a list of points
        for (int i = 1; i < sphereCenters.Count; i++)
        {
            for (int j = 0; j < sphereCenters[i].Count; j++)
            {
                Vector3 firstCenter = sphereCenters[i][j];
                // access the lineRenderer for each sphere
                // GameObject reference = sphereReferences[i][j];
                for (int k = 0; k < sphereCenters[i - 1].Count; k++)
                {
                    LineRenderer lineRenderer = emptyGameObjects[i][j][k].GetComponent<LineRenderer>();
                    float width = weights[i][k];
                    width += 1.0f;
                    width /= 2.0f;
                    width = Mathf.Lerp(0.004f, 0.04f, width);
                    lineRenderer.startWidth = width;
                    lineRenderer.endWidth = width;
                    // 0.004 to 0.02
                    lineRenderer.material = material;
                    Vector3 sphereCenter = sphereCenters[i - 1][k];
                    Vector3[] points = new Vector3[2];
                    points[0] = firstCenter;
                    points[1] = sphereCenter;
                    lineRenderer.SetPositions(points);
                }
            }
        }
        // requires an array of transforms
        // lineController.SetUpLine(points);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
