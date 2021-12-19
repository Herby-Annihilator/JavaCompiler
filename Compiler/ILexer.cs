using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
	public interface ILexer
	{
		int CurrentRow { get; }
		int CurrentColumn { get; }
		string Text { get; set; }
		int Position { get; set; }  // при ручной установке, свойства CurrentRow и CurrentColumn будут сброшены в 0
		Token GetNextToken();
	}
}
