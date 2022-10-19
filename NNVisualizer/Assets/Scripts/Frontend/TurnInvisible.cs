using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnInvisible : MonoBehaviour {
    Renderer r;
    // Start is called before the first frame update
    void Start()
    {
        r = GetComponent<MeshRenderer>();
        r.enabled = false;
    }
}
