using CollabLib.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CollabLib.Struct;
using Array = CollabLib.Struct.Array;

namespace CollabLib
{
    public class Decoder
    {
        public byte[] data;
        public int index;
        public Document doc;

        public Decoder(byte[] data, Document doc)
        {
            this.data = data;
            this.doc = doc;
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

        public ContentBinary ReadContentBinary()
        {
            int length = ReadInt();
            byte[] value = data.Skip(index).Take(length).ToArray();
            index += length;
            return new ContentBinary(value);
        }

        public ContentString ReadContentString()
        {
            return new ContentString(ReadString());
        }

        public AbstractStruct ReadStruct()
        {
            var type = ReadByte();
            switch (type)
            {
                case Text.ContentTypeRef:
                    return new Text();
                case Array.ContentTypeRef:
                    return new Array();
                case Map.ContentTypeRef:
                    return new Map();
                default:
                    return null;
            }
        }

        public AbstractContent ReadContent(byte info)
        {
            switch (info & Encoder.Bits5)
            {
                case ContentString.ContentRef:
                    return ReadContentString();
                case ContentBinary.ContentRef:
                    return ReadContentBinary();
                case AbstractStruct.ContentRef:
                    return ReadStruct();
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
