using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Buckland
{
    public class State<entity_type>
    {

        //this will execute when the state is entered
        public virtual void Enter(entity_type type) { }

        //this is the states normal update function
        public virtual void Execute(entity_type type) { }

        //this will execute when the state is exited. 
        public virtual void Exit(entity_type type) { }

        //this executes if the agent receives a message from the 
        //message dispatcher
        public virtual bool OnMessage(entity_type type, Telegram msg) { return false; }
    }
}

