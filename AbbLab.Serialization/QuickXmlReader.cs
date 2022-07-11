using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using AbbLab.Extensions;

namespace AbbLab.Serialization
{
    public class QuickXmlReader
    {
        public QuickXmlReader(TextReader reader)
        {
            Reader = reader;
            totalBufferLength = 4096;
            charBuffer = new char[totalBufferLength];
            attributesReadonly = new ReadOnlyCollection<StoredAttribute>(nodeAttributes);
            ParseBeginning();
        }
        private readonly TextReader Reader;
        private readonly char[] charBuffer;
        private readonly int totalBufferLength;
        private int charPos;
        private int charLength;
        private readonly StringBuilder stringBuilder = new StringBuilder();
        private readonly Stack<string> elementStack = new Stack<string>();

        public bool IgnoreWhitespace { get; set; }

        private TSReaderState parserState;
        private bool readHeader;
        public TSReaderState State { get; private set; } // public state

        private bool nodeIsEmpty;
        private string? nodeName;
        private string? nodeValue;
        private readonly List<StoredAttribute> nodeAttributes = new List<StoredAttribute>();
        private readonly ReadOnlyCollection<StoredAttribute> attributesReadonly;
        public ReadOnlyCollection<StoredAttribute> Attributes
        {
            get
            {
                if (State is not TSReaderState.StartElement) throw new NotImplementedException();
                if (parserState is not TSReaderState.Undefined) ReadData();
                return attributesReadonly;
            }
        }
        public bool IsEmpty
        {
            get
            {
                if (State is not TSReaderState.StartElement) throw new NotImplementedException();
                if (parserState is not TSReaderState.Undefined) ReadData();
                return nodeIsEmpty;
            }
        }
        public string Name
        {
            get
            {
                if (State is not TSReaderState.StartElement and not TSReaderState.EndElement and not TSReaderState.ProcessingInstruction)
                    throw new NotImplementedException();
                if (parserState is TSReaderState.EndElement) ReadData();
                else if (!readHeader)
                {
                    if (parserState is TSReaderState.StartElement)
                        ParseStartElementHeader();
                    else ParseProcessingInstructionHeader();
                }
                return nodeName!;
            }
        }
        public string Value
        {
            get
            {
                if (State is not TSReaderState.Text and not TSReaderState.CharacterData
                    and not TSReaderState.Comment and not TSReaderState.ProcessingInstruction)
                    throw new NotImplementedException();
                if (parserState is not TSReaderState.Undefined) ReadData();
                return nodeValue!;
            }
        }

        public string? GetAttribute(string name) => nodeAttributes.Find(attr => attr.Name == name).Value;
        public bool TryGetAttribute(string name, out string? value)
        {
            int index = nodeAttributes.FindIndex(attr => attr.Name == name);
            if (index is -1) return Util.Fail(out value);
            value = nodeAttributes[index].Value;
            return true;
        }

        [SuppressMessage("ReSharper", "CommentTypo")]
        private int ReadBuffer()
        {
            // everything that wasn't yet read in the buffer is shifted to the start
            // |abcdefghijklmnopqrstuvwxyz|
            // |     ^                    |
            // |fghijklmnopqrstuvwxyz01234|
            // |^                         |

            int left = charLength - charPos;
            int newlyRead;
            if (left is 0) // Completely replace the current buffer
            {
                newlyRead = Reader.ReadBlock(charBuffer, 0, totalBufferLength);
                charLength = newlyRead;
            }
            else // Copy the end of the buffer into the beginning, and read the rest
            {
                Buffer.BlockCopy(charBuffer, charPos * sizeof(char), charBuffer, 0, left * sizeof(char));
                newlyRead = Reader.ReadBlock(charBuffer, left, totalBufferLength - left);
                charLength = newlyRead + left;
            }
            charPos = 0;
            return newlyRead;
        }
        private int ReadBuffer(out int pos, out int length)
        {
            int res = ReadBuffer();
            pos = charPos;
            length = charLength;
            return res;
        }
        private static bool MatchString(char[] buffer, int pos, string str)
        {
            for (int i = 0, length = str.Length; i < length; i++)
                if (buffer[pos + i] != str[i])
                    return false;
            return true;
        }

        private void ParseBeginning()
        {
            parserState = ParseBeginningInternal();
            if (IgnoreWhitespace && parserState is TSReaderState.Whitespace)
            {
                // move on to the next node
                SkipData();
                parserState = ParseBeginningInternal();
            }
            State = parserState;
            nodeName = null;
            nodeValue = null;
            nodeIsEmpty = false;
            readHeader = false;
        }
        private TSReaderState ParseBeginningInternal()
        {
            // identify the type of the next node and consume the characters

            int pos = charPos; // always stays at '<'
            int length = charLength;
            char[] buffer = charBuffer;

            if (pos >= length && ReadBuffer(out pos, out length) is 0)
                return TSReaderState.EndOfFile;

            if (buffer[pos] is not '<')
            {
                return char.IsWhiteSpace(buffer[pos]) ? TSReaderState.Whitespace : TSReaderState.Text;
            }

            if (pos + 1 >= length && ReadBuffer(out pos, out length) is 0)
                throw new NotImplementedException(); // expected a char after '<'

            switch (buffer[pos + 1])
            {
                case '?':

                    charPos = pos + 2; // consumed 2 chars - '<?'
                    return TSReaderState.ProcessingInstruction;

                case '!':

                    if (pos + 3 >= length && ReadBuffer(out pos, out length) is 0)
                        throw new NotImplementedException(); // expected 2 chars after '<!' ('--', or part of CDATA)

                    if (buffer[pos + 2] is '-' && buffer[pos + 3] is '-')
                    {
                        charPos = pos + 4; // consumed 4 chars - '<!--'
                        return TSReaderState.Comment;
                    }

                    if (pos + 8 >= length && ReadBuffer(out pos, out length) is 0)
                        throw new NotImplementedException(); // expected 7 chars after '<!' ('[CDATA[')

                    if (MatchString(buffer, pos + 2, "[CDATA["))
                    {
                        charPos = pos + 9; // consumed 9 chars - '<![CDATA['
                        return TSReaderState.CharacterData;
                    }
                    throw new NotImplementedException(); // expected either '<!--' or '<![CDATA['

                case '/':

                    charPos = pos + 2; // consumed 2 chars - '</'
                    return TSReaderState.EndElement;

                default:

                    charPos = pos + 1; // consumed 1 char - '<'
                    return TSReaderState.StartElement;
            }
        }

        private void ReadData()
        {
            // if the current node was already read, return
            if (parserState is TSReaderState.Undefined) return;
            // read data w/ stringBuilder
            switch (parserState)
            {
                case TSReaderState.StartElement:
                    nodeIsEmpty = !ParseStartElement(nodeAttributes);
                    if (nodeIsEmpty) elementStack.Pop();
                    break;
                case TSReaderState.EndElement:
                    nodeName = elementStack.Peek();
                    ParseEndElement();
                    break;
                case TSReaderState.Text:
                    ParseText(stringBuilder);
                    nodeValue = stringBuilder.ToStringAndClear();
                    break;
                case TSReaderState.Whitespace:
                    ParseWhitespaces(stringBuilder);
                    nodeValue = stringBuilder.ToStringAndClear();
                    break;
                case TSReaderState.CharacterData:
                    ParseCharacterData(stringBuilder);
                    nodeValue = stringBuilder.ToStringAndClear();
                    break;
                case TSReaderState.Comment:
                    ParseComment(stringBuilder);
                    nodeValue = stringBuilder.ToStringAndClear();
                    break;
                case TSReaderState.ProcessingInstruction:
                    ParseProcessingInstruction(stringBuilder);
                    nodeValue = stringBuilder.ToStringAndClear();
                    break;
                default: throw new NotImplementedException(); // Undefined or EndOfFile
            }
            parserState = TSReaderState.Undefined; // mark node as read
            // current node type is still available as the public State
        }
        private void SkipData()
        {
            // if the current node was already read, return
            if (parserState is TSReaderState.Undefined) return;
            // skip data w/o stringBuilder
            switch (parserState)
            {
                case TSReaderState.StartElement:
                    nodeIsEmpty = !ParseStartElement(null);
                    if (nodeIsEmpty) elementStack.Pop();
                    break;
                case TSReaderState.EndElement:
                    ParseEndElement();
                    break;
                case TSReaderState.Text:
                    ParseText(null);
                    break;
                case TSReaderState.Whitespace:
                    ParseWhitespaces(null);
                    break;
                case TSReaderState.CharacterData:
                    ParseCharacterData(null);
                    break;
                case TSReaderState.Comment:
                    ParseComment(null);
                    break;
                case TSReaderState.ProcessingInstruction:
                    ParseProcessingInstruction(null);
                    break;
                default: throw new NotImplementedException(); // Undefined or EndOfFile
            }
            parserState = TSReaderState.Undefined; // mark node as read
            // current node type is still available as the public State
        }

        private static bool ParseReference(char[] buffer, int pos, ref int length)
        {
            int valuePos = pos + 1; // value inside '&' and ';'
            int semicolonIndex = Array.IndexOf(buffer, ';', valuePos, length - valuePos);
            if (semicolonIndex is -1)
            {
                if (pos is 0) throw new NotImplementedException(); // reference is longer than total buffer size?
                return false; // ReadBuffer() up to the current position
            }

            char targetChar;
            int valueLength = semicolonIndex - valuePos; // length excluding '&' and ';'
            if (valueLength >= 1 && buffer[valuePos] is '#')
            {
                if (valueLength >= 2 && buffer[valuePos + 1] is 'x')
                {
                    // '&#x' [0-9a-fA-F]+ ';'
                    uint value = 0u;
                    for (int i = valuePos + 2; i < semicolonIndex; i++)
                    {
                        char c = buffer[i];
                        uint part = (uint)(c switch
                        {
                            >= '0' and <= '9' => c - '0',
                            >= 'a' and <= 'f' => c + 10 - 'a',
                            >= 'A' and <= 'F' => c + 10 - 'A',
                            _ => throw new NotImplementedException(), // invalid character
                        });
                        value = (value << 8) | part;
                    }
                    targetChar = (char)value;
                }
                else
                {
                    // '&#' [0-9]+ ';'
                    uint value = 0u;
                    for (int i = valuePos + 1; i < semicolonIndex; i++)
                    {
                        char c = buffer[i];
                        if (c is < '0' or > '9') throw new NotImplementedException(); // invalid character
                        value = (value << 8) | (uint)(c - '0');
                    }
                    targetChar = (char)value;
                }
            }
            else
            {
                // '&' name ';'
                if (MatchString(buffer, valuePos, "amp")) targetChar = '&';
                else if (MatchString(buffer, valuePos, "lt")) targetChar = '<';
                else if (MatchString(buffer, valuePos, "gt")) targetChar = '>';
                else if (MatchString(buffer, valuePos, "apos")) targetChar = '\'';
                else if (MatchString(buffer, valuePos, "quot")) targetChar = '"';
                else throw new NotImplementedException(); // invalid reference
            }

            buffer[pos] = targetChar;
            int referenceLength = valueLength + 2;
            int posAfterReference = pos + referenceLength;
            const int replaceLength = 1;
            // it's replaced by a single char in all instances
            // replacing with a value longer than the reference could mess everything up

            // abc&gt;abc123| length -= 3, that is (referenceLength - replaceLength)
            // abc>abc123   | copied from (pos + referenceLength) to (pos + replaceLength) up to the end of the buffer

            Buffer.BlockCopy(buffer, posAfterReference * sizeof(char), buffer, (pos + replaceLength) * sizeof(char), length - posAfterReference);
            length -= referenceLength - replaceLength;

            return true;
        }

        // Parse_____ methods just append to StringBuilder,
        // leaving charPos at the first unparsed character,
        // without changing the ReaderState.

        private void ParseWhitespaces(StringBuilder? sb)
        {
            bool res;
            do
            {
                res = ParseWhitespacesPart(out int start, out int end);
                sb?.Append(charBuffer, start, end - start);
            }
            while (res && ReadBuffer() > 0);
        }
        private bool ParseWhitespacesPart(out int start, out int end)
        {
            int pos = charPos;
            int length = charLength;
            char[] buffer = charBuffer;
            start = pos;

            bool continueParsing = true;
            while (pos < length)
            {
                char c = buffer[pos];
                if (!char.IsWhiteSpace(c))
                {
                    continueParsing = false;
                    break;
                }
                pos++;
            }
            end = pos;
            charPos = pos;
            return continueParsing;
        }

        private void ParseName(StringBuilder? sb)
        {
            bool res;
            do
            {
                res = ParseNamePart(out int start, out int end);
                sb?.Append(charBuffer, start, end - start);
            }
            while (res && ReadBuffer() > 0);
        }
        private bool ParseNamePart(out int start, out int end)
        {
            int pos = charPos;
            int length = charLength;
            char[] buffer = charBuffer;
            start = pos;

            bool continueParsing = true;
            while (pos < length)
            {
                char c = buffer[pos];
                if (c is '/' or '>' or '=' || char.IsWhiteSpace(c))
                {
                    continueParsing = false;
                    break;
                }
                pos++;
            }
            end = pos;
            charPos = pos;
            return continueParsing;
        }

        private void ParseAttributeValue(StringBuilder? sb)
        {
            if (charPos >= charLength && ReadBuffer() is 0)
                throw new NotImplementedException(); // expected a quote char

            char quote = charBuffer[charPos++]; // consume the quote char
            if (quote is not '\'' and not '"')
                throw new NotImplementedException(); // invalid quote char

            // TODO: Here's a far-fetched idea: if the quote character is invalid,
            // TODO: try parsing it as a name, allowing the following syntax:
            // TODO: <element key=0 name=John>

            bool res;
            do
            {
                res = ParseAttributeValuePart(quote, out int start, out int end);
                sb?.Append(charBuffer, start, end - start);
            }
            while (res && ReadBuffer() > 0);
            charPos++; // consume the quote char
        }
        private bool ParseAttributeValuePart(char quote, out int start, out int end)
        {
            int pos = charPos;
            int length = charLength;
            char[] buffer = charBuffer;
            start = pos;

            bool continueParsing = true;
            while (pos < length)
            {
                char c = buffer[pos];
                if (c == quote)
                {
                    continueParsing = false;
                    break;
                }
                if (c is '&')
                {
                    // Reference length is limited by total buffer length
                    if (!ParseReference(buffer, pos, ref length)) break;
                }
                pos++;
            }
            end = pos;
            charPos = pos;
            return continueParsing;
        }

        // The methods below check the state, parse the data,
        // but don't change the state.

        private void ParseStartElementHeader()
        {
            if (parserState is not TSReaderState.StartElement) throw new NotImplementedException();
            // after ParseBeginning(), '<' is already parsed
            if (readHeader) return;

            // parse the element's name
            ParseName(stringBuilder);
            string elementName = stringBuilder.ToStringAndClear();
            elementStack.Push(elementName);
            nodeName = elementName;

            readHeader = true;
        }
        private bool ParseStartElement(List<StoredAttribute>? storeAttributes)
        {
            if (!readHeader) ParseStartElementHeader();

            storeAttributes?.Clear();
            bool isEmpty;
            while (true)
            {
                // parse whitespaces
                ParseWhitespaces(null);

                if (charPos >= charLength && ReadBuffer() is 0)
                    throw new NotImplementedException(); // expected at least a '/>' or '>'

                char c = charBuffer[charPos];
                if (c is '/')
                {
                    charPos++; // consume '/'
                    if (charPos >= charLength && ReadBuffer() is 0 || charBuffer[charPos] is not '>')
                        throw new NotImplementedException(); // expected '>' after '/'

                    charPos++; // consume '>'
                    isEmpty = true; // self-closing start element
                    break;
                }
                if (c is '>')
                {
                    charPos++; // consume '>'
                    isEmpty = false; // end of a simple start element
                    break;
                }
                if (storeAttributes is not null)
                {
                    ParseName(stringBuilder); // parse the attribute's name
                    string attributeName = stringBuilder.ToStringAndClear();
                    ParseWhitespaces(null); // parse the whitespaces

                    if (charPos >= charLength && ReadBuffer() is 0)
                        throw new NotImplementedException(); // expected at least a '=' or '/' or '>'

                    if (charBuffer[charPos] is '=')
                    {
                        charPos++;
                        ParseWhitespaces(null); // parse the whitespaces
                        ParseAttributeValue(stringBuilder); // parse the attribute's value in quotes
                        string attributeValue = stringBuilder.ToStringAndClear();
                        storeAttributes.Add(new StoredAttribute(attributeName, attributeValue));
                    }
                    else
                    {
                        // value-less attribute
                        storeAttributes.Add(new StoredAttribute(attributeName, null));
                    }
                }
                else
                {
                    // the same thing, but without stringBuilder
                    ParseName(null);
                    ParseWhitespaces(null);

                    if (charPos >= charLength && ReadBuffer() is 0)
                        throw new NotImplementedException(); // expected at least a '=' or '/' or '>'

                    if (charBuffer[charPos] is '=')
                    {
                        charPos++;
                        ParseWhitespaces(null);
                        ParseAttributeValue(null);
                    }
                }
            }
            return !isEmpty;
        }
        private void ParseEndElement()
        {
            if (parserState is not TSReaderState.EndElement) throw new NotImplementedException();
            // after ParseBeginning(), '</' is already parsed

            // parse the element's name
            ParseName(stringBuilder);
            string elementName = stringBuilder.ToStringAndClear();
            if (elementStack.Pop() != elementName)
                throw new NotImplementedException(); // element's name does not match!

            ParseWhitespaces(null); // parse the whitespaces

            if (charPos >= charLength && ReadBuffer() is 0)
                throw new NotImplementedException(); // expected a '>'
            if (charBuffer[charPos] is not '>')
                throw new NotImplementedException(); // expected a '>'
            charPos++; // consume '>'
        }

        private void ParseCharacterData(StringBuilder? sb)
        {
            if (parserState is not TSReaderState.CharacterData) throw new NotImplementedException();
            // after ParseBeginning(), '<![CDATA[' is already parsed

            bool res;
            do
            {
                res = ParseCharacterDataPart(out int start, out int end);
                sb?.Append(charBuffer, start, end - start);
            }
            while (res && ReadBuffer() > 0);

            if (charPos >= charLength && ReadBuffer() is 0)
                throw new NotImplementedException(); // expected ']]>'
            if (!MatchString(charBuffer, charPos, "]]>"))
                throw new NotImplementedException(); // expected ']]>'
            charPos += 3; // consume ']]>'
        }
        private bool ParseCharacterDataPart(out int start, out int end)
        {
            int pos = charPos;
            int length = charLength;
            char[] buffer = charBuffer;
            start = pos;

            bool continueParsing = true;
            while (pos < length)
            {
                if (buffer[pos] is ']')
                {
                    if (pos + 1 >= length)
                    {
                        if (pos is 0) throw new NotImplementedException(); // expected a char after ']'
                        break;
                    }
                    if (buffer[pos + 1] is ']')
                    {
                        if (pos + 2 >= length)
                        {
                            if (pos is 0) throw new NotImplementedException(); // expected a char after ']]'
                            break;
                        }
                        if (buffer[pos + 2] is '>')
                        {
                            continueParsing = false;
                            break;
                        }
                    }
                }
                pos++;
            }
            end = pos;
            charPos = pos;
            return continueParsing;
        }

        private void ParseText(StringBuilder? sb)
        {
            if (parserState is not TSReaderState.Text) throw new NotImplementedException();
            // after ParseBeginning(), nothing is consumed

            bool res;
            do
            {
                res = ParseTextPart(out int start, out int end);
                sb?.Append(charBuffer, start, end - start);
            }
            while (res && ReadBuffer() > 0);
        }
        private bool ParseTextPart(out int start, out int end)
        {
            int pos = charPos;
            int length = charLength;
            char[] buffer = charBuffer;
            start = pos;

            bool continueParsing = true;
            while (pos < length)
            {
                char c = buffer[pos];
                if (c is '&')
                {
                    // Reference length is limited by total buffer length
                    if (!ParseReference(buffer, pos, ref length)) break;
                }
                else if (c is '<')
                {
                    continueParsing = false;
                    break;
                }
                pos++;
            }
            end = pos;
            charPos = pos;
            return continueParsing;
        }

        private void ParseComment(StringBuilder? sb)
        {
            if (parserState is not TSReaderState.Comment) throw new NotImplementedException();
            // after ParseBeginning(), '<!--' is already parsed

            bool res;
            do
            {
                res = ParseCommentPart(out int start, out int end);
                sb?.Append(charBuffer, start, end - start);
            }
            while (res && ReadBuffer() > 0);

            if (charPos >= charLength && ReadBuffer() is 0)
                throw new NotImplementedException(); // expected '-->'
            if (!MatchString(charBuffer, charPos, "-->"))
                throw new NotImplementedException(); // expected '-->'
            charPos += 3; // consume '-->'
        }
        private bool ParseCommentPart(out int start, out int end)
        {
            int pos = charPos;
            int length = charLength;
            char[] buffer = charBuffer;
            start = pos;

            bool continueParsing = true;
            while (pos < length)
            {
                if (buffer[pos] is '-')
                {
                    if (pos + 1 >= length)
                    {
                        if (pos is 0) throw new NotImplementedException(); // expected a char after '-'
                        break;
                    }
                    if (buffer[pos + 1] is '-')
                    {
                        if (pos + 2 >= length)
                        {
                            if (pos is 0) throw new NotImplementedException(); // expected a char after '--'
                            break;
                        }
                        if (buffer[pos + 2] is '>')
                        {
                            continueParsing = false;
                            break;
                        }
                    }
                }
                pos++;
            }
            end = pos;
            charPos = pos;
            return continueParsing;
        }

        private void ParseProcessingInstructionHeader()
        {
            if (parserState is not TSReaderState.ProcessingInstruction) throw new NotImplementedException();
            // after ParseBeginning(), '<?' is already parsed
            if (readHeader) return;

            // parse the target's name
            ParseName(stringBuilder);
            nodeName = stringBuilder.ToStringAndClear();

            readHeader = true;
        }
        private void ParseProcessingInstruction(StringBuilder? sb)
        {
            if (parserState is not TSReaderState.ProcessingInstruction) throw new NotImplementedException();
            // after ParseBeginning(), '<?' is already parsed
            if (!readHeader) ParseProcessingInstructionHeader();

            ParseWhitespaces(null);

            bool res;
            do
            {
                res = ParseProcessingInstructionPart(out int start, out int end);
                sb?.Append(charBuffer, start, end - start);
            }
            while (res && ReadBuffer() > 0);

            if (charPos >= charLength && ReadBuffer() is 0)
                throw new NotImplementedException(); // expected '?>'
            if (!MatchString(charBuffer, charPos, "?>"))
                throw new NotImplementedException(); // expected '?>'
            charPos += 2; // consume '?>'
        }
        private bool ParseProcessingInstructionPart(out int start, out int end)
        {
            int pos = charPos;
            int length = charLength;
            char[] buffer = charBuffer;
            start = pos;

            bool continueParsing = true;
            while (pos < length)
            {
                if (buffer[pos] is '?')
                {
                    if (pos + 1 >= length)
                    {
                        if (pos is 0) throw new NotImplementedException(); // expected a char after '?'
                        break;
                    }
                    if (buffer[pos + 1] is '>')
                    {
                        continueParsing = false;
                        break;
                    }
                }
                pos++;
            }
            end = pos;
            charPos = pos;
            return continueParsing;
        }

        public void Read()
        {
            if (parserState is not TSReaderState.Undefined) SkipData();
            ParseBeginning();
        }

        public bool ReadStartElement()
        {
            if (State is not TSReaderState.StartElement) throw new NotImplementedException();
            if (parserState is not TSReaderState.Undefined) SkipData();
            bool isEmpty = nodeIsEmpty;
            ParseBeginning();
            return !isEmpty;
        }
        public void ReadEndElement()
        {
            if (State is not TSReaderState.EndElement) throw new NotImplementedException();
            if (parserState is not TSReaderState.Undefined) SkipData();
            ParseBeginning();
        }

        public string ReadText()
        {
            if (State is not TSReaderState.Text) throw new NotImplementedException();
            string res;
            if (parserState is TSReaderState.Undefined)
                res = nodeValue!;
            else
            {
                ParseText(stringBuilder);
                res = stringBuilder.ToStringAndClear();
            }
            ParseBeginning();
            return res;
        }
        public void ReadText(StringBuilder? sb)
        {
            if (State is not TSReaderState.Text) throw new NotImplementedException();
            if (parserState is TSReaderState.Undefined)
                sb?.Append(nodeValue);
            else ParseText(sb);
            ParseBeginning();
        }

        public string ReadCharacterData()
        {
            if (State is not TSReaderState.CharacterData) throw new NotImplementedException();
            string res;
            if (parserState is TSReaderState.Undefined)
                res = nodeValue!;
            else
            {
                ParseCharacterData(stringBuilder);
                res = stringBuilder.ToStringAndClear();
            }
            ParseBeginning();
            return res;
        }
        public void ReadCharacterData(StringBuilder? sb)
        {
            if (State is not TSReaderState.CharacterData) throw new NotImplementedException();
            if (parserState is TSReaderState.Undefined)
                sb?.Append(nodeValue);
            else ParseCharacterData(sb);
            ParseBeginning();
        }

        public string ReadComment()
        {
            if (State is not TSReaderState.Comment) throw new NotImplementedException();
            string res;
            if (parserState is TSReaderState.Undefined)
                res = nodeValue!;
            else
            {
                ParseComment(stringBuilder);
                res = stringBuilder.ToStringAndClear();
            }
            ParseBeginning();
            return res;
        }
        public void ReadComment(StringBuilder? sb)
        {
            if (State is not TSReaderState.Comment) throw new NotImplementedException();
            if (parserState is TSReaderState.Undefined)
                sb?.Append(nodeValue);
            else ParseComment(sb);
            ParseBeginning();
        }

        public string ReadProcessingInstruction()
        {
            if (State is not TSReaderState.ProcessingInstruction) throw new NotImplementedException();
            string res;
            if (parserState is TSReaderState.Undefined)
                res = nodeValue!;
            else
            {
                ParseProcessingInstruction(stringBuilder);
                res = stringBuilder.ToStringAndClear();
            }
            ParseBeginning();
            return res;
        }
        public void ReadProcessingInstruction(StringBuilder? sb)
        {
            if (State is not TSReaderState.ProcessingInstruction) throw new NotImplementedException();
            if (parserState is TSReaderState.Undefined)
                sb?.Append(nodeValue);
            else ParseProcessingInstruction(sb);
            ParseBeginning();
        }

    }
    public readonly struct StoredAttribute
    {
        public StoredAttribute(string name, string? value)
        {
            Name = name;
            Value = value;
        }
        public readonly string Name;
        public readonly string? Value;
    }
    public enum TSReaderState
    {
        Undefined,
        Text,
        Whitespace,
        StartElement,
        EndElement,
        Comment,
        CharacterData,
        ProcessingInstruction,
        EndOfFile,
    }
}
