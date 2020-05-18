using System;
using Xunit;
using CollabLib;
using CollabLib.Struct;
using System.Collections.Generic;
using System.Linq;

namespace CollabLib.Tests
{
    public class DocTests
    {
        public Document[] GetDocuments(int numUsers)
        {
            List<Document> documents = new List<Document>();

            for (int i = 0; i < numUsers; ++i)
            {
                Document doc = new Document();
                doc.clientId = i;
                doc.AddTextField("text");
                documents.Add(doc);
            }

            return documents.ToArray();
        }

        [Fact]
        public void CreateTextDoc()
        {
            Document doc = GetDocuments(1)[0];

            Text text = doc.GetTextField("text");
            text.InsertText(0, "Hell world!");
            text.InsertText(4, "o,");
            Assert.Equal("Hello, world!", text.toString());
        }
    }
}
