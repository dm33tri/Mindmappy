using System;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using CollabLib;
using CollabLib.Struct;
using CollabLib.Content;
using Array = CollabLib.Struct.Array;
using System.Threading.Tasks;
using System.Diagnostics;

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
                doc.AddText("text");
                doc.AddArray("array");

                documents.Add(doc);
            }

            return documents.ToArray();
        }

        [Fact]
        public void CreateTextDoc()
        {
            Document doc = GetDocuments(1)[0];

            Text text = doc.GetText("text");
            text.InsertText(0, "Hell world!");
            text.InsertText(4, "o,");
            Assert.Equal("Hello, world!", text.ToString());
        }

        [Fact]
        public void CreateArrayDoc()
        {
            Document doc = GetDocuments(1)[0];

            Array array = doc.GetArray("array");
            array.Insert(0, new ContentBinary(BitConverter.GetBytes(1)));
            array.Insert(1, new ContentBinary(BitConverter.GetBytes(3)));
            array.Insert(1, new ContentBinary(BitConverter.GetBytes(2)));

            Assert.Equal(1, BitConverter.ToInt32((array[0] as ContentBinary).data));
            Assert.Equal(2, BitConverter.ToInt32((array[1] as ContentBinary).data));
            Assert.Equal(3, BitConverter.ToInt32((array[2] as ContentBinary).data));
        }

        [Fact]
        public void EncodeAndDecodeArrays()
        {
            Document[] docs = GetDocuments(2);

            Array array1 = docs[0].GetArray("array");
            Array array2 = docs[1].GetArray("array");

            docs[0].Update += (doc, data) =>
            {
                docs[1].ApplyUpdate(data);
            };

            docs[0].Transact((transaction) =>
            {
                array1.Insert(0, new ContentBinary(BitConverter.GetBytes(1)));
                array1.Insert(1, new ContentBinary(BitConverter.GetBytes(3)));
            });

            docs[0].Transact((transaction) =>
            {
                array1.Insert(1, new ContentBinary(BitConverter.GetBytes(2)));
            });

            Assert.Equal(1, BitConverter.ToInt32((array2[0] as ContentBinary).data));
            Assert.Equal(2, BitConverter.ToInt32((array2[1] as ContentBinary).data));
            Assert.Equal(3, BitConverter.ToInt32((array2[2] as ContentBinary).data));
        }

        [Fact]
        public void EncodeAndDecodeStrings()
        {
            Document[] docs = GetDocuments(2);

            Text text1 = docs[0].GetText("text");
            Text text2 = docs[1].GetText("text");

            docs[0].Update += (doc, data) =>
            {
                docs[1].ApplyUpdate(data);
            };

            docs[0].Transact((transaction) =>
            {
                text1.InsertTextFunc(0, "Where is clothes?", transaction);
                text1.InsertTextFunc(9, "my ", transaction);
            });

            docs[0].Transact((transaction) =>
            {
                text1.InsertTextFunc(0, "Yo Obama! ", transaction);
            });

            Assert.Equal("Yo Obama! Where is my clothes?", text2.ToString());
        }

        [Fact]
        public void NestedStructs()
        {
            Document doc = new Document();
            Document doc2 = new Document();

            doc2.AddArray("array");
            doc.clientId = 0;
            var array = doc.AddArray("array");
            var map = new Map();
            var text = new Text();

            doc.Update += (doc, data) =>
            {
                doc2.ApplyUpdate(data);
            };

            array.Insert(0, map);
            map.Set("text", text);
            text.InsertText(0, "ABEF");
            text.InsertText(2, "CD");

            Assert.Equal("ABCDEF", ((doc2.GetArray("array")[0] as Map).Get("text") as Text).ToString());
        }

        [Fact]
        public void AnyStruct()
        {
            Document doc0 = new Document();
            Document doc1 = new Document();
            Document doc2 = new Document();
            Document doc3 = new Document();

            doc0.clientId = 0;
            doc1.clientId = 1;
            doc2.clientId = 2;
            doc3.clientId = 3;

            doc0.Update += (doc, data) =>
            {
                doc1.ApplyUpdate(data);
                doc2.ApplyUpdate(data);
                doc3.ApplyUpdate(data);
            };

            doc1.Update += (doc, data) =>
            {
                doc0.ApplyUpdate(data);
            };

            doc2.Update += (doc, data) =>
            {
                doc0.ApplyUpdate(data);
            };

            doc3.Update += (doc, data) =>
            {
                doc0.ApplyUpdate(data);
            };

            var map0 = doc0.AddMap("map");
            var map1 = doc1.AddMap("map");
            var map2 = doc2.AddMap("map");
            var map3 = doc3.AddMap("map");

            var text0 = new Text();
            map0.Set("text", text0);

            var text1 = map1.Get("text") as Text;
            var text2 = map2.Get("text") as Text;
            var text3 = map3.Get("text") as Text;

            //var tasks = new Task[] {
            //    Task.Run(() => {
            //        text0.InsertText(0, "text0");
            //    }),
            //    Task.Run(() => {
            //        text1.InsertText(0, "text1"); 
            //    }),
            //    Task.Run(() => {
            //        text2.InsertText(0, "text2"); 
            //    }),
            //    Task.Run(() => {
            //        text3.InsertText(0, "text3"); 
            //    }),
            //};

            //Task.WaitAll(tasks);
            text0.InsertText(0, "text0");
            text1.InsertText(0, "text1");
            text2.InsertText(0, "text2");
            text3.InsertText(0, "text3");
            Debug.WriteLine(text3.ToString());
        }
    }
}
