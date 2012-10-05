using System;
using System.Collections.Generic;
using Fsm;

namespace BnfToDfa
{
	class ConcatanationExpression
		: IExpression
	{
		private readonly List<IExpression> expressions;

		public ConcatanationExpression()
		{
			expressions = new List<IExpression>();
		}

		public void Add(IExpression expression)
		{
			expressions.Add(expression);
		}

		public State GetNfa(RulePath path, GetNfaParams param)
		{
			return State.NoCloneConcatanation(Builder.BaseExpression.GetNfasArray(path, expressions, param));
		}
	}
}
