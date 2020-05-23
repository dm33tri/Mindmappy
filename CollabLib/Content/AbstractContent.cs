using System;
using System.Collections.Generic;
using System.Text;

namespace CollabLib.Content
{
    public abstract class AbstractContent
    {
        public abstract bool Countable { get; }
        public abstract int Length { get; }
        public abstract AbstractContent Splice(int index);
        public abstract void Integrate(Transaction transaction, Item item);
        public abstract int Ref { get; }
        public abstract byte[] Encode(int offset);


    }
}
