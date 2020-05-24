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
                new ID(id.client, id.clock + index),
                this,
                new ID(id.client, id.clock + index - 1),
                right,
                rightOrigin,
                parent,
                parentKey,
                content.Splice(index)
            );

            rightItem.deleted = deleted;

            right = rightItem;
            if (rightItem.right != null)
            {
                rightItem.right.left = rightItem;
            }

            transaction.mergeItems.Add(rightItem.id);

            if (rightItem.parentKey != null && rightItem.right == null)
            {
                rightItem.parent.map[rightItem.parentKey] = rightItem;
            }

            length = index;

            return rightItem;
        }

        public void Integrate(Transaction transaction)
        {
            Store store = transaction.doc.store;
            Item conflictingItem;

            if (left != null)
            {
                conflictingItem = left.right;
            } 
            else if (parentKey != null)
            {
                conflictingItem = parent.map[parentKey];
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

            while (conflictingItem != null && conflictingItem != right)
            {
                itemsBeforeOrigin.Add(conflictingItem);
                conflictingItems.Add(conflictingItem);

                if (
                    (ID.AreSame(leftOrigin, conflictingItem.leftOrigin) && conflictingItem.id.client < id.client) ||
                    (conflictingItem.leftOrigin != null && itemsBeforeOrigin.Contains(store.FindItem(conflictingItem.leftOrigin)))
                )
                {
                    left = conflictingItem;
                    conflictingItems.Clear();
                }
                else
                {
                    break;
                }

                conflictingItem = conflictingItem.right;
            }

            if (left != null)
            {
                right = left.right;
                left.right = this;
            }
            else
            {
                Item right;

                if (parentKey != null)
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

            if (right != null)
            {
                right.left = this;
            }
            else if (parentKey != null)
            {
                parent.map[parentKey] = this;

                if (left != null)
                {
                    left.Delete(transaction);
                }
            }

            if (parentKey == null && countable && !deleted)
            {
                parent.length += length;
            }

            store.AddItem(this);

            content.Integrate(transaction, this);

            transaction.SetChanged(parent, parentKey);

            if (parent.Deleted || (right != null && parentKey != null))
            {
                Delete(transaction);
            }
        }

        public void Delete(Transaction transaction)
        {
            if (!deleted)
            {
                if (countable && parentKey == null)
                {
                    parent.length -= length;
                }
                deleted = true;

                transaction.SetDeleted(this);
                transaction.SetChanged(parent, parentKey);
            }
        }

        public bool MergeWith(Item right)
        {
            if (
                ID.AreSame(right.leftOrigin, new ID(id.client, id.clock + length - 1)) &&
                this.right == right &&
                ID.AreSame(rightOrigin, right.rightOrigin) &&
                id.client == right.id.client &&
                id.clock + length == right.id.clock &&
                deleted == right.deleted &&
                redone == null &&
                right.redone == null &&
                content.MergeWith(right.content)
            ) 
            {
                this.right = right.right;
                if (this.right != null)
                {
                    this.right.left = this;
                }
                length += right.length;
                return true;
            }
            return false;
        }
    }
}
