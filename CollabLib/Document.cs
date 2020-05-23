using CollabLib.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CollabLib
{
    public delegate void UpdateHandler(Document sender, byte[] binaryChanges);

    public class Document
    {
        public int clientId;

        public Dictionary<string, AbstractStruct> share;

        public Transaction transaction;
        public List<Transaction> transactionCleanups;
        public Document()
        {
            share = new Dictionary<string, AbstractStruct>();
            store = new Store();
            transactionCleanups = new List<Transaction>();
        }

        public Text AddTextField(string name)
        {
            if (!share.ContainsKey(name))
            {
                Text text = new Text();
                text.docName = name;
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

        public event UpdateHandler Update;

        public Store store;

        public void CleanupTransactions(int i = 0)
        {
            Transaction transaction = transactionCleanups[i];
            try
            {
                this.transaction = null;
                transaction.afterState = store.StateVector;
            }
            finally
            {
                foreach (var pair in transaction.afterState) {
                    int client = pair.Key;
                    int clock = pair.Value;
                    int beforeClock;
                    transaction.beforeState.TryGetValue(client, out beforeClock);

                    //if (beforeClock != clock)
                    //{
                    //    List<Item> items;
                    //    store.clientStates.TryGetValue(client, out items);
                    //    int replacedItemPos = Store.FindIndex(items, clock);
                    //    if (replacedItemPos + 1 < items.Count)
                    //    {
                    //        // todo try merge left replacedItemPos + 1
                    //    }
                    //    if (replacedItemPos > 0)
                    //    {
                    //        // todo try merge left replacedItemPos
                    //    }
                    //} 
                }
                Encoder encoder = new Encoder();
                encoder.Encode(transaction);
                if (Update != null)
                {
                    Update(this, encoder.Data);
                }
                if (transactionCleanups.Count <= i + 1)
                {
                    transactionCleanups.Clear();
                }
                else
                {
                    CleanupTransactions(i + 1);
                }
            }
        }

        public void Transact(TransactionFunc transactionFunc) {
            bool initialCall = false;

            if (transaction == null)
            {
                initialCall = true;
                transaction = new Transaction(this);
                transactionCleanups.Add(transaction);
            }
            try
            {
                transactionFunc(transaction);
            }
            finally
            {
                if (initialCall && transactionCleanups[0] == transaction)
                {
                    CleanupTransactions();
                }
            }
        }

        public void ApplyUpdate(byte[] data)
        {
            Decoder decoder = new Decoder(data);
            Transact((transaction) =>
            {
                decoder.ReadItems(transaction, store);
            });
        }
    }
}
