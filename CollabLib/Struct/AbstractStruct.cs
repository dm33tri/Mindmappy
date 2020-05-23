using CollabLib.Content;
using System;
using System.Collections.Generic;
using System.Text;

namespace CollabLib.Struct
{
    public abstract class AbstractStruct : AbstractContent
    {
        public Document doc;
        public string docName;
        public ID id;
        public Item item;

        public Item start;
        public Item First;

        public int length;

        public abstract int TypeRef { get; }

        public bool Deleted { get => item?.deleted ?? false; }

        public Dictionary<string, Item> map;

        public void Integrate(Document doc, Item item)
        {
            this.doc = doc;
            this.item = item;
        }

        public override bool Countable { get; } = true;
        public override int Length { get; } = 1;
        public override int Ref { get; } = 7;
        public override void Integrate(Transaction transaction, Item item)
        {
            Integrate(transaction.doc, item);
        }
        public override AbstractContent Splice(int index)
        {
            throw new NotImplementedException();
        }
        public override byte[] Encode(int offset)
        {
            return new[] { (byte)TypeRef };
        }
    }
}
