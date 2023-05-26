using System;
using System.Collections;
using UnityEngine;

namespace Assets.Window
{
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
}