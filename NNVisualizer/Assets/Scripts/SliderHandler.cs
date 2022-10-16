using UnityEngine;
using Leap.Unity.Interaction;

public class SliderHandler : MonoBehaviour {

    GameObject mainUI, slider;
    private void Start()
    {
        mainUI = GameObject.Find("UI");
        slider = GameObject.Find("Cube UI Slider");
        if (mainUI == null)
            Debug.LogError("UI Game object not found");
    }
    public void destroySlider()
    {
        UIHandler handler = mainUI.GetComponent<UIHandler>();
        handler.numberOfNeurons = slider.GetComponent<InteractionSlider>().horizontalStepValue;
        Debug.Log("Neuron number selected is " + mainUI.GetComponent<UIHandler>().numberOfNeurons);
        Destroy(gameObject);

        //Spawn your boxes
        handler.spawnLayerBoxes();
    }
}
