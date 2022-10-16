using Leap;
using Leap.Unity;
using Leap.Unity.Interaction;
using UnityEngine;

public class LMCInteraction : InteractionHand {

    private Hand hand;
    public Transform headTransform;

    [Range(0f, 15f)]
    public float projectionScale = 10f;
    [Range(0f, 1f)]
    public float handMergeDistance = 0.35f;

    protected override void Start()
    {
        // leapProvider.OnFixedFrame -= customFixedFrameUpdate;
        leapProvider.OnFixedFrame += customFixedFrameUpdate;
        base.Start();
    }

    // Update is called once per frame
    public void customFixedFrameUpdate(Leap.Frame frame)
    {
        hand = handAccessorFunc(frame);

        if (hand != null) {
            // Debug.Log("Hand ID: " + hand.Id);
            // Debug.Log("Hand ID from Accessor: " + handAccessorFunc(frame).Id);
            // Debug.Log(( _hand.IsLeft ? "Left" : "Right" ) + "Hand deteced");
            if (headTransform == null) { headTransform = Camera.main.transform; }
            Vector3 headPos = headTransform.position;
            var shoulderBasis = Quaternion.LookRotation(
              Vector3.ProjectOnPlane(headTransform.forward, Vector3.up),
              Vector3.up);

            // Approximate shoulder position with magic values.
            Vector3 shoulderPos = headPos
                                  + ( shoulderBasis * ( new Vector3(0f, -0.13f, -0.1f)
                                  + Vector3.left * 0.15f * ( hand.IsLeft ? 1f : -1f ) ) );

            // Calculate the projection of the hand if it extends beyond the
            // handMergeDistance.
            Vector3 shoulderToHand = hand.PalmPosition - shoulderPos;

            // RayCast from each finger

            float handShoulderDist = shoulderToHand.magnitude;
            float projectionDistance = Mathf.Max(0f, handShoulderDist - handMergeDistance);
            float projectionAmount = 1f + ( projectionDistance * projectionScale );
            hand.SetTransform(shoulderPos + shoulderToHand * projectionAmount,
                              hand.Rotation);
            // Debug.Log("Interaction Hand : " + _hand);

        }
    }

}
