using System;

namespace Lexer
{
	public class LexerEventArgs : EventArgs
	{
		public string Message { get; set; }

		public LexerEventArgs(string message)
		{
			Message = message;
		}
	}
}