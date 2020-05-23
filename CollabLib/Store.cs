using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CollabLib
{
    public struct PendingRefs
    {
        public int i;
        public List<ItemRef> refs;
    }

    public class Store
    {
        public Dictionary<int, List<Item>> clientStates = new Dictionary<int, List<Item>>();
        public Dictionary<int, PendingRefs> pendingClientRefs = new Dictionary<int, PendingRefs>();
        public List<ItemRef> pendingStack = new List<ItemRef>();

        public Dictionary<int, int> StateVector { 
            get {
                var vector = new Dictionary<int, int>();
                foreach (var state in clientStates)
                {
                    var items = state.Value;
                    if (items == null || items.Count == 0)
                    {
                        vector[state.Key] = 0;
                    }
                    else
                    {
                        var lastItem = items[items.Count - 1];
                        vector[state.Key] = lastItem.id.clock + lastItem.length;
                    }
                }
                return vector;
            } 
        }

        public int GetState(int client)
        {
            if (!clientStates.ContainsKey(client))
            {
                return 0;
            }

            var state = clientStates[client];
            var lastItem = state[state.Count - 1];
            return lastItem.id.clock + lastItem.length;
        }

        public static int FindIndex(List<Item> items, int clock)
        {
            int left = 0, right = items.Count - 1;

            while (left <= right)
            {
                int midIndex = left + (right - left) / 2;
                int midClock = items[midIndex].id.clock;
                if (midClock <= clock)
                {
                    if (clock < midClock + items[midIndex].length)
                    {
                        return midIndex;
                    }

                    left = midIndex + 1;
                }
                else
                {
                    right = midIndex;
                }
            }

            throw new Exception($"Item {clock} not found");
        }

        public int FindIndex(ID id)
        {
            var items = clientStates[id.client];

            return FindIndex(items, id.clock);
        } 

        public Item FindItem(ID id)
        {
            return clientStates[id.client][FindIndex(id)];
        }

        public void AddItem(Item item)
        {
            if (!clientStates.ContainsKey(item.id.client))
            {
                clientStates[item.id.client] = new List<Item>();
            } 

            clientStates[item.id.client].Add(item);
        }

        public Item GetItemCleanStart(Transaction transaction, ID id)
        {
            var items = this.clientStates[id.client];
            int index = FindIndex(id);
            Item item = items[index];

            if (item.id.clock < id.clock)
            {
                items.Insert(index + 1, item.Split(id.clock - item.id.clock, transaction));
            }

            return item;
        }

        public void IntegratePending(Transaction transaction)
        {
            while (pendingStack.Count != 0 || pendingClientRefs.Count != 0)
            {
                if (pendingStack.Count == 0)
                {
                    var pair = pendingClientRefs.First();
                    var client = pair.Key;
                    var itemRefs = pair.Value;

                    pendingStack.Add(itemRefs.refs[itemRefs.i++]);

                    if (itemRefs.refs.Count == itemRefs.i)
                    {
                        pendingClientRefs.Remove(client);
                    }
                    else
                    {
                        pendingClientRefs[client] = itemRefs;
                    }
                }
                ItemRef itemRef = pendingStack.Last();
                int localClock = GetState(itemRef.id.client);
                int offset = itemRef.id.clock < localClock ? localClock - itemRef.id.clock : 0;
                if (itemRef.id.clock + offset != localClock)
                {
                    PendingRefs pendingRefs;
                    if (pendingClientRefs.TryGetValue(itemRef.id.client, out pendingRefs))
                    {
                        ItemRef nextRef = pendingRefs.refs[pendingRefs.i];
                        if (nextRef.id.clock < itemRef.id.clock)
                        {
                            pendingRefs.refs[pendingRefs.i] = itemRef;
                            pendingStack[pendingStack.Count - 1] = nextRef;
                            var newRefs = pendingRefs.refs.GetRange(pendingRefs.i, pendingRefs.refs.Count - pendingRefs.i);
                            newRefs.Sort((r1, r2) => r1.id.clock - r2.id.clock);
                            pendingRefs.refs = newRefs;
                            pendingRefs.i = 0;
                            continue;
                        }

                    }

                    return;
                }
                while (itemRef.missing.Count > 0)
                {
                    ID missing = itemRef.missing.Last();
                    if (GetState(missing.client) <= missing.clock)
                    {
                        PendingRefs pendingRefs;
                        if (pendingClientRefs.TryGetValue(missing.client, out pendingRefs))
                        {
                            pendingStack.Add(pendingRefs.refs[pendingRefs.i++]);
                            if (pendingRefs.i == pendingRefs.refs.Count)
                            {
                                pendingClientRefs.Remove(missing.client);
                            }
                            break;
                        }
                        else
                        {
                            return;
                        }
                    }
                    itemRef.missing.RemoveAt(itemRef.missing.Count - 1);
                }
                if (itemRef.missing.Count == 0)
                {
                    if (offset < itemRef.length)
                    {
                        itemRef.ToItem(transaction, this, offset).Integrate(transaction);
                    }
                    pendingStack.RemoveAt(pendingStack.Count - 1);
                } 
            }

        }

        public void AddToPending(Dictionary<int, List<ItemRef>> clientItemRefs)
        {
            foreach (var pair in clientItemRefs)
            {
                int client = pair.Key;
                var itemRefs = pair.Value;
                PendingRefs pendingRefs;
                if (!pendingClientRefs.TryGetValue(client, out pendingRefs))
                {
                    pendingClientRefs.Add(client, new PendingRefs { i = 0, refs = itemRefs });
                }
                else
                {
                    int count = pendingRefs.refs.Count - pendingRefs.i;
                    List<ItemRef> merged = pendingRefs.i > 0 ? pendingRefs.refs.GetRange(pendingRefs.i, count) : pendingRefs.refs;
                    merged.AddRange(itemRefs);
                    pendingRefs.i = 0;
                    merged.Sort((a, b) => a.id.clock - b.id.clock);
                    pendingRefs.refs = merged;
                    pendingClientRefs[client] = pendingRefs;
                }
            }
        }
    }
}
