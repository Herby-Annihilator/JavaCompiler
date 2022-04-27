﻿using Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynaxAnalyzer
{
	public class JavaSyntaxAnalyzer
	{
		protected ILexer _lexer;
		protected Token _token;
		protected SemanticTree _table;

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
						break;
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

			SemanticTree toReturn = _table.IncludeLexeme(_token.Value, LexemeImageCategory.ClassType);
			Console.WriteLine("Выделение памяти под описание класса. Дерево имеет вид:");
			_table.Print();

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
				Console.WriteLine("Выделение памяти под сложный оператор. Дерево имеет вид:");
				_table.Print();
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
			operatorReturnType = DataTypesTable.UndefType;
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
			int realType = DataTypesTable.UndefType;
			SemanticTree toSetValue;
			_token = _lexer.GetNextToken();
			if (_token.Lexeme == Lexemes.TypeDataInt || _token.Lexeme == Lexemes.TypeDataDouble || _token.Lexeme == Lexemes.TypeIdentifier)
			{
				// определение типа
				if (_token.Lexeme == Lexemes.TypeDataInt)
					type = DataTypesTable.IntegerType;
				else if (_token.Lexeme == Lexemes.TypeDataDouble)
					type = DataTypesTable.DoubleType;
				else if (_token.Lexeme == Lexemes.TypeIdentifier)
					type = DataTypesTable.UserType;

				do
				{
					string dataTypeImage = _token.Value;
					_token = _lexer.GetNextToken();
					if (_token.Lexeme == Lexemes.TypeIdentifier)
					{
						// заносим в таблицу объект класса
						if (type == DataTypesTable.UserType)
                        {
							toSetValue = _table.IncudeClassObject(_token.Value, dataTypeImage);
							Console.WriteLine($"Выделение памяти для объекта '{_token.Value}' класса '{dataTypeImage}'." +
                                $" Дерево имеет вид:");
							_table.Print();
						}
						else // заносим в таблицу переменную
						{
							toSetValue = _table.IncludeVariable(_token.Value, type);  // значение пока не известно
							Console.WriteLine($"Выделение памяти для переменной '{_token.Value}'. Дерево имеет вид:");
							_table.Print();
						}					

						_token = _lexer.GetNextToken();
						if (_token.Lexeme == Lexemes.TypeComma)
							continue;
						else if (_token.Lexeme == Lexemes.TypeAssignmentSign)
						{
							Expression(out realType); // должно возвращать тип данных
							if (!DataTypesTable.CheckTypesCompatibility(type, realType))
                            {
								throw new Exception($"Нельзя присвоить тип {DataTypesTable.TypeToString(realType)}" +
									$" переменной типа {DataTypesTable.TypeToString(type)}");
                            }
                            else
                            {
								toSetValue.Data.DataType = DataTypesTable.MixTypes(type, realType);
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
					if (DataTypesTable.CanTwoTypesBeCompared(previousType, dataType, operation))
                    {
						dataType = DataTypesTable.BoolType;
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
			dataType = DataTypesTable.UndefType;
			_token = _lexer.GetNextToken();
			if (_token.Lexeme == Lexemes.TypeInt || _token.Lexeme == Lexemes.TypeDouble)
			{
				// Семантика
				if (_token.Lexeme == Lexemes.TypeInt)
					dataType = DataTypesTable.IntegerType;
				else if (_token.Lexeme == Lexemes.TypeDouble)
					dataType = DataTypesTable.DoubleType;
				return;
			}
			else if (_token.Lexeme == Lexemes.TypeIdentifier)
			{
				SemanticTree obj = _table.FindUp(_table.CurrentVertex, _token.Value);
				if (obj == null)
				{
					throw new Exception($"Идентификатор {_token.Value} ни разу не описан");
				}
				dataType = obj.Data.DataType;

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
					dataType = DataTypesTable.OperationResultType(dataType, previousDataType);
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
					if (dataType == DataTypesTable.UndefType)
                    {
						throw new Exception($"Нельзя производить операции инкремента или декремента над неопределнным типом");
                    }
					else if (dataType == DataTypesTable.BoolType)
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
					/* Исполнение тела функции (выделение памяти) */
					/* 
					 * на данном моменте obj содержит указатель на узел с функцией в качестве содержимого
					 * осталось извлечь правого потомка - тело функции и прилепить в качестве правого
					 * потомка текущего узла
					 */



					/* Исполнение тела функции (выделение памяти) */
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
					type = DataTypesTable.DoubleType;
				}
				else if (_token.Lexeme == Lexemes.TypeDataInt)
				{
					type = DataTypesTable.IntegerType;
				}

				// /*************семантика*************\

				_token = _lexer.GetNextToken();
				if (_token.Lexeme == Lexemes.TypeIdentifier)
				{
					// \*************семантика*************/
					toReturn = _table.IncludeLexeme(_token.Value, LexemeImageCategory.Function);
					toReturn.Data.DataType = type;
					Console.WriteLine("Выделение памяти под функцию. Дерево имеет вид:");
					_table.Print();
					// /*************семантика*************\
					_token = _lexer.GetNextToken();
					if (_token.Lexeme == Lexemes.TypeOpenParenthesis)
					{
						_token = _lexer.GetNextToken();
						if (_token.Lexeme == Lexemes.TypeCloseParenthesis)
						{
							Operator(out returningType);
							if (DataTypesTable.CheckTypesCompatibility(type, returningType))
                            {
								toReturn.Data.DataType = DataTypesTable.MixTypes(type, returningType);
							}
							else
							{
								throw new Exception($"Функция, возвращающая {DataTypesTable.TypeToString(type)} " +
									$"не может возвращать тип {DataTypesTable.TypeToString(returningType)}");
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
						type = DataTypesTable.IntegerType;
					else if (_token.Lexeme == Lexemes.TypeDataDouble)
						type = DataTypesTable.DoubleType;
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
							Console.WriteLine($"Процесс выделения памяти для константы '{_token.Value}'. Дерево имеет вид:");
							_table.Print();
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
                                {
									realType = DataTypesTable.IntegerType;
									toReturn.Data.LexemeValue = new LexemeValue { IntegerValue = Convert.ToInt32(_token.Value) };
								}
									
								if (_token.Lexeme == Lexemes.TypeDouble)
                                {
									realType = DataTypesTable.DoubleType;
									toReturn.Data.LexemeValue = new LexemeValue { DoubleValue = Convert.ToDouble(_token.Value) };
								}
									
								if (realType != type)
                                {
									throw new Exception($"Нельзя присвоить тип {DataTypesTable.TypeToString(realType)}" +
										$" константе типа {DataTypesTable.TypeToString(type)}");
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
			operatorReturnType = DataTypesTable.UndefType;
			_token = _lexer.GetNextToken();
			if (_token.Lexeme == Lexemes.TypeSemicolon)
			{
				operatorReturnType = DataTypesTable.UndefType;
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
					dataType = DataTypesTable.OperationResultType(dataType, previousDataType);
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
			operatorReturnType = DataTypesTable.UndefType;
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
				else if (_token.Lexeme == Lexemes.TypeAssignmentSign)
				{
					_lexer.Position = position;
					AssignmentOperator(out operatorReturnType);
				}
				else
				{
					_lexer.Position = position;
					Expression(out operatorReturnType);
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
				if (dataType == DataTypesTable.UndefType)
                {
					throw new Exception($"Нельзя осуществить операцию прекремента для неопределенного типа");
                }
				if (dataType == DataTypesTable.BoolType)
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
					if (expressionReturnType != DataTypesTable.BoolType)
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

		protected bool IsItFunctionDescription()
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

		protected bool IsItClassDescription()
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

		protected bool IsItDataDescription() => IsItData() || IsItNamedConstant();

		protected bool IsItNamedConstant()
		{
			bool ok = false;
			int position = _lexer.Position;
			_token = _lexer.GetNextToken();
			if (_token.Lexeme == Lexemes.TypeConst)
				ok = true;
			_lexer.Position = position;
			return ok;
		}

		protected bool IsItData()
		{
			bool ok = false;
			int position = _lexer.Position;
			_token = _lexer.GetNextToken();
			if (_token.Lexeme == Lexemes.TypeDataDouble || _token.Lexeme == Lexemes.TypeDataInt || _token.Lexeme == Lexemes.TypeIdentifier)
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

		protected bool IsItWhileCycle()
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
