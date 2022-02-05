using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public class Node
    {
        public NameCategory Category { get; set; }
        public string Name { get; set; }
        public Lexemes Type { get; set; }

    }
}
