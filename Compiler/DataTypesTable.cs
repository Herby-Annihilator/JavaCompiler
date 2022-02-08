using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public class DataTypesTable
    {
        public int IntegerType { get; private set; } = 0;
        public int DoubleType { get; private set; } = 1;

        public int BoolType { get; private set; } = 2;

        public int UndefType { get; private set; } = 1000;

        public string TypeToString(int type)
        {
            if (type == IntegerType) return "int";
            if (type == DoubleType) return "double";
            if (type == BoolType) return "bool";
            if (type == UndefType) return "неопределенный тип";
            return "";
        }

        public int MixTypes(int type1, int type2)
        {
            if (type1 == UndefType || type2 == UndefType)
                return UndefType;
            if (type1 == IntegerType && type2 == IntegerType)
                return IntegerType;
            if (type1 == DoubleType || type2 == DoubleType)
                return DoubleType;
            return UndefType;
        }

        public bool CheckTypesCompatibility(int leftType, int rightType)
        {
            if (leftType == IntegerType && rightType == DoubleType)
                return false;
            return true;
        }

        public int OperationResultType(int type1, int type2)
        {
            if (type1 == UndefType || type2 == UndefType)
                throw new Exception("Невозможно использовать арифметическую операцию с неопределенным типом");
            if (type1 == BoolType || type2 == BoolType)
                throw new Exception("Невозможно использовать арифметическую операцию с логическим типом");
            if (type1 == DoubleType || type2 == DoubleType)
                return DoubleType;
            return type1;
        }

        public bool CanTwoTypesBeCompared(int leftType, int rightType, Lexemes comparingOperation)
        {
            if (leftType == UndefType || rightType == UndefType)
                return false;
            if (comparingOperation == Lexemes.TypeEqualSign || comparingOperation == Lexemes.TypeNotEqualSign)
                return true;
            if (comparingOperation == Lexemes.TypeLessOrEqualSign
                || comparingOperation == Lexemes.TypeMoreOrEqualSign
                || comparingOperation == Lexemes.TypeMoreSign
                || comparingOperation == Lexemes.TypeLessSign)
            {
                if (leftType == BoolType || rightType == BoolType)
                    return false;
                else
                    return true;
            }
            else
            {
                throw new Exception($"Недопустимая лексема {comparingOperation} в качестве логической операции");
            }
        }
    }
}
