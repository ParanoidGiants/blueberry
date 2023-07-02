using Assets.Window;
using RootMath;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnEnterWindowZone : MonoBehaviour
{
    public ConversationWindow window;

    private void OnTriggerEnter(Collider other)
    {

        if (!RMath.IsLayerPlayer(other.gameObject.layer) || window.IsInteractionOnGoing) return;

        window.ContinueInteraction(other);
    }
}
