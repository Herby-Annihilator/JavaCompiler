using Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynaxAnalyzer
{
	public class JavaSyntaxAnalyzer
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

		public void AssignmentOperator(out int operatorReturnType)
		{
			SemanticTree obj;
			_token = _lexer.GetNextToken();
			if (_token.Lexeme == Lexemes.TypeIdentifier)
			{
				obj = _table.FindUp(_table.CurrentVertex, _token.Value);
				if (obj == null)
                {
					throw new Exception($"Идентификатор {_token.Value} ни разу не описан");
                }
				else if (obj.Data.Category != LexemeImageCategory.Variable)
                {
					throw new Exception($"Идентификатор {_token.Value} не является переменной");
				}
				while (true)
				{
					_token = _lexer.GetNextToken();
					if (_token.Lexeme == Lexemes.TypeDot)
					{
						_token = _lexer.GetNextToken();
						if (_token.Lexeme == Lexemes.TypeIdentifier)
						{
							obj = _table.FindRightLeft(obj, _token.Value);
							if (obj == null)
							{
								throw new Exception($"Идентификатор {_token.Value} ни разу не описан");
							}
							else if (obj.Data.Category != LexemeImageCategory.Variable)
							{
								throw new Exception($"Идентификатор {_token.Value} не является переменной");
							}
							continue;
						}
						else
						{
							throw new Exception($"Ожидался идентификатор, но отсканировано '{_token.Lexeme}': {_token.Value}");
						}
					}
					else if (_token.Lexeme == Lexemes.TypeAssignmentSign)
					{
						Expression(out operatorReturnType);
						if (obj.Data.DataType != operatorReturnType)
                        {
							throw new Exception($"Нельзя присвоить переменной типа {obj.Data.DataType} значение типа {operatorReturnType}");
                        }
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

		public void CompoundOperator(out int operatorReturnType)
		{
			_token = _lexer.GetNextToken();
			SemanticTree toReturn;
			if (_token.Lexeme == Lexemes.TypeOpenCurlyBrace)
			{
				// \*************семантика*************/
				toReturn = _table.IncludeCompoundOperator();
				// /*************семантика*************\
				CompoundOperatorBody(out operatorReturnType);
				_token = _lexer.GetNextToken();
				if (_token.Lexeme != Lexemes.TypeCloseCurlyBrace)
				{
					throw new Exception("Ожидался символ '}', но отсканировано '" + _token.Lexeme + "': " + _token.Value);
				}
				// \*************семантика*************/
				_table.CurrentVertex = toReturn;  // возврат
				// /*************семантика*************\
			}
			else
			{
				throw new Exception("Ожидался символ '{', но отсканировано '" + _token.Lexeme + "': " + _token.Value);
			}
		}

		public void CompoundOperatorBody(out int operatorReturnType)
		{
			int position;
			operatorReturnType = _dataTypesTable.UndefType;
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
						Operator(out operatorReturnType);
					}
				}
			}
		}

		public void Data()
		{
			int position;
			int type = -1;
			int realType = _dataTypesTable.UndefType;
			SemanticTree toSetValue;
			_token = _lexer.GetNextToken();
			if (_token.Lexeme == Lexemes.TypeDataInt || _token.Lexeme == Lexemes.TypeDataDouble)
			{
				// определение типа
				if (_token.Lexeme == Lexemes.TypeDataInt)
					type = _dataTypesTable.IntegerType;
				else if (_token.Lexeme == Lexemes.TypeDataDouble)
					type = _dataTypesTable.DoubleType;


				do
				{
					_token = _lexer.GetNextToken();
					if (_token.Lexeme == Lexemes.TypeIdentifier)
					{
						// заносим в таблицу переменную
						toSetValue = _table.IncludeVariable(_token.Value, type);  // значение пока неизвестно

						_token = _lexer.GetNextToken();
						if (_token.Lexeme == Lexemes.TypeComma)
							continue;
						else if (_token.Lexeme == Lexemes.TypeAssignmentSign)
						{
							Expression(out realType); // должно возвращать тип данных
							if (!_dataTypesTable.CheckTypesCompatibility(type, realType))
                            {
								throw new Exception($"Нельзя присвоить тип {realType} переменной типа {type}");
                            }
                            else
                            {
								toSetValue.Data.DataType = _dataTypesTable.MixTypes(type, realType);
                            }
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

		public void Expression(out int dataType)
		{
			int position = _lexer.Position;
			bool shouldSetBool = false;
			Lexemes operation = Lexemes.TypeAssignmentSign;
			int previousType = -1;
			_token = _lexer.GetNextToken();
			if (_token.Lexeme != Lexemes.TypePlus && _token.Lexeme != Lexemes.TypeMinus)
				_lexer.Position = position;
			do
			{
				FirstLevel(out dataType);
				if (shouldSetBool)
                {
					if (_dataTypesTable.CanTwoTypesBeCompared(previousType, dataType, operation))
                    {
						dataType = _dataTypesTable.BoolType;
					}
                    else
                    {
						throw new Exception($"Недопустимое сравнение типов");
                    }
                }
				shouldSetBool = true;
				previousType = dataType;

				position = _lexer.Position;
				_token = _lexer.GetNextToken();

				operation = _token.Lexeme;

			} while (_token.Lexeme == Lexemes.TypeMoreOrEqualSign || _token.Lexeme == Lexemes.TypeMoreSign
			|| _token.Lexeme == Lexemes.TypeLessOrEqualSign || _token.Lexeme == Lexemes.TypeLessSign
			|| _token.Lexeme == Lexemes.TypeEqualSign || _token.Lexeme == Lexemes.TypeNotEqualSign);
			_lexer.Position = position;
		}

		public void FifthLevel(out int dataType)
		{
			int position = _lexer.Position;
			int position1;
			dataType = _dataTypesTable.UndefType;
			_token = _lexer.GetNextToken();
			if (_token.Lexeme == Lexemes.TypeInt || _token.Lexeme == Lexemes.TypeDouble)
			{
				// Семантика
				if (_token.Lexeme == Lexemes.TypeInt)
					dataType = _dataTypesTable.IntegerType;
				else if (_token.Lexeme == Lexemes.TypeDouble)
					dataType = _dataTypesTable.DoubleType;
				return;
			}
			else if (_token.Lexeme == Lexemes.TypeIdentifier)
			{
				position1 = _lexer.Position;
				_token = _lexer.GetNextToken();
				if (_token.Lexeme == Lexemes.TypeDot)
				{
					_lexer.Position = position;
					NameWithReturningType(out dataType);
				}
				else if (_token.Lexeme == Lexemes.TypeOpenParenthesis)
				{
					_lexer.Position = position;
					FunctionCall(out dataType);
				}
				else
				{
					_lexer.Position = position1;
				}
			}
			else
			{
				_lexer.Position = position;
				Expression(out dataType);
			}
		}

		public void FirstLevel(out int dataType)
		{
			int position;
			int previousDataType = -1;
			bool shouldCheck = false;
			do
			{
				SecondLevel(out dataType);
				// проверка на допустимость и вычисление типа результата операции
				if (shouldCheck)
                {
					dataType = _dataTypesTable.OperationResultType(dataType, previousDataType);
                }
				shouldCheck = true;
				previousDataType = dataType;

				position = _lexer.Position;
				_token = _lexer.GetNextToken();
			} while (_token.Lexeme == Lexemes.TypePlus || _token.Lexeme == Lexemes.TypeMinus);
			_lexer.Position = position;
		}

		public void FourthLevel(out int dataType)
		{
			int position;
			FifthLevel(out dataType);
			while (true)
			{
				position = _lexer.Position;
				_token = _lexer.GetNextToken();
				if (_token.Lexeme == Lexemes.TypeIncrement || _token.Lexeme == Lexemes.TypeDecrement)
				{
					if (dataType == _dataTypesTable.UndefType)
                    {
						throw new Exception($"Нельзя производить операции инкремента или декремента над неопределнным типом");
                    }
					else if (dataType == _dataTypesTable.BoolType)
                    {
						throw new Exception($"Нельзя производить операции инкремента или декремента над логическим типом");
					}
					continue;
				}
				else
				{
					_lexer.Position = position;
					break;
				}
			}
		}

		public void FunctionCall(out int dataType)
		{
			_token = _lexer.GetNextToken();
			SemanticTree obj = null;
			if (_token.Lexeme == Lexemes.TypeIdentifier)
			{
				obj = _table.FindUp(_table.CurrentVertex, _token.Value);
				if (obj == null)
                {
					throw new Exception($"Описание функции {_token.Value} не найдено");
                }
				else if (obj.Data.Category != LexemeImageCategory.Function)
                {
					throw new Exception($"{_token.Value} не является функцией");
				}
				else
                {
					dataType = obj.Data.DataType;
                }
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
			SemanticTree toReturn;
			_token = _lexer.GetNextToken();
			int returningType;
			if (_token.Lexeme == Lexemes.TypeDataDouble || _token.Lexeme == Lexemes.TypeDataInt)
			{
				// \*************семантика*************/

				int type = -1;
				if (_token.Lexeme == Lexemes.TypeDataDouble)
				{
					type = _dataTypesTable.DoubleType;
				}
				else if (_token.Lexeme == Lexemes.TypeDataInt)
				{
					type = _dataTypesTable.DoubleType;
				}

				// /*************семантика*************\

				_token = _lexer.GetNextToken();
				if (_token.Lexeme == Lexemes.TypeIdentifier)
				{
					// \*************семантика*************/
					toReturn = _table.IncludeLexeme(_token.Value, LexemeImageCategory.Function);
					toReturn.Data.DataType = type;
					// /*************семантика*************\
					_token = _lexer.GetNextToken();
					if (_token.Lexeme == Lexemes.TypeOpenParenthesis)
					{
						_token = _lexer.GetNextToken();
						if (_token.Lexeme == Lexemes.TypeCloseParenthesis)
						{
							Operator(out returningType);
							if (_dataTypesTable.CheckTypesCompatibility(type, returningType))
                            {
								toReturn.Data.DataType = _dataTypesTable.MixTypes(type, returningType);
                            }
                            else
                            {
								throw new Exception($"Функция, возвращающая {type} не может возвращать тип {returningType}");
                            }
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
			// возврат
			_table.CurrentVertex = toReturn;
		}

		public void NameWithReturningType(out int dataType)
		{
			StringBuilder builder = new StringBuilder(100);
			int position;
			SemanticTree obj = null;
			while (true)
			{
				_token = _lexer.GetNextToken();
				if (_token.Lexeme == Lexemes.TypeIdentifier)
				{
					builder.Append(_token.Value);
					if (obj == null)
                    {
						obj = _table.FindUp(_table.CurrentVertex, _token.Value);
						if (obj == null)
                        {
							throw new Exception($"Идентификатор {builder.ToString()} еще не описан");
                        }
					}						
                    else
                    {
						obj = _table.FindRightLeft(obj, _token.Value);
						if (obj == null)
                        {
							throw new Exception($"Класс или структура {builder.ToString()} не содержит член {_token.Value}");
                        }
                    }
					position = _lexer.Position;
					_token = _lexer.GetNextToken();
					if (_token.Lexeme != Lexemes.TypeDot)
					{
						_lexer.Position = position;
						dataType = obj.Data.DataType;
						return;
					}
					builder.Append(".");
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
			int type = -1;
			SemanticTree toReturn;
			if (_token.Lexeme == Lexemes.TypeConst)
			{
				_token = _lexer.GetNextToken();
				if (_token.Lexeme == Lexemes.TypeDataInt || _token.Lexeme == Lexemes.TypeDataDouble)
				{
					// \*************семантика*************/
					if (_token.Lexeme == Lexemes.TypeDataInt)
						type = _dataTypesTable.IntegerType;
					else if (_token.Lexeme == Lexemes.TypeDataDouble)
						type = _dataTypesTable.DoubleType;
					// /*************семантика*************\
					_token = _lexer.GetNextToken();
					if (_token.Lexeme == Lexemes.TypeIdentifier)
					{
						// \*************семантика*************/
						if (_table.IsLexemeRepeatsInBlock(_table.CurrentVertex, _token.Value))
                        {
							throw new Exception($"Сущность с именем {_token.Value} уже была описана ранее.");
                        }
                        else
                        {
							toReturn = _table.IncludeConstant(_token.Value, type);
                        }
						// \*************семантика*************/
						_token = _lexer.GetNextToken();
						if (_token.Lexeme == Lexemes.TypeAssignmentSign)
						{
							_token = _lexer.GetNextToken();
							if (_token.Lexeme == Lexemes.TypeInt || _token.Lexeme == Lexemes.TypeDouble)
							{
								// \*************семантика*************/
								int realType = -1;
								if (_token.Lexeme == Lexemes.TypeInt)
									realType = _dataTypesTable.IntegerType;
								if (_token.Lexeme == Lexemes.TypeDouble)
									realType = _dataTypesTable.DoubleType;
								if (realType != type)
                                {
									throw new Exception($"Нельзя присвоить тип {realType} константе типа {type}");
                                }
								else
                                {
									toReturn.Data.LexemeValue = _token.Value;
                                }
								// \*************семантика*************/
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

		public void Operator(out int operatorReturnType)
		{
			int position = _lexer.Position;
			operatorReturnType = _dataTypesTable.UndefType;
			_token = _lexer.GetNextToken();
			if (_token.Lexeme == Lexemes.TypeSemicolon)
			{
				operatorReturnType = _dataTypesTable.UndefType;
				return;
			}
			else if (_token.Lexeme == Lexemes.TypeOpenCurlyBrace)
			{
				_lexer.Position = position;
				CompoundOperator(out operatorReturnType);
			}
			else
			{
				_lexer.Position = position;
				SimpleOperator(out operatorReturnType);
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

		public void SecondLevel(out int dataType)
		{
			int position;
			int previousDataType = -1;
			bool shouldCheck = false;
			do
			{
				ThirdLevel(out dataType);
				// проверка на допустимость и вычисление типа результата операции
				if (shouldCheck)
                {
					dataType = _dataTypesTable.OperationResultType(dataType, previousDataType);
                }
				shouldCheck = true;
				previousDataType = dataType;

				position = _lexer.Position;
				_token = _lexer.GetNextToken();
			} while (_token.Lexeme == Lexemes.TypeDiv || _token.Lexeme == Lexemes.TypeMult || _token.Lexeme == Lexemes.TypeMod);
			_lexer.Position = position;
		}
		public void SimpleOperator(out int operatorReturnType)
		{
			int position = _lexer.Position;
			operatorReturnType = _dataTypesTable.UndefType;
			_token = _lexer.GetNextToken();
			if (_token.Lexeme == Lexemes.TypeWhile)
			{
				_lexer.Position = position;
				WhileCycle();
			}
			else if (_token.Lexeme == Lexemes.TypeReturn)
			{
				_lexer.Position = position;
				ReturnOperator(out operatorReturnType);
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
					FunctionCall(out operatorReturnType);					
				}
				else
				{
					_lexer.Position = position;
					AssignmentOperator(out operatorReturnType);
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

		public void ReturnOperator(out int returnType)
		{
			_token = _lexer.GetNextToken();
			if (_token.Lexeme == Lexemes.TypeReturn)
			{
				Expression(out returnType);
			}
			else
			{
				throw new Exception($"Ожидался оператор return, но отсканировано '{_token.Lexeme}': {_token.Value}");
			}
		}

		public void ThirdLevel(out int dataType)
		{
			int position;
			int precrementsCount = -1; // чтобы определить, нужно ли проверять допустимость операций прекремента
			do
			{
				precrementsCount++;
				position = _lexer.Position;
				_token = _lexer.GetNextToken();
			} while (_token.Lexeme == Lexemes.TypeDecrement || _token.Lexeme == Lexemes.TypeIncrement);
			_lexer.Position = position;
			FourthLevel(out dataType);
			// проверка допустимости операций
			if (precrementsCount > 0)
            {
				if (dataType == _dataTypesTable.UndefType)
                {
					throw new Exception($"Нельзя осуществить операцию прекремента для неопределенного типа");
                }
				if (dataType == _dataTypesTable.BoolType)
                {
					throw new Exception($"Нельзя осуществить операцию прекремента для логического типа");
				}
            }
		}

		public void WhileCycle()
		{
			int expressionReturnType;
			_token = _lexer.GetNextToken();
			if (_token.Lexeme == Lexemes.TypeWhile)
			{
				_token = _lexer.GetNextToken();
				if (_token.Lexeme == Lexemes.TypeOpenParenthesis)
				{
					// проверка типа
					Expression(out expressionReturnType);
					if (expressionReturnType != _dataTypesTable.BoolType)
                    {
						throw new Exception($"Условие в цикле должно иметь логический тип");
                    }
					_token = _lexer.GetNextToken();
					if (_token.Lexeme == Lexemes.TypeCloseParenthesis)
					{
						Operator(out int operatorReturnType);
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
