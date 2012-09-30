using System;
using System.Collections.Generic;

namespace Fsm
{
	class IntArrayComparer
		: IEqualityComparer<int[]>
	{
		public bool Equals(int[] x, int[] y)
		{
			if (x.Length != y.Length)
				return false;
			for (int i = 0; i < x.Length; i++)
				if (x[i] != y[i])
					return false;
			return true;
		}

		public int GetHashCode(int[] array)
		{
			int hash = 0;

			for (int i = 0; i < array.Length; i++)
				hash += array[i];

			return hash;
		}
	}
}
