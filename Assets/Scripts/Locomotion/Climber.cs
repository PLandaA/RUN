using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class Climber : MonoBehaviour
{
    private CharacterController character;
    public static UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor climbingHand;
    private ContinuosMovement continuousMovement;

    // Start is called before the first frame update
    void Start()
    {
        character = GetComponent<CharacterController>();
        continuousMovement = GetComponent<ContinuosMovement>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // The controller can be disabled during scene transitions.
        if (character == null || !character.enabled || !character.gameObject.activeInHierarchy)
        {
            return;
        }

        if (climbingHand != null)
        {
            continuousMovement.enabled = false;
            Climb();
        }
        else
        {
            continuousMovement.enabled = true;
        }
    }

    //climbing Computations
void Climb()
    {
        // Determine which physical controller is gripping. handedness is often left at
        // 'None' on XRDirectInteractor (both hands were None here), which made the old
        // ternary always fall through to RightHand — so climbing with the left hand read
        // the right controller's velocity and did nothing. We detect the side robustly:
        // handedness first, then fall back to the interactor's GameObject name.
        XRNode handNode = XRNode.RightHand;

        var handed = climbingHand.handedness;
        if (handed == UnityEngine.XR.Interaction.Toolkit.Interactors.InteractorHandedness.Left)
        {
            handNode = XRNode.LeftHand;
        }
        else if (handed == UnityEngine.XR.Interaction.Toolkit.Interactors.InteractorHandedness.Right)
        {
            handNode = XRNode.RightHand;
        }
        else
        {
            // handedness unset: infer from the interactor's object name.
            string n = climbingHand.gameObject.name.ToLowerInvariant();
            handNode = n.Contains("left") ? XRNode.LeftHand : XRNode.RightHand;
        }

        InputDevices.GetDeviceAtXRNode(handNode).TryGetFeatureValue(CommonUsages.deviceVelocity, out Vector3 velocity);

        character.Move(transform.rotation * -velocity * Time.fixedDeltaTime);
    }
}
