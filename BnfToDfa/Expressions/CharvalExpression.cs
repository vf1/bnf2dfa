using System;
using System.Collections.Generic;
using Fsm;

namespace BnfToDfa
{
	class CharvalExpression
		: Builder.BaseExpression
		, IExpression
	{
		private readonly string chars;

		public CharvalExpression(Builder builder, string chars)
		{
			this.chars = chars;
		}

		public State GetNfa(RulePath path, GetNfaParams param)
		{
			return param.OnMarkRule(Thompson.Create(chars), path + chars);
		}
	}
}
