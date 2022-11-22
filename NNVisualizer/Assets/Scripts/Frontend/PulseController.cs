using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PulseController : MonoBehaviour
{
    private List<List<Vector3>> positions;
    private List<int> correspondingLayer;
    private List<List<Vector3>> layerPositions;
    private int currentLayer = 0;
    private List<GameObject> trailObjects = new List<GameObject>();
    public GameObject NNPulsePrefab;
    bool sendPulse = false;
    private float moveSpeed = 0;
    public int numLayers = 0;
    private bool initialized = false;
    private bool spawnNetwork = true; // changed by external files to stop the spawning of any networks temporarily

    // TODO: read all the comments and incorporate changes
    // do it layer by layer: when the current positions array is empty, add the positions from the next layer into the array while there are still positions left

    public void Initialize()
    {
        positions = new List<List<Vector3>>();
        layerPositions = new List<List<Vector3>>();
        correspondingLayer = new List<int>();
    }

    public void sendNNPulse(List<List<Vector3>> NeuronPositions, List<int> NNCorrespondingLayer, float moveSpeedVal)
    {
        if (!initialized)
        {
            Initialize();
            initialized = true;
        }
        for (int i = 0; i < NeuronPositions.Count; i++)
        {
            positions.Add(NeuronPositions[i]);
            correspondingLayer.Add(NNCorrespondingLayer[i]);
        }
        sendPulse = true;
        moveSpeed = moveSpeedVal;
    }

    public void Clean_Up()
    {
        for (int i = 0; i < trailObjects.Count; i++)
            Destroy(trailObjects[i]);
        trailObjects.Clear();
        Assert.IsTrue(trailObjects.Count == 0);
        if (layerPositions != null) layerPositions.Clear();
        if (positions != null) positions.Clear();
        if (correspondingLayer != null) correspondingLayer.Clear();
        currentLayer = 0;
        numLayers = 0;
        spawnNetwork = false;
        this.GetComponentInParent<NeuronInstantiator>().Clean_Up();
    }

    public void Start_Network()
    {
        spawnNetwork = true;
    }

    void Update()
    {
        if (!spawnNetwork) return;

        if (!initialized)
        {
            Initialize();
            initialized = true;
        }

        if (positions.Count == 0 && layerPositions.Count == 0)
        {
            this.GetComponentInParent<NeuronInstantiator>().InstantiateNetwork();
        }

        int runs = 0;
        List<bool> tempMarked = new List<bool>(new bool[positions.Count]);
        while (positions.Count > 0 && layerPositions.Count == 0)
        {
            runs++;
            Assert.IsTrue(runs < 1000);
            currentLayer--;
            if (currentLayer < 0) currentLayer += numLayers;
            for (int i = 0; i < positions.Count; i++)
            {
                if (correspondingLayer[i] == currentLayer)
                {
                    layerPositions.Add(positions[i]);
                    tempMarked[i] = true;
                }
            }
        }

        int tmp = 0;
        while (tmp < positions.Count)
        {
            if (tempMarked[tmp])
            {
                tempMarked.RemoveAt(tmp);
                positions.RemoveAt(tmp);
            }
            else
            {
                tmp++;
            }
        }

        //if (numLayers <= 0) return;

        if (sendPulse)
        {
            int n = layerPositions.Count;
            int currentSize = trailObjects.Count;
            while (currentSize < n)
            {
                GameObject reference = Instantiate(NNPulsePrefab);
                reference.transform.position = layerPositions[currentSize][0];
                GameObject temp = new GameObject();
                Transform target = temp.transform;
                target.position = layerPositions[currentSize][1];
                reference.transform.LookAt(target);
                Destroy(temp);
                trailObjects.Add(reference);
                currentSize++;
            }

            List<bool> marked = new List<bool>(new bool[n]);
            for (int i = 0; i < n; i++)
            {
                Vector3 EndPoint = layerPositions[i][1];
                if (trailObjects[i].transform.position != EndPoint)
                {
                    Vector3 directionToMove = EndPoint - trailObjects[i].transform.position;
                    directionToMove = directionToMove.normalized;
                    float stepSize = moveSpeed * Time.deltaTime;
                    directionToMove = directionToMove * stepSize;
                    float maxDistance = Vector3.Distance(trailObjects[i].transform.position, EndPoint);
                    //Vector3 distTravelled = Vector3.ClampMagnitude(directionToMove, maxDistance);
                    //Vector3 LineDistanceVector = EndPoint - layerPositions[i][0];
                    //float totalDistance = LineDistanceVector.magnitude;
                    //float timeRatio = elapsedTime / 2.0f;
                    //float expectedDistance = totalDistance * timeRatio;
                    //float distanceThisFrame = expectedDistance - (trailObjects[i].transform.position - layerPositions[i][0]).magnitude;
                    trailObjects[i].transform.position = trailObjects[i].transform.position + Vector3.ClampMagnitude((directionToMove * 20.0f), maxDistance);
                    //trailObjects[i].transform.position = trailObjects[i].transform.position + Vector3.ClampMagnitude(directionToMove, distanceThisFrame);
                    //trailObjects[i].transform.position = trailObjects[i].transform.position + (directionToMove * distanceThisFrame);
                }
                else
                {
                    marked[i] = true;
                }
            }

            int cur = 0;
            while (cur < trailObjects.Count)
            {
                if (marked[cur])
                {
                    Destroy(trailObjects[cur]);
                    marked.RemoveAt(cur);
                    trailObjects.RemoveAt(cur);
                    layerPositions.RemoveAt(cur); // make this happen one layer at a time
                }
                else
                {
                    ++cur;
                }
            }

        }
    }
}
