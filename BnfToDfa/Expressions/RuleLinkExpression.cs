using System;
using System.Collections.Generic;
using Fsm;

namespace BnfToDfa
{
	class RuleLinkExpression
		: Builder.BaseExpression
		, IExpression
	{
		private readonly string name;
		private readonly Dictionary<string, AlternationExpression> rules;

		public RuleLinkExpression(Builder builder, string name, Dictionary<string, AlternationExpression> rules)
		{
			this.name = name;
			this.rules = rules;
		}

		public State GetNfa(RulePath path, GetNfaParams param)
		{
			if (rules.ContainsKey(name) == false)
				throw new Exception(string.Format(@"@RuleLinkExpression: |{0}| rule not found", name));

			var newPath = path + name;

			return param.OnMarkRule(rules[name].GetNfa(newPath, param), newPath);
		}
	}
}
