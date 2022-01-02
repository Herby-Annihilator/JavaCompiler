using System;

namespace Compiler
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