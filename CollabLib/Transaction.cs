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
        public HashSet<ID> mergeItems;
        public bool local;

        public Transaction(Document doc)
        {
            this.doc = doc;
            deleted = new Dictionary<int, List<DeleteItem>>();
            beforeState = doc.store.StateVector;
            afterState = new Dictionary<int, int>();
            changed = new Dictionary<AbstractStruct, HashSet<string>>();
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


        public void MergeMergeSet()
        {
            Store store = doc.store;
            foreach (ID id in mergeItems)
            {
                List<Item> items;
                if (store.clientStates.TryGetValue(id.client, out items))
                {
                    int replacePos = store.FindIndex(id);
                    if (replacePos + 1 < items.Count)
                    {
                        if (items[replacePos].MergeWith(items[replacePos + 1]))
                        {
                            items.RemoveAt(replacePos + 1);
                        }
                    }
                    if (replacePos > 0)
                    {
                        if (items[replacePos - 1].MergeWith(items[replacePos]))
                        {
                            items.RemoveAt(replacePos);
                        }
                    }
                }
            }
        }
    }
}
