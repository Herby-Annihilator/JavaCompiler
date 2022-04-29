using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Compiler;

namespace SynaxAnalyzer
{
    /// <summary>
    /// Класс предоставляет методы для осуществления арифметических операций над значениями <see cref="LexemeValue"/>
    /// </summary>
    public static class LexemeValueCalculator
    {
        /// <summary>
        /// Применяет арифметическую операцию <paramref name="operation"/> к значениям <paramref name="firstValue"/> и
        /// <paramref name="secondValue"/> в соответствии с их типами <paramref name="firstValueType"/> и
        /// <paramref name="secondValueType"/> соответственно.
        /// </summary>
        /// <param name="firstValueType">Тип первого значения</param>
        /// <param name="firstValue">Первое значение</param>
        /// <param name="secondValueType">Тип второго значения</param>
        /// <param name="secondValue">второе значение</param>
        /// <param name="operation">Арифметическая операция из лексем <see cref="Lexemes"/></param>
        /// <param name="resultType">Тип результата в соответствии с таблицей <see cref="DataTypesTable"/></param>
        /// <returns><see cref="LexemeValue"/> с установленным занчением <see cref="LexemeValue.IntegerValue"/> 
        /// или <see cref="LexemeValue.DoubleValue"/> в зависимости от типов передаваемых значений</returns>
        public static LexemeValue ApplyArithmeticOperation(int firstValueType, LexemeValue firstValue,
            int secondValueType, LexemeValue secondValue, Lexemes operation, out int resultType)
        { 
            if (firstValueType == DataTypesTable.IntegerType && secondValueType == DataTypesTable.IntegerType)
            {
                resultType = DataTypesTable.IntegerType;
                return ApplyArithmeticOperationToIntegerValues(firstValue, secondValue, operation);
            }
            else
            {
                if (firstValueType == DataTypesTable.IntegerType)
                    firstValue.DoubleValue = firstValue.IntegerValue;
                if (secondValueType == DataTypesTable.IntegerType)
                    secondValue.DoubleValue = secondValue.IntegerValue;
                resultType = DataTypesTable.DoubleType;
                return ApplyArithmeticOperationToDoubleValues(firstValue, secondValue, operation);
            }
        }
        /// <summary>
        /// Применяет арфиметическую операцию <see cref="Lexemes"/> 
        /// к значениями типа <see cref="DataTypesTable.IntegerType"/>
        /// </summary>
        /// <param name="firstValue">Первое значение</param>
        /// <param name="secondValue">Второе значение</param>
        /// <param name="operation">Арифметическая операция из лексем <see cref="Lexemes"/></param>
        /// <returns><see cref="LexemeValue"/> с вычисленным значением <see cref="LexemeValue.IntegerValue"/></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static LexemeValue ApplyArithmeticOperationToIntegerValues(LexemeValue firstValue,
            LexemeValue secondValue, Lexemes operation)
        {
            LexemeValue result = new LexemeValue();
            switch (operation)
            {
                case Lexemes.TypePlus:
                    result.IntegerValue = firstValue.IntegerValue + secondValue.IntegerValue;
                    return result;
                case Lexemes.TypeMinus:
                    result.IntegerValue = firstValue.IntegerValue - secondValue.IntegerValue;
                    return result;
                case Lexemes.TypeMult:
                    result.IntegerValue = firstValue.IntegerValue * secondValue.IntegerValue;
                    return result;
                case Lexemes.TypeDiv:
                    result.IntegerValue = firstValue.IntegerValue / secondValue.IntegerValue;
                    return result;
                case Lexemes.TypeMod:
                    result.IntegerValue = firstValue.IntegerValue % secondValue.IntegerValue;
                    return result;
                default:
                    throw new InvalidOperationException($"{operation} не является арифметической операцией.");
            }
        }
        /// <summary>
        /// Применяет арфиметическую операцию <see cref="Lexemes"/> 
        /// к значениями типа <see cref="DataTypesTable.DoubleType"/>
        /// </summary>
        /// <param name="firstValue">Первое значение</param>
        /// <param name="secondValue">Второе значение</param>
        /// <param name="operation">Арифметическая операция из лексем <see cref="Lexemes"/></param>
        /// <returns><see cref="LexemeValue"/> с вычисленным значением <see cref="LexemeValue.DoubleValue"/></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static LexemeValue ApplyArithmeticOperationToDoubleValues(LexemeValue firstValue,
            LexemeValue secondValue, Lexemes operation)
        {
            LexemeValue result = new LexemeValue();
            switch (operation)
            {
                case Lexemes.TypePlus:
                    result.DoubleValue = firstValue.DoubleValue + secondValue.DoubleValue;
                    return result;
                case Lexemes.TypeMinus:
                    result.DoubleValue = firstValue.DoubleValue - secondValue.DoubleValue;
                    return result;
                case Lexemes.TypeMult:
                    result.DoubleValue = firstValue.DoubleValue * secondValue.DoubleValue;
                    return result;
                case Lexemes.TypeDiv:
                    result.DoubleValue = firstValue.DoubleValue / secondValue.DoubleValue;
                    return result;
                case Lexemes.TypeMod:
                    result.DoubleValue = firstValue.DoubleValue % secondValue.DoubleValue;
                    return result;
                default:
                    throw new InvalidOperationException($"{operation} не является арифметической операцией.");
            }
        }
    }
}
