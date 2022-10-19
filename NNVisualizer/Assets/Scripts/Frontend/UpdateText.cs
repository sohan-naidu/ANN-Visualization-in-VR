using UnityEngine;
using TMPro;
using Leap.Unity.Interaction;

public class UpdateText : MonoBehaviour {

    public TMP_Text text;
    [SerializeField]
    private InteractionSlider slider;
    private int currentValue;
    private bool toUpdate;
    void Start()
    {
        text = GetComponent<TMP_Text>();
        if (text == null) {
            Debug.LogError("TextMeshPro Text component not found");
        }
        if (slider == null)
            Debug.LogError("Slider not set");
        currentValue = 1;
        toUpdate = true;
    }

    public void updateValue()
    {
        Debug.Log(slider.horizontalStepValue);
        if (currentValue != slider.horizontalStepValue) {
            toUpdate = true;
            currentValue = slider.horizontalStepValue;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (toUpdate) {
            toUpdate = false;
            text.text = currentValue.ToString();
        }
    }
}
