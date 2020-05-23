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
        public Dictionary<int, int> beforeState;
        public Dictionary<int, int> afterState;
        public Dictionary<AbstractStruct, HashSet<string>> changed;
        public Dictionary<int, List<DeleteItem>> deleted;
        public Dictionary<AbstractStruct, List<Event>> changedParentTypes;
        public HashSet<ID> mergeItems;
        public bool local;

        public Transaction(Document doc)
        {
            this.doc = doc;
            deleted = new Dictionary<int, List<DeleteItem>>();
            beforeState = doc.store.StateVector;
            afterState = new Dictionary<int, int>();
            changed = new Dictionary<AbstractStruct, HashSet<string>>();
            changedParentTypes = new Dictionary<AbstractStruct, List<Event>>();
            mergeItems = new HashSet<ID>();
        }

        public void SetDeleted(Item item)
        {

        }

        public void SetChanged(AbstractStruct changedStruct, string parentKey) 
        {
            Item item = changedStruct.item;
            if (item == null || (item.id.clock < beforeState[item.id.client] && !item.deleted))
            {
                if (!changed.ContainsKey(changedStruct))
                {
                    changed[changedStruct] = new HashSet<string>();
                }
                changed[changedStruct].Add(parentKey);
            }
        }
        
        public ID NextID()
        {
            return new ID(doc.clientId, doc.store.GetState(doc.clientId));
        }
    }


}
