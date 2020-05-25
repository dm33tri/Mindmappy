using System.Collections.Generic;
using CollabLib.Content;

namespace CollabLib.Struct
{
    public class Map : AbstractStruct
    {
        public void SetFunc(string key, AbstractContent content, Transaction transaction)
        {
            Item left = null;
            map.TryGetValue(key, out left);

            Item newItem = new Item(
                transaction.NextID(),
                left,
                left != null ? new ID(left.id.client, left.id.clock + left.length - 1) : null,
                null,
                null,
                this,
                key,
                content
            );

            newItem.Integrate(transaction);
        }

        public void Set(string key, AbstractContent content)
        {
            if (doc != null)
            {
                doc.Transact((transaction) =>
                {
                    SetFunc(key, content, transaction);
                });
            }
        }

        public AbstractContent Get(string key)
        {
            if (map.ContainsKey(key))
            {
                return map[key].content;
            }

            return null;
        }

        public Map()
        {
            map = new Dictionary<string, Item>();
        }

        public const int ContentTypeRef = 1;
        public override int TypeRef { get => ContentTypeRef; }
    }
}
