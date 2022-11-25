using UnityEngine;
using Leap;
using Leap.Unity;
using UnityEngine.XR;

public class Movement : MonoBehaviour {
    // Start is called before the first frame update

    public Camera head;
    public GameObject headParent;
    void Start()
    {
        Debug.DrawLine(Vector3.zero, new Vector3(5, 0, 0), Color.black, 2.5f);
    }

    // Update is called once per frame
    void Update()
    {
        var leftHand = Hands.Left;
        var rightHand = Hands.Right;

        //if (leftHand != null && leftHand.IsPinching()) {
        //Debug.Log("Left hand is pinching at position " + leftHand.GetStablePinchPosition());
        //Debug.Log("Direction to move " + ( leftHand.GetStablePinchPosition() - head.transform.position ).ToString());
        //Debug.DrawLine(head.transform.position, leftHand.GetStablePinchPosition(), Color.black);
        //}

        //handle movement
        if (leftHand != null && rightHand != null) {
            if (leftHand.IsPinching()) {
                var leftVector = leftHand.GetStablePinchPosition() - head.transform.position;
                var rightVector = rightHand.PalmPosition - head.transform.position;
                var projectionVector = Vector3.RotateTowards(rightVector, leftVector, Vector3.Angle(leftVector, rightVector) / 2f, 0f).normalized;
                var leftProject = Vector3.Project(leftVector, projectionVector);
                var rightProject = Vector3.Project(rightVector, projectionVector);

                // Debug.DrawLine(head.transform.position, leftVector);

                var moveDirection = rightProject - leftProject;
                float scale = 0.3f;
                float moveMagnitude = Mathf.Abs(moveDirection.magnitude);

                //Debug.Log("Left Vector: " + leftVector + "\nRight Vector: " + rightVector + "\nMove direction: " + moveDirection.ToString() + "\nHead position" + headParent.transform.position.ToString());
                // Debug.Log("Move direction: " + moveDirection.ToString() + "\nHead position" + head.transform.position.ToString());

                //guard for hands being very close
                //if (moveMagnitude >= 0.05) {
                headParent.transform.position = headParent.transform.position + moveDirection * scale;
                //head.transform.position = head.transform.position + moveDirection * scale;
                //}

                //You move away from the pinched position if your rightHand moves towards you
                //If right hand moves away from you i.e. in forward direction, you move towards the pinched position

                // Debug.Log("Left hand is pinching at position " + leftHand.GetStablePinchPosition());
            }

            //If both are being pinched, check if rightHand pinch is more towards you than the leftHand pinch
            //If yes, move backwards at speed relative to distance between the two
            //Otherwise, move forwards

            //Another way to move would probably be to specifically pinch at a particular position and open hand
            //to move towards the pinched point 
            //Get eye to pinch vector and move along that 
            //How to move backwards?
        }

        //handle rotation
        if (leftHand != null && rightHand != null) {
            //possible movement input
            if (leftHand.IsPinching() || rightHand.IsPinching()) {
                return;
            }

            //check if you make contact with NNCube with both hands
            //if yes, move in direction that your hands move in
            //requires NNCube. 
        }
    }
}
