/*
 * Minimal JSON parser for Unity.
 * Source: https://gist.github.com/darktable/1411710 (MIT)
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

public static class MiniJSON
{
    public static object Deserialize(string json)
    {
        if (string.IsNullOrEmpty(json)) return null;
        json = NormalizeJson(json);
        return Parser.Parse(json);
    }

    private static string NormalizeJson(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return json;
        }

        var cleaned = json.Replace("\u0000", string.Empty);

        var startIndex = FindJsonStart(cleaned);
        if (startIndex > 0 && startIndex < cleaned.Length)
        {
            cleaned = cleaned.Substring(startIndex);
        }

        if (cleaned.Length > 0 && cleaned[0] == '\uFEFF')
        {
            cleaned = cleaned.Substring(1);
        }

        return cleaned.TrimEnd('\u0000', '\uFEFF');
    }

    private static int FindJsonStart(string json)
    {
        for (int i = 0; i < json.Length; i++)
        {
            var c = json[i];
            if (c == '{' || c == '[' || c == '"' || c == '-' || char.IsDigit(c))
            {
                return i;
            }
        }

        return 0;
    }

    public static string Serialize(object obj)
    {
        return Serializer.Serialize(obj);
    }

    private sealed class Parser : IDisposable
    {
        private const string WORD_BREAK = "{}[],:\"";

        private enum TOKEN
        {
            NONE,
            CURLY_OPEN,
            CURLY_CLOSE,
            SQUARED_OPEN,
            SQUARED_CLOSE,
            COLON,
            COMMA,
            STRING,
            NUMBER,
            TRUE,
            FALSE,
            NULL
        };

        private StringReader json;

        private Parser(string jsonString)
        {
            json = new StringReader(jsonString);
        }

        public static object Parse(string jsonString)
        {
            using (var instance = new Parser(jsonString))
            {
                return instance.ParseValue();
            }
        }

        public void Dispose()
        {
            json.Dispose();
            json = null;
        }

        private Dictionary<string, object> ParseObject()
        {
            var table = new Dictionary<string, object>();

            json.Read(); // {

            while (true)
            {
                switch (NextToken)
                {
                    case TOKEN.NONE:
                        return null;
                    case TOKEN.COMMA:
                        continue;
                    case TOKEN.CURLY_CLOSE:
                        return table;
                    default:
                        // name
                        string name = ParseString();
                        if (name == null)
                        {
                            return null;
                        }

                        // :
                        if (NextToken != TOKEN.COLON)
                        {
                            return null;
                        }
                        // Skip the colon
                        json.Read();

                        // value
                        table[name] = ParseValue();
                        break;
                }
            }
        }

        private List<object> ParseArray()
        {
            var array = new List<object>();

            json.Read(); // [

            var parsing = true;
            while (parsing)
            {
                TOKEN nextToken = NextToken;

                switch (nextToken)
                {
                    case TOKEN.NONE:
                        return null;
                    case TOKEN.COMMA:
                        continue;
                    case TOKEN.SQUARED_CLOSE:
                        parsing = false;
                        break;
                    default:
                        object value = ParseValue();
                        array.Add(value);
                        break;
                }
            }

            return array;
        }

        private object ParseValue()
        {
            switch (NextToken)
            {
                case TOKEN.STRING:
                    return ParseString();
                case TOKEN.NUMBER:
                    return ParseNumber();
                case TOKEN.CURLY_OPEN:
                    return ParseObject();
                case TOKEN.SQUARED_OPEN:
                    return ParseArray();
                case TOKEN.TRUE:
                    return true;
                case TOKEN.FALSE:
                    return false;
                case TOKEN.NULL:
                    return null;
                default:
                    return null;
            }
        }

        private string ParseString()
        {
            var s = new StringBuilder();
            char c;

            json.Read(); // "

            bool parsing = true;
            while (parsing)
            {
                if (json.Peek() == -1)
                {
                    parsing = false;
                    break;
                }

                c = NextChar;
                switch (c)
                {
                    case '"':
                        parsing = false;
                        break;
                    case '\\':
                        if (json.Peek() == -1)
                        {
                            parsing = false;
                            break;
                        }

                        c = NextChar;
                        switch (c)
                        {
                            case '"':
                            case '\\':
                            case '/':
                                s.Append(c);
                                break;
                            case 'b':
                                s.Append('\b');
                                break;
                            case 'f':
                                s.Append('\f');
                                break;
                            case 'n':
                                s.Append('\n');
                                break;
                            case 'r':
                                s.Append('\r');
                                break;
                            case 't':
                                s.Append('\t');
                                break;
                            case 'u':
                                var hex = new char[4];
                                for (int i = 0; i < 4; i++)
                                {
                                    hex[i] = NextChar;
                                }
                                s.Append((char)Convert.ToInt32(new string(hex), 16));
                                break;
                        }
                        break;
                    default:
                        s.Append(c);
                        break;
                }
            }

            return s.ToString();
        }

        private object ParseNumber()
        {
            string number = NextWord;

            if (number.IndexOf('.') == -1 && number.IndexOf('e') == -1 && number.IndexOf('E') == -1)
            {
                long parsedInt;
                long.TryParse(number, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsedInt);
                return parsedInt;
            }

            double parsedDouble;
            double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out parsedDouble);
            return parsedDouble;
        }

        private void EatWhitespace()
        {
            while (char.IsWhiteSpace(PeekChar))
            {
                json.Read();
                if (json.Peek() == -1) break;
            }
        }

        private char PeekChar
        {
            get
            {
                var peek = json.Peek();
                return peek == -1 ? '\0' : Convert.ToChar(peek);
            }
        }

        private char NextChar
        {
            get
            {
                var next = json.Read();
                return next == -1 ? '\0' : Convert.ToChar(next);
            }
        }

        private string NextWord
        {
            get
            {
                var builder = new StringBuilder();

                while (!IsWordBreak(PeekChar))
                {
                    builder.Append(NextChar);
                    if (json.Peek() == -1)
                    {
                        break;
                    }
                }

                return builder.ToString();
            }
        }

        private TOKEN NextToken
        {
            get
            {
                EatWhitespace();

                if (json.Peek() == -1)
                {
                    return TOKEN.NONE;
                }

                switch (PeekChar)
                {
                    case '{':
                        return TOKEN.CURLY_OPEN;
                    case '}':
                        json.Read();
                        return TOKEN.CURLY_CLOSE;
                    case '[':
                        return TOKEN.SQUARED_OPEN;
                    case ']':
                        json.Read();
                        return TOKEN.SQUARED_CLOSE;
                    case ',':
                        json.Read();
                        return TOKEN.COMMA;
                    case '"':
                        return TOKEN.STRING;
                    case ':':
                        return TOKEN.COLON;
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                    case '-':
                        return TOKEN.NUMBER;
                }

                string word = NextWord;

                switch (word)
                {
                    case "false":
                        return TOKEN.FALSE;
                    case "true":
                        return TOKEN.TRUE;
                    case "null":
                        return TOKEN.NULL;
                }

                return TOKEN.NONE;
            }
        }

        private static bool IsWordBreak(char c)
        {
            return c == '\0' || char.IsWhiteSpace(c) || WORD_BREAK.IndexOf(c) != -1;
        }
    }

    private sealed class Serializer
    {
        private readonly StringBuilder builder;

        private Serializer()
        {
            builder = new StringBuilder();
        }

        public static string Serialize(object obj)
        {
            var instance = new Serializer();

            instance.SerializeValue(obj);

            return instance.builder.ToString();
        }

        private void SerializeValue(object value)
        {
            if (value == null)
            {
                builder.Append("null");
            }
            else if (value is string)
            {
                SerializeString((string)value);
            }
            else if (value is bool)
            {
                builder.Append((bool)value ? "true" : "false");
            }
            else if (value is IDictionary)
            {
                SerializeObject((IDictionary)value);
            }
            else if (value is IList)
            {
                SerializeArray((IList)value);
            }
            else if (value is char)
            {
                SerializeString(new string((char)value, 1));
            }
            else
            {
                SerializeOther(value);
            }
        }

        private void SerializeObject(IDictionary obj)
        {
            bool first = true;

            builder.Append('{');

            foreach (object e in obj.Keys)
            {
                if (!first)
                {
                    builder.Append(',');
                }

                SerializeString(e.ToString());
                builder.Append(':');

                SerializeValue(obj[e]);

                first = false;
            }

            builder.Append('}');
        }

        private void SerializeArray(IList anArray)
        {
            builder.Append('[');

            bool first = true;

            foreach (object obj in anArray)
            {
                if (!first)
                {
                    builder.Append(',');
                }

                SerializeValue(obj);
                first = false;
            }

            builder.Append(']');
        }

        private void SerializeString(string str)
        {
            builder.Append('"');

            char[] charArray = str.ToCharArray();
            foreach (var c in charArray)
            {
                switch (c)
                {
                    case '"':
                        builder.Append("\\\"");
                        break;
                    case '\\':
                        builder.Append("\\\\");
                        break;
                    case '\b':
                        builder.Append("\\b");
                        break;
                    case '\f':
                        builder.Append("\\f");
                        break;
                    case '\n':
                        builder.Append("\\n");
                        break;
                    case '\r':
                        builder.Append("\\r");
                        break;
                    case '\t':
                        builder.Append("\\t");
                        break;
                    default:
                        builder.Append(c);
                        break;
                }
            }

            builder.Append('"');
        }

        private void SerializeOther(object value)
        {
            if (value is float || value is double || value is decimal)
            {
                builder.Append(Convert.ToString(value, CultureInfo.InvariantCulture));
            }
            else
            {
                builder.Append(Convert.ToString(value, CultureInfo.InvariantCulture));
            }
        }
    }

    private sealed class StringReader : IDisposable
    {
        private readonly string json;
        private int index;

        public StringReader(string json)
        {
            this.json = json;
        }

        public int Peek()
        {
            if (index >= json.Length)
            {
                return -1;
            }

            return json[index];
        }

        public int Read()
        {
            if (index >= json.Length)
            {
                return -1;
            }

            return json[index++];
        }

        public void Dispose()
        {
        }
    }
}

