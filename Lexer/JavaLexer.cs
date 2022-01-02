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
			if (!SkipIgnorableSymbols())
			{
				_token.Value = "";
				_token.Lexeme = Lexemes.TypeEnd;
				return _token;
			}
			ScanToken();
			return _token;
		}

		/// <summary>
		/// Пропускает игнорируемые символы. Устанавливает _position на первый не игнорируемый символ. Если достигнут конец
		/// текста, то вернет false, иначе true
		/// </summary>
		/// <returns>false - если достигнут конец текста, true - если все нормально</returns>
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
							if (!SkipSimpleComment())  // установит _position, соответствующий \n, нужно читать дальше
								return false;
						}
						else if (_symbol == '*') // это комментарий типа /**/
						{
							if (!SkipDifficultComment())  // установит _position соответствующий '/', нужно читать дальше
								return false;
						}
						else  // это операция деления, будет обработана дальше
						{
							_position--;
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
		/// <summary>
		/// Пропустит простой комментарий, вернет true в случае успеха, если достигнут конец текста, то вернет false
		/// </summary>
		/// <returns></returns>
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
		/// <summary>
		/// Пропустит сложный комментарий, вернет true в случае успеха, если достигнут конец текста, то вернет false
		/// </summary>
		/// <returns></returns>
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
		/// <summary>
		/// Сканиурет любую допустимую грамматикой лексему при условии, что все игнорируемые символы пропущены.
		/// </summary>
		private void ScanToken()
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
				_token.Lexeme = Lexemes.TypeMoreSign;
				_token.Value = _symbol.ToString();
				_stringBuilder.Append(_symbol);			
				TryScanNext('=', Lexemes.TypeMoreOrEqualSign);
				_position++;
			}
			else if (_symbol == '<')
			{
				_token.Lexeme = Lexemes.TypeLessSign;
				_token.Value = _symbol.ToString();
				_stringBuilder.Append(_symbol);
				TryScanNext('=', Lexemes.TypeLessOrEqualSign);
				_position++;
			}
			else if (_symbol == '=')
			{
				_token.Lexeme = Lexemes.TypeAssignmentSign;
				_token.Value = _symbol.ToString();
				_stringBuilder.Append(_symbol);
				TryScanNext('=', Lexemes.TypeEqualSign);
				_position++;
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
					//CurrentColumn++;
					_token.Lexeme = Lexemes.TypeNotEqualSign;
					_token.Value = "!=";
				}
				else
				{
					//OnErrorDetected($"Ожидался символ '=', но отсканирован символ '{_symbol}'");
					_token.Value = _symbol.ToString();
					_token.Lexeme = Lexemes.TypeError;
					_token.Value = $"Одиночно стоящий символ '!'";
					return;
				}
			}
			else if (_symbol == '+')
			{
				_token.Lexeme = Lexemes.TypePlus;
				_token.Value = _symbol.ToString();
				_stringBuilder.Append(_symbol);
				TryScanNext('+', Lexemes.TypeIncrement);
				_position++;
			}
			else if (_symbol == '-')
			{
				_token.Lexeme = Lexemes.TypeMinus;
				_token.Value = _symbol.ToString();
				_stringBuilder.Append(_symbol);
				TryScanNext('-', Lexemes.TypeDecrement);
				_position++;
			}
			else
			{
				_token.Value = _symbol.ToString();
				ChooseOtherLexemeForToken(_token);
				_position++;
			}
			return;
		}

		private bool IsDigit(char symbol) => symbol >= '0' && symbol <= '9';
		private bool IsLetter(char symbol) => (symbol >= 'a' && symbol <= 'z')
			|| (symbol >= 'A' && symbol <= 'Z')
			|| symbol == '_';
		/// <summary>
		/// Обработка идентификатора
		/// </summary>
		private void ProcessIdentifier()
		{
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
			// сюда попадем только в том случае, когда достигнут конец всего текста
			_token.Value = _stringBuilder.ToString();
			ChooseKeyLexemeForToken(_token);
			return;
		}
		/// <summary>
		/// Проверяет, является ли идентификатор ключевым словом
		/// </summary>
		/// <param name="token"></param>
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
		/// <summary>
		/// Обработка числа. Сканирование ведется в два этапа, т.к. число типа double состоит из двух чисел типа int и точки
		/// </summary>
		private void ProcessDigit()
		{
			string value;
			value = ScanInteger();  // сканируем int 
			_token.Value = value;
			_token.Lexeme = Lexemes.TypeInt;
			if (_position >= Text.Length)
			{
				return;
			}
			else
			{
				_symbol = Text[_position];
				if (_symbol == '.')  // если встретили точку
				{
					_position++;
					CurrentColumn++;
					_token.Value += ".";
					if (_position >= Text.Length)
					{
						_token.Lexeme = Lexemes.TypeError;
						return;
					}
					_symbol = Text[_position];					
					if (!IsDigit(_symbol))
					{
						_token.Value += _symbol;
						_token.Lexeme = Lexemes.TypeError;
						_position++;
						CurrentColumn++;
						return;
					}
					value = ScanInteger();  // то сканируем второую часть числа в виде int
					_token.Value += value;
					_token.Lexeme = Lexemes.TypeDouble;
					return;
				}
			}
		}
		/// <summary>
		/// Возвращает строку, представляющую число типа int, начинающееся с _symbol
		/// </summary>
		/// <returns></returns>
		private string ScanInteger()
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
				else
				{
					return _stringBuilder.ToString();
				}
			}
			// если достигнут конец текста либо в процессе чтения числа, либо даже если в цикл не заходили
			return _stringBuilder.ToString();
		}
		/// <summary>
		/// Сканирует следующий символ, которым предположительно должен быть nextSymbolShouldBe, если отсканирован другой 
		/// символ, то ничего не делает
		/// </summary>
		/// <param name="nextSymbolShouldBe">символ, который, возможно, будет частью лексемы</param>
		/// <param name="nextLexemeShouldBe"></param>
		private void TryScanNext(char nextSymbolShouldBe, Lexemes nextLexemeShouldBe)
		{
			_position++;
			CurrentColumn++;
			if (_position >= Text.Length || (_symbol = Text[_position]) != nextSymbolShouldBe)
			{
				_position--;
				CurrentColumn--;
				return;
			}
			else
			{
				_stringBuilder.Append(_symbol);
				_token.Lexeme = nextLexemeShouldBe;
				_token.Value = _stringBuilder.ToString();
				return;
			}
		}
		/// <summary>
		/// На основе свойства Value токена выбирает необходимую лексему
		/// </summary>
		/// <param name="token"></param>
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
					token.Value = $"Неожиданный символ: '{token.Value}'";
					//OnErrorDetected($"Неожиданный символ: '{token.Value}'");
					break;
			}
		}
	}
}
