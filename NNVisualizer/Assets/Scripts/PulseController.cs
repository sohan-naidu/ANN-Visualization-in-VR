using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulseController : MonoBehaviour
{
    private List<List<Vector3>> Positions;
    private List<GameObject> trailObjects = new List<GameObject>();
    public GameObject NNPulsePrefab;
    bool sendPulse = false;
    private float moveSpeed = 0;
    public void sendNNPulse(List<List<Vector3>> NeuronPositions, float moveSpeedVal)
    {
        Positions = NeuronPositions;
        sendPulse = true;
        moveSpeed = moveSpeedVal;
    }

    void Update()
    {
        if (sendPulse)
        {
            int n = Positions.Count;
            int currentSize = trailObjects.Count;
            while (currentSize < n)
            {
                // instantiate new obj
                GameObject reference = Instantiate(NNPulsePrefab);
                reference.transform.position = Positions[currentSize][0];
                GameObject temp = new GameObject();
                Transform target = temp.transform;
                target.position = Positions[currentSize][1];
                reference.transform.LookAt(target);
                Destroy(temp);
                trailObjects.Add(reference);
                currentSize++;
            }
            List<bool> marked = new List<bool>(new bool[n]);
            for (int i = 0; i < n; i++)
            {
                Vector3 EndPoint = Positions[i][1];
                if (trailObjects[i].transform.position != EndPoint)
                {
                    // move it towards the pulse
                    Vector3 directionToMove = EndPoint - trailObjects[i].transform.position;
                    directionToMove = directionToMove.normalized;
                    float stepSize = moveSpeed * Time.fixedDeltaTime;
                    directionToMove = directionToMove * stepSize;
                    float maxDistance = Vector3.Distance(trailObjects[i].transform.position, EndPoint);
                    Vector3 distTravelled = Vector3.ClampMagnitude(directionToMove, maxDistance);
                    trailObjects[i].transform.position = trailObjects[i].transform.position + Vector3.ClampMagnitude(directionToMove, maxDistance);
                } else {
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
                    Positions.RemoveAt(cur);
                } 
                else 
                {
                    ++cur;
                }
            }
        }
    }
}
