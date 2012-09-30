using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fsm
{
	static class DfaIntersect
	{
		public static DfaState Intersect(DfaState dfa1, DfaState dfa2)
		{
			var list1 = new List<DfaState>();
			var list2 = new List<DfaState>();

			dfa1.ForEachNR((state) => { list1.Add(state); });
			dfa2.ForEachNR((state) => { list2.Add(state); });

			var states = new DfaState[list1.Count, list2.Count];

			for (int i = 0; i < list1.Count; i++)
				for (int j = 0; j < list2.Count; j++)
				{
					list1[i].Index = i;
					list2[j].Index = j;

					var ids = new List<int>();
					ids.AddRange(list1[i].NfaIds);
					ids.AddRange(list2[j].NfaIds);

					states[i, j] = new DfaState(ids.ToArray());
				}

			for (int i = 0; i < list1.Count; i++)
				for (int j = 0; j < list2.Count; j++)
				{
					var state = states[i, j];

					for (int ch = 0; ch <= 255; ch++)
					{
						byte key = (byte)ch;
						var node1 = list1[i].Transition[key];
						var node2 = list2[j].Transition[key];

						if (node1 != null && node2 != null)
							state.AddTransition(key, states[node1.Index, node2.Index]);
					}
				}

			var result = states[0, 0];
			//result.Minimize4(false);

			return result;
		}
	}
}
