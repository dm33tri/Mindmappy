using CollabLib.Content;
using CollabLib.Struct;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace CollabLib
{
    class Encoder
    {
        public const byte Bits5 = 0x1f;
        public const byte Bit6 = 0x20;
        public const byte Bit7 = 0x40;
        public const byte Bit8 = 0x80;

        private List<byte> data;
        public byte[] Data { get => data.ToArray(); }

        public Encoder()
        {
            data = new List<byte>();
        }

        public int WriteID(ID id)
        {
            data.AddRange(BitConverter.GetBytes(id.client));
            data.AddRange(BitConverter.GetBytes(id.clock));

            return 8;
        }

        // Encode content
        public int Encode(AbstractContent content, int offset = 0)
        {
            var bytes = content.Encode(offset);
            data.AddRange(bytes);

            return bytes.Length;
        }

        // Encode single item
        public int Encode(Item item, int offset = 0)
        {
            int count = 0;
            ID origin = offset > 0 ? new ID(item.id.client, item.id.clock + offset - 1) : item.leftOrigin;

            byte info = (byte)((Bits5 & item.content.Ref) |
                (origin != null ? Bit8 : 0) |
                (item.rightOrigin != null ? Bit7 : 0) |
                (item.parentKey != null ? Bit6 : 0));

            data.Add(info);
            count += 1;

            if (origin != null)
            {
                count += WriteID(origin);
            }
            if (item.rightOrigin != null)
            {
                count += WriteID(item.rightOrigin);
            }
            if (origin == null && item.rightOrigin == null)
            {
                if (item.parent?.item != null)
                {
                    data.Add(0);
                    count += 1;
                    count += WriteID(item.parent.item.id);
                }
                else
                {
                    data.Add(1);
                    count += 1;
                    var str = Encoding.UTF8.GetBytes(item.parent.docName ?? "");
                    data.AddRange(BitConverter.GetBytes(str.Length));
                    count += 4;
                    data.AddRange(str);
                    count += str.Length;
                }
                if (item.parentKey != null)
                {
                    var str = Encoding.UTF8.GetBytes(item.parentKey);
                    data.AddRange(BitConverter.GetBytes(str.Length));
                    count += 4;
                    data.AddRange(str);
                    count += str.Length;
                }
            }

            count += Encode(item.content, offset);

            return count;
        }

        // Encode all client states diff
        public int Encode(Store store, Dictionary<int, int> fromState)
        {
            int count = 0;
            var toState = new Dictionary<int, int>();

            foreach (var pair in fromState)
            {
                int client = pair.Key;
                int clock = pair.Value;
                if (store.GetState(client) > clock)
                {
                    toState.Add(client, clock);
                }
            }

            foreach (var pair in store.StateVector)
            {
                int client = pair.Key;
                if (!toState.ContainsKey(client))
                {
                    toState.Add(client, 0);
                }
            }

            if (toState.Count == 0)
            {
                return 0;
            }

            data.AddRange(BitConverter.GetBytes(toState.Count));
            count += 4;

            foreach (var pair in toState)
            {
                int client = pair.Key;
                int clock = pair.Value;
                count += Encode(store.clientStates[client], client, clock);
            }

            return count;
        }

        // Encode multiple items from a client starting at clock
        public int Encode(List<Item> items, int client, int clock)
        {
            int count = 0;
            int start = Store.FindIndex(items, clock);

            data.AddRange(BitConverter.GetBytes(items.Count - start));
            count += 4;
            count += WriteID(new ID(client, clock));
            count += Encode(items[start], clock - items[start].id.clock);

            for (int i = start + 1; i < items.Count; ++i)
            {
                count += Encode(items[i], 0);
            }

            return count;
        }


        // Encode latest transaction
        public int Encode(Transaction transaction)
        {
            return Encode(transaction.doc.store, transaction.beforeState);
        }
    }
}
