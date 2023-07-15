using Assets.Window;
using UnityEngine;

public class OnEnterWindowZone : MonoBehaviour
{
    public Window window;

    private void OnTriggerEnter(Collider other)
    {

        if (!Utils.Helper.IsLayerPlayer(other.gameObject.layer) || window.IsInteractionOnGoing) return;

        window.UpdateInteraction();
    }
}
