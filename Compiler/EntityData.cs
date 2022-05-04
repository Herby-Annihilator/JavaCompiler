using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    public class EntityData
    {
        #region Required
        /// <summary>
        /// Что обозначает лексема, например, int a = 10. Если LexemeImage = а, тогда Category = Variable
        /// </summary>
        public LexemeImageCategory Category { get; set; }

        /// <summary>
        /// Изображение лексемы
        /// </summary>
        public string LexemeImage { get; set; }

        /// <summary>
        /// Тип лексемы
        /// </summary>
        public Lexemes Lexeme { get; set; }
        #endregion

        #region Situational
        /// <summary>
        /// Значение для переменных или констант, например, а = 10, тогда LexemeImage = а, LexemeValue.IntegerValue = 10.
        /// </summary>
        public LexemeValue LexemeValue { get; set; }

        /// <summary>
        /// Флаг константы, например, const int a = 10, тогда LexemeImage = а, LexemeValue = 10, IsConstant = true. Чаще всего false
        /// </summary>
        public bool IsConstant { get; set; } 

        /// <summary>
        /// Тип данных лексемы из таблицы DataTypesTable. Если int a = 10, то DataType == int (значение 0)
        /// </summary>
        public int DataType { get; set; }
        /// <summary>
        /// Позиция в тексте - необходимо для того, чтобы выполнить тело функции.
        /// По сути - только для функции это поле имеет значение, отличное от -1
        /// </summary>
        public int LexerPosition { get; set; } = -1;
        #endregion

        public EntityData Clone()
        {
            EntityData result = new EntityData();
            result.Category = Category;
            result.LexemeImage = (string)LexemeImage.Clone();
            result.Lexeme = Lexeme;
            result.LexerPosition = LexerPosition;

            return result;
        }
    }
}
