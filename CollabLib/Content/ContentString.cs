using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CollabLib.Content
{
    public class ContentString : AbstractContent
    {
        public ContentString(string str)
        {
            this.str = str;
        }
        public string str;
        public bool MergeWith(ContentString right)
        {
            str = str + right.str;
            return true;
        }

        public override bool Countable { get; } = true;

        public override int Length { get => str.Length;  }

        public override void Integrate(Transaction transaction, Item item) { }

        public override AbstractContent Splice(int index)
        {
            string newString = str.Substring(index);
            str = str.Substring(0, index);
            return new ContentString(newString);
        }

        public const int ContentRef = 4;
        public override int Ref { get => ContentRef; }

        public override byte[] Encode(int offset)
        {
            string str = this.str;
            if (offset > 0)
            {
                str = str.Substring(offset);
            }

            byte[] strBytes = Encoding.UTF8.GetBytes(str);
            byte[] lengthBytes = BitConverter.GetBytes(strBytes.Length);

            return lengthBytes.Concat(strBytes).ToArray();
        }

        public override bool MergeWith(AbstractContent right)
        {
            if (right is ContentString)
            {
                str += (right as ContentString).str;
            }
            return false;
        }
    }
}
