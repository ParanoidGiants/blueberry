using System;
using UnityEngine;
using DarkTonic.MasterAudio;
using System.Collections;

namespace Assets.Window
{
    [Serializable]
    public class ConversationWindow : Window
    {
        public Conversation conversation;

        private bool statement_ongoing = false;
        private bool conversation_done = false;
        private int conversation_counter = 0;

        private GameObject centerGO;

        private void Awake()
        {
            jsonContent = jsonFile.ToString();
            conversation = new Conversation(JsonHelper.FromJson<Statement>(jsonContent));
            centerGO = new GameObject();
            BoxCollider[] boxes = GetComponentsInChildren<BoxCollider>();
            foreach (BoxCollider box in boxes)
            {
                if (box.tag == "WindowCollider")
                {
                    centerGO.transform.position = box.center;
                }
            }
        }

        private IEnumerator OnTriggerStay(Collider other)
        {
            if (interaction_ongoing && !statement_ongoing && !conversation_done)
            {
                yield return StartCoroutine(ContinueInteraction());                
            }
            else if (!interaction_ongoing && !conversation_done)
            {
                StartInteraction();
                yield break;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            TerminateInteraction();
        }

        override
        protected IEnumerator ContinueInteraction()
        {
            statement_ongoing = true;
            string statement_text = conversation.statements[conversation_counter].text;
            // Text output GUI
            Debug.Log(statement_text);
            string soundGroup = conversation.statements[conversation_counter].participant + "_" + conversation.statements[conversation_counter].length;
            yield return StartCoroutine(MasterAudio.PlaySound3DAtTransformAndWaitUntilFinished(soundGroup, centerGO.transform)); ;
            if (conversation_counter < conversation.statements.Length)
            {
                conversation_counter++;
            }
            statement_ongoing = false;
            if (conversation_counter == conversation.statements.Length)
            {
                conversation_done = true;
            }
        }

        override
        protected void TerminateInteraction()
        {
            interaction_ongoing = false;
            conversation_done = false;
            conversation_counter = 0;
        }
    }
}