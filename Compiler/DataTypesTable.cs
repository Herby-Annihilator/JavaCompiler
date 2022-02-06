using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public class DataTypesTable
    {
        private List<int> _types;
        private int _nextTypeIndex;

        public int IntegerType { get; private set; } = 0;
        public int DoubleType { get; private set; } = 1;
        public int VoidType { get; private set; } = 2;

        public DataTypesTable()
        {
            _types = new List<int>();
            _types.Add(IntegerType);
            _types.Add(DoubleType);
            _types.Add(VoidType);

            _nextTypeIndex = _types.Count;
        }

        public int AddType()
        {
            _types.Add(_nextTypeIndex);
            _nextTypeIndex++;
            return _types[_nextTypeIndex - 1];
        }

        public void RemoveType(int type)
        {
            _types.Remove(type);
        }

        public bool Contains(int type) => _types.Contains(type);
    }
}
