using Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynaxAnalyzer
{
	public class JavaSyntaxAnalyzer : ISyntaxAnalyzer
	{
		private ILexer _lexer;
		private Token _token;
		private SemanticTree _table;
		private DataTypesTable _dataTypesTable;

		public JavaSyntaxAnalyzer(ILexer lexer)
		{
			_lexer = lexer;
			_table = new SemanticTree(null, null, null, null);
		}

		public JavaSyntaxAnalyzer(ILexer lexer, string text)
		{
			_lexer = lexer;
			_table = new SemanticTree(null, null, null, null);
			_lexer.Text = text;
		}

		public void Analyze()
		{
			try
			{
				Program();
			}
			catch (Exception e)
			{
				// todo
			}
		}

		public void AssignmentOperator()
		{
			_token = _lexer.GetNextToken();
			if (_token.Lexeme == Lexemes.TypeIdentifier)
			{
				while (true)
				{
					_token = _lexer.GetNextToken();
					if (_token.Lexeme == Lexemes.TypeDot)
					{
						_token = _lexer.GetNextToken();
						if (_token.Lexeme == Lexemes.TypeIdentifier)
						{
							continue;
						}
						else
						{
							throw new Exception($"Ожидался идентификатор, но отсканировано '{_token.Lexeme}': {_token.Value}");
						}
					}
					else if (_token.Lexeme == Lexemes.TypeAssignmentSign)
					{
						Expression();
					}
					else
					{
						throw new Exception($"Ожидался символ '.' или знак присваивания, но отсканировано '{_token.Lexeme}': {_token.Value}");
					}
				}
			}
			else
			{
				throw new Exception($"Ожидался идентификатор, но отсканировано '{_token.Lexeme}': {_token.Value}");
			}
		}

		public void ClassDescription()
		{
			_token = _lexer.GetNextToken();
			if (_token.Lexeme != Lexemes.TypeClass)
			{
				throw new Exception($"Ожидалось ключевое слово 'class', но отсканировано '{_token.Lexeme}': {_token.Value}");
			}
			_token = _lexer.GetNextToken();
			if (_token.Lexeme != Lexemes.TypeIdentifier)
			{
				throw new Exception($"Ожидался идентификатор, но отсканировано '{_token.Lexeme}': {_token.Value}");
			}
			// \*************семантика*************/

			SemanticTree toReturn = _table.CurrentVertex.IncludeLexeme(_token.Value, LexemeImageCategory.ClassObject);

			// /*************семантика*************\
			_token = _lexer.GetNextToken();
			if (_token.Lexeme != Lexemes.TypeOpenCurlyBrace)
			{
				throw new Exception("Ожидался символ '{', но отсканировано '" + _token.Lexeme + "': " + _token.Value);
			}
			int position = _lexer.Position;
			_token = _lexer.GetNextToken();
			if (_token.Lexeme == Lexemes.TypeCloseCurlyBrace)
			{
				// \*************семантика*************/

				_table.CurrentVertex = toReturn; // возврат

				// /*************семантика*************\
				return;
			}
			else
			{
				_lexer.Position = position;
				Descriptions();
				_token = _lexer.GetNextToken();
				if (_token.Lexeme != Lexemes.TypeCloseCurlyBrace)
				{
					throw new Exception("Ожидался символ '}', но отсканировано '" + _token.Lexeme + "': " + _token.Value);
				}
			}
			// \*************семантика*************/

			_table.CurrentVertex = toReturn;  // возврат

			// /*************семантика*************\
		}

		public void CompoundOperator()
		{
			_token = _lexer.GetNextToken();
			if (_token.Lexeme == Lexemes.TypeOpenCurlyBrace)
			{
				CompoundOperatorBody();
				_token = _lexer.GetNextToken();
				if (_token.Lexeme != Lexemes.TypeCloseCurlyBrace)
				{
					throw new Exception("Ожидался символ '}', но отсканировано '" + _token.Lexeme + "': " + _token.Value);
				}
			}
			else
			{
				throw new Exception("Ожидался символ '{', но отсканировано '" + _token.Lexeme + "': " + _token.Value);
			}
		}

		public void CompoundOperatorBody()
		{
			int position;
			while (true)
			{
				if (IsItClassDescription())
				{
					ClassDescription();
				}
				else if (IsItDataDescription())
				{
					DataDescription();
				}
				else if (IsItFunctionDescription())
				{
					FunctionDescription();
				}
				else
				{
					position = _lexer.Position;
					_token = _lexer.GetNextToken();
					if (_token.Lexeme == Lexemes.TypeCloseCurlyBrace)
					{
						_lexer.Position = position;
						return;
					}
					else
					{
						_lexer.Position = position;
						Operator();
					}
				}
			}
		}

		public void Data()
		{
			int position;
			_token = _lexer.GetNextToken();
			if (_token.Lexeme == Lexemes.TypeDataInt || _token.Lexeme == Lexemes.TypeDataDouble)
			{
				do
				{
					_token = _lexer.GetNextToken();
					if (_token.Lexeme == Lexemes.TypeIdentifier)
					{
						_token = _lexer.GetNextToken();
						if (_token.Lexeme == Lexemes.TypeComma)
							continue;
						else if (_token.Lexeme == Lexemes.TypeAssignmentSign)
						{
							Expression();
							position = _lexer.Position;
							_token = _lexer.GetNextToken();
							if (_token.Lexeme == Lexemes.TypeComma)
								continue;
							else if (_token.Lexeme == Lexemes.TypeSemicolon)
								break;
							else
							{
								_lexer.Position = position;
							}
						}
						else if (_token.Lexeme == Lexemes.TypeSemicolon)
							break;
						else
						{
							throw new Exception($"Ожидались символы ',', '=' или ';', но отсканировано '{_token.Lexeme}': {_token.Value}");
						}
					}
					else
					{
						throw new Exception($"Ожидался идентификатор, но отсканировано '{_token.Lexeme}': {_token.Value}");
					}
				} while (_token.Lexeme != Lexemes.TypeSemicolon);
			}
			else
			{
				throw new Exception($"Ожидался тип данных int или double, но отсканировано '{_token.Lexeme}': {_token.Value}");
			}
		}

		public void DataDescription()
		{
			if (IsItNamedConstant())
			{
				NamedConstant();
			}
			else if (IsItData())
			{
				Data();
			}
			else
			{
				throw new Exception($"Ожидалось описание данных или именованой константы, но отсканировано '{_token.Lexeme}': {_token.Value}");
			}
		}

		public void Descriptions()
		{
			while (true)
			{
				if (IsItClassDescription())
				{
					ClassDescription();
				}
				else if (IsItFunctionDescription())
				{
					FunctionDescription();
				}
				else if (IsItDataDescription())
				{
					DataDescription();
				}
				else
					return;
			}			
		}

		public void Expression()
		{
			int position = _lexer.Position;
			_token = _lexer.GetNextToken();
			if (_token.Lexeme != Lexemes.TypePlus && _token.Lexeme != Lexemes.TypeMinus)
				_lexer.Position = position;
			do
			{
				FirstLevel();
				position = _lexer.Position;
				_token = _lexer.GetNextToken();
			} while (_token.Lexeme == Lexemes.TypeMoreOrEqualSign || _token.Lexeme == Lexemes.TypeMoreSign
			|| _token.Lexeme == Lexemes.TypeLessOrEqualSign || _token.Lexeme == Lexemes.TypeLessSign
			|| _token.Lexeme == Lexemes.TypeEqualSign || _token.Lexeme == Lexemes.TypeNotEqualSign);
			_lexer.Position = position;
		}

		public void FifthLevel()
		{
			int position = _lexer.Position;
			int position1;
			_token = _lexer.GetNextToken();
			if (_token.Lexeme == Lexemes.TypeInt || _token.Lexeme == Lexemes.TypeDouble)
			{
				return;
			}
			else if (_token.Lexeme == Lexemes.TypeIdentifier)
			{
				position1 = _lexer.Position;
				_token = _lexer.GetNextToken();
				if (_token.Lexeme == Lexemes.TypeDot)
				{
					_lexer.Position = position;
					Name();
				}
				else if (_token.Lexeme == Lexemes.TypeOpenParenthesis)
				{
					_lexer.Position = position;
					FunctionCall();
				}
				else
				{
					_lexer.Position = position1;
				}
			}
			else
			{
				_lexer.Position = position;
				Expression();
			}
		}

		public void FirstLevel()
		{
			int position;
			do
			{
				SecondLevel();
				position = _lexer.Position;
				_token = _lexer.GetNextToken();
			} while (_token.Lexeme == Lexemes.TypePlus || _token.Lexeme == Lexemes.TypeMinus);
			_lexer.Position = position;
		}

		public void FourthLevel()
		{
			int position;
			FifthLevel();
			while (true)
			{
				position = _lexer.Position;
				_token = _lexer.GetNextToken();
				if (_token.Lexeme == Lexemes.TypeIncrement || _token.Lexeme == Lexemes.TypeDecrement)
				{
					continue;
				}
				else
				{
					_lexer.Position = position;
					break;
				}
			}
		}

		public void FunctionCall()
		{
			_token = _lexer.GetNextToken();
			if (_token.Lexeme == Lexemes.TypeIdentifier)
			{
				_token = _lexer.GetNextToken();
				if (_token.Lexeme == Lexemes.TypeOpenParenthesis)
				{
					_token = _lexer.GetNextToken();
					if (_token.Lexeme != Lexemes.TypeCloseParenthesis)
					{
						throw new Exception("Ожидался символ ')', но отсканировано '" + _token.Lexeme + "': " + _token.Value);
					}
				}
				else
				{
					throw new Exception("Ожидался символ '(', но отсканировано '" + _token.Lexeme + "': " + _token.Value);
				}
			}
			else
			{
				throw new Exception($"Ожидался идентификатор, но отсканировано '{_token.Lexeme}': {_token.Value}");
			}
		}

		public void FunctionDescription()
		{
			_token = _lexer.GetNextToken();
			if (_token.Lexeme == Lexemes.TypeDataDouble || _token.Lexeme == Lexemes.TypeDataInt)
			{
				
				_token = _lexer.GetNextToken();
				if (_token.Lexeme == Lexemes.TypeIdentifier)
				{
					_token = _lexer.GetNextToken();
					if (_token.Lexeme == Lexemes.TypeOpenParenthesis)
					{
						_token = _lexer.GetNextToken();
						if (_token.Lexeme == Lexemes.TypeCloseParenthesis)
						{
							Operator();
						}
						else
						{
							throw new Exception($"Ожидался символ ')', но отсканировано '{_token.Lexeme}': {_token.Value}");
						}
					}
					else
					{
						throw new Exception($"Ожидался символ '(', но отсканировано '{_token.Lexeme}': {_token.Value}");
					}
				}
				else
				{
					throw new Exception($"Ожидался идентификатор, но отсканировано '{_token.Lexeme}': {_token.Value}");
				}
			}
			else
			{
				throw new Exception($"Ожидался тип данных int или double, но отсканировано '{_token.Lexeme}': {_token.Value}");
			}
		}

		public void Name()
		{
			int position;
			while (true)
			{
				_token = _lexer.GetNextToken();
				if (_token.Lexeme == Lexemes.TypeIdentifier)
				{
					position = _lexer.Position;
					_token = _lexer.GetNextToken();
					if (_token.Lexeme != Lexemes.TypeDot)
					{
						_lexer.Position = position;
						return;
					}
				}
				else
				{
					throw new Exception($"Ожидался идентификатор, но отсканировано '{_token.Lexeme}': {_token.Value}");
				}
			}
		}

		public void NamedConstant()
		{
			_token = _lexer.GetNextToken();
			if (_token.Lexeme == Lexemes.TypeConst)
			{
				_token = _lexer.GetNextToken();
				if (_token.Lexeme == Lexemes.TypeDataInt || _token.Lexeme == Lexemes.TypeDataDouble)
				{
					_token = _lexer.GetNextToken();
					if (_token.Lexeme == Lexemes.TypeIdentifier)
					{
						_token = _lexer.GetNextToken();
						if (_token.Lexeme == Lexemes.TypeAssignmentSign)
						{
							_token = _lexer.GetNextToken();
							if (_token.Lexeme == Lexemes.TypeInt || _token.Lexeme == Lexemes.TypeDouble)
							{
								_token = _lexer.GetNextToken();
								if (_token.Lexeme != Lexemes.TypeSemicolon)
								{
									throw new Exception($"Ожидался символ ';', но отсканировано '{_token.Lexeme}': {_token.Value}");
								}
							}
							else
							{
								throw new Exception($"Ожидался константа, но отсканировано '{_token.Lexeme}': {_token.Value}");
							}
						}
						else
						{
							throw new Exception($"Ожидался символ '=', но отсканировано '{_token.Lexeme}': {_token.Value}");
						}
					}
					else
					{
						throw new Exception($"Ожидался идентификатор, но отсканировано '{_token.Lexeme}': {_token.Value}");
					}
				}
				else
				{
					throw new Exception($"Ожидался тип данных int или double, но отсканировано '{_token.Lexeme}': {_token.Value}");
				}
			}
			else
			{
				throw new Exception($"Ожидалось ключевое слово 'const', но отсканировано '{_token.Lexeme}': {_token.Value}");
			}
		}

		public void Operator()
		{
			int position = _lexer.Position;
			_token = _lexer.GetNextToken();
			if (_token.Lexeme == Lexemes.TypeSemicolon)
			{
				return;
			}
			else if (_token.Lexeme == Lexemes.TypeOpenCurlyBrace)
			{
				_lexer.Position = position;
				CompoundOperator();
			}
			else
			{
				_lexer.Position = position;
				SimpleOperator();
			}
		}

		public void Program()
		{
			int position = _lexer.Position;
			_token = _lexer.GetNextToken();
			if (_token.Lexeme == Lexemes.TypeEnd)
			{
				return;
			}
			else if (_token.Lexeme == Lexemes.TypeClass)
			{
				_lexer.Position = position;
				ClassDescription();
			}
			else
			{
				throw new Exception($"Не был достигнут конец файла, отсканировано '{_token.Lexeme}': {_token.Value}");
			}
		}

		public void SecondLevel()
		{
			int position;
			do
			{
				ThirdLevel();
				position = _lexer.Position;
				_token = _lexer.GetNextToken();
			} while (_token.Lexeme == Lexemes.TypeDiv || _token.Lexeme == Lexemes.TypeMult || _token.Lexeme == Lexemes.TypeMod);
			_lexer.Position = position;
		}
		public void SimpleOperator()
		{
			int position = _lexer.Position;
			_token = _lexer.GetNextToken();
			if (_token.Lexeme == Lexemes.TypeWhile)
			{
				_lexer.Position = position;
				WhileCycle();
			}
			else if (_token.Lexeme == Lexemes.TypeReturn)
			{
				_lexer.Position = position;
				ReturnOperator();
				_token = _lexer.GetNextToken();
				if (_token.Lexeme != Lexemes.TypeSemicolon)
				{
					throw new Exception($"Ожидался символ ';', но отсканировано '{_token.Lexeme}': {_token.Value}");
				}
			}
			else if (_token.Lexeme == Lexemes.TypeIdentifier)
			{
				_token = _lexer.GetNextToken();
				if (_token.Lexeme == Lexemes.TypeOpenParenthesis)
				{
					_lexer.Position = position;
					FunctionCall();					
				}
				else
				{
					_lexer.Position = position;
					AssignmentOperator();
				}
				_token = _lexer.GetNextToken();
				if (_token.Lexeme != Lexemes.TypeSemicolon)
				{
					throw new Exception($"Ожидался символ ';', но отсканировано '{_token.Lexeme}': {_token.Value}");
				}
			}
			else
			{
				throw new Exception($"Ожидалось ключевое слово 'while', 'return' или идентификатор," +
					$" но отсканировано '{_token.Lexeme}': {_token.Value}");
			}
		}

		public void ReturnOperator()
		{
			_token = _lexer.GetNextToken();
			if (_token.Lexeme == Lexemes.TypeReturn)
			{
				Expression();
			}
			else
			{
				throw new Exception($"Ожидался оператор return, но отсканировано '{_token.Lexeme}': {_token.Value}");
			}
		}

		public void ThirdLevel()
		{
			int position;
			do
			{
				position = _lexer.Position;
				_token = _lexer.GetNextToken();
			} while (_token.Lexeme == Lexemes.TypeDecrement || _token.Lexeme == Lexemes.TypeIncrement);
			_lexer.Position = position;
			FourthLevel();
		}

		public void WhileCycle()
		{
			_token = _lexer.GetNextToken();
			if (_token.Lexeme == Lexemes.TypeWhile)
			{
				_token = _lexer.GetNextToken();
				if (_token.Lexeme == Lexemes.TypeOpenParenthesis)
				{
					Expression();
					_token = _lexer.GetNextToken();
					if (_token.Lexeme == Lexemes.TypeCloseParenthesis)
					{
						Operator();
					}
					else
					{
						throw new Exception($"Ожидался символ ')', но отсканировано '{_token.Lexeme}': {_token.Value}");
					}
				}
				else
				{
					throw new Exception($"Ожидался символ '(', но отсканировано '{_token.Lexeme}': {_token.Value}");
				}
			}
			else
			{
				throw new Exception($"Ожидалось ключевое слово 'while', но отсканировано '{_token.Lexeme}': {_token.Value}");
			}
		}

		#region CheckExpressions

		private bool IsItFunctionDescription()
		{
			bool ok = false;
			int position = _lexer.Position;
			_token = _lexer.GetNextToken();
			if (_token.Lexeme == Lexemes.TypeDataInt || _token.Lexeme == Lexemes.TypeDataDouble)
			{
				_token = _lexer.GetNextToken();
				if (_token.Lexeme == Lexemes.TypeIdentifier)
				{
					_token = _lexer.GetNextToken();
					if (_token.Lexeme == Lexemes.TypeOpenParenthesis)
						ok = true;
				}
			}
			_lexer.Position = position;
			return ok;
		}

		private bool IsItClassDescription()
		{
			bool ok = false;
			int position = _lexer.Position;
			_token = _lexer.GetNextToken();
			if (_token.Lexeme == Lexemes.TypeClass)
			{
				ok = true;
			}
			_lexer.Position = position;
			return ok;
		}

		private bool IsItDataDescription() => IsItData() || IsItNamedConstant();

		private bool IsItNamedConstant()
		{
			bool ok = false;
			int position = _lexer.Position;
			_token = _lexer.GetNextToken();
			if (_token.Lexeme == Lexemes.TypeConst)
				ok = true;
			_lexer.Position = position;
			return ok;
		}

		private bool IsItData()
		{
			bool ok = false;
			int position = _lexer.Position;
			_token = _lexer.GetNextToken();
			if (_token.Lexeme == Lexemes.TypeDataDouble || _token.Lexeme == Lexemes.TypeDataInt)
			{
				_token = _lexer.GetNextToken();
				if (_token.Lexeme == Lexemes.TypeIdentifier)
				{
					_token = _lexer.GetNextToken();
					if (_token.Lexeme == Lexemes.TypeAssignmentSign
						|| _token.Lexeme == Lexemes.TypeComma
						|| _token.Lexeme == Lexemes.TypeSemicolon)
					{
						ok = true;
					}
				}
			}
			_lexer.Position = position;
			return ok;
		}

		private bool IsItWhileCycle()
		{
			bool ok = false;
			int position = _lexer.Position;
			_token = _lexer.GetNextToken();
			if (_token.Lexeme == Lexemes.TypeWhile)
				ok = true;
			_lexer.Position = position;
			return ok;
		}

		#endregion
	}
}
