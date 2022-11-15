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
        AddLayer,
        DeleteLayer,
        None
    }

    [SerializeField]
    Camera head;
    [SerializeField]
    GameObject sliderPrefab;
    [SerializeField]
    GameObject UIButtonsPrefab;
    [SerializeField]
    GameObject layerBoxPrefab;
    [SerializeField]
    GameObject neuronSelectPrefab;
    GameObject layerBoxParent;
    private string fileName;

    GameObject buttons;
    GameObject slider;
    GameObject layerBoxes;
    public GameObject neuronSelect;
    public ButtonType buttonType;
    public int numberOfNeurons;
    public int layer;
    public int neuronPosition;

    private void Start()
    {
        fileName = Application.dataPath + "/Scripts/PythonScripts/scripts/backend.py";
        Debug.Log(fileName);
        instantiateUI();
        // callUpdate();
    }
    public void instantiateUI()
    {
        buttonType = UIHandler.ButtonType.None;
        buttons = Instantiate(UIButtonsPrefab);
        buttons.name = "UIButtons";
        buttons.transform.SetParent(GameObject.Find("UI").transform);
        // buttons.transform.position = GameObject.Find("UI").transform.position + buttons.transform.position;
        // buttons.transform.position = GameObject.Find("UI").transform.position;
    }

    /*
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
    */

    public void spawnNeuronSelectText()
    {
        neuronSelect = Instantiate(neuronSelectPrefab);
        neuronSelect.name = "Neurons select Text Box";
        neuronSelect.transform.SetParent(GameObject.Find("Camera").transform);
    }

    public void findNeuronPosition()
    {
        neuronSelect = Instantiate(neuronSelectPrefab);
    }

    public void spawnSlider()
    {
        Debug.Log("slider has been spawned");
        slider = Instantiate(sliderPrefab, this.transform);
        slider.name = "Cube UI Slider Panel";
        slider.transform.SetParent(GameObject.Find("UI").transform);
        // slider.transform.position = GameObject.Find("UI").transform.position + slider.transform.position;
    }

    public void spawnLayerBoxes()
    {
        layerBoxParent = GameObject.Find("Layer Parent");

        layerBoxes = new GameObject("Layer Boxes");
        layerBoxes.transform.position = layerBoxParent.transform.position;

        if (layerBoxParent == null) {
            Debug.LogError("NO LAYER PARENT DETECTED");
            return;
        }

        for (int i = 0; i < layerBoxParent.transform.childCount; i++) {
            GameObject obj = Instantiate(layerBoxPrefab);
            obj.name = string.Format("LayerBox_{0}", i);
            obj.transform.SetParent(layerBoxes.transform);
            obj.transform.position = layerBoxParent.transform.GetChild(i).transform.position;
            NeuronInstantiator.NNCube cube = GameObject.Find("NeuralNetworkSpawner").GetComponent<NeuronInstantiator>().cube;
            obj.transform.localScale = new Vector3(cube.jumpLength.x / 2f, cube.height, cube.width);
        }


        //int i = 0;
        //layerBoxes = new GameObject("LayerBoxes");
        //layerBoxes.transform.position = layerBoxParent.transform.position;

        //fix size (hard-coded for now)
        //for lim of 5 and 2 divs per layer
        //set Scales to (4, 7, 3)
        //z value depends on number of layer divs
        //y value depends on maxDrawHeight => 1 perfectly encapsulates one sphere
        /*foreach (Transform child in layerBoxParent.transform) {
            //Debug.Log(child.gameObject.name);
            GameObject obj = Instantiate(layerBoxPrefab);
            obj.name = string.Format("LayerBox_{0}", i);
            obj.transform.GetChild(0).GetComponent<LayerInteraction>().layerNum = i;
            obj.transform.SetParent(layerBoxes.transform);
            obj.transform.position = child.position;
            obj.transform.localScale = new Vector3(4, 7, 3);
            i++;
        }*/

    }

    private void runPythonProcess(string cmd)
    {
        System.Diagnostics.Process p = new System.Diagnostics.Process();
        p.StartInfo = new System.Diagnostics.ProcessStartInfo();
        //check if works
        //specific to my system
        p.StartInfo.FileName = "C:/My_Files/Dev/Anaconda3/envs/minimal_ds/python.exe";
        //p.StartInfo.RedirectStandardError = true;
        p.StartInfo.Arguments = string.Format("{0} {1}", fileName, cmd);
        //p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.UseShellExecute = true;
        //p.StartInfo.CreateNoWindow = true;

        Debug.Log("Sohan's part called\nCmd: " + p.StartInfo.FileName + " " + fileName + " " + cmd);

        //Debug.Log(p.StartInfo.Arguments);
        p.Start();

        /*
        string output = p.StandardOutput.ReadToEnd();
        Debug.Log(output);

        string stringErrorOutput = p.StandardError.ReadToEnd();
        Debug.Log(stringErrorOutput);
        */

        p.WaitForExit();
    }
    //Manzood + Sohan integration
    //Do what they want here
    public void callUpdate()
    {
        string args = "";
        //add epoch number later
        switch (buttonType) {
            case ButtonType.AddNeuron:
                args = string.Format("add {0} {1}", layer, numberOfNeurons);
                break;
            case ButtonType.AddLayer:
                args = string.Format("addL {0} {1}", layer, numberOfNeurons);
                break;
            case ButtonType.DeleteNeuron:
                args = string.Format("del {0} {1}", layer, neuronPosition);
                break;
            case ButtonType.DeleteLayer:
                args = string.Format("delL {0}", layer);
                break;
            default:
                Debug.LogError("Illegal button type");
                break;
        }
        runPythonProcess(args);

        //Change model to new model
        //Incorporate in NeuronInstantiator
        //GameObject.Find("NeuralNetworkSpawner").GetComponent<NeuronInstantiator>().InstantiateNetwork();
        instantiateUI();

    }
}

