using Leap.Unity.Interaction;
using UnityEngine;

public class ButtonInteraction : InteractionButton {

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    public void destroyButtons()
    {
        UIHandler handler = GameObject.Find("UI").GetComponent<UIHandler>();
        if (name == "Add Neuron Button") {
            handler.buttonType = UIHandler.ButtonType.AddNeuron;
            handler.spawnSlider();
        }
        else if (name == "Add Layer Button") {
            handler.buttonType = UIHandler.ButtonType.AddLayer;
            handler.spawnSlider();
        }
        else if (name == "Delete Neuron Button") {
            handler.buttonType = UIHandler.ButtonType.DeleteNeuron;
            //Pick Neuron
            // Spawn textbox for Neural select
            //handler.spawnNeuronSelectText();
        }
        else if (name == "Delete Layer Button") {
            handler.buttonType = UIHandler.ButtonType.DeleteLayer;
            handler.spawnLayerBoxes();
        }

        Destroy(GameObject.Find("UIButtons"));
    }

}
