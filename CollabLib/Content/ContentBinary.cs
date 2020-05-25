using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace CollabLib.Content
{
    public class ContentBinary : AbstractContent
    {
        public ContentBinary(byte[] data)
        {
            this.data = data;
        }
        public byte[] data;
        public bool MergeWith(ContentBinary right)
        {
            return false;
        }

        public override bool Countable { get; } = true;

        public override int Length { get => 1; }

        public override void Integrate(Transaction transaction, Item item) { }

        public override AbstractContent Splice(int index)
        {
            throw new NotImplementedException();
        }

        public const int ContentRef = 3;
        public override int Ref { get => ContentRef; }

        public override byte[] Encode(int offset)
        {
            return BitConverter.GetBytes(data.Length).Concat(data).ToArray();
        }

        public override bool MergeWith(AbstractContent right)
        {
            return false;
        }
    }
}
