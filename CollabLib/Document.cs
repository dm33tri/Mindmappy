using CollabLib.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CollabLib
{
    public class Document
    {
        public int clientId;

        public Dictionary<string, AbstractStruct> share;

        public Transaction transaction;

        public Document()
        {
            this.share = new Dictionary<string, AbstractStruct>();
            this.store = new Store();
        }

        public Text AddTextField(string name)
        {
            if (!share.ContainsKey(name))
            {
                Text text = new Text();
                text.Integrate(this, null);
                share[name] = text;
                return text;
            }

            throw new Exception($"{name} is already defined");
        }

        public Text GetTextField(string name)
        {
            if (!share.ContainsKey(name))
            {
                throw new Exception($"{name} is not defined");
            }
            if (share[name] is Text)
            {
                return share[name] as Text;
            }

            throw new Exception($"{name} is not a Text instance");
        }

        public delegate void UpdateHandler(Action action);
        public event UpdateHandler Update;

        public Store store;
        public void Transact(TransactionFunc transactionFunc) {     
            if (this.transaction == null)
            {
                this.transaction = new Transaction(this);
            }
            try
            {
                transactionFunc(this.transaction);
            }
            finally
            {
                // cleanup
            }
        }
    }
}
