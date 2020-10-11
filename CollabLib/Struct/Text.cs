using System.Collections.Generic;
using CollabLib.Content;

namespace CollabLib.Struct
{
    public class Text : AbstractStruct
    {
        public override string ToString()
        {
            List<string> chunks = new List<string>();
            var current = start;
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

        public void InsertTextFunc(int index, string text, Transaction transaction)
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
            if (doc != null)
            {
                doc.Transact((transaction) =>
                {
                    InsertTextFunc(index, text, transaction);
                });
            }
        }

        public void Diff(string newText)
        {
            if (newText.Length > length) // insert
            {
                var curr = ToString();
                int start = curr.Length;
                for (int i = 0; i < curr.Length; ++i)
                {
                    if (newText[i] != curr[i])
                    {
                        start = i;
                        break;
                    }
                }
                var len = newText.Length - curr.Length;
                var diff = newText.Substring(start, len);
                InsertText(start, diff);
            } 
            else if (newText.Length < length) // delete
            {
                var curr = ToString();
                int start = curr.Length;
                for (int i = 0; i < newText.Length; ++i)
                {
                    if (newText[i] != curr[i])
                    {
                        start = i;
                        break;
                    }
                }
                var len = curr.Length - newText.Length;
                DeleteText(start, len);
            }
        }

        public void DeleteTextFunc(int index, int length, Transaction transaction)
        {
            Item left = null, right = null; ;
            if (index != 0) // find left
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
                            right = left.right;
                            break;
                        }

                        index -= left.length;
                    }
                }
            }

            for (; right != null && length > 0; right = right.right)
            {
                if (!right.deleted && right.countable)
                {
                    if (length <= right.length)
                    {
                        if (length < right.length)
                        {
                            transaction.doc.store.GetItemCleanStart(transaction, new ID(right.id.client, right.id.clock + length));
                        }
                        break;
                    }
                    right.Delete(transaction);
                    length -= right.length;
                }
            }

            right?.Delete(transaction);
            this.length -= right?.length ?? 0;

            if (left != null)
            {
                Item newRight = right?.right;
                left.right = newRight;
            }
        }

        public void DeleteText(int index, int length)
        {
            doc.Transact((transaction) =>
            {
                DeleteTextFunc(index, length, transaction);
            });
        }

        public const int ContentTypeRef = 2;
        public override int TypeRef { get => ContentTypeRef; }
    }
}
