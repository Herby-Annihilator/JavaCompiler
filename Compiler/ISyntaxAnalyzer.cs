using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
	public interface ISyntaxAnalyzer
	{
		void Program();
		void ClassDescription();
		void Descriptions();
		void DataDescription();
		void NamedConstant();
		void Data();
		void FunctionDescription();
		void Operator();
		void CompoundOperator();
		void CompoundOperatorBody();
		void SimpleOperator();
		void AssignmentOperator();
		void Name();
		void WhileCycle();
		void FunctionCall();
		void Expression();
		void FirstLevel();
		void SecondLevel();
		void ThirdLevel();
		void FourthLevel();
		void FifthLevel();
	}
}
