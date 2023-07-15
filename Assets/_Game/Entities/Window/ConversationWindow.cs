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

        private GameUI.WindowConversationUI conversationUI;

        private void Awake()
        {
            conversationUI = FindObjectOfType<GameUI.WindowConversationUI>();
            jsonContent = jsonFile.ToString();
            conversation = new Conversation(JsonHelper.FromJson<Statement>(jsonContent));
        }

        override
        public void UpdateInteraction()
        {
            if (!interaction_ongoing && !conversation_done)
            {
                StartInteraction();
            }
            if (interaction_ongoing && !statement_ongoing && !conversation_done)
            {
                StartCoroutine(ContinueInteraction());
            } 
        }

        override
        protected IEnumerator ContinueInteraction()
        {
            string statement_text = conversation.statements[conversation_counter].text;
            statement_ongoing = true;
            Debug.Log(statement_text);
            // Text output GUI
            conversationUI.SetText(statement_text, transform.position);
            string soundGroup = conversation.statements[conversation_counter].participant + "_" + conversation.statements[conversation_counter].length;
            yield return StartCoroutine(MasterAudio.PlaySound3DAtTransformAndWaitUntilFinished(soundGroup, transform)); ;
            if (conversation_counter < conversation.statements.Length)
            {
                conversation_counter++;
            }
            statement_ongoing = false;
            if (conversation_counter == conversation.statements.Length)
            {
                TerminateInteraction();
            }
        }

        override
        public void TerminateInteraction()
        {
            interaction_ongoing = false;
            conversation_done = false;
            conversation_counter = 0;
            conversationUI.DisableConversation();
        }
    }
}
