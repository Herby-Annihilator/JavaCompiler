using Compiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynaxAnalyzer
{
    public static class EntityDataPrinter
    {
        public static void Print(EntityData entityData, int dataType)
        {
            if (entityData == null)
                throw new ArgumentNullException(nameof(entityData));
            if (dataType == DataTypesTable.IntegerType)
            {
                Console.WriteLine($"{entityData.LexemeImage} = {entityData.LexemeValue.IntegerValue} " +
                    $"({DataTypesTable.GetTypeName(dataType)})");
            }
            else if (dataType == DataTypesTable.DoubleType)
            {
                Console.WriteLine($"{entityData.LexemeImage} = {entityData.LexemeValue.DoubleValue} " +
                    $"({DataTypesTable.GetTypeName(dataType)})");
            }
            else if (dataType == DataTypesTable.BoolType)
            {
                Console.WriteLine($"{entityData.LexemeImage} = {entityData.LexemeValue.BoolValue} " +
                    $"({DataTypesTable.GetTypeName(dataType)})");
            }
        }
    }
}
