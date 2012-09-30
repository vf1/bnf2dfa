using System;
using System.Collections.Generic;
using Fsm;

namespace BnfToDfa
{
	partial class Builder
	{
		public class BaseExpression
		{
			private readonly Builder builder;

			public BaseExpression(Builder builder)
			{
				this.builder = builder;
			}

			public static IEnumerable<State> GetNfas(RulePath path, IEnumerable<IExpression> expressions)
			{
				foreach (var expression in expressions)
					yield return expression.GetNfa(path);
			}

			public static State[] GetNfasArray(RulePath path, IEnumerable<IExpression> expressions)
			{
				return new List<State>(GetNfas(path, expressions)).ToArray();
			}

			protected State OnMarkRule(State start, RulePath path)
			{
				return builder.OnMarkRule(start, path);
			}
		}
	}
}
