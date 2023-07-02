using System;
using System.Collections;
using UnityEngine;

namespace Assets.Window
{
    [Serializable]
    public class Window : MonoBehaviour
    {
        public TextAsset jsonFile;
        protected string jsonContent;

        protected bool interaction_ongoing = false;
        public bool IsInteractionOnGoing => interaction_ongoing;

        protected void StartInteraction() {
            interaction_ongoing = true;
        }
        virtual protected IEnumerator ContinueInteraction() { yield break; }
        virtual public void TerminateInteraction() { }           
    }
}