using CollabLib.Content;
using System;
using System.Collections.Generic;
using System.Text;

namespace CollabLib.Struct
{
    public class Array : AbstractStruct
    {
        public override string ToString()
        {
            return null;
        }

        public void InsertFunc(int index, AbstractContent content, Transaction transaction)
        {
            Item left = null;

            // find insert position

            if (index != 0)
            {
                left = start;

                for (; left != null && index > 0; left = left.right)
                {
                    if (!left.deleted && left.countable)
                    {
                        if (index <= left.length)
                        {
                            // split item
                            if (index < left.length)
                            {
                                transaction.doc.store.GetItemCleanStart(transaction, new ID(left.id.client, left.id.clock + index));
                            }

                            break;
                        }

                        index -= left.length;
                    }
                }
            }

            Item right = left?.right ?? start;

            Item newItem = new Item(
                transaction.NextID(),
                left,
                left != null ? new ID(left.id.client, left.id.clock + left.length - 1) : null,
                right,
                right?.id,
                this,
                null,
                content
            );

            newItem.Integrate(transaction);
        }

        public void Insert(int index, AbstractContent content)
        {
            if (doc != null)
            {
                doc.Transact((transaction) =>
                {
                    InsertFunc(index, content, transaction);
                });
            }
        }

        public const int ContentTypeRef = 0;
        public override int TypeRef { get => ContentTypeRef; }

        public void Push(AbstractContent content)
        {
            Insert(length, content);
        } 

        public AbstractContent this[int i]
        {
            get
            {
                for (var item = start; item != null; item = item.right)
                {
                    if (!item.deleted && item.countable)
                    {
                        if (i < item.length)
                        {
                            return item.content;
                        }
                        i -= item.length;
                    }
                }

                throw new Exception("Index not found");
            }
        }
    }
}
