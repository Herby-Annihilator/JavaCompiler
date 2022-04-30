using Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynaxAnalyzer
{
    /// <summary>
    /// Присваиватель значений
    /// </summary>
    public static class LexemeValueAssignor
    {
        /// <summary>
        /// Присваивает значение <paramref name="lexemeValue"/> соответствующему полю объекта <paramref name="entityData"/>
        /// и делает приведение типов при необходимости в соответствии с типом <see cref="EntityData.DataType"/>
        /// </summary>
        /// <param name="entityData">Обеъкт информации об сущности языка</param>
        /// <param name="lexemeValue">Значение, которое необходимо присвоить</param>
        /// <param name="lexemeValueType">Тип присваиваемого значения</param>
        /// <exception cref="Exception"></exception>
        public static void AssingValue(EntityData entityData, LexemeValue lexemeValue, int lexemeValueType)
        {
            if (entityData.DataType == DataTypesTable.UndefType)
            {
                entityData.LexemeValue = lexemeValue;
                entityData.DataType = lexemeValueType;
                return;
            }
            else if (entityData.DataType == DataTypesTable.UserType) // сюда попадать вообще не надо.
            {
                if (lexemeValueType != DataTypesTable.UserType)
                    throw new Exception($"Нельзя присвоить тип {DataTypesTable.GetTypeName(DataTypesTable.UserType)} " +
                        $"типу {DataTypesTable.GetTypeName(entityData.DataType)}");
                else
                {
                    entityData.LexemeValue = lexemeValue;
                    entityData.DataType = lexemeValueType;
                    return;
                }
            }
            else if (entityData.DataType == DataTypesTable.IntegerType)
            {
                if (lexemeValueType == DataTypesTable.UndefType 
                    || lexemeValueType == DataTypesTable.UserType
                    || lexemeValueType == DataTypesTable.BoolType)
                    throw new Exception($"Нельзя присвоить тип {DataTypesTable.GetTypeName(lexemeValueType)} " +
                        $"типу {DataTypesTable.GetTypeName(entityData.DataType)}");
                if (lexemeValueType == DataTypesTable.IntegerType)
                {
                    entityData.LexemeValue = lexemeValue;
                    entityData.DataType = lexemeValueType;
                }
                else // double
                {
                    // обрезаем
                    entityData.LexemeValue = new LexemeValue() { IntegerValue = (int)lexemeValue.DoubleValue };
                    entityData.DataType = DataTypesTable.IntegerType;
                }
                return;
            }
            else if (entityData.DataType == DataTypesTable.DoubleType)
            {
                if (lexemeValueType == DataTypesTable.UndefType
                    || lexemeValueType == DataTypesTable.UserType
                    || lexemeValueType == DataTypesTable.BoolType)
                    throw new Exception($"Нельзя присвоить тип {DataTypesTable.GetTypeName(lexemeValueType)} " +
                        $"типу {DataTypesTable.GetTypeName(entityData.DataType)}");
                if (lexemeValueType == DataTypesTable.DoubleType)
                {
                    entityData.LexemeValue = lexemeValue;
                    entityData.DataType = lexemeValueType;
                }
                else // int
                {
                    entityData.LexemeValue = new LexemeValue() { DoubleValue = lexemeValue.IntegerValue };
                    entityData.DataType = DataTypesTable.DoubleType;
                }
            }
        }
    }
}
