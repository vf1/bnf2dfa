using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fsm
{
	public static class Nfa1
	{
		public static State To(this char from, char to)
		{
			return Thompson.Create(from, to);
		}

		public static State To(this int from, int to)
		{
			return Thompson.Create((byte)from, (byte)to);
		}

		public static State Nfa(this string string1)
		{
			return Thompson.Create(string1);
		}
	}
}
