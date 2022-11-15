using Leap.Unity;
using Leap.Unity.Interaction;
using UnityEngine;

/// <summary>
/// This simple script changes the color of an InteractionBehaviour as
/// a function of its distance to the palm of the closest hand that is
/// hovering nearby.
/// </summary>
[AddComponentMenu("")]
[RequireComponent(typeof(InteractionBehaviour))]
public class LayerInteraction : MonoBehaviour {

    private InteractionBehaviour _intObj;
    private Outline outline;
    public int layerNum;

    void Start()
    {
        _intObj = GetComponent<InteractionBehaviour>();
        outline = GetComponent<Outline>();
    }

    void Update()
    {
        // We can also check the depressed-or-not-depressed state of InteractionButton objects
        // and assign them a unique color in that case.
        Debug.Log(_intObj.name);
        if (_intObj is InteractionButton && ( _intObj as InteractionButton ).isPressed) {
            outline.OutlineColor = Color.red;
        }
        else {
            outline.OutlineColor = Color.black;
        }
    }

    public void onUnpress()
    {
        UIHandler _UIobj = GameObject.Find("UI").GetComponent<UIHandler>();
        _UIobj.layer = layerNum;

        GameObject _LayerBoxesobj = GameObject.Find("LayerBoxes");
        if (_LayerBoxesobj != null)
            Destroy(_LayerBoxesobj);
        // Destroy(GameObject.Find("Layer Parent"));
        _UIobj.callUpdate();

    }

}
