using System;
using System.Collections.Generic;
using Fsm;

namespace BnfToDfa
{
	class OptionExpression
		: IExpression
	{
		private readonly IExpression expression;

		public OptionExpression(IExpression expression)
		{
			this.expression = expression;
		}

		public State GetNfa(RulePath path)
		{
			return State.NoCloneOption(expression.GetNfa(path));
		}
	}
}
