using System;
using UnityEngine;
using DarkTonic.MasterAudio;
using System.Collections;
using Unity.VisualScripting;

namespace Assets.Window
{
    [Serializable]
    public class ConversationWindow : Window
    {
        public Conversation conversation;

        private bool statement_ongoing = false;
        private bool conversation_done = false;
        private int conversation_counter = 0;
        

        private void Awake()
        {
            jsonContent = jsonFile.ToString();
            conversation = new Conversation(JsonHelper.FromJson<Statement>(jsonContent));
        }

        private IEnumerator OnTriggerStay(Collider other)
        {
            if (interaction_ongoing && !conversation_done)
            {
                if (!statement_ongoing)
                {
                    yield return StartCoroutine(ContinueInteraction());
                }
            }
            else
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
            Debug.Log(statement_text);
            string soundGroup = conversation.statements[conversation_counter].participant + "_" + conversation.statements[conversation_counter].length;
            yield return StartCoroutine(MasterAudio.PlaySound3DAtTransformAndWaitUntilFinished(soundGroup, transform));
            if (conversation_counter + 1 == conversation.statements.Length)
            {
                conversation_done = true;
            }
            if (conversation_counter +1 < conversation.statements.Length)
            {
                conversation_counter++;
            }
            
            statement_ongoing = false;
        }

        override
        protected void TerminateInteraction()
        {
            interaction_ongoing = false;
            statement_ongoing = false;
            conversation_done = false;
            conversation_counter = 0;
        }
    }
}