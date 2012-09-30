using System;
using System.Collections.Generic;
using Fsm;

namespace BnfToDfa
{
	class NumvalExpression
		: IExpression
	{
		private readonly byte? hex1;
		private readonly byte? hex2;
		private readonly List<byte> hexs;

		public NumvalExpression(byte hex1)
		{
			this.hex1 = hex1;
		}

		public NumvalExpression(byte hex1, byte hex2)
		{
			this.hex1 = hex1;
			this.hex2 = hex2;
		}

		public NumvalExpression()
		{
			hexs = new List<byte>();
		}

		public void Add(byte hex)
		{
			hexs.Add(hex);
		}

		public State GetNfa(RulePath path)
		{
			if (hex1 != null)
			{
				if (hex2 == null)
					return Thompson.Create(hex1.Value);
				else
					return Thompson.Create(hex1.Value, hex2.Value);
			}
			else
			{
				return Thompson.Create(hexs.ToArray());
			}
		}
	}
}
