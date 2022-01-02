using Compiler;
using Lexer;
using System;
using System.IO;
using System.Text;

namespace JavaCompilerTest
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				FileStream stream = new FileStream(@"D:\GarbageCan\Projects\JavaCompiler\JavaCompilerTest\lexemes.txt", FileMode.Open, FileAccess.Read);
				JavaLexer lexer = new JavaLexer();
				lexer.Text = StreamToString(stream);
				Token token;

				while ((token = lexer.GetNextToken()).Lexeme != Lexemes.TypeEnd)
				{
					if (token.Lexeme == Lexemes.TypeError)
					{
						Console.WriteLine($"Ошибка лексера: '{token.Value}'");
					}
					else
					{
						Console.WriteLine($"Отсканирована лексема: '{token.Value}'");
					}
				}

				stream.Close();
			}
			catch(Exception e)
			{
				Console.WriteLine("\r\n\r\n\\------------------Error------------------------/");
				Console.WriteLine(e.Message);
				Console.WriteLine("\\-----------------------------------------------/");
			}
		}


		private static string StreamToString(FileStream stream)
		{
			StringBuilder builder = new StringBuilder((int)stream.Length);
			char symbol;
			for (int i = 0; i < stream.Length; i++)
			{
				symbol = (char)stream.ReadByte();
				builder.Append(symbol);
			}
			return builder.ToString();
		}
	}
}
