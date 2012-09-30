using System;
using System.Collections.Generic;
using Fsm;

namespace BnfToDfa
{
	class SubtractionExpression
		: IExpression
	{
		private readonly IExpression value1;
		private readonly IExpression value2;

		public SubtractionExpression(IExpression value1, IExpression value2)
		{
			this.value1 = value1;
			this.value2 = value2;
		}

		public State GetNfa(RulePath path)
		{
			return State.Substract(value1.GetNfa(path), value2.GetNfa(path));
		}
	}
}
