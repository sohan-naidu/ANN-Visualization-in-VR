// using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Barracuda;

public class NeuronInstantiator : MonoBehaviour {
    public NNModel testModel;
    private Model model;
    // private IWorker worker;
    public GameObject neuronPrefab;
    public Material material;
    public GameObject NeuralNetWorkSpawner;
    List<List<Vector3>> sphereCenters = new List<List<Vector3>>();
    float elapsedTime;
    public NNModel testInputModel;
    public NNModel testChangedModel;
    public List<GameObject> layerObjects = new List<GameObject>();
    public GameObject LayerParentGameObject;
    List<List<GameObject>> sphereReferences = new List<List<GameObject>>();
    List<List<List<GameObject>>> emptyGameObjects = new List<List<List<GameObject>>>();
    // private LineController lineController;

    public List<float[]> Generate_Weights(Model currentNNModel)
    {
        // private Model model = ModelLoader.Load(testModel);
        List<float[]> weights = new List<float[]>();
        foreach (var layer in currentNNModel.layers) {
            if (layer.type == Unity.Barracuda.Layer.Type.Dense) {
                var currentTensor = layer.DataSetToTensor(0);
                weights.Add(currentTensor.data.Download(currentTensor.shape));
            }
        }
        return weights;
    }

    public void InstantiateNetwork()
    {
        model = ModelLoader.Load(testModel);
        sphereCenters = new List<List<Vector3>>();
        // worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, model);
        // float[] w = model.layers[3].weights;
        // var tempvariable = model.layers[3].datasets[0]; // 35!!!
        // Debug.Log(tempvariable);
        List<int> layersToBeDrawn = new List<int>();
        // List<Layer> allLayers = model.layers;
        // Tensor tensorThree = allLayers[3].DataSetToTensor(0);
        // w = tensorThree.data.Download(tensorThree.shape);
        // int cols = tensorThree.shape.channels;
        // int rows = tensorThree.shape.batch;
        // Debug.Log(w);

        List<float[]> weights = Generate_Weights(model);
        Debug.Log(weights);

        // it's present in: model.layers[3].datasets[0].shape.flatHeight, model.layers[3].datasets[0].shape.flatWidth.
        // the first three layers are present to load the model????
        // get the details of the balls asap
        // List<List<float>> weights = new List<List<float>>();
        foreach (var layer in model.layers) {
            if (layer.type == Unity.Barracuda.Layer.Type.Dense) {
                // found a layer. Add this to the list of layers to be drawn
                layersToBeDrawn.Add(layer.datasets[0].shape.flatWidth);
            }
        }
        int maxDepth = 0;

        // base your logic off the center instead of the corner
        Vector3 NNCenter = new Vector3(-2.5f, 0.25f, 2.25f);
        float maxDrawSpaceHeight = 7.5f;
        // Vector3 startPosition = new Vector3(-2.5f, 0.25f, 2.25f);
        int layersToTheLeft = layersToBeDrawn.Count / 2;
        if (layersToBeDrawn.Count % 2 == 1) {
            layersToTheLeft--;
        }
        // depth of the deepest layer
        Vector3 toSubtract = new Vector3(3.0f * layersToTheLeft, maxDrawSpaceHeight, maxDepth);
        Vector3 currentPosition = NNCenter - toSubtract;
        int index = 0;

        for (int i = 0; i < sphereReferences.Count; i++)
            for (int j = 0; j < sphereReferences[i].Count; j++)
                Destroy(sphereReferences[i][j]);

        for (int i = 0; i < emptyGameObjects.Count; i++)
            for (int j = 0; j < emptyGameObjects[i].Count; j++)
                for (int k = 0; k < emptyGameObjects[i][j].Count; k++)
                    Destroy(emptyGameObjects[i][j][k]);

        for (int i = 0; i < layerObjects.Count; i++)
            Destroy(layerObjects[i]);

        sphereReferences = new List<List<GameObject>>();
        emptyGameObjects = new List<List<List<GameObject>>>();
        foreach (int size in layersToBeDrawn) {
            sphereCenters.Add(new List<Vector3>());
            sphereReferences.Add(new List<GameObject>());
            emptyGameObjects.Add(new List<List<GameObject>>());
            float currentDrawSpaceHeight = maxDrawSpaceHeight;
            float drawSpaceCenter = maxDrawSpaceHeight / 2.0f + NNCenter.y;
            if (size < 5)
                currentDrawSpaceHeight /= 2.0f;
            float jump = (float)currentDrawSpaceHeight / size;
            int toTheSide = size / 2;
            currentPosition.y = drawSpaceCenter - ( toTheSide * jump );
            // spawn that many balls at a certain height
            for (int i = 0; i < size; i++) {
                // spawn a ball
                GameObject reference = Instantiate(neuronPrefab, currentPosition, Quaternion.identity);
                reference.transform.parent = NeuralNetWorkSpawner.transform;
                sphereReferences[index].Add(reference);
                sphereCenters[index].Add(currentPosition);
                emptyGameObjects[index].Add(new List<GameObject>());
                currentPosition.y += jump;
            }
            index++;
            currentPosition.x += 3.0f;
        }

        int limit = 5;
        for (int i = 0; i < layersToBeDrawn.Count; i++) {
            float mx_y = 0;
            for (int j = 0; j < layersToBeDrawn[i]; j++) {
                sphereCenters[i][j] = new Vector3(sphereCenters[i][j].x, sphereCenters[i][j % limit].y, sphereCenters[i][j].z + ( ( j / limit ) * 2.0f ));
                sphereReferences[i][j].transform.position = sphereCenters[i][j];
                mx_y = Mathf.Max(mx_y, sphereCenters[i][j].y);
            }
            GameObject temp = new GameObject("Layer " + i.ToString());
            temp.transform.position = new Vector3(sphereCenters[i][0].x, ( mx_y + sphereCenters[i][0].y ) / 2.0f, ( layersToBeDrawn[i] > 1 ? layersToBeDrawn[i] / limit + 1 : 0 ) / 2.0f + 2.25f);
            temp.transform.parent = LayerParentGameObject.transform;
            layerObjects.Add(temp);
        }

        // LOGIC FOR REPRESENTING WEIGHTS
        for (int i = 1; i < sphereReferences.Count; i++) {
            for (int j = 0; j < sphereReferences[i].Count; j++) {
                for (int k = 0; k < sphereReferences[i - 1].Count; k++) {
                    GameObject temp = new GameObject();
                    temp.AddComponent<LineRenderer>();
                    emptyGameObjects[i][j].Add(temp);
                    emptyGameObjects[i][j][k].transform.parent = NeuralNetWorkSpawner.transform;
                }
            }
        }

        // getting a list of points
        for (int i = 1; i < sphereCenters.Count; i++) {
            for (int j = 0; j < sphereCenters[i].Count; j++) {
                Vector3 firstCenter = sphereCenters[i][j];
                // access the lineRenderer for each sphere
                // GameObject reference = sphereReferences[i][j];
                for (int k = 0; k < sphereCenters[i - 1].Count; k++) {
                    LineRenderer lineRenderer = emptyGameObjects[i][j][k].GetComponent<LineRenderer>();
                    float width = weights[i][k];
                    width += 1.0f;
                    width /= 2.0f;
                    width = Mathf.Lerp(0.003f, 0.03f, width);
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

    // ideally, should compare two neural networks to see changes/differences, and then send pulses to the ones that have been changed
    // at the moment, it sends pulses everywhere (as a test).
    void SendPulses(List<float[]> previousModel, List<float[]> currentModel)
    {
        List<List<Vector3>> finalList = new List<List<Vector3>>();
        List<bool[]> diff = NeuralNetWorkSpawner.GetComponent<NNDiff>().Generate_Diff(previousModel, currentModel);
        // go through diff, finding out which one is to be added
        for (int i = 0; i < sphereCenters.Count - 1; i++) {
            for (int j = 0; j < sphereCenters[i].Count; j++) {
                for (int k = 0; k < sphereCenters[i + 1].Count; k++) {
                    int ind = j * sphereCenters[i + 1].Count + k;
                    if (diff[i][ind] == true) {
                        List<Vector3> tempList = new List<Vector3>();
                        tempList.Add(sphereCenters[i + 1][k]);
                        tempList.Add(sphereCenters[i][j]);
                        finalList.Add(tempList);
                        // Debug.Log("sending pulse")
                    }
                }
            }
        }
        /* for (int i = 0; i < sphereCenters.Count - 1; i++)
        {
            for (int j = 0; j < sphereCenters[i].Count; j++)
            {
                for (int k = 0; k < sphereCenters[i + 1].Count; k++)
                {
                    List<Vector3> tempList = new List<Vector3>();
                    tempList.Add(sphereCenters[i][j]);
                    tempList.Add(sphereCenters[i + 1][k]);
                    finalList.Add(tempList);
                }
            }
        } */
        NeuralNetWorkSpawner.GetComponent<PulseController>().sendNNPulse(finalList, 0.1f);
    }

    void Start()
    {
        elapsedTime = 0;
        LayerParentGameObject = new GameObject("Layer Parent");
        LayerParentGameObject.transform.position = new Vector3(0, 0, 0);
        LayerParentGameObject.transform.rotation = Quaternion.identity;
        InstantiateNetwork();
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= 10.0f) {
            elapsedTime = 0;
            // check for updates
            // Model updatedModel = ModelLoader.Load(testModel);
            List<float[]> oldWeights = Generate_Weights(model);
            // TODO: make sure to check layers as well
            // get a diff between both models
            // update the weights (also do layers at some point)
            // SendPulses(model, updatedModel);
            // don't do it at the moment
            //GameObject.Find("UI").GetComponent<UIHandler>().callUpdate();
            InstantiateNetwork();
            SendPulses(oldWeights, Generate_Weights(model));
        }
    }
}
