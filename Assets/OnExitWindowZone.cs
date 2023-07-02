using Assets.Window;
using RootMath;
using UnityEngine;

public class OnExitWindowZone : MonoBehaviour
{
    public Window window;

    private void OnTriggerStay(Collider other)
    {
        if (!RMath.IsLayerPlayer(other.gameObject.layer) || !window.IsInteractionOnGoing) return;

        window.UpdateInteraction();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!RMath.IsLayerPlayer(other.gameObject.layer)) return;

        window.TerminateInteraction();
    }
}
