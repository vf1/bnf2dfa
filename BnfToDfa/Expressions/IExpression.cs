using System;
using System.Collections.Generic;
using Fsm;

namespace BnfToDfa
{
	class GetNfaParams
	{
		private Func<State, RulePath, State> markAction;

		public GetNfaParams(Func<State, RulePath, State> markAction)
		{
			this.markAction = markAction;
		}

		public State OnMarkRule(State start, RulePath path)
		{
			if (markAction != null)
				return markAction(start, path);

			return start;
		}
	}

	interface IExpression
	{
		State GetNfa(RulePath path, GetNfaParams param);
	}
}
