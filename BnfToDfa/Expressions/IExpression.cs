using System;
using System.Collections.Generic;
using Fsm;

namespace BnfToDfa
{
	interface IExpression
	{
		State GetNfa(RulePath path);
	}
}
