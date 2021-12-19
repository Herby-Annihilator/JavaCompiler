using Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexer
{
	internal abstract class State
	{
		protected JavaLexer lexer;

		internal State(JavaLexer lexer)
		{
			this.lexer = lexer;
		}

		internal abstract void CorrectToken(Token token);
	}
}
