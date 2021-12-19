using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
	public enum Lexemes
	{
		TypeClass,
		TypeWhile,
		TypeDataDouble,
		TypeDataInt,
		TypeReturn,
		TypeConst,

		TypeInt,
		TypeDouble,

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
