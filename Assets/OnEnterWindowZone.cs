using Assets.Window;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindowEffectZone : MonoBehaviour
{
    public ConversationWindow window;

    private void OnTriggerStay(Collider other)
    {
        window.ContinueInteraction(other);
    }
}
