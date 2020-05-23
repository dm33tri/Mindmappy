using CollabLib.Content;
using System;
using System.Collections.Generic;
using System.Text;

namespace CollabLib
{
    public class Decoder
    {
        public byte[] data;
        public int index;

        public Decoder(byte[] data)
        {
            this.data = data;
            index = 0;
        }

        public ID ReadID()
        {
            int client = ReadInt();
            int clock = ReadInt();
            return new ID(client, clock);
        }

        public int ReadInt()
        {
            int value = BitConverter.ToInt32(data, index);
            index += 4;
            return value;
        }

        public byte ReadByte()
        {
            byte value = data[index];
            index += 1;
            return value;
        }

        public string ReadString()
        {
            int length = ReadInt();
            string value = Encoding.UTF8.GetString(data, index, length);
            index += length;
            return value;
        }

        public ContentString ReadContentString()
        {
            return new ContentString(ReadString());
        }

        public AbstractContent ReadContent(byte info)
        {
            switch (info & Encoder.Bits5)
            {
                case ContentString.ContentRef:
                    return ReadContentString();
                default:
                    return null;
            }
        }

        public List<ItemRef> ReadItemRefs(int count, ID nextID)
        {
            var result = new List<ItemRef>();
            for (int i = 0; i < count; ++i)
            {
                byte info = ReadByte();
                ItemRef itemRef = (Encoder.Bits5 & info) != 0 ? new ItemRef(this, nextID, info) : null;
                nextID = new ID(nextID.client, nextID.clock + itemRef.length);
                result.Add(itemRef);
            }
            return result;
        }

        public Dictionary<int, List<ItemRef>> ReadClientItemRefs()
        {
            var result = new Dictionary<int, List<ItemRef>>();
            int numOfUpdates = ReadInt();

            for (int i = 0; i < numOfUpdates; ++i)
            {
                int numberOfItems = ReadInt();
                ID nextId = ReadID();
                var clientRefs = ReadItemRefs(numberOfItems, nextId);
                result.Add(nextId.client, clientRefs);
            }

            return result;
        }

        public void ReadItems(Transaction transaction, Store store)
        {
            var clientItemsRefs = ReadClientItemRefs();
            store.AddToPending(clientItemsRefs);
            store.IntegratePending(transaction);
        }
    }
}
