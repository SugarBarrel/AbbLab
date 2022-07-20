using System;
using System.IO;
using Xunit;

namespace AbbLab.Serialization.Tests
{
    public class WriterReaderTests
    {
        [Fact]
        public void Test()
        {
            TestWriterReader(static w =>
            {
                w.WriteStartElement("element");
                w.WriteText("value");
                w.WriteEndElement();
            }, "<element>value</element>", static r =>
            {
                Assert.Equal(TSReaderState.StartElement, r.State);
                Assert.Equal("element", r.Name);
                Assert.True(r.ReadStartElement());
                Assert.Equal(TSReaderState.Text, r.State);
                Assert.Equal("value", r.Value);
                Assert.Equal("value", r.ReadText());
                Assert.Equal(TSReaderState.EndElement, r.State);
                Assert.Equal("element", r.Name);
                r.ReadEndElement();
                Assert.Equal(TSReaderState.EndOfFile, r.State);
            });

            TestWriterReader(static w =>
            {
                w.WriteStartElement("el");
                w.WriteEndElement();
            }, "<el/>", static r =>
            {
                Assert.Equal(TSReaderState.StartElement, r.State);
                Assert.Equal("el", r.Name);
                Assert.False(r.ReadStartElement());
                Assert.Equal(TSReaderState.EndOfFile, r.State);
            });

            TestWriterReader(static w =>
            {
                w.WriteStartElement("el");
                w.WriteAttribute("attr", "'value'");
                w.WriteAttribute("another", "\"value\"");
                w.WriteAttribute("flag");
                w.WriteEndElement();
            }, "<el attr=\"'value'\" another='\"value\"' flag/>", static r =>
            {
                Assert.Equal(TSReaderState.StartElement, r.State);
                Assert.Equal("el", r.Name);
                Assert.Equal(new StoredAttribute[3]
                {
                    new StoredAttribute("attr", "'value'"),
                    new StoredAttribute("another", "\"value\""),
                    new StoredAttribute("flag", null),
                }, r.Attributes);
                Assert.False(r.ReadStartElement());
                Assert.Equal(TSReaderState.EndOfFile, r.State);
            });

            TestWriterReader(static w =>
            {
                w.WriteStartElement("root");
                {
                    w.WriteStartElement("first");
                    w.WriteAttribute("id", "1");
                    w.WriteEndElement();
                    w.WriteStartElement("second");
                    w.WriteAttribute("id", "2");
                    w.WriteText("value");
                    w.WriteEndElement();
                }
                w.WriteEndElement();
            }, "<root><first id=\"1\"/><second id=\"2\">value</second></root>", static r =>
            {
                Assert.Equal(TSReaderState.StartElement, r.State);
                Assert.Equal("root", r.Name);
                Assert.True(r.ReadStartElement());
                {
                    Assert.Equal(TSReaderState.StartElement, r.State);
                    Assert.Equal("first", r.Name);
                    Assert.Equal(new StoredAttribute[1]
                    {
                        new StoredAttribute("id", "1"),
                    }, r.Attributes);
                    Assert.False(r.ReadStartElement());

                    Assert.Equal(TSReaderState.StartElement, r.State);
                    Assert.Equal("second", r.Name);
                    Assert.Equal(new StoredAttribute[1]
                    {
                        new StoredAttribute("id", "2"),
                    }, r.Attributes);
                    Assert.True(r.ReadStartElement());
                    Assert.Equal(TSReaderState.Text, r.State);
                    Assert.Equal("value", r.Value);
                    Assert.Equal("value", r.ReadText());
                    Assert.Equal(TSReaderState.EndElement, r.State);
                    Assert.Equal("second", r.Name);
                    r.ReadEndElement();
                }
                Assert.Equal(TSReaderState.EndElement, r.State);
                Assert.Equal("root", r.Name);
                r.ReadEndElement();
            });

            TestWriterReader(static w =>
            {
                w.WriteStartElement("root");
                w.WriteComment(" comment ");
                w.WriteEndElement();
            }, "<root><!-- comment --></root>", static r =>
            {
                Assert.Equal(TSReaderState.StartElement, r.State);
                Assert.Equal("root", r.Name);
                Assert.True(r.ReadStartElement());
                Assert.Equal(TSReaderState.Comment, r.State);
                Assert.Equal(" comment ", r.Value);
                Assert.Equal(" comment ", r.ReadComment());
                Assert.Equal(TSReaderState.EndElement, r.State);
                Assert.Equal("root", r.Name);
                r.ReadEndElement();
            });

            TestWriterReader(static w =>
            {
                w.WriteStartElement("root");
                w.WriteCData("<&> [text] <&>");
                w.WriteEndElement();
            }, "<root><![CDATA[<&> [text] <&>]]></root>", static r =>
            {
                Assert.Equal(TSReaderState.StartElement, r.State);
                Assert.Equal("root", r.Name);
                Assert.True(r.ReadStartElement());
                Assert.Equal(TSReaderState.CharacterData, r.State);
                Assert.Equal("<&> [text] <&>", r.Value);
                Assert.Equal("<&> [text] <&>", r.ReadCharacterData());
                Assert.Equal(TSReaderState.EndElement, r.State);
                Assert.Equal("root", r.Name);
                r.ReadEndElement();
            });

            TestWriterReader(static w =>
            {
                w.WriteStartElement("root");
                w.WriteProcessingInstruction("target", "instructions");
                w.WriteEndElement();
            }, "<root><?target instructions?></root>", static r =>
            {
                Assert.Equal(TSReaderState.StartElement, r.State);
                Assert.Equal("root", r.Name);
                Assert.True(r.ReadStartElement());
                Assert.Equal(TSReaderState.ProcessingInstruction, r.State);
                Assert.Equal("target", r.Name);
                Assert.Equal("instructions", r.Value);
                Assert.Equal("instructions", r.ReadProcessingInstruction());
                Assert.Equal(TSReaderState.EndElement, r.State);
                Assert.Equal("root", r.Name);
                r.ReadEndElement();
            });

        }
        private static void TestWriterReader(Action<QuickXmlWriter> writeAction, string expectedOutput, Action<QuickXmlReader> readAction)
        {
            StringWriter stringWriter = new StringWriter();
            QuickXmlWriter quickXmlWriter = new QuickXmlWriter(stringWriter)
            {
                IndentMultiplier = 0,
                NewLine = string.Empty,
            };
            writeAction(quickXmlWriter);
            Assert.Equal(expectedOutput, stringWriter.ToString());
            StringReader stringReader = new StringReader(expectedOutput);
            QuickXmlReader quickXmlReader = new QuickXmlReader(stringReader) { IgnoreWhitespace = true };
            readAction(quickXmlReader);
        }
    }
}
