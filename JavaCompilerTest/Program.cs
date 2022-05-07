using Compiler;
using Lexer;
using SynaxAnalyzer;
using System;
using System.IO;
using System.Text;

namespace JavaCompilerTest
{
	class Program
	{
		string str = StreamToString(new FileStream("", FileMode.Open, FileAccess.Read));
		static void Main(string[] args)
		{
			try
			{
				FileStream stream = new FileStream(@"C:\Users\Rukin\source\repos\JavaCompiler\JavaCompilerTest\lab_4.txt", FileMode.Open, FileAccess.Read);
				JavaLexer lexer = new JavaLexer();
				lexer.Text = StreamToString(stream);
				stream.Close();
				JavaSyntaxAnalyzer analyzer = new JavaSyntaxAnalyzer(lexer);
				analyzer.Program();
				Console.WriteLine("Ошибок не выявлено");
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
