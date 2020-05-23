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
            //Document doc = GetDocuments(1)[0];

            //Text text = doc.GetTextField("text");
            //text.InsertText(0, "Hell world!");
            //text.InsertText(4, "o,");
            //Assert.Equal("Hello, world!", text.toString());
        }

        [Fact]
        public void EncodeAndDecodeUpdates()
        {
            Document[] docs = GetDocuments(2);

            Text text1 = docs[0].GetTextField("text");
            Text text2 = docs[1].GetTextField("text");

            docs[0].Update += (doc, data) =>
            {
                docs[1].ApplyUpdate(data);
            };

            docs[0].Transact((transaction) =>
            {
                text1.InsertTextFunc(0, "Hell world!", transaction);
                text1.InsertTextFunc(4, "o,", transaction);
            });


            Assert.Equal("Hello, world!", text2.toString());
        }
    }
}
