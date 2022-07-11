using System;
using System.Collections.Generic;
using System.IO;

namespace AbbLab.Serialization
{
    public class QuickXmlWriter
    {
        public QuickXmlWriter(TextWriter writer) => Writer = writer;
        private readonly TextWriter Writer;

        private enum WriterState
        {
            Default = 0,
            StartElement,
            StartAttribute,
            Attribute,
            Comment,
            CharacterData,
            ProcessingInstruction,
        }

        private WriterState State;
        private readonly Stack<string> elementStack = new Stack<string>();

        public string NewLine { get; set; } = Environment.NewLine;
        private int currentIndent;
        private int indentMultiplier = 2;
        public int IndentMultiplier
        {
            get => indentMultiplier;
            set
            {
                if (indentMultiplier < 0) throw new ArgumentOutOfRangeException(
                    nameof(value), value, "Indent multiplier cannot be negative.");
                indentMultiplier = value;
            }
        }
        private char[] indentChars = defaultIndentChars;
        public char IndentChar
        {
            get => indentChars[0];
            set => indentChars = value is ' ' ? defaultIndentChars : FillIndentChars(value);
        }
        public bool CompatibilityMode { get; set; }

        private char quoteChar;

        private static readonly char[] defaultIndentChars = FillIndentChars(' ');
        private static char[] FillIndentChars(char c)
        {
            char[] array = new char[64];
            Array.Fill(array, c);
            return array;
        }

        private void DoIndent()
        {
            if (NewLine.Length > 0)
                Writer.Write(NewLine);
            if (indentMultiplier is 0) return;
            int i = currentIndent * indentMultiplier;
            if (i < indentChars.Length)
            {
                Writer.Write(indentChars, 0, i);
                return;
            }
            while (i > 0)
            {
                Writer.Write(indentChars, 0, Math.Min(i, indentChars.Length));
                i -= indentChars.Length;
            }
        }
        private void CloseStarting()
        {
            Writer.Write('>');
            currentIndent++;
            State = WriterState.Default;
        }

        public void WriteStartElement(string elementName)
        {
            if (State is WriterState.StartElement) CloseStarting();
            switch (State)
            {
                case WriterState.Default:
                    Writer.Write('<');
                    Writer.Write(elementName);
                    elementStack.Push(elementName);

                    State = WriterState.StartElement;
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
        public void WriteEndElement()
        {
            switch (State)
            {
                case WriterState.StartElement:
                    Writer.Write("/>");
                    State = WriterState.Default;
                    elementStack.Pop();
                    break;
                case WriterState.Default:
                    if (!elementStack.TryPop(out string? elementName))
                        throw new NotImplementedException();

                    currentIndent--;
                    DoIndent();
                    Writer.Write("</");
                    Writer.Write(elementName);
                    Writer.Write('>');
                    State = WriterState.Default;
                    break;
                default:
                    throw new NotImplementedException();
            }

        }

        public void WriteStartAttribute(string attributeName)
        {
            switch (State)
            {
                case WriterState.StartElement:
                    if (attributeName.Length > 0)
                    {
                        Writer.Write(' ');
                        Writer.Write(attributeName);
                    }
                    State = WriterState.StartAttribute;
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
        public void WriteEndAttribute()
        {
            switch (State)
            {
                case WriterState.StartAttribute:
                    // non-valued attribute
                    State = WriterState.StartElement;
                    break;

                case WriterState.Attribute:
                    // attribute has a value, close it
                    Writer.Write(quoteChar);
                    State = WriterState.StartElement;
                    quoteChar = default;
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        public void WriteAttribute(string attributeName)
        {
            WriteStartAttribute(attributeName);
            WriteEndAttribute();
        }
        public void WriteAttribute(string attributeName, string? attributeValue)
        {
            WriteStartAttribute(attributeName);
            if (attributeValue is not null) WriteText(attributeValue.AsSpan());
            WriteEndAttribute();
        }
        public void WriteAttribute(string attributeName, ReadOnlySpan<char> attributeValue)
        {
            WriteStartAttribute(attributeName);
            WriteText(attributeValue);
            WriteEndAttribute();
        }

        public void WriteText(string text) => WriteText(text.AsSpan());
        public void WriteText(ReadOnlySpan<char> text)
        {
            switch (State)
            {
                case WriterState.StartElement:
                    CloseStarting();
                    break;

                case WriterState.StartAttribute:
                    static bool ShouldUseApostrophe(ReadOnlySpan<char> text)
                    {
                        int apostrophes = 0;
                        int quotes = 0;
                        for (int i = 0, length = text.Length; i < length; i++)
                        {
                            char c = text[i];
                            if (c is '\'') apostrophes++;
                            else if (c is '"') quotes++;
                        }
                        return quotes > apostrophes;
                    }

                    quoteChar = ShouldUseApostrophe(text) ? '\'' : '"';
                    Writer.Write('=');
                    Writer.Write(quoteChar);
                    State = WriterState.Attribute;
                    break;
            }
            SanitizeWrite(text);
        }

        public void WriteStartComment()
        {
            if (State is WriterState.StartElement) CloseStarting();
            switch (State)
            {
                case WriterState.Default:
                    Writer.Write("<!--");
                    State = WriterState.Comment;
                    // Comment uses 1 char buffer to detect '--' in Compatibility Mode
                    lastCharLength = 0;
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
        public void WriteEndComment()
        {
            switch (State)
            {
                case WriterState.Comment:
                    if (CompatibilityMode && lastChar0 is '-')
                        throw new NotImplementedException(); // two consecutive hyphens
                    Writer.Write("-->");
                    State = WriterState.Default;
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        public void WriteComment(string text)
            => WriteComment(text.AsSpan());
        public void WriteComment(ReadOnlySpan<char> text)
        {
            WriteStartComment();
            WriteText(text);
            WriteEndComment();
        }

        public void WriteStartCData()
        {
            if (State is WriterState.StartElement) CloseStarting();
            switch (State)
            {
                case WriterState.Default:
                    Writer.Write("<![CDATA[");
                    State = WriterState.CharacterData;
                    // CharacterData uses 2 char buffer to detect ']]>' and replace it
                    lastCharLength = 0;
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
        public void WriteEndCData()
        {
            switch (State)
            {
                case WriterState.CharacterData:
                    if (lastCharLength > 0) Writer.Write(lastChar0); // write the buffered characters
                    if (lastCharLength > 1) Writer.Write(lastChar1);
                    Writer.Write("]]>");
                    State = WriterState.Default;
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        public void WriteCData(string text)
            => WriteCData(text.AsSpan());
        public void WriteCData(ReadOnlySpan<char> text)
        {
            WriteStartCData();
            WriteText(text);
            WriteEndCData();
        }

        public void WriteStartProcessingInstruction(string piTarget)
        {
            if (State is WriterState.StartElement) CloseStarting();
            switch (State)
            {
                case WriterState.Default:
                    Writer.Write("<?");
                    Writer.Write(piTarget);
                    Writer.Write(' ');
                    State = WriterState.ProcessingInstruction;
                    // ProcessingInstruction uses 1 char buffer to detect '?>'
                    lastCharLength = 0;
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
        public void WriteEndProcessingInstruction()
        {
            switch (State)
            {
                case WriterState.ProcessingInstruction:
                    Writer.Write("?>");
                    State = WriterState.Default;
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        public void WriteProcessingInstruction(string piTarget, string text)
            => WriteProcessingInstruction(piTarget, text.AsSpan());
        public void WriteProcessingInstruction(string piTarget, ReadOnlySpan<char> text)
        {
            WriteStartProcessingInstruction(piTarget);
            WriteText(text);
            WriteEndProcessingInstruction();
        }

        private char lastChar0; // 1-char buffer: comments ('--'), PI ('?>')
        private char lastChar1; // 2-char buffer: CData (']]>')
        private int lastCharLength; // length buffer: CData (']]>')

        private void SanitizeWrite(ReadOnlySpan<char> text)
        {
            // TODO: Compatibility Mode - escape ']]>' as ']]&gt;' everywhere except for CDATA

            switch (State)
            {
                case WriterState.Default:
                    SanitizeWriteDefault(text);
                    break;
                case WriterState.Attribute:
                    SanitizeWriteAttribute(text);
                    break;
                case WriterState.Comment:
                    SanitizeWriteComment(text);
                    break;
                case WriterState.CharacterData:
                    SanitizeWriteCharacterData(text);
                    break;
                case WriterState.ProcessingInstruction:
                    SanitizeWriteProcessingInstruction(text);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
        private static string EscapeSequence(char c) => c switch
        {
            '<' => "&lt;",
            '&' => "&amp;",
            '>' => "&gt;",
            '\'' => "&apos;",
            '"' => "&quot;",
            _ => throw new NotImplementedException(),
        };

        private void SanitizeWriteDefault(ReadOnlySpan<char> text)
        {
            int pos = 0;
            int length = text.Length;
            int startWrite = 0;

            while (pos < length)
            {
                char c = text[pos];
                // escape '<', '&' and '>'
                if (c is '<' or '&' or '>')
                {
                    if (startWrite < pos) Writer.Write(text[startWrite..pos]); // write previous block in bulk
                    startWrite = ++pos; // set the next character as the starting point

                    Writer.Write(EscapeSequence(c));
                }
                else pos++; // just move forward (write later in bulk)
            }

            if (startWrite < pos) Writer.Write(text[startWrite..length]); // write previous block in bulk

        }
        private void SanitizeWriteAttribute(ReadOnlySpan<char> text)
        {
            int pos = 0;
            int length = text.Length;
            int startWrite = 0;
            bool compat = CompatibilityMode;
            char quote = quoteChar;

            while (pos < length)
            {
                char c = text[pos];
                // Compatibility Mode - escape '<', '&', '>' and both quote characters
                // Normal Mode - escape only the current quote character
                if (compat ? c is '<' or '&' or '>' or '\'' or '"' : c == quote)
                {
                    if (startWrite < pos) Writer.Write(text[startWrite..pos]); // write previous block in bulk
                    startWrite = ++pos; // set the next character as the starting point

                    Writer.Write(EscapeSequence(c));
                }
                else pos++; // just move forward (write later in bulk)
            }

            if (startWrite < pos) Writer.Write(text[startWrite..length]); // write previous block in bulk

        }
        private void SanitizeWriteComment(ReadOnlySpan<char> text)
        {
            // do not recognize entities - therefore, do not escape anything
            // TODO: escape '-->' sequence (can't?)

            if (CompatibilityMode)
            {
                // Compatibility Mode - throw on '--'
                ReadOnlySpan<char> span = text;
                int hyphenIndex = span.IndexOf('-');
                if (hyphenIndex is 0 && lastCharLength > 0 && lastChar0 is '-')
                    throw new NotImplementedException(); // two consecutive hyphens

                while (hyphenIndex is not -1)
                {
                    span = span[(hyphenIndex + 1)..]; // get everything after the hyphen
                    hyphenIndex = span.IndexOf('-');
                    if (hyphenIndex is 0) throw new NotImplementedException(); // two consecutive hyphens
                }
            }
            Writer.Write(text);

            if (text.Length > 0) // 1+ characters, rewrite the buffer
            {
                lastChar0 = text[^1];
                lastCharLength = 1;
            }

        }
        private void SanitizeWriteCharacterData(ReadOnlySpan<char> text)
        {
            // do not recognize entities - therefore, do not escape anything
            // escape ']]>' as the following: (works in most situations), but throw in Compatibility Mode
            const string cDataEndEscape = "]]" + "]]>" + "<![CDATA[" + ">";

            int pos = 0;
            int length = text.Length;
            int startWrite = 0;

            if (length > 0 && lastCharLength > 1) // 1+ characters to write (buffered ']]', writing '>')
            {
                if (lastChar0 is ']' && lastChar1 is ']' && text[0] is '>')
                {
                    if (CompatibilityMode) throw new NotImplementedException();
                    Writer.Write(cDataEndEscape);
                    pos = 1;
                    startWrite = 1;
                    lastCharLength = 0;
                }
                else Writer.Write(lastChar0);
            }
            if (length > 1 && lastCharLength > 0) // 2+ characters to write (buffered ']', writing ']>')
            {
                if (lastChar1 is ']' && text[0] is ']' && text[1] is '>')
                {
                    if (CompatibilityMode) throw new NotImplementedException();
                    Writer.Write(cDataEndEscape);
                    pos = 2;
                    startWrite = 2;
                    lastCharLength = 1;
                }
                else Writer.Write(lastChar1);
            }

            while (pos < length - 2)
            {
                char c = text[pos];
                if (c is ']' && text[pos + 1] is ']' && text[pos + 2] is '>')
                {
                    if (startWrite < pos) Writer.Write(text[startWrite..pos]); // write previous block in bulk
                    startWrite = pos += 3; // set the next character as the starting point

                    if (CompatibilityMode) throw new NotImplementedException();
                    Writer.Write(cDataEndEscape);
                }
                else pos++; // just move forward (write later in bulk)
            }

            if (startWrite < pos) Writer.Write(text[startWrite..^2]); // write previous block in bulk (except for last 2)

            // these characters will be written later
            if (length > 1) // 2+ characters, rewrite the buffer
            {
                lastChar0 = text[^2];
                lastChar1 = text[^1];
                lastCharLength = 2;
            }
            else if (length > 0) // 1 character, shift the buffer
            {
                lastChar0 = lastChar1;
                lastChar1 = text[^1];
                if (lastCharLength is 0) lastCharLength = 1; // if it's 2, keep it at 2
            }

        }
        private void SanitizeWriteProcessingInstruction(ReadOnlySpan<char> text)
        {
            // do not recognize entities - therefore, do not escape anything
            // '?>' cannot be escaped (just throw)

            int length = text.Length;
            if (length > 0 && lastCharLength > 0) // 1+ characters to write (buffered '?', writing '>')
            {
                if (lastChar0 is '?' && text[0] is '>')
                    throw new NotImplementedException();
            }

            ReadOnlySpan<char> span = text;
            int questionIndex = span.IndexOf('?'); // find a '?'
            while (questionIndex is not -1)
            {
                span = span[(questionIndex + 1)..]; // get everything after the question mark
                if (span.Length > 0 && span[0] is '>') // find a '>' right after '?'
                    throw new NotImplementedException();
                questionIndex = span.IndexOf('?');
            }
            Writer.Write(text);

            if (length > 0) // 1+ characters, shift the buffer
            {
                lastChar0 = text[^1];
                lastCharLength = 1;
            }

        }




    }
}
