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
                Lexer l = new Lexer(args[0]);
                switch (args[1])
                {
                    case "-lexer":
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
                    case "-se":
                        Console.WriteLine(l.GetSimpleExpression());
                        break;
                    default:
                        Console.WriteLine("The program is not designed to work with this key.");
                        break;
                }
            }
            else if (args.Length == 1)
            {
                int testsCount;
                switch (args[0])
                {
                    case "-lexer-test":
                        testsCount = 60;
                        for (int i = 1; i <= testsCount; i++)
                        {
                            string dir = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
                            string inputPath = string.Format(dir + @"\tests\tests\lexer\input\{0}.txt", i);
                            string outputPath = string.Format(dir + @"\tests\tests\lexer\output\{0}.txt", i);
                            

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
                        break;
                    case "-se-test":
                        testsCount = 20;
                        for (int i = 1; i <= testsCount; i++)
                        {
                            string dir = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.FullName;
                            string outputPath = string.Format(dir + @"\tests\tests\se\output\{0}.txt", i);
                            string inputPath = string.Format(dir + @"\tests\tests\se\input\{0}.txt", i);

                            string output = File.ReadAllText(outputPath);
                            Lexer l = new Lexer(inputPath);
                            if (output == l.GetSimpleExpression()) Console.WriteLine(string.Format("Test {0} passed", i));
                            else Console.WriteLine(string.Format("Test {0} failed", i));
                        }
                        break;
                    default:
                        Console.WriteLine("The program is not designed to work with this key.");
                        break;
                }

            }
        
            else Console.WriteLine("Incorrect number of arguments entered.");
        }
    }
}
