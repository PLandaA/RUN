using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ClimbInteractable : UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable
{
    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        if (args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor directInteractor)
        {
            Climber.climbingHand = directInteractor;
        }
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);

        if (args.interactorObject is UnityEngine.XR.Interaction.Toolkit.Interactors.XRDirectInteractor directInteractor)
        {
            if (Climber.climbingHand != null && Climber.climbingHand == directInteractor)
            {
                Climber.climbingHand = null;
            }
        }
    }
}