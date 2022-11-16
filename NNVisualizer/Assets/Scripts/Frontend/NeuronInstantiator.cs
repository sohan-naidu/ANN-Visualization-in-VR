// using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.Barracuda;
using UnityEngine.Assertions;

public class NeuronInstantiator : MonoBehaviour {
    // Prefabs and Materials
    public GameObject neuronPrefab;
    public Material lineMaterial;

    // neural network models
    private Model model; // loads up Model
    private Model prevModel; // loads up Model
    public NNModel Model;
    public NNModel testInputModel;
    public NNModel testChangedModel;

    // global objects that contain network data
    public GameObject NeuralNetWorkSpawner;
    public List<GameObject> layerObjects = new List<GameObject>();
    public NNCube cube;
    private GameObject LayerParentGameObject; // will be instantiated at runtime
    List<List<GameObject>> sphereReferences = new List<List<GameObject>>();
    List<List<List<GameObject>>> emptyGameObjects = new List<List<List<GameObject>>>();
    List<List<Vector3>> sphereCenters = new List<List<Vector3>>();

    // keeps track of elapsed time
    public int currentEpoch;
    float elapsedTime;

    public class NNCube {
        public float height;
        public float width;
        public float length;
        public int verticalLimit;
        public int neuronsCountLimit;
        public Vector3 cubeCenter;
        public Vector3 jumpLength;

        public NNCube(Vector3 cubeDimensions, int verticalLimit, int neuronsCountLimit, Vector3 cubeCenter, GameObject neuronPrefab)
        {
            this.length = cubeDimensions.x;
            this.height = cubeDimensions.y;
            this.width = cubeDimensions.z;
            this.verticalLimit = verticalLimit;
            this.neuronsCountLimit = neuronsCountLimit;
            this.cubeCenter = cubeCenter;

            float neuronRadius = neuronPrefab.transform.localScale.x;
            jumpLength.x = ( this.length - ( 2 * neuronRadius ) ) / ( this.neuronsCountLimit - 1 );
            jumpLength.y = ( this.height - ( 2 * neuronRadius ) ) / ( this.neuronsCountLimit - 1 );
            jumpLength.z = ( this.width - ( 2 * neuronRadius ) ) / ( this.neuronsCountLimit - 1 );
        }
    }

    public List<float[]> Generate_Weights(Model currentNNModel)
    {
        List<float[]> weights = new List<float[]>();
        foreach (var layer in currentNNModel.layers) {
            if (layer.type == Unity.Barracuda.Layer.Type.Dense) {
                var currentTensor = layer.DataSetToTensor(0);
                weights.Add(currentTensor.data.Download(currentTensor.shape));
            }
        }
        return weights;
    }

    // cleans up network before re-drawing
    private void Clean_Up()
    {
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
    }

    private void Spawn_Vertically(float x, float z, int numberOfNeurons, int layer, NNCube cube, int numberOfNeuronsDrawn)
    {
        // jump by a certain amount
        float lowestPosition = cube.cubeCenter.y - ( cube.jumpLength.y * ( numberOfNeurons / 2 ) );
        if (numberOfNeurons % 2 == 0)
            lowestPosition += cube.jumpLength.y / 2;

        float curY = lowestPosition;

        for (int i = 0; i < numberOfNeurons; i++) {
            // spawn a neuron at this position
            Vector3 currentPosition = new Vector3(x, curY, z);

            GameObject reference = Instantiate(neuronPrefab, currentPosition, Quaternion.identity);
            reference.name = "Neuron (" + layer.ToString() + ", " + i.ToString() + ")";
            NeuronInteraction interactionObj = reference.transform.GetChild(0).GetComponent<NeuronInteraction>();
            interactionObj.layer = layer;
            interactionObj.neuronPosition = i + numberOfNeuronsDrawn;
            reference.transform.parent = NeuralNetWorkSpawner.transform;

            sphereReferences[layer].Add(reference);
            sphereCenters[layer].Add(currentPosition);
            emptyGameObjects[layer].Add(new List<GameObject>());

            curY += cube.jumpLength.y;
        }
        //Assert.IsTrue(curY <= cube.cubeCenter.y + (cube.height / 2.0f));
    }

    private void Spawn_Neurons(ref List<int> layersToBeDrawn)
    {
        int maxDepth = 0;
        float maxDrawSpaceHeight = 7.5f;
        Vector3 NNCenter = new Vector3(-2.5f, 5.25f, 2.25f);
        Vector3 cubeDimensions = new Vector3(15, maxDrawSpaceHeight, 10);
        int neuronsCountLimit = 5;
        int verticalSpaceLimit = 10;
        cube = new NNCube(cubeDimensions, verticalSpaceLimit, neuronsCountLimit, NNCenter, neuronPrefab);

        int layersToTheLeft = ( layersToBeDrawn.Count % 2 == 1 ) ? layersToBeDrawn.Count / 2 : layersToBeDrawn.Count / 2 - 1;
        Vector3 toSubtract = new Vector3(3.0f * layersToTheLeft, maxDrawSpaceHeight, maxDepth);
        Vector3 currentPosition = NNCenter - toSubtract;

        for (int layer = 0; layer < layersToBeDrawn.Count; layer++) {
            sphereCenters.Add(new List<Vector3>());
            sphereReferences.Add(new List<GameObject>());
            emptyGameObjects.Add(new List<List<GameObject>>());

            GameObject temp = new GameObject("Layer " + layer.ToString());
            //temp.transform.position = new Vector3(sphereCenters[i][0].x, ( mx_y + sphereCenters[i][0].y ) / 2.0f, ( layersToBeDrawn[i] > 1 ? layersToBeDrawn[i] / limit + 1 : 0 ) / 2.0f + 2.25f);
            temp.transform.position = new Vector3(currentPosition.x, cube.cubeCenter.y, cube.cubeCenter.z);
            temp.transform.parent = LayerParentGameObject.transform;
            layerObjects.Add(temp);
            currentPosition.z = NNCenter.z - toSubtract.z;

            int batchSize = layersToBeDrawn[layer];
            if (batchSize > cube.neuronsCountLimit) {
                // find a better batchSize
                int best = cube.neuronsCountLimit - ( layersToBeDrawn[layer] % cube.neuronsCountLimit );
                if (best == cube.neuronsCountLimit)
                    best = 0;
                for (int i = cube.neuronsCountLimit - 1; i >= 2; i--) {
                    int val = i - ( layersToBeDrawn[layer] % i );
                    if (val == i)
                        val = 0;
                    if (val < best) {
                        batchSize = i;
                        best = val;
                    }
                }
            }

            int columnsAlongZAxis = layersToBeDrawn[layer] / batchSize;
            if (layersToBeDrawn[layer] % batchSize > 0)
                columnsAlongZAxis++;

            currentPosition.z -= ( cube.jumpLength.z * ( columnsAlongZAxis / 2 ) );
            if (columnsAlongZAxis % 2 == 0)
                currentPosition.z += cube.jumpLength.z / 2;

            int rem = layersToBeDrawn[layer];
            while (rem > 0) {
                Spawn_Vertically(currentPosition.x, currentPosition.z, Mathf.Min(rem, batchSize), layer, cube, layersToBeDrawn[layer] - rem);
                rem -= batchSize;
                currentPosition.z += cube.jumpLength.z;
            }

            currentPosition.x += cube.jumpLength.x;
        }

        // update the number of layers for the pulse controller
        NeuralNetWorkSpawner.GetComponent<PulseController>().numLayers = layersToBeDrawn.Count;
    }

    private void Spawn_Weights(ref List<float[]> weights)
    {
        // LOGIC FOR REPRESENTING WEIGHTS
        for (int i = 1; i < sphereReferences.Count; i++) {
            for (int j = 0; j < sphereReferences[i].Count; j++) {
                for (int k = 0; k < sphereReferences[i - 1].Count; k++) {
                    GameObject temp = new GameObject("Line between (" + ( i - 1 ).ToString() + ", " + k.ToString() + ") and (" + ( i ).ToString() + ", " + j.ToString() + ")");
                    temp.AddComponent<LineRenderer>();
                    emptyGameObjects[i][j].Add(temp);
                    emptyGameObjects[i][j][k].transform.parent = NeuralNetWorkSpawner.transform;
                }
            }
        }

        // getting a list of points between which to draw the respective lines
        for (int i = 1; i < sphereCenters.Count; i++) {
            for (int j = 0; j < sphereCenters[i].Count; j++) {
                Vector3 firstCenter = sphereCenters[i][j];
                // access the lineRenderer for each sphere
                // GameObject reference = sphereReferences[i][j];
                for (int k = 0; k < sphereCenters[i - 1].Count; k++) {
                    LineRenderer lineRenderer = emptyGameObjects[i][j][k].GetComponent<LineRenderer>();
                    float width = weights[i - 1][k * ( sphereCenters[i].Count ) + j];
                    width += 1.0f;
                    width /= 2.0f;
                    width = Mathf.Lerp(0.003f, 0.03f, width);
                    lineRenderer.startWidth = width;
                    lineRenderer.endWidth = width;
                    // 0.004 to 0.02
                    lineRenderer.material = lineMaterial;
                    Vector3 sphereCenter = sphereCenters[i - 1][k];
                    Vector3[] points = new Vector3[2];
                    points[0] = firstCenter;
                    points[1] = sphereCenter;
                    lineRenderer.SetPositions(points);
                }
            }
        }
    }

    private Model getNextModel()
    {
        NNModel Model;
        string epochNumber = "epoch_" + currentEpoch.ToString() + ".onnx";
        Model = (NNModel)AssetDatabase.LoadAssetAtPath("Assets/Scripts/Backend/epochs/" + epochNumber, typeof(NNModel));
        if (Model == null) { 
            Debug.Log("Could not find next model");
            return null;
        }
        Model newModel;
        newModel = ModelLoader.Load(Model);
        Debug.Log("Starting Epoch number " + currentEpoch);
        currentEpoch += 5;
        return newModel;
    }

    // should be called to check for every update to the network
    public void InstantiateNetwork()
    {
        prevModel = model;
        model = getNextModel();
        if (model == null)
        {
            // return because there are no changes to be made
            return;
        }
        sphereCenters = new List<List<Vector3>>();
        List<int> layersToBeDrawn = new List<int>();
        List<float[]> weights = Generate_Weights(model);
        // int cols = tensorThree.shape.channels;
        // int rows = tensorThree.shape.batch;

        // Debug.Log(weights);

        // fetching details on each layer
        bool first = true;
        foreach (var layer in model.layers) {
            if (layer.type == Unity.Barracuda.Layer.Type.Dense) {
                if (first) {
                    layersToBeDrawn.Add(layer.datasets[0].shape.flatHeight);
                }
                first = false;
                layersToBeDrawn.Add(layer.datasets[0].shape.flatWidth);
            }
        }

        Clean_Up();
        Spawn_Neurons(ref layersToBeDrawn);
        //Create_Layer_Objects(ref layersToBeDrawn);
        Spawn_Weights(ref weights);
        SendPulses(Generate_Weights(prevModel), Generate_Weights(model));
    }

    // ideally, should compare two neural networks to see changes/differences, and then send pulses to the ones that have been changed
    // at the moment, it sends pulses everywhere (as a test).
    void SendPulses(List<float[]> previousModel, List<float[]> currentModel)
    {
        List<List<Vector3>> finalList = new List<List<Vector3>>();
        List<int> correspondingLayer = new List<int>();
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
                        correspondingLayer.Add(i + 1);
                        // Debug.Log("Sending pulse from (" + i.ToString() + ", " + j.ToString() + ") to (" + ( i + 1 ).ToString() + ", " + k.ToString() + ")");
                    }
                }
            }
        }
        NeuralNetWorkSpawner.GetComponent<PulseController>().sendNNPulse(finalList, correspondingLayer, 0.1f);
    }

    void Start()
    {
        elapsedTime = 0;
        currentEpoch = 0;
        LayerParentGameObject = new GameObject("Layer Parent");
        LayerParentGameObject.transform.position = new Vector3(0, 0, 0);
        LayerParentGameObject.transform.rotation = Quaternion.identity;
        InstantiateNetwork();
        //Model testOldModel = ModelLoader.Load(testInputModel);
        //Model testUpdatedModel = ModelLoader.Load(testChangedModel);
        //SendPulses(Generate_Weights(testOldModel), Generate_Weights(testUpdatedModel));
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= 5.0f) {
            elapsedTime = 0;
            // check for updates
            //List<float[]> oldWeights = Generate_Weights(model);
            // get a diff between both models
            // update the weights (also do layers at some point)
            //GameObject.Find("UI").GetComponent<UIHandler>().callUpdate();
            //InstantiateNetwork();
            //Model testOldModel = ModelLoader.Load(testInputModel);
            //Model testUpdatedModel = ModelLoader.Load(testChangedModel);
        }
    }
}
