using System;
using System.Collections.Generic;
using System.Text;

namespace CollabLib
{
    public class Store
    {
        public Dictionary<int, List<Item>> clientStates = new Dictionary<int, List<Item>>();

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

        public int FindIndex(ID id)
        {
            var items = this.clientStates[id.client];
            int left = 0, right = items.Count - 1;

            while (left <= right) 
            {
                int midIndex = left + (right - left) / 2;
                int midClock = items[midIndex].id.clock;
                if (midClock <= id.clock)
                {
                    if (id.clock < midClock + items[midIndex].length)
                    {
                        return midIndex;
                    }

                    left = midIndex + 1;
                }
                else
                {
                    right = midIndex + 1;
                }
            }

            throw new Exception($"Item {id.clock} not found in client {id.client}");
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

        public void GetItemCleanEnd()
        {

        }
    }
}
