using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
	public enum  NameCategory
	{
		Variable,  // a, b, c
		SimpleType,  // int, double

		ClassObject,   // Human h = new Human(); // h is object
		ClassType,   // Human is type

		Function,

		Constant,  // const int a = 100; // a is constant

		Struct,
		StructType,

		ArrayObject,  // int[] a; // a is object
		ArrayType,  // int[], double[], ...

		Label  // start:
	}
}
