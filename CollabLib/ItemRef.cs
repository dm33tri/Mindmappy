using CollabLib.Content;
using CollabLib.Struct;
using System;
using System.Collections.Generic;
using System.Text;

namespace CollabLib
{
    public class ItemRef
    {
        public Decoder decoder;
        public ID right;
        public ID left;
        public string parentDocKey;
        public string parentKey;
        public ID parent;
        public List<ID> missing;
        public ID id;
        public AbstractContent content;
        public int length;

        public ItemRef(Decoder decoder, ID id, byte info)
        {
            this.decoder = decoder;
            this.id = id;
            missing = new List<ID>();
            left = (info & Encoder.Bit8) != 0 ? decoder.ReadID() : null;
            right = (info & Encoder.Bit7) != 0 ? decoder.ReadID() : null;
            bool canCopyParentInfo = (info & (Encoder.Bit8 | Encoder.Bit7)) == 0;
            bool hasParentDocKey = canCopyParentInfo && decoder.ReadByte() == 1;
            parentDocKey = hasParentDocKey ? decoder.ReadString() : null;
            parent = canCopyParentInfo && !hasParentDocKey ? decoder.ReadID() : null;
            parentKey = canCopyParentInfo && (info & Encoder.Bit6) != 0 ? decoder.ReadString() : null;
            if (left != null)
            {
                missing.Add(left);
            }
            if (right != null)
            {
                missing.Add(right);
            }
            if (parent != null)
            {
                missing.Add(parent);
            }

            content = decoder.ReadContent(info);
            length = content.Length;
        }

        public Item ToItem(Transaction transaction, Store store, int offset = 0)
        {
            if (offset > 0)
            {
                id.clock += offset;
                this.left = new ID(id.client, id.clock - 1);
                content = content.Splice(offset);
                length -= offset;
            }

            Item left = this.left != null ? store.GetItemCleanEnd(transaction, this.left) : null;
            Item right = this.right != null ? store.GetItemCleanStart(transaction, this.right) : null;
            AbstractStruct parent = null;
            string parentKey = this.parentKey;
            if (this.parent != null)
            {
                Item parentItem = store.FindItem(this.parent);
                if (!parentItem.deleted && left == null && right == null)
                {
                    parent = parentItem.content as AbstractStruct;
                }
            }
            else if (parentDocKey != null)
            {
                transaction.doc.share.TryGetValue(parentDocKey, out parent);
            }
            else if (left != null)
            {
                parent = left.parent;
                parentKey = left.parentKey;
            }
            else if (right != null)
            {
                parent = right.parent;
                parentKey = right.parentKey;
            }
            else
            {
                throw new Exception("could not convert to item");
            }

            return parent != null ? new Item(id, left, this.left, right, this.right, parent, parentKey, content) : null;
        }
    }
}
