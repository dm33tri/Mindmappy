using CollabLib.Struct;
using System;
using System.Collections.Generic;
using System.Text;

namespace CollabLib
{
    public class Event
    {
        public AbstractStruct target;
        public Transaction transaction;
        public AbstractStruct currentTarget;
         
        public Event(AbstractStruct target, Transaction transaction)
        {
            this.target = target;
            this.currentTarget = target;
            this.transaction = transaction;
        }
    }
}
