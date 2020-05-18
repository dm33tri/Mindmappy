using System;
using System.Collections.Generic;
using System.Text;

namespace CollabLib.Content
{
    class ContentString : AbstractContent
    {
        public ContentString(string str)
        {
            this.str = str;
        }
        public string str;
        public bool MergeWith(ContentString right)
        {
            this.str = this.str + right.str;
            return true;
        }

        public override bool Countable { get => true; }

        public override int Length { get => this.str.Length;  }

        public override void Integrate(Transaction transaction, Item item) { }

        public override AbstractContent Splice(int index)
        {
            string newString = this.str.Substring(index);
            this.str = this.str.Substring(0, index);
            return new ContentString(newString);
        }
    }
}
