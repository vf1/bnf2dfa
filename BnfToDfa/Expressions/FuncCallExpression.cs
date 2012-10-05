using System;
using System.Collections.Generic;
using Fsm;

namespace BnfToDfa
{
	class FuncCallExpression
		: IExpression
	{
		private readonly string name;
		private readonly List<IExpression> arguments;

		public FuncCallExpression(string name)
		{
			this.name = name;
			this.arguments = new List<IExpression>();
		}

		public void AddArgument(IExpression arg)
		{
			arguments.Add(arg);
		}

		public State GetNfa(RulePath path, GetNfaParams param)
		{
			switch (name)
			{
				case "RepeatBy":
					return State.NoCloneRepeatBy(arguments[0].GetNfa(path, param), arguments[1].GetNfa(path, param));
				default:
					throw new NotImplementedException(string.Format(@"@FuncCallExpression: |{0}| function is not defined", name));
			}
		}
	}
}
