using CollabLib.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using Array = CollabLib.Struct.Array;

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

        public Text AddText(string name)
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

        public Array AddArray(string name)
        {
            if (!share.ContainsKey(name))
            {
                Array array = new Array();
                array.docName = name;
                array.Integrate(this, null);
                share[name] = array;
                return array;
            }

            throw new Exception($"{name} is already defined");
        }

        public Map AddMap(string name)
        {
            if (!share.ContainsKey(name))
            {
                Map map = new Map();
                map.docName = name;
                map.Integrate(this, null);
                share[name] = map;
                return map;
            }

            throw new Exception($"{name} is already defined");
        }

        public Text GetText(string name)
        {
            if (!share.ContainsKey(name))
            {
                return AddText(name);
            }
            if (share[name] is Text)
            {
                return share[name] as Text;
            }

            throw new Exception($"{name} is not a Text instance");
        }

        public Array GetArray(string name)
        {
            if (!share.ContainsKey(name))
            {
                return AddArray(name);
            }
            if (share[name] is Array)
            {
                return share[name] as Array;
            }

            throw new Exception($"{name} is not an Array instance");
        }

        public Map GetMap(string name)
        {
            if (!share.ContainsKey(name))
            {
                return AddMap(name);
            }
            if (share[name] is Map)
            {
                return share[name] as Map;
            }

            throw new Exception($"{name} is not a Map instance");
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
                foreach (var pair in transaction.changed)
                {
                    pair.Key.TriggerUpdate(pair.Value.ToArray());
                }
            }
            finally
            {
                transaction.MergeMergeSet();

                Encoder encoder = new Encoder();
                encoder.Encode(transaction);
                var data = encoder.Data;

                if (transactionCleanups.Count <= i + 1)
                {
                    transactionCleanups.Clear();
                }
                else
                {
                    CleanupTransactions(i + 1);
                }
                if (data.Length > 0)
                {
                    Update?.Invoke(this, encoder.Data);
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

        public byte[] EncodeState()
        {
            var encoder = new Encoder();
            encoder.Encode(store, new Dictionary<int, int>() { { 0, 0 } });
            return encoder.Data;
        }
    }
}
