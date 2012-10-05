using System;
using System.Collections.Generic;
using Fsm;

namespace BnfToDfa
{
	class AlternationExpression
		: IExpression
	{
		private readonly List<IExpression> expressions;

		public AlternationExpression()
		{
			this.expressions = new List<IExpression>();
		}

		public void Add(IExpression expression)
		{
			expressions.Add(expression);
		}

		public State GetNfa(RulePath path, GetNfaParams param)
		{
			return State.NoCloneAlternation(Builder.BaseExpression.GetNfasArray(path, expressions, param));
		}
	}
}
