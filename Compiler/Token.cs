using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
	public class Token
	{
		public Lexemes Lexeme { get; set; } = Lexemes.TypeEnd;
		public string Value { get; set; } = "";
	}
}
