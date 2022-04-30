using Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynaxAnalyzer
{
    /// <summary>
    /// Класс помощник в синтаксическом анализе. Предоставляет операции сравнения.
    /// </summary>
    public static class LexemeValueComparer
    {
        /// <summary>
        /// Сравнивает значения <see cref="LexemeValue"/> двух типов <see cref="DataTypesTable"/> 
        /// в соответствии с операцией <paramref name="operation"/>.
        /// </summary>
        /// <param name="firstType">Тип первого значения</param>
        /// <param name="firstValue">Перове значение</param>
        /// <param name="secondType">Тип второго значения</param>
        /// <param name="secondValue">второе значение</param>
        /// <param name="operation">Логическая операция <see cref="Lexemes"/></param>
        /// <returns><see cref="LexemeValue"/> с установленным результатом <see cref="LexemeValue.BoolValue"/></returns>
        /// <exception cref="Exception"></exception>
        public static LexemeValue CompareValues(int firstType, LexemeValue firstValue, int secondType, LexemeValue secondValue, Lexemes operation)
        {
            if (DataTypesTable.CanTwoTypesBeCompared(firstType, secondType, operation))
            {
                if (firstType == DataTypesTable.IntegerType && secondType == DataTypesTable.IntegerType)
                {
                    return CompareIntegerTypes(firstValue, secondValue, operation);               
                }
                if (firstType == DataTypesTable.BoolType && secondType == DataTypesTable.BoolType)
                {
                    return CompareBoolTypes(firstValue, secondValue, operation);
                }
                if (firstType == DataTypesTable.IntegerType)
                    firstValue.DoubleValue = firstValue.IntegerValue;
                if (secondType == DataTypesTable.IntegerType)
                    secondValue.DoubleValue = secondValue.IntegerValue;
                return CompareDoubleTypes(firstValue, secondValue, operation);
            }
            else
            {
                throw new Exception($"Недопустимое сравнение типов");
            }           
        }
        /// <summary>
        /// Сравнивает два значения типа <see cref="DataTypesTable.BoolType"/> из таблицы <see cref="DataTypesTable"/>.
        /// Сравниваются только <see cref="DataTypesTable.BoolType"/>.
        /// </summary>
        /// <param name="firstValue">Первое значение</param>
        /// <param name="secondValue">Второе значение</param>
        /// <param name="operation">Операция сравнения - лексема <see cref="Lexemes"/></param>
        /// <returns><see cref="LexemeValue"/> с уже известным результатом <see cref="LexemeValue.BoolValue"/></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static LexemeValue CompareBoolTypes(LexemeValue firstValue, LexemeValue secondValue, Lexemes operation)
        {
            LexemeValue result = new LexemeValue();
            switch (operation)
            {
                case Lexemes.TypeEqualSign:
                    result.BoolValue = firstValue.BoolValue == secondValue.BoolValue;
                    return result;
                case Lexemes.TypeNotEqualSign:
                    result.BoolValue = firstValue.BoolValue != secondValue.BoolValue;
                    return result;
                default:
                    throw new InvalidOperationException($"{operation} не не может быть применена к операндам типа boolean");
            }
        }

        /// <summary>
        /// Сравнивает два значения типа <see cref="DataTypesTable.IntegerType"/> из таблицы <see cref="DataTypesTable"/>.
        /// Сравниваются только <see cref="DataTypesTable.IntegerType"/>.
        /// </summary>
        /// <param name="firstValue">Первое значение</param>
        /// <param name="secondValue">Второе значение</param>
        /// <param name="operation">Операция сравнения - лексема <see cref="Lexemes"/></param>
        /// <returns><see cref="LexemeValue"/> с уже известным результатом <see cref="LexemeValue.BoolValue"/></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static LexemeValue CompareIntegerTypes(LexemeValue firstValue, LexemeValue secondValue, Lexemes operation)
        {
            LexemeValue result = new LexemeValue();
            switch (operation)
            {
                case Lexemes.TypeEqualSign:
                    result.BoolValue = firstValue.IntegerValue == secondValue.IntegerValue;
                    return result;
                case Lexemes.TypeNotEqualSign:
                    result.BoolValue = firstValue.IntegerValue != secondValue.IntegerValue;
                    return result;
                case Lexemes.TypeMoreSign:
                    result.BoolValue = firstValue.IntegerValue > secondValue.IntegerValue;
                    return result;
                case Lexemes.TypeMoreOrEqualSign:
                    result.BoolValue = firstValue.IntegerValue >= secondValue.IntegerValue;
                    return result;
                case Lexemes.TypeLessSign:
                    result.BoolValue = firstValue.IntegerValue < secondValue.IntegerValue;
                    return result;
                case Lexemes.TypeLessOrEqualSign:
                    result.BoolValue = firstValue.IntegerValue <= secondValue.IntegerValue;
                    return result;
                default:
                    throw new InvalidOperationException($"{operation} не является операцией сравнения");
            }
        }
        /// <summary>
        /// Сравнивает два значения типа <see cref="DataTypesTable.DoubleType"/> из таблицы <see cref="DataTypesTable"/>.
        /// Сравниваются только <see cref="DataTypesTable.DoubleType"/>.
        /// </summary>
        /// <param name="firstValue">Первое значение</param>
        /// <param name="secondValue">Второе значение</param>
        /// <param name="operation">Операция сравнения - лексема <see cref="Lexemes"/></param>
        /// <returns><see cref="LexemeValue"/> с уже известным результатом <see cref="LexemeValue.BoolValue"/></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static LexemeValue CompareDoubleTypes(LexemeValue firstValue, LexemeValue secondValue, Lexemes operation)
        {
            LexemeValue result = new LexemeValue();
            switch (operation)
            {
                case Lexemes.TypeEqualSign:
                    result.BoolValue = firstValue.DoubleValue == secondValue.DoubleValue;
                    return result;
                case Lexemes.TypeNotEqualSign:
                    result.BoolValue = firstValue.DoubleValue != secondValue.DoubleValue;
                    return result;
                case Lexemes.TypeMoreSign:
                    result.BoolValue = firstValue.DoubleValue > secondValue.DoubleValue;
                    return result;
                case Lexemes.TypeMoreOrEqualSign:
                    result.BoolValue = firstValue.DoubleValue >= secondValue.DoubleValue;
                    return result;
                case Lexemes.TypeLessSign:
                    result.BoolValue = firstValue.DoubleValue < secondValue.DoubleValue;
                    return result;
                case Lexemes.TypeLessOrEqualSign:
                    result.BoolValue = firstValue.DoubleValue <= secondValue.DoubleValue;
                    return result;
                default:
                    throw new InvalidOperationException($"{operation} не является операцией сравнения");
            }
        }

        public static bool GetBoolValueOfString(string pattern) => pattern == "true" ? true: false;
    }
}
