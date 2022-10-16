using UnityEngine;
using System.Collections.Generic;
using Leap.Unity.Interaction;

public class UIHandler : MonoBehaviour {

    // In charge of: 
    //1. Add buttons to left of camera
    //2. After detecting an upress of a button, spawn slider
    //3. After detecting unpress of slider, allow for layer selection 
    //4. After a layer has been selected, call manzood's update which internally calls
    //   Sohan's function
    public enum ButtonType {
        AddNeuron,
        DeleteNeuron,
        AddLayer
    }

    [SerializeField]
    Camera head;
    [SerializeField]
    GameObject sliderPrefab;
    [SerializeField]
    GameObject UIButtonsPrefab;
    [SerializeField]
    GameObject layerBoxPrefab;
    GameObject layerBoxParent;

    GameObject buttons;
    GameObject slider;
    public ButtonType buttonType;
    public int numberOfNeurons;
    public int layer;

    private void Start()
    {
        instantiateUI();
    }
    public void instantiateUI()
    {
        buttons = Instantiate(UIButtonsPrefab);
        buttons.transform.SetParent(GameObject.Find("UI").transform);
    }
    private void destroyObject(GameObject obj)
    {
        foreach (Transform child in obj.transform) {
            Destroy(child.gameObject);
        }
    }

    //Destroy All the buttons
    public void destroyButtons(string button)
    {
        // Debug.Log("Destroy buttons called");
        // Debug.Log(button);
        if (button == "Add Neuron Button")
            buttonType = ButtonType.AddNeuron;
        else if (button == "Delete Neuron Button")
            buttonType = ButtonType.DeleteNeuron;

        destroyObject(buttons);
    }

    //After button, get slider
    public void spawnSlider()
    {
        Debug.Log("slider has been spawned");
        slider = Instantiate(sliderPrefab, this.transform);
        slider.transform.SetParent(GameObject.Find("UI").transform);
    }

    public void spawnLayerBoxes()
    {
        layerBoxParent = GameObject.Find("Layer Parent");
        //Layer information
        //  transform of layer
        //  name/index of layer
        //spawn the boxes
        for (int i = 0; i < layerBoxParent.transform.childCount; i++) {
            GameObject obj = Instantiate(layerBoxPrefab);
            obj.name = "LayerBox" + i;
            obj.transform.SetParent(layerBoxParent.transform);
            obj.transform.position = layerBoxParent.transform.GetChild(i).position;
        }
    }

    //Manzood + Sohan integration
    //Do what they want here
    public void callUpdate()
    {
        //call Sohan's script

        instantiateUI();

    }
}
