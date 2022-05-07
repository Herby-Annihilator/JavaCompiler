using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    /// <summary>
    /// Операнд
    /// </summary>
    public class Operand
    {
        /// <summary>
        /// Имя операнда
        /// </summary>
        public Lexemes Lexemes { get; set; }
        /// <summary>
        /// Указывает, является ли операнд ссылкой на триаду
        /// </summary>
        public bool IsLink { get; set; } = false;
    }

    /// <summary>
    /// Триада
    /// </summary>
    public class Triad
    {
        /// <summary>
        /// Id триады
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Массив операндов, размер = 2, т.к. в триаде только два операнда
        /// </summary>
        public Operand[] Operands { get; private set; } = new Operand[2];
    }
}
