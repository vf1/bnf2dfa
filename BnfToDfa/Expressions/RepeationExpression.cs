using System;
using System.Collections.Generic;
using Fsm;

namespace BnfToDfa
{
	class RepeationExpression
		: IExpression
	{
		private readonly IExpression value;
		private readonly int count1;
		private readonly int count2;

		public RepeationExpression(int count1, int count2, IExpression value)
		{
			this.count1 = count1;
			this.count2 = count2;
			this.value = value;
		}

		public State GetNfa(RulePath path, GetNfaParams param)
		{
			return State.Repeat(count1, count2, value.GetNfa(path, param));
		}
	}
}
