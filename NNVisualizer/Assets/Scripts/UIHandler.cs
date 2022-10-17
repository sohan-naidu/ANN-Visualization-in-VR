using UnityEngine;
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
    public string fileName;

    GameObject buttons;
    GameObject slider;
    GameObject layerBoxes;
    public ButtonType buttonType;
    public int numberOfNeurons;
    public int layer;

    private void Start()
    {
        fileName = Application.dataPath + "/Scripts/PythonScripts/scripts/backend.py";
        Debug.Log(fileName);
        instantiateUI();
        // callUpdate();
    }
    public void instantiateUI()
    {
        buttons = Instantiate(UIButtonsPrefab);
        buttons.name = "UIButtons";
        buttons.transform.SetParent(GameObject.Find("UI").transform);
        buttons.transform.position = GameObject.Find("UI").transform.position;
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
        slider.name = "Cube UI Slider Panel";
        slider.transform.SetParent(GameObject.Find("UI").transform);
    }

    public void spawnLayerBoxes()
    {
        layerBoxParent = GameObject.Find("Layer Parent");
        if (layerBoxParent == null) {
            Debug.LogError("NO LAYER PARENT DETECTED");
            return;
        }


        int i = 0;
        layerBoxes = new GameObject("LayerBoxes");
        layerBoxes.transform.position = layerBoxParent.transform.position;

        //for lim of 5 and 2 divs per layer
        //set Scales to (4, 7, 3)
        //z value depends on number of layer divs
        //y value depends on maxDrawHeight => 1 perfectly encapsulates one sphere
        foreach (Transform child in layerBoxParent.transform) {
            //Debug.Log(child.gameObject.name);
            GameObject obj = Instantiate(layerBoxPrefab);
            obj.name = string.Format("LayerBox_{0}", i);
            obj.transform.GetChild(0).GetComponent<LayerInteraction>().layerNum = i;
            obj.transform.SetParent(layerBoxes.transform);
            obj.transform.position = child.position;
            obj.transform.localScale = new Vector3(4, 7, 3);
            i++;
        }

    }

    //Manzood + Sohan integration
    //Do what they want here
    public void callUpdate()
    {
        string cmd;
        switch (buttonType) {
            case ButtonType.AddNeuron:
                cmd = "add";
                break;
            case ButtonType.AddLayer:
                cmd = "addLayer";
                break;
            default:
                cmd = "not Supported";
                break;
        }
        string args = string.Format("{0} {1} {2}", cmd, layer, numberOfNeurons);
        System.Diagnostics.Process p = new System.Diagnostics.Process();
        p.StartInfo = new System.Diagnostics.ProcessStartInfo();
        //check if works
        p.StartInfo.FileName = "python";
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.Arguments = string.Format("{0} {1}", fileName, args);
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.CreateNoWindow = true;
        Debug.Log(p.StartInfo.Arguments);
        p.Start();

        string output = p.StandardOutput.ReadToEnd();
        Debug.Log(output);

        string stringErrorOutput = p.StandardError.ReadToEnd();
        Debug.Log(stringErrorOutput);

        p.WaitForExit();

        Debug.Log("Successfully called Sohan's part");

        //Change model to new model
        //Incorporate in NeuronInstantiator
        // GameObject.Find("NeuralNetworkSpawner").GetComponent<NeuronInstantiator>().InstantiateNetwork();
        instantiateUI();

    }
}
