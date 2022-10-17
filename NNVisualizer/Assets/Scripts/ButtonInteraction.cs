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
        if (name == "Add Neuron Button")
            handler.buttonType = UIHandler.ButtonType.AddNeuron;
        else if (name == "Add Layer Button")
            handler.buttonType = UIHandler.ButtonType.AddLayer;

        handler.spawnSlider();
        Destroy(GameObject.Find("UIButtons"));
    }

}
