using CollabLib.Content;
using System;
using System.Collections.Generic;
using System.Text;

namespace CollabLib.Struct
{
    public class StructPath
    {
        int index;
        string key;
        bool isIndexed;
    }

    public abstract class AbstractStruct
    {
        public Document doc;
        public ID id;
        public Item item;

        public Item start;
        public Item First;

        public int length;

        public bool Deleted { get => this.item?.deleted ?? false; }

        public Dictionary<string, Item> map;

        public void Integrate(Document doc, Item item)
        {
            this.doc = doc;
            this.item = item;
        }
    }
}
