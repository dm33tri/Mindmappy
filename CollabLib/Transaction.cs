using CollabLib.Struct;
using System;
using System.Collections.Generic;
using System.Text;

namespace CollabLib
{
    public delegate void TransactionFunc(Transaction transaction);

    public struct DeleteItem
    {
        int clock;
        int len;

        public DeleteItem(int clock, int len)
        {
            this.clock = clock;
            this.len = len;
        }
    }

    public class Transaction
    {
        public Document doc;
        Dictionary<int, int> beforeState;
        Dictionary<int, int> afterState;
        public Dictionary<AbstractStruct, HashSet<string>> changed;
        public Dictionary<int, List<DeleteItem>> deleted;
        Dictionary<AbstractStruct, List<Event>> changedParentTypes;
        public HashSet<ID> mergeItems;
        public bool local;

        public Transaction(Document doc)
        {
            this.doc = doc;
            this.deleted = new Dictionary<int, List<DeleteItem>>();
            this.beforeState = this.doc.store.StateVector;
            this.afterState = new Dictionary<int, int>();
            this.changed = new Dictionary<AbstractStruct, HashSet<string>>();
            this.changedParentTypes = new Dictionary<AbstractStruct, List<Event>>();
            this.mergeItems = new HashSet<ID>();
        }

        public static void Transact(Document document, TransactionFunc action)
        {

        }

        public void SetDeleted(Item item)
        {

        }

        public void SetChanged(AbstractStruct changedStruct, string parentKey) 
        {
            Item item = changedStruct.item;
            if (item == null || (item.id.clock < this.beforeState[item.id.client] && !item.deleted))
            {
                if (!this.changed.ContainsKey(changedStruct))
                {
                    this.changed[changedStruct] = new HashSet<string>();
                }
                this.changed[changedStruct].Add(parentKey);
            }
        }
        
        public ID NextID()
        {
            return new ID(this.doc.clientId, this.doc.store.GetState(this.doc.clientId));
        }
    }


}
