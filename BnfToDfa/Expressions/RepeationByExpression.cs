using System;
using System.Collections.Generic;
using Fsm;

namespace BnfToDfa
{
	class RepeationByExpression
		: IExpression
	{
		private readonly IExpression value;
		private readonly IExpression by;
		private readonly int count1;
		private readonly int count2;

		public RepeationByExpression(int count1, int count2, IExpression value, IExpression by)
		{
			this.count1 = count1;
			this.count2 = count2;
			this.value = value;
			this.by = by;
		}

		public State GetNfa(RulePath path, GetNfaParams param)
		{
			var result = State.NoCloneRepeatBy(value.GetNfa(path, param), by.GetNfa(path, param));

			if (count1 <= 0)
				result = State.NoCloneOption(result);

			if (count1 > 1)
				result = State.Repeat(count1, count2, result);

			return result;
		}
	}
}
