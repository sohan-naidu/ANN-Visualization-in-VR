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
    private int currentEpoch;
    bool updated;

    GameObject buttons;
    GameObject slider;
    GameObject layerBoxes;
    public GameObject neuronSelect;
    public ButtonType buttonType;
    public GameObject NNSpawner;
    public int numberOfNeurons;
    public int layer;
    public int neuronPosition;
    public GameObject loadingScreen;


    System.Diagnostics.Process p;

    private void Start()
    {
        fileName = Application.dataPath + "/Scripts/Backend/scripts/backend.py";
        Debug.Log(fileName);
        instantiateUI();
        updated = false;
        // callUpdate();
    }

    private void Update()
    {
        if (p != null && p.HasExited && !updated) {
            //remove loading screen
            loadingScreen.SetActive(false);

            //Load new network and pulse
            NNSpawner.GetComponent<PulseController>().Start_Network();

            //Change model to new model
            //Incorporate in NeuronInstantiator
            //GameObject.Find("NeuralNetworkSpawner").GetComponent<NeuronInstantiator>().InstantiateNetwork();
            instantiateUI();

            updated = true;
        }
    }

    public void instantiateUI()
    {
        //buttonType = UIHandler.ButtonType.None;
        buttons = Instantiate(UIButtonsPrefab);
        buttons.name = "UIButtons";
        buttons.transform.SetParent(GameObject.Find("UI").transform);
        // buttons.transform.position = GameObject.Find("UI").transform.position + buttons.transform.position;
        // buttons.transform.position = GameObject.Find("UI").transform.position;
    }

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
            obj.transform.GetChild(0).GetComponent<LayerInteraction>().layerNum = i;
        }

    }

    private void runPythonProcess(string cmd)
    {
        p = new System.Diagnostics.Process();
        p.StartInfo = new System.Diagnostics.ProcessStartInfo();
        //check if works
        //specific to my system
        p.StartInfo.FileName = "C:/My_Files/Dev/Python39/python.exe";
        //p.StartInfo.RedirectStandardError = true;
        p.StartInfo.Arguments = string.Format("{0} {1}", fileName, cmd);
        //p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.UseShellExecute = false;
        //p.StartInfo.CreateNoWindow = true;

        Debug.Log("Sohan's part called\nCmd: " + p.StartInfo.FileName + " " + fileName + " " + cmd);

        //Debug.Log(p.StartInfo.Arguments);
        p.Start();


        //string output = p.StandardOutput.ReadToEnd();
        //Debug.Log(output);

        //string stringErrorOutput = p.StandardError.ReadToEnd();
        //Debug.Log(stringErrorOutput);

        //Make it synchronous
        //p.WaitForExit();
    }

    public void callUpdate()
    {
        //Remove network from screen
        NNSpawner.GetComponent<PulseController>().Clean_Up();
        // Activate loading screen
        loadingScreen.SetActive(true);

        int currentEpoch = 5;
        string args = "";
        //add epoch number later
        switch (buttonType) {
            case ButtonType.AddNeuron:
                args = string.Format("add {0} {1} {2}", layer - 1, numberOfNeurons, currentEpoch);
                break;
            case ButtonType.AddLayer:
                args = string.Format("addL {0} {1} {2}", layer - 1, numberOfNeurons, currentEpoch);
                break;
            case ButtonType.DeleteNeuron:
                args = string.Format("del {0} {1} {2}", layer - 1, neuronPosition, currentEpoch);
                break;
            case ButtonType.DeleteLayer:
                args = string.Format("delL {0} {2}", layer - 1, currentEpoch);
                break;
            default:
                Debug.LogError("Illegal button type");
                break;
        }
        runPythonProcess(args);
        updated = false;
    }
}

