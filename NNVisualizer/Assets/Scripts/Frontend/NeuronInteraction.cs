using UnityEngine;
using Leap.Unity.Interaction;
using Leap.Unity;

public class NeuronInteraction : MonoBehaviour {

    private InteractionBehaviour _intObj;
    private Outline outline;
    private bool hasBeenPressed;
    private UIHandler _UIHandler;

    public int layer;
    public int neuronPosition;
    // Start is called before the first frame update
    void Start()
    {
        _intObj = GetComponent<InteractionBehaviour>();
        outline = GetComponent<Outline>();
        _UIHandler = GameObject.Find("UI").GetComponent<UIHandler>();
    }

    // Update is called once per frame
    void Update()
    {
        //If you press into a neuron, activate the outline
        if (_intObj is InteractionButton && ( _intObj as InteractionButton ).isPressed) {
            //Activate outline script
            outline.enabled = true;
            hasBeenPressed = true;
        }
        else {
            outline.enabled = false;
        }

        //Detect an upress after user pressed delete neuron button and chooses a neuron
        if (_UIHandler.buttonType == UIHandler.ButtonType.DeleteNeuron && hasBeenPressed && !( _intObj as InteractionButton ).isPressed) {
            _UIHandler.layer = layer;
            _UIHandler.neuronPosition = neuronPosition;
            Destroy(_UIHandler.neuronSelect);
            _UIHandler.callUpdate();
            hasBeenPressed = false;
        }
    }
}
