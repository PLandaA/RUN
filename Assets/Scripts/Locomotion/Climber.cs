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
        XRNode handNode = climbingHand.handedness == UnityEngine.XR.Interaction.Toolkit.Interactors.InteractorHandedness.Left
            ? XRNode.LeftHand
            : XRNode.RightHand;

        InputDevices.GetDeviceAtXRNode(handNode).TryGetFeatureValue(CommonUsages.deviceVelocity, out Vector3 velocity);

        character.Move(transform.rotation * -velocity * Time.fixedDeltaTime);
    }
}
