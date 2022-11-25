using System;
using System.Text;
using System.Globalization;
using System.IO;


namespace Compiler
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 2)
            {
                switch (args[1])
                {
                    case "-la":
                        Lexer l = new Lexer(args[0]);
                        string[] lexem;
                        string answer = "";
                        do
                        {
                            lexem = l.GetLexem();
                            if (lexem.Length > 1) answer += lexem[0] + " " + lexem[1] + " " + lexem[2] + " " + lexem[3] + " " + lexem[4] + '\n';
                            else answer = lexem[0];
                        }
                        while (lexem.Length > 1 && lexem[3] != "EOF");
                        Console.WriteLine(answer.Trim());
                        break;
                    default:
                        Console.WriteLine("The program is not designed to work with this key.");
                        break;
                }
            }
            else if (args.Length == 1 && args[0] == "-test")
            {
                const int testsCount = 60;
                for (int i = 1; i <= testsCount; i++)
                {
                    string dir = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
                    string outputPath = string.Format(dir + @"\tests\output\{0}.txt", i);
                    string inputPath = string.Format(dir + @"\tests\input\{0}.txt", i);

                    string output = File.ReadAllText(outputPath);
                    Lexer l = new Lexer(inputPath);
                    string[] lexem;
                    string answer = "";
                    do
                    {
                        lexem = l.GetLexem();
                        if (lexem.Length > 1) answer += lexem[0] + " " + lexem[1] + " " + lexem[2] + " " + lexem[3] + " " + lexem[4] + '\n';
                        else answer = lexem[0];
                    }
                    while (lexem.Length > 1 && lexem[3] != "EOF");

                    if (output.Equals(answer.Trim())) Console.WriteLine(string.Format("Test {0} passed", i));
                    else Console.WriteLine(string.Format("Test {0} failed", i));
                }
            }
            /*else if (args.Length == 1 && args[0] == "-testcreate")
            {
                const int testsCount = 60;
                for (int i = 1; i <= testsCount; i++)
                {
                    string dir = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
                    string outputPath = string.Format(dir + @"\tests\output\{0}.txt", i);
                    string inputPath = string.Format(dir + @"\tests\input\{0}.txt", i);

                    FileStream output = File.OpenWrite(outputPath);
                    Lexer l = new Lexer(inputPath);
                    string[] lexem;
                    string answer = "";
                    do
                    {
                        lexem = l.GetLexem();
                        if (lexem.Length > 1) answer += lexem[0] + " " + lexem[1] + " " + lexem[2] + " " + lexem[3] + " " + lexem[4] + '\n';
                        else answer = lexem[0];
                    }
                    while (lexem.Length > 1 && lexem[3] != "EOF");
                    Console.WriteLine(answer);
                    byte[] buffer = Encoding.Default.GetBytes(answer.Trim());
                    output.Write(buffer, 0, buffer.Length);
                    output.Close();
                }
            }*/
            else Console.WriteLine("Incorrect number of arguments entered.");
        }
    }
}
