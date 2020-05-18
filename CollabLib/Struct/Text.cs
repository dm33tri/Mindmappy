using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using CollabLib.Content;

namespace CollabLib.Struct
{
    public class Text : AbstractStruct
    {
        public string toString()
        {
            List<string> chunks = new List<string>();
            var current = this.start;
            while (current != null)
            {
                if (!current.deleted && current.countable && current.content is ContentString)
                {
                    chunks.Add((current.content as ContentString).str);
                }

                current = current.right;
            }

            return string.Join("", chunks.ToArray());
        }

        private void InsertTextFunc(int index, string text, Transaction transaction)
        {
            Item left = start;

            // find insert position
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

            Item right = left?.right ?? this.start;

            Item newItem = new Item(
                transaction.NextID(),
                left,
                left != null ? new ID(left.id.client, left.id.clock + this.length - 1) : null,
                right,
                right?.id,
                this,
                null,
                new ContentString(text)
            );

            newItem.Integrate(transaction);
        }

        public void InsertText(int index, string text)
        {
            if (text.Length <= 0)
            {
                return;
            }
            Document doc = this.doc;
            if (doc != null)
            {
                doc.Transact((transaction) =>
                {
                    this.InsertTextFunc(index, text, transaction);
                });
            }
        }

        public void Delete(int index, int length)
        {

        }


    }
}
