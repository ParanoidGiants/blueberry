using System;
using System.Collections;
using UnityEngine;

namespace Assets.Window
{
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
}