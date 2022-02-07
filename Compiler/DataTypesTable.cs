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
    }
}
