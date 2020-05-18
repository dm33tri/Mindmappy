using System;
using System.Collections.Generic;
using System.Text;
using CollabLib.Struct;
using CollabLib.Content;

namespace CollabLib
{
    public class Item
    {
        public ID id;
        public Item left;
        public Item right;
        public ID leftOrigin;
        public ID rightOrigin;
        public AbstractStruct parent;
        public AbstractContent content;
        public string parentKey;
        public bool deleted;
        public bool countable;
        public int length;
        public ID redone;

        public Item(ID id, Item left, ID leftOrigin, Item right, ID rightOrigin, AbstractStruct parent, string parentKey, AbstractContent content)
        {
            this.id = id;
            this.left = left;
            this.leftOrigin = leftOrigin;
            this.right = right;
            this.rightOrigin = rightOrigin;
            this.parent = parent;
            this.content = content;
            this.parentKey = parentKey;
            this.deleted = false;
            this.countable = content.Countable;
            this.length = content.Length;
            this.redone = null;
        }

        public Item Split(int index, Transaction transaction)
        {
            Item rightItem = new Item(
                new ID(this.id.client, this.id.clock + index),
                this,
                new ID(this.id.client, this.id.clock + index - 1),
                this.right,
                this.rightOrigin,
                this.parent,
                this.parentKey,
                this.content.Splice(index)
            );

            rightItem.deleted = this.deleted;
            this.right = rightItem;
            if (rightItem.right != null)
            {
                rightItem.right.left = rightItem;
            }

            transaction.mergeItems.Add(rightItem.id);

            if (rightItem.parentKey != null && rightItem.right == null)
            {
                rightItem.parent.map[rightItem.parentKey] = rightItem;
            }

            this.length = index;

            return rightItem;
        }

        public void Integrate(Transaction transaction)
        {
            Store store = transaction.doc.store;
            Item conflictingItem;

            if (this.left != null)
            {
                conflictingItem = this.left.right;
            } 
            else if (this.parentKey != null)
            {
                conflictingItem = parent.map[this.parentKey];
                while (conflictingItem != null && conflictingItem.left != null)
                {
                    conflictingItem = conflictingItem.left;
                }
            }
            else
            {
                conflictingItem = parent.start;
            }

            var conflictingItems = new HashSet<Item>();
            var itemsBeforeOrigin = new HashSet<Item>();

            while (conflictingItem != null && conflictingItem != this.right)
            {
                itemsBeforeOrigin.Add(conflictingItem);
                conflictingItems.Add(conflictingItem);

                if (
                    (ID.AreSame(this.leftOrigin, conflictingItem.leftOrigin) && conflictingItem.id.client < this.id.client) ||
                    (conflictingItem.leftOrigin != null && itemsBeforeOrigin.Contains(store.FindItem(conflictingItem.leftOrigin)))
                )
                {
                    this.left = conflictingItem;
                    conflictingItems.Clear();
                }
                else
                {
                    break;
                }

                conflictingItem = conflictingItem.right;
            }

            if (this.left != null)
            {
                this.right = this.left.right;
                this.left.right = this;
            }
            else
            {
                Item right = null;

                if (this.parentKey != null)
                {
                    right = parent.map[parentKey];
                    while (right != null && right.left != null)
                    {
                        right = right.left;
                    }
                }
                else
                {
                    right = parent.start;
                    parent.start = this;
                }

                this.right = right;
            }

            if (this.right != null)
            {
                this.right.left = this;
            }
            else if (this.parentKey != null)
            {
                parent.map[this.parentKey] = this;

                if (this.left != null)
                {
                    this.left.Delete(transaction);
                }
            }

            if (parentKey == null && this.countable && !this.deleted)
            {
                parent.length += this.length;
            }

            store.AddItem(this);

            this.content.Integrate(transaction, this);

            transaction.SetChanged(this.parent, this.parentKey);

            if (parent.Deleted || (this.right != null && this.parentKey != null))
            {
                this.Delete(transaction);
            }
        }

        public void Delete(Transaction transaction)
        {
            if (!this.deleted)
            {
                if (this.countable && this.parentKey == null)
                {
                    this.parent.length -= this.length;
                }
                this.deleted = true;

                transaction.SetDeleted(this);
                transaction.SetChanged(this.parent, this.parentKey);
            }
        }
    }
}
