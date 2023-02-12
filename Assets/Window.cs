using System;
using UnityEngine;
using DarkTonic.MasterAudio;
using System.Net;
using Unity.VisualScripting;
using System.Collections;

namespace Window
{
    [Serializable]

    public class Window : MonoBehaviour
    {
        public TextAsset jsonFile = null;
        private string jsonContent = "";
        public Conversation conversation;

        private IEnumerator statement_coroutine;

        private Transform transform;

        private bool conversation_ongoing = false;
        private bool statement_ongoing = false;
        private bool conversation_done = false;
        private int conversation_counter = 0;
        

        private void Awake()
        {
            transform = GetComponent<Transform>();
            jsonContent = jsonFile.ToString();
            conversation = new Conversation(JsonHelper.FromJson<Statement>(jsonContent));
        }

        private IEnumerator OnTriggerStay(Collider other)
        {
            if (conversation_ongoing && !conversation_done)
            {
                if (!statement_ongoing)
                {
                    yield return StartCoroutine(TriggerStatement());
                }
            }
            else
            {
                StartConversation();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            TerminateConversation();
        }

        // Conversation methods

        private void StartConversation()
        {
            conversation_ongoing = true;
        }

        private IEnumerator TriggerStatement()
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

        private void TerminateConversation()
        {
            conversation_ongoing = false;
            statement_ongoing = false;
            conversation_done = false;
            conversation_counter = 0;
        }
    }

    

    // Statement and Conversation classes

    [Serializable]
    public class Statement
    {
        public string text = "";
        public string participant = "";
        public string length = "";

        public Statement(string text, string participant, string length)
        {
            this.text = text;
            this.participant = participant;
            this.length = length;
        }

        override
        public string ToString()
        {
            string str = "";
            str += "Text: ";
            str += text + "\n";
            str += "Length: " + length + "\n";
            str += "Participant: " + participant;
            return str;
        }
    }

    [Serializable]
    public class Conversation
    {
        public Statement[] statements;

        public Conversation()
        {
            statements = new Statement[] { };
        }

        public Conversation(Statement[] statements)
        {
            this.statements = statements;
        }
    }


    // JSON Helper 
    //[Serializable]

    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
            return wrapper.Items;
        }

        public static string ToJson<T>(T[] array)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.Items = array;
            return JsonUtility.ToJson(wrapper);
        }

        public static string ToJson<T>(T[] array, bool prettyPrint)
        {
            Wrapper<T> wrapper = new Wrapper<T>();
            wrapper.Items = array;
            return JsonUtility.ToJson(wrapper, prettyPrint);
        }

        [Serializable]
        private class Wrapper<T>
        {
            public T[] Items;
        }
    }
}