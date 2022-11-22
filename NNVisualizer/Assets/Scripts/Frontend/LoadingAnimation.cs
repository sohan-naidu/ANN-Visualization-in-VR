using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingAnimation : MonoBehaviour
{
    // Start is called before the first frame update
    float elapsedTime;
    float lastRotationTime;
    float rotation;
    float ROTATION_SPEED = 300;

    void Start()
    {
        elapsedTime = 0;
        lastRotationTime = 0;
        rotation = 0;
    }

    // Update is called once per frame
    void Update()
    {
        elapsedTime += Time.deltaTime;
        double deltaTime = elapsedTime - lastRotationTime;
        transform.rotation = Quaternion.Euler(0, 0, -rotation);
        rotation += (int)(ROTATION_SPEED * deltaTime);
        rotation = rotation % 360;
        if (rotation < 0) rotation += 360;

        lastRotationTime = elapsedTime;
    }
}
