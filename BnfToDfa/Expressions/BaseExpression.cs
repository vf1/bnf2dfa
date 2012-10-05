using System;
using System.Collections.Generic;
using Fsm;

namespace BnfToDfa
{
	partial class Builder
	{
		public class BaseExpression
		{
			public static IEnumerable<State> GetNfas(RulePath path, IEnumerable<IExpression> expressions, GetNfaParams param)
			{
				foreach (var expression in expressions)
					yield return expression.GetNfa(path, param);
			}

			public static State[] GetNfasArray(RulePath path, IEnumerable<IExpression> expressions, GetNfaParams param)
			{
				return new List<State>(GetNfas(path, expressions, param)).ToArray();
			}
		}
	}
}
