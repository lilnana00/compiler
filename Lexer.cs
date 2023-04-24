using System;
using System.Globalization;
using System.IO;
using System.Collections.Generic;



public class Lexer
{
    public Lexer(string file)
    {
        stream = File.OpenText(file);
    }


    char[] delimiters = { ':', ';', ',', '(', ')', '[', ']'};
    char[] operartion_signs = { '+', '-', '*', '/', '=', '@', '^', '>', '<'};
    string[] keywords = {"abs", "absolute", "arctan", "array", "as", "asm", "begin", "boolean", "break",
        "case", "char", "class", "const", "constructor", "continue", "cos", "destructor", "dispose", "div", "do", "downto",
        "else", "end", "eof", "eoln", "except", "exp", "exports", "false", "file", "finalization","finally", "for", "function",
        "goto", "if", "implementation", "in", "inherited", "initialization", "inline", "input", "integer", "interface", "is",
        "label", "library", "ln", "maxint", "mod", "new", "nil", "object", "odd", "of", "on", "operator", "ord",
        "output", "pack", "packed", "page", "pred", "procedure", "program", "property", "raise", "read", "readln", "real",
        "record", "reintroduce", "repeat", "reset", "rewrite", "round", "self", "set", "shl", "shr", "sin", "sqr", "sqrt",
        "string", "succ", "text", "then", "threadvar", "to", "true", "trunc", "try", "type", "unit", "until", "uses", "var",
        "while", "with", "write", "writelnxor"};
    string[] operation_logic = { "or", "and", "xor", "not" };

    string hex_letters = "ABCDEFabcdef";
    string oct_digits = "01234567";

    public enum States
    {
        error,
        end_of_file,
        delimiter,
        operation_sign,
        operation_logic,
        keyword,
        identifier,
        str,
        integer,
        real,
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

    public string GetLexemType(States state)
    {
        switch(state)
        { 
            case States.end_of_file:
                return "EOF";
            case States.delimiter:
                return "Delimiter";
            case States.operation_sign:
                return "Operation sign";
            case States.operation_logic:
                return "Logic operation";
            case States.keyword:
                return "Keyword";
            case States.identifier:
                return "Identifier";
            case States.str:
                return "String";
            case States.integer:
                return "Integer";
            case States.real:
                return "Real";
            default:
                return "Error";
        }
    }

    public string GetLexemValue(States state)
    {
        switch (state)
        {
            case States.end_of_file:
                buffer = "EOF";
                return buffer;
            case States.delimiter:
                return buffer;
            case States.operation_sign:
                return buffer;
            case States.keyword:
                return buffer.ToLower();
            case States.operation_logic:
                return buffer.ToLower();
            case States.identifier:
                return buffer;
            case States.str:
                return buffer.Substring(1, buffer.Length - 2);
            case States.integer:
                return valueInt.ToString();
            case States.real:
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
    public bool IsLogical(string str)
    {
        foreach (string s in operation_logic)
        {
            if (str.ToLower().Equals(s)) return true;
        }
        return false;
    }

    private bool IsString()
    {
        if (currentChar.Equals('\''))
        {
            currentChar = stream.Read();
            buffer += (char)currentChar;
            while (!currentChar.Equals('\''))
            {
                currentChar = stream.Read();
                buffer += (char)currentChar;
                if (currentChar == -1)
                {
                    charCount += buffer.Length - 1;
                    errorMessage = string.Format("({0}, {1}): Missing closing quote.", lineCount, charCount);
                    return false;
                }
            }
            charCount += buffer.Length - 1;
            return true;
        }
        return false;
    }

    private bool IsBinInt()
    {
        if (currentChar.Equals('%'))
        {
            while (stream.Peek().Equals('0') || stream.Peek().Equals('1'))
            {
                currentChar = stream.Read();
                buffer += (char)currentChar;
            }

            try { valueInt = Convert.ToInt32(buffer.Remove(0, 1), 2); }
            /*catch (FormatException)
            {
                errorMessage = string.Format("({0}, {1}): The number does not match the binary notation.", lineCount, charCount);
            }*/
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
            while (oct_digits.Contains((char)stream.Peek()))
            {
                currentChar = stream.Read();
                buffer += (char)currentChar;
            }
            try { valueInt = Convert.ToInt32(buffer.Remove(0, 1), 8); }
            /*catch (FormatException)
            {
                errorMessage = string.Format("({0}, {1}): The number does not match the octal notation.", lineCount, charCount);
            }*/
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
            while (char.IsDigit((char)stream.Peek()) || stream.Peek() == '.')
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
        while (char.IsDigit((char)stream.Peek()))
        {
            currentChar = stream.Read();
            buffer += (char)currentChar;
        }
        try
        {
            valueReal = Convert.ToDouble(buffer, CultureInfo.InvariantCulture);
        }
        /*catch (FormatException)
        {
            errorMessage = string.Format("({0}, {1}): The number does not match the real notation.", lineCount, charCount);
        }*/
        catch (OverflowException)
        {
            errorMessage = string.Format("({0}, {1}): Overflow in string to real conversion.", lineCount, charCount);
        }
        if(double.IsNaN(valueReal) || double.IsInfinity(valueReal))
            errorMessage = string.Format("({0}, {1}): NaN or Infinity.", lineCount, charCount);
        charCount += buffer.Length - 1;
    }
    private bool IsHexInt()
    {
        if (currentChar.Equals('$'))
        {
            while (char.IsDigit((char)stream.Peek()) || hex_letters.Contains((char)stream.Peek()))
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

            while (!currentChar.Equals('}'))
            {
                currentChar = stream.Read();
                charCount++;
                if (currentChar.Equals('\n'))
                {
                    lineCount++;
                    charCount = 0;
                }
                if (currentChar == -1) return true;
            }
            return true;
        }
        return false;
    }

    public States GetLexemState()
    {
        if (currentChar == -1) return States.end_of_file;
        
        buffer = "";
        valueInt = -1;
        valueReal = -1.0;

        NextLexem();

        if (currentChar == -1) return States.end_of_file;

        if (SkipSingleLineComment() || SkipMultiLineComment()) return GetLexemState();

        buffer += (char)currentChar;

        if (IsDelimiter((char)currentChar)) return States.delimiter;

        if (IsOperationSign((char)currentChar)) return States.operation_sign;

        if (IsIdentifier())
        {
            if (IsKeyword(buffer)) return States.keyword;
            else if (IsLogical(buffer)) return States.operation_logic;
            else return States.identifier;
        }

        if (IsString()) return States.str;

        if ((IsBinInt() || IsOctInt() || IsHexInt()) && errorMessage == "") return States.integer;

        if (errorMessage == "" && IsIntOrReal() && errorMessage == "")
        {
            if (valueInt == -1)
            {
                ReadReal();
                if (errorMessage == "") return States.real;
            }
            if (errorMessage == "") return States.integer;
        }

        if (errorMessage == "") errorMessage = string.Format("({0}, {1}): Unexpected lexem.", lineCount, charCount);
        return States.error;
    }

    public string[] GetLexem()
    {
        States state = GetLexemState();
        if (state == States.error) return new string[1] { errorMessage };
        return new string[5] { lineCount.ToString(), GetCharInd().ToString(), GetLexemType(state), GetLexemValue(state), buffer};
    }


    private List<int> GetInflectionPoint(List<string> expr)
    {
        int braces = 0;
        int maxPriority = int.MaxValue, index = 0;
        string signsStr = "+-*/=";
        Dictionary<string, int> priority = new Dictionary<string, int>()
        {
            { "=", 1 },
            { "+", 2 },
            { "-", 2 },
            { "*", 3 },
            { "/", 3 }
        };
        for (int i = 0; i < expr.Count; i++)
        {
            if (expr[i] == "(") braces++;
            else if (expr[i] == ")") braces--;
            else if (signsStr.Contains(expr[i]))
            {
                if (priority[expr[i]] + 3 * braces <= maxPriority)
                {
                    maxPriority = priority[expr[i]] + 3 * braces;
                    index = i;
                }
            }
        }
        int extraBraces = (maxPriority - 1) / 3;
        return new List<int>() { index, extraBraces };
    }

    private string GetSimpleExpression(List<string> expr, int depth)
    {
        string ans = "";
        for (int i = 0; i < depth; i++) ans += "\t";
        if (expr.Count == 1) return ans + expr[0];
        List<int> infl = GetInflectionPoint(expr);
        if (infl[0] == 0) return ans + expr[expr.Count / 2];
        List<string> leftExpr = new List<string>(), rightExpr = new List<string>();
        leftExpr.AddRange(expr.GetRange(infl[1], infl[0] - infl[1]));
        rightExpr.AddRange(expr.GetRange(infl[0] + 1, expr.Count - 1 - infl[0] - infl[1]));
        ans += expr[infl[0]] + "\n\n" + GetSimpleExpression(leftExpr, depth + 1) + "\n\n" + GetSimpleExpression(rightExpr, depth + 1);
        return ans;
    }

    public string GetSimpleExpression()
    {
        int openBraces = 0;
        List<string> expression = new List<string>();
        bool haveEquals = false;
        bool isSignOpen = true;
        while (true)
        {
            States state = GetLexemState();
            if (state == States.end_of_file) break;
            if (state == States.error) return errorMessage;
            string lexem = GetLexemValue(state);
            if (state == States.integer || state == States.real || state == States.identifier ||
                lexem == "+" || lexem == "-" || lexem == "*" || lexem == "/" || lexem == "(" || lexem == ")" || lexem == "=")
            {
                expression.Add(lexem);
            }
            else
            {
                errorMessage = string.Format("({0}, {1}): Wrong lexem for simple expressions.", lineCount, charCount);
                return errorMessage;
            }

           /*
            if (lexem == "=")
            {
                if (haveEquals) return errorMessage = string.Format("({0}, {1}): Too much equals", lineCount, charCount);
                haveEquals = true;
            }
           */
            

            if (lexem == "+" || lexem == "-" || lexem == "*" || lexem == "/" || lexem == "=")
            {
                if (isSignOpen)
                {
                    errorMessage = string.Format("({0}, {1}): The expression doesn't have a left term.", lineCount, charCount);
                    return errorMessage;
                }
                isSignOpen = true;
            }
            else if (lexem == "(")
            {
                openBraces++;
                isSignOpen = true;
            }
            else if (lexem == ")")
            {
                if (isSignOpen)
                {
                    errorMessage = string.Format("({0}, {1}): The expression doesn't have a right term.", lineCount, charCount);
                    return errorMessage;
                }
                openBraces--;
                if (openBraces < 0)
                   return errorMessage = string.Format("({0}, {1}): The expression has extra closing braces.", lineCount, charCount);

                isSignOpen = false;
            }
            else
            {
                if (!isSignOpen)
                {
                    errorMessage = string.Format("({0}, {1}): The expression doesn't have a operation sign.", lineCount, charCount);
                    return errorMessage;
                }
                isSignOpen = false;
            }
        }

        if (isSignOpen)
            errorMessage = string.Format("({0}, {1}): The expression doesn't have a right term.", lineCount, charCount);
        if (openBraces > 0)
            errorMessage = string.Format("({0}, {1}): The expression has unclosed braces.", lineCount, charCount);
        if (errorMessage != "") return errorMessage;
        return GetSimpleExpression(expression, 0);
    }
}
