using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
	/// <summary>
	/// Набор лексем языка, таких как точка, запятая, ключевые слова, и т.д.
	/// </summary>
	public enum Lexemes
	{
		TypeClass,
		TypeWhile,
		TypeDataDouble,
		TypeDataInt,
		TypeDataBool,  // добавление поддержики типа bool
		TypeReturn,
		TypeConst,

		TypeInt,
		TypeDouble,
		TypeBool,  // добавление поддержики типа bool

		TypeIdentifier,

		TypeLessSign,
		TypeLessOrEqualSign,
		TypeMoreSign,
		TypeMoreOrEqualSign,
		TypeEqualSign,
		TypeNotEqualSign,
		TypeAssignmentSign,

		TypePlus,
		TypeMinus,
		TypeIncrement,
		TypeDecrement,
		TypeMult,
		TypeDiv,
		TypeMod,

		TypeDot,
		TypeSemicolon,
		TypeOpenCurlyBrace,
		TypeCloseCurlyBrace,
		TypeOpenParenthesis,
		TypeCloseParenthesis,
		TypeComma,

		TypeError,
		TypeEnd,

	}
}
