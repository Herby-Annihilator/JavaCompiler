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
		private int _position;
		private Token _token;

		public JavaSyntaxAnalyzer(ILexer lexer, string text)
		{
			_lexer = lexer;
			_lexer.Text = text;
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
							// error
						}
					}
					else if (_token.Lexeme == Lexemes.TypeAssignmentSign)
					{
						Expression();
					}
					else
					{
						// error
					}
				}
			}
			else
			{
				// error
			}
		}

		public void ClassDescription()
		{
			_token = _lexer.GetNextToken();
			if (_token.Lexeme != Lexemes.TypeClass)
			{
				// error
			}
			_token = _lexer.GetNextToken();
			if (_token.Lexeme != Lexemes.TypeIdentifier)
			{
				// error
			}
			_token = _lexer.GetNextToken();
			if (_token.Lexeme != Lexemes.TypeOpenCurlyBrace)
			{
				// error
			}
			_token = _lexer.GetNextToken();
			if (_token.Lexeme == Lexemes.TypeCloseCurlyBrace)
			{
				return;
			}
			else
			{
				Descriptions();
			}
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
					// error
				}
			}
			else
			{
				// error
			}
		}

		public void CompoundOperatorBody()
		{
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
				// todo
			}
		}

		public void Data()
		{
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
						}
						else if (_token.Lexeme == Lexemes.TypeSemicolon)
							break;
						else
						{
							// error
						}
					}
					else
					{
						// error
					}
				} while (_token.Lexeme != Lexemes.TypeSemicolon);
			}
			else
			{
				// error
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
				// error
			}
		}

		public void Descriptions()
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
			return;
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
			throw new NotImplementedException();
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
			FifthLevel();
			while (true)
			{
				_position = _lexer.Position;
				_token = _lexer.GetNextToken();
				if (_token.Lexeme == Lexemes.TypeIncrement || _token.Lexeme == Lexemes.TypeDecrement)
				{
					continue;
				}
				else
				{
					_lexer.Position = _position;
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
						// error
					}
				}
				else
				{
					// error
				}
			}
			else
			{
				// error
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
							// error
						}
					}
				}
			}
		}

		public void Name()
		{
			throw new NotImplementedException();
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
									// error
								}
							}
							else
							{
								// error
							}
						}
						else
						{
							// error
						}
					}
					else
					{
						// error
					}
				}
				else
				{
					// error
				}
			}
			else
			{
				// error
			}
		}

		public void Operator()
		{
			_token = _lexer.GetNextToken();
			_position = _lexer.Position;
			if (_token.Lexeme == Lexemes.TypeSemicolon)
			{
				return;
			}
			else if (_token.Lexeme == Lexemes.TypeOpenCurlyBrace)
			{
				_lexer.Position = _position;
				CompoundOperator();
			}
			else
			{
				SimpleOperator();
			}
		}

		public void Program()
		{
			_position = _lexer.Position;
			_token = _lexer.GetNextToken();
			if (_token.Lexeme == Lexemes.TypeEnd)
			{
				return;
			}
			else if (_token.Lexeme == Lexemes.TypeClass)
			{
				_lexer.Position = _position;
				ClassDescription();
			}
			else
			{
				// error
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
			_position = _lexer.Position;
			_token = _lexer.GetNextToken();
			if (_token.Lexeme == Lexemes.TypeWhile)
			{
				_lexer.Position = _position;
				WhileCycle();
			}
			else if (_token.Lexeme == Lexemes.TypeIdentifier)
			{
				_token = _lexer.GetNextToken();
				if (_token.Lexeme == Lexemes.TypeOpenParenthesis)
				{
					_lexer.Position = _position;
					FunctionCall();					
				}
				else
				{
					_lexer.Position = _position;
					AssignmentOperator();
				}
				_token = _lexer.GetNextToken();
				if (_token.Lexeme != Lexemes.TypeSemicolon)
				{
					// error
				}
			}
			else
			{
				// error
			}
		}

		public void ThirdLevel()
		{
			do
			{
				_position = _lexer.Position;
				_token = _lexer.GetNextToken();
			} while (_token.Lexeme == Lexemes.TypeDecrement || _token.Lexeme == Lexemes.TypeIncrement);
			_lexer.Position = _position;
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
						// error
					}
				}
				else
				{
					// error
				}
			}
			else
			{
				// error
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

		private bool IsItOperator()
		{

		}

		private bool IsItSimpleOperator()
		{

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
