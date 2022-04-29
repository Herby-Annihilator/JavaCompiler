using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public class LexemeValue
    {
        public int IntegerValue { get; set; }
        public double DoubleValue { get; set; }
        public bool BoolValue { get; set; }

        public LexemeValue Clone() =>
            new LexemeValue() { BoolValue = BoolValue, DoubleValue = DoubleValue, IntegerValue = IntegerValue };

        public override string ToString() => 
            $"IntegerValue = {IntegerValue}, DoubleValue = {DoubleValue}, BoolValue = {BoolValue}";
    }
}
