using Leap;
using Leap.Unity;
using UnityEngine;

public class ProjectionBehaviour : PostProcessProvider {

    public Transform headTransform;

    [Range(0f, 15f)]
    public float projectionScale = 10f;
    [Range(0f, 1f)]
    public float handMergeDistance = 0.35f;
    public override void ProcessFrame(ref Frame inputFrame)
    {
        foreach (var hand in inputFrame.Hands) {
            // Debug.Log("Visual hand: " + hand.Id);
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

            float handShoulderDist = shoulderToHand.magnitude;
            float projectionDistance = Mathf.Max(0f, handShoulderDist - handMergeDistance);
            float projectionAmount = 1f + ( projectionDistance * projectionScale );
            hand.SetTransform(shoulderPos + shoulderToHand * projectionAmount,
                              hand.Rotation);
            //Debug.Log(shoulderPos + shoulderToHand * projectionAmount);
        }
    }
}