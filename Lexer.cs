using System;
using System.Globalization;
using System.IO;


public class Lexer
{
    public Lexer(string file)
    {
        stream = File.OpenText(file);
    }

    char[] delimiters = { ':', ';', ',', '(', ')', '[', ']'};
    char[] operartion_signs = { '+', '-', '*', '/', '=', '@', '^', '>', '<'};
    string[] keywords = {"abs", "absolute", "and", "arctan", "array", "as", "asm", "begin", "boolean", "break",
        "case", "char", "class", "const", "constructor", "continue", "cos", "destructor", "dispose", "div", "do", "downto",
        "else", "end", "eof", "eoln", "except", "exp", "exports", "false", "file", "finalization","finally", "for", "function",
        "goto", "if", "implementation", "in", "inherited", "initialization", "inline", "input", "integer", "interface", "is",
        "label", "library", "ln", "maxint", "mod", "new", "nil", "not", "object", "odd", "of", "on", "operator", "or", "ord",
        "output", "pack", "packed", "page", "pred", "procedure", "program", "property", "raise", "read", "readln", "real",
        "record", "reintroduce", "repeat", "reset", "rewrite", "round", "self", "set", "shl", "shr", "sin", "sqr", "sqrt",
        "string", "succ", "text", "then", "threadvar", "to", "true", "trunc", "try", "type", "unit", "until", "uses", "var",
        "while", "with", "write", "writelnxor", "xor"};

    enum States
    {
        error = -1,
        end_of_file = 0,
        delimiter = 1,
        operation_sign = 2,
        keyword = 3,
        identifier = 4,
        str = 5,
        integer = 6,
        real = 7,
    };

    StreamReader stream;
    int charCount = 0;
    int lineCount = 1;
    string buffer = "";
    string errorMessage = "";
    int valueInt = -1;
    double valueReal = -1.0;
    int currentChar = ' ';

    private int GetCharInd()
    {
        return buffer.Length > 0 ? charCount - buffer.Length + 1 : charCount;
    }

    private void NextLexem()
    {
        do
        {
            if (currentChar.Equals('\n'))
            {
                lineCount++;
                charCount = 0;
            }
            currentChar = stream.Read();
            charCount++;
        }
        while (currentChar != -1 && char.IsWhiteSpace((char)currentChar));
    }

    public string GetLexemType(int state)
    {
        switch(state)
        { 
            case (int)States.end_of_file:
                return "EOF";
            case (int)States.delimiter:
                return "Delimiter";
            case (int)States.operation_sign:
                return "Operation sign";
            case (int)States.keyword:
                return "Keyword";
            case (int)States.identifier:
                return "Identifier";
            case (int)States.str:
                return "String";
            case (int)States.integer:
                return "Integer";
            case (int)States.real:
                return "Real";
            default:
                return "Error";
        }
    }

    public string GetLexemValue(int state)
    {
        switch (state)
        {
            case (int)States.end_of_file:
                buffer = "EOF";
                return buffer;
            case (int)States.delimiter:
                return buffer;
            case (int)States.operation_sign:
                return buffer;
            case (int)States.keyword:
                return buffer.ToLower();
            case (int)States.identifier:
                return buffer;
            case (int)States.str:
                return buffer.Substring(1, buffer.Length - 2);
            case (int)States.integer:
                return valueInt.ToString();
            case (int)States.real:
                return valueReal.ToString().Replace(',', '.');
            default:
                return "Error";
        }
    }

    public bool IsDelimiter(char c)
    {
        foreach (char ch in delimiters)
        {
            if (c == ch) return true;
        }
        return false;
    }

    public bool IsOperationSign(char c)
    {
        foreach (char ch in operartion_signs)
        {
            if (c == ch) return true;
        }
        return false;
    }

    private bool IsIdentifier()
    {
        if (char.IsLetter((char)currentChar) || currentChar == '_')
        {
            while (char.IsLetterOrDigit((char)stream.Peek()) || stream.Peek().Equals('_'))
            {
                currentChar = stream.Read();
                buffer += (char)currentChar;
            }
            charCount += buffer.Length - 1;
            return true;
        }
        return false;
    }

    public bool IsKeyword(string str)
    {
        foreach (string s in keywords)
        {
            if (str.ToLower().Equals(s)) return true;
        }
        return false;
    }

    private bool IsString()
    {
        if (currentChar.Equals('\''))
        {
            while (!(currentChar = stream.Read()).Equals('\''))
            {
                buffer += (char)currentChar;
                if (currentChar == -1)
                {
                    charCount += buffer.Length - 1;
                    errorMessage = string.Format("({0}, {1}): String error: missing closing quote.", lineCount, charCount);
                    return false;
                }
            }
            buffer += (char)currentChar;
            charCount += buffer.Length - 1;
            return true;
        }
        return false;
    }

    private bool IsBinInt()
    {
        if (currentChar.Equals('%'))
        {
            while (char.IsLetterOrDigit((char)stream.Peek()))
            {
                currentChar = stream.Read();
                buffer += (char)currentChar;
            }
            try { valueInt = Convert.ToInt32(buffer.Remove(0, 1), 2); }
            catch (FormatException)
            {
                errorMessage = string.Format("({0}, {1}): The number does not match the binary notation.", lineCount, charCount);
            }
            catch (OverflowException)
            {
                errorMessage = string.Format("({0}, {1}): Overflow in string to int conversion.", lineCount, charCount);
            }
            charCount += buffer.Length - 1;
            return true;
        }
        return false;
    }
    private bool IsOctInt()
    {
        if (currentChar.Equals('&'))
        {
            while (char.IsLetterOrDigit((char)stream.Peek()))
            {
                currentChar = stream.Read();
                buffer += (char)currentChar;
            }
            try { valueInt = Convert.ToInt32(buffer.Remove(0, 1), 8); }
            catch (FormatException)
            {
                errorMessage = string.Format("({0}, {1}): The number does not match the octal notation.", lineCount, charCount);
            }
            catch (OverflowException)
            {
                errorMessage = string.Format("({0}, {1}): Overflow in string to int conversion.", lineCount, charCount);
            }
            charCount += buffer.Length - 1;
            return true;
        }
        return false;
    }
    private bool IsIntOrReal()
    {
        if (char.IsDigit((char)currentChar))
        {
            bool isReal = false;
            while (char.IsLetterOrDigit((char)stream.Peek()) || stream.Peek() == '.')
            {
                currentChar = stream.Read();
                buffer += (char)currentChar;
                if (currentChar == '.')
                {
                    isReal = true;
                    break;
                }
            }
            if (isReal) return true;
            try
            {
                valueInt = Convert.ToInt32(buffer);
            }
            catch (FormatException)
            {
                errorMessage = string.Format("({0}, {1}): The number does not match the normal notation.", lineCount, charCount);
            }
            catch (OverflowException)
            {
                errorMessage = string.Format("({0}, {1}): Overflow in string to int conversion.", lineCount, charCount);
            }
            charCount += buffer.Length - 1;
            return true;
        }
        return false;
    }

    private void ReadReal()
    {
        while (char.IsLetterOrDigit((char)stream.Peek()))
        {
            currentChar = stream.Read();
            buffer += (char)currentChar;
            if(currentChar == 'e' || currentChar == 'E')
            {
                currentChar = stream.Read();
                buffer += (char)currentChar;
            }
        }
        try
        {
            valueReal = Convert.ToDouble(buffer, CultureInfo.InvariantCulture);
        }
        catch (FormatException)
        {
            errorMessage = string.Format("({0}, {1}): The number does not match the real notation.", lineCount, charCount);
        }
        catch (OverflowException)
        {
            errorMessage = string.Format("({0}, {1}): Overflow in string to real conversion.", lineCount, charCount);
        }
        if(double.IsNaN(valueReal) || double.IsInfinity(valueReal))
            errorMessage = string.Format("({0}, {1}): Overflow in string to real conversion.", lineCount, charCount);
        charCount += buffer.Length - 1;
    }
    private bool IsHexInt()
    {
        if (currentChar.Equals('$'))
        {
            while (char.IsLetterOrDigit((char)stream.Peek()))
            {
                currentChar = stream.Read();
                buffer += (char)currentChar;
            }
            try { valueInt = Convert.ToInt32(buffer.Remove(0, 1), 16); }
            catch (FormatException)
            {
                errorMessage = string.Format("({0}, {1}): The number does not match the hexadecimal notation.", lineCount, charCount);
            }
            catch (OverflowException)
            {
                errorMessage = string.Format("({0}, {1}): Overflow in string to int conversion.", lineCount, charCount);
            }
            charCount += buffer.Length - 1;
            return true;
        }
        return false;
    }
    private bool SkipSingleLineComment()
    {
        if (currentChar.Equals('/'))
        {
            if (stream.Peek().Equals('/'))
            {
                do
                {
                    currentChar = stream.Read();
                    charCount++;
                } while (!stream.Peek().Equals('\n') && stream.Peek() != -1);
                return true;
            }
        }
        return false;
    }

    private bool SkipMultiLineComment()
    {
        if (currentChar.Equals('{'))
        {
            while (!(currentChar = stream.Read()).Equals('}'))
            {
                charCount++;
                if (currentChar.Equals('\n'))
                {
                    lineCount++;
                    charCount = 0;
                }
                if (currentChar == -1) return true;
            }
            charCount++;
            return true;
        }
        return false;
    }

    public int GetLexemState()
    {
        if (currentChar == -1) return (int)States.end_of_file;
        
        buffer = "";
        valueInt = -1;
        valueReal = -1.0;

        NextLexem();

        if (currentChar == -1) return (int)States.end_of_file;

        if (SkipSingleLineComment() || SkipMultiLineComment()) return GetLexemState();

        buffer += (char)currentChar;

        if (IsDelimiter((char)currentChar)) return (int)States.delimiter;

        if (IsOperationSign((char)currentChar)) return (int)States.operation_sign;

        if (IsIdentifier())
        {
            if (IsKeyword(buffer)) return (int)States.keyword;
            return (int)States.identifier;
        }

        if (IsString()) return (int)States.str;

        if ((IsBinInt() || IsOctInt() || IsHexInt()) && errorMessage == "") return (int)States.integer;

        if (errorMessage == "" && IsIntOrReal() && errorMessage == "")
        {
            if (valueInt == -1)
            {
                ReadReal();
                if (errorMessage == "") return (int)States.real;
            }
            if (errorMessage == "") return (int)States.integer;
        }

        if (errorMessage == "") errorMessage = string.Format("({0}, {1}): Unexpected lexem.", lineCount, charCount);
        return (int)States.error;
    }

    public string[] GetLexem()
    {
        int state = GetLexemState();
        if (state == (int)States.error) return new string[1] { errorMessage };
        return new string[5] { lineCount.ToString(), GetCharInd().ToString(), GetLexemType(state), GetLexemValue(state), buffer};
    }
}
