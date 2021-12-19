using Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lexer
{
	public class JavaLexer : ILexer
	{
		private int _currentRow;
		private int _currentColumn;
		internal int _position;
		private string _text;
		public int CurrentRow { get => _currentRow; private set => _currentRow = value; }
		public int CurrentColumn { get => _currentColumn; private set => _currentColumn = value; }
		public string Text { get => _text; set => _text = value; }
		public int Position // при ручной установке, свойства CurrentRow и CurrentColumn будут сброшены в 0
		{ 
			get => _position;
			set
			{
				_position = value;
				CurrentColumn = 0;
				CurrentRow = 0;
			}
		} 

		private char _symbol;
		private Token _token = new Token();

		public Token GetNextToken()
		{
			SkipIgnorableSymbols();
			CorrectToken();
			return _token;
		}

		private bool SkipIgnorableSymbols()
		{
			while (_position < Text.Length)
			{
				_symbol = Text[_position];
				if (_symbol == '\t')
				{
					CurrentColumn += 4;
				}
				else if (_symbol == '\r')
				{
					CurrentColumn = 0;
				}
				else if (_symbol == ' ')
				{
					CurrentColumn++;
				}
				else if (_symbol == '\n')
				{
					CurrentRow++;
				}
				else if (_symbol == '/') // начался комментарий или операция деления
				{
					CurrentColumn++;
					_position++;
					if (_position < Text.Length)
					{
						_symbol = Text[_position];
						CurrentColumn++;
						if (_symbol == '/')  // комментарий начинается на // - нужно пропустить все до \n
						{
							SkipSimpleComment();  // установит _position, соответствующий \n, нужно читать дальше
						}
						else if (_symbol == '*') // это комментарий типа /**/
						{
							SkipDifficultComment();  // установит _position соответствующий '/', нужно читать дальше
						}
						else  // это операция деления, будет обработана дальше
						{
							return true;
						}
					}
					
				}
				else
				{
					return true;
				}
				_position++;
			}
			return false;
		}

		private bool SkipSimpleComment()
		{
			while (_position < Text.Length)
			{
				_symbol = Text[_position];
				if (_symbol == '\t')
					CurrentColumn += 4;
				else if (_symbol == '\r')
					CurrentColumn = 0;
				else if (_symbol == '\n')
				{
					return true;
				}
				else
					CurrentColumn++;
				_position++;
			}
			return false;
		}

		private bool SkipDifficultComment()
		{
			while (_position < Text.Length)
			{
				_symbol = Text[_position];
				if (_symbol == '\t')
					CurrentColumn += 4;
				else if (_symbol == '\r')
					CurrentColumn = 0;
				else if (_symbol == '\n')
					CurrentRow++;
				else if (_symbol == '*')
				{
					_position++;
					while (_position < Text.Length)
					{
						_symbol = Text[_position];
						if (_symbol == '/')  // конец комментария
						{
							CurrentColumn++;
							return true;
						}
						else if (_symbol == '*')
						{
							CurrentColumn++;
							_position++;
							continue;
						}
						else
						{
							CurrentColumn++;
							break;
						}
					}
				}
				else
				{
					CurrentColumn++;
				}
				_position++;
			}
			return false;
		}

		private Token CorrectToken()
		{
			_symbol = Text[_position];
			
			return _token;
		}

		private bool IsDigit(char symbol) => symbol >= '0' && symbol <= '9';
		private bool IsLetter(char symbol) => (symbol >= 'a' && symbol <= 'z')
			|| (symbol >= 'A' && symbol <= 'Z');
	}
}
