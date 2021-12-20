using Compiler;
using System;
using System.Text;

namespace Lexer
{
	public class JavaLexer : ILexer
	{
		private int _currentRow;
		private int _currentColumn;
		private int _position;
		private string _text;
		private char _symbol;
		private Token _token = new Token();
		private const int LEXEME_MAX_LENGTH = 1000;
		private StringBuilder _stringBuilder = new StringBuilder(LEXEME_MAX_LENGTH); // динамическая строка длиной максимум 1000

		public int CurrentRow { get => _currentRow; private set => _currentRow = value; }
		public int CurrentColumn { get => _currentColumn; private set => _currentColumn = value; }
		public string Text { get => _text; set => _text = value; }
		public int Position // при ручной установке (клиентом), свойства CurrentRow и CurrentColumn будут сброшены в 0
		{
			get => _position;
			set
			{
				_position = value;
				CurrentColumn = 0;
				CurrentRow = 0;
			}
		}

		public event EventHandler<LexerEventArgs> ErrorDetected;
		protected void OnErrorDetected(string message) => ErrorDetected?.Invoke(this, new LexerEventArgs(message));

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

		private void CorrectToken()
		{
			_stringBuilder.Clear();  // очистили поле лексемы
			_symbol = Text[_position];
			if (IsLetter(_symbol))
			{
				ProcessIdentifier();
			}
			else if (IsDigit(_symbol))
			{
				ProcessDigit();
			}
			else if (_symbol == '>')
			{
				_token.Value = ">";
				_token.Lexeme = Lexemes.TypeMoreSign;
				_position++;
				CurrentColumn++;
				if (_position >= Text.Length)
				{
					return;
				}
				_symbol = Text[_position];
				if (_symbol == '=')
				{
					_position++;
					CurrentColumn++;
					_token.Lexeme = Lexemes.TypeMoreOrEqualSign;
					_token.Value = ">=";
				}
				else
				{
					OnErrorDetected($"Ожидался символ '=', но отсканирован символ '{_symbol}'");
					_token.Value = _symbol.ToString();
					_token.Lexeme = Lexemes.TypeError;
					return;
				}
			}
			else if (_symbol == '<')
			{
				_token.Value = "<";
				_token.Lexeme = Lexemes.TypeLessSign;
				_position++;
				CurrentColumn++;
				if (_position >= Text.Length)
				{
					return;
				}
				_symbol = Text[_position];
				if (_symbol == '=')
				{
					_position++;
					CurrentColumn++;
					_token.Lexeme = Lexemes.TypeLessOrEqualSign;
					_token.Value = "<=";
				}
				else
				{
					OnErrorDetected($"Ожидался символ '=', но отсканирован символ '{_symbol}'");
					_token.Value = _symbol.ToString();
					_token.Lexeme = Lexemes.TypeError;
					return;
				}
			}
			else if (_symbol == '=')
			{
				_token.Value = "=";
				_token.Lexeme = Lexemes.TypeAssignmentSign;
				_position++;
				CurrentColumn++;
				if (_position >= Text.Length)
				{
					return;
				}
				_symbol = Text[_position];
				if (_symbol == '=')
				{
					_position++;
					CurrentColumn++;
					_token.Lexeme = Lexemes.TypeEqualSign;
					_token.Value = "==";
				}
				else
				{
					OnErrorDetected($"Ожидался символ '=', но отсканирован символ '{_symbol}'");
					_token.Value = _symbol.ToString();
					_token.Lexeme = Lexemes.TypeError;
					return;
				}
			}
			else if (_symbol == '!')
			{
				_token.Value = "!";
				_token.Lexeme = Lexemes.TypeError;
				_position++;
				CurrentColumn++;
				if (_position >= Text.Length)
				{
					return;
				}
				_symbol = Text[_position];
				if (_symbol == '=')
				{
					_position++;
					CurrentColumn++;
					_token.Lexeme = Lexemes.TypeNotEqualSign;
					_token.Value = "!=";
				}
				else
				{
					OnErrorDetected($"Ожидался символ '=', но отсканирован символ '{_symbol}'");
					_token.Value = _symbol.ToString();
					_token.Lexeme = Lexemes.TypeError;
					return;
				}
			}
			else if (_symbol == '+')
			{
				_token.Value = "+";
				_token.Lexeme = Lexemes.TypePlus;
				_position++;
				CurrentColumn++;
				if (_position >= Text.Length)
				{
					return;
				}
				_symbol = Text[_position];
				if (_symbol == '+')
				{
					_position++;
					CurrentColumn++;
					_token.Lexeme = Lexemes.TypeIncrement;
					_token.Value = "++";
				}
				else
				{
					OnErrorDetected($"Ожидался символ '+', но отсканирован символ '{_symbol}'");
					_token.Value = _symbol.ToString();
					_token.Lexeme = Lexemes.TypeError;
					return;
				}
			}
			else if (_symbol == '-')
			{
				_token.Value = "-";
				_token.Lexeme = Lexemes.TypeMinus;
				_position++;
				CurrentColumn++;
				if (_position >= Text.Length)
				{
					return;
				}
				_symbol = Text[_position];
				if (_symbol == '-')
				{
					_position++;
					CurrentColumn++;
					_token.Lexeme = Lexemes.TypeDecrement;
					_token.Value = "--";
				}
				else
				{
					OnErrorDetected($"Ожидался символ '-', но отсканирован символ '{_symbol}'");
					_token.Value = _symbol.ToString();
					_token.Lexeme = Lexemes.TypeError;
					return;
				}
			}
			else
			{
				_token.Value = _symbol.ToString();
				ChooseOtherLexemeForToken(_token);
			}
			return;
		}

		private bool IsDigit(char symbol) => symbol >= '0' && symbol <= '9';
		private bool IsLetter(char symbol) => (symbol >= 'a' && symbol <= 'z')
			|| (symbol >= 'A' && symbol <= 'Z')
			|| symbol == '_';

		private void ProcessIdentifier()
		{
			_stringBuilder.Clear();
			_stringBuilder.Append(_symbol);
			_position++;
			CurrentColumn++;
			while (_position < Text.Length)
			{
				_symbol = Text[_position];
				if (IsDigit(_symbol) || IsLetter(_symbol))
				{
					_stringBuilder.Append(_symbol);
					_position++;
					CurrentColumn++;
				}
				else
				{
					_token.Value = _stringBuilder.ToString();
					ChooseKeyLexemeForToken(_token);
					return;
				}
			}
		}

		private void ChooseKeyLexemeForToken(Token token)
		{
			switch (token.Value)
			{
				case "double":
					{
						token.Lexeme = Lexemes.TypeDataDouble;
						break;
					}
				case "int":
					{
						token.Lexeme = Lexemes.TypeDataInt;
						break;
					}					
				case "class":
					{
						token.Lexeme = Lexemes.TypeClass;
						break;
					}
				case "while":
					{
						token.Lexeme = Lexemes.TypeWhile;
						break;
					}
				case "const":
					{
						token.Lexeme = Lexemes.TypeConst;
						break;
					}
				case "return":
					{
						token.Lexeme = Lexemes.TypeReturn;
						break;
					}
				default:
					{
						token.Lexeme = Lexemes.TypeIdentifier;
						break;
					}
			}
			return;
		}

		private void ProcessDigit()
		{
			_stringBuilder.Clear();
			_stringBuilder.Append(_symbol);
			_position++;
			CurrentColumn++;
			while (_position < Text.Length)
			{
				_symbol = Text[_position];
				if (IsDigit(_symbol))
				{
					_stringBuilder.Append(_symbol);
					_position++;
					CurrentColumn++;
				}
				else if (_symbol == '.')
				{
					_stringBuilder.Append(_symbol);
					_position++;
					CurrentColumn++;
					while (_position < Text.Length)
					{
						if (IsDigit(_symbol))
						{
							_stringBuilder.Append(_symbol);
							_position++;
							CurrentColumn++;
						}
						else
						{
							_token.Value = _stringBuilder.ToString();
							_token.Lexeme = Lexemes.TypeDouble;
							return;
						}
					}
				}
				else
				{
					_token.Value = _stringBuilder.ToString();
					_token.Lexeme = Lexemes.TypeInt;
					return;
				}
			}
		}

		private void ChooseOtherLexemeForToken(Token token)
		{
			switch (token.Value)
			{
				case "/":
					{
						token.Lexeme = Lexemes.TypeDiv;
						break;
					}
				case "*":
					{
						token.Lexeme = Lexemes.TypeMult;
						break;
					}
				case "%":
					{
						token.Lexeme = Lexemes.TypeMod;
						break;
					}
				case ";":
					{
						token.Lexeme = Lexemes.TypeSemicolon;
						break;
					}
				case ".":
					{
						token.Lexeme = Lexemes.TypeDot;
						break;
					}
				case ",":
					{
						token.Lexeme = Lexemes.TypeComma;
						break;
					}
				case "{":
					{
						token.Lexeme = Lexemes.TypeOpenCurlyBrace;
						break;
					}
				case "}":
					{
						token.Lexeme = Lexemes.TypeCloseCurlyBrace;
						break;
					}
				case "(":
					{
						token.Lexeme = Lexemes.TypeOpenParenthesis;
						break;
					}
				case ")":
					{
						token.Lexeme = Lexemes.TypeCloseParenthesis;
						break;
					}
				default:
					token.Lexeme = Lexemes.TypeError;
					OnErrorDetected($"Неожиданный символ: '{token.Value}'");
					break;
			}
		}
	}
}
