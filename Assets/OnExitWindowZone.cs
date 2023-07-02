using Assets.Window;
using RootMath;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnExitWindowZone : MonoBehaviour
{
    public ConversationWindow window;


    private void OnTriggerStay(Collider other)
    {
        if (!RMath.IsLayerPlayer(other.gameObject.layer) || !window.IsInteractionOnGoing) return;

        window.ContinueInteraction(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!RMath.IsLayerPlayer(other.gameObject.layer)) return;

        window.TerminateInteraction();
    }
}
