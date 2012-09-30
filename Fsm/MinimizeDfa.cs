using System;
using System.Collections.Generic;
using System.Linq;

namespace Fsm
{
	public static class MinimizeDfa
	{
		public static int Minimize(this DfaState start, bool showProgress)
		{
			return Minimize5(start, showProgress);
		}

		#region Version #1

		//public static int Minimize(this DfaState start, bool showProgress)
		//{
		//    if (showProgress)
		//        Console.WriteLine("Minimize DFA");

		//    var states = new List<DfaState>();
		//    start.ForEach((state) => { state.SetId = 0; states.Add(state); });

		//    int id = 1;

		//    foreach (var state in states)
		//    {
		//        if (state.HasMarks)
		//        {
		//            foreach (var state2 in states)
		//                if (state.IsSame(state2))
		//                    if (state2.SetId > 0)
		//                    {
		//                        state.SetId = state2.SetId;
		//                        break;
		//                    }

		//            if (state.SetId < 1)
		//                state.SetId = id++;
		//        }
		//        else
		//            state.SetId = 0;
		//    }
		//    if (GetFirstInSet(states, 0) == null)
		//        return -1; // nothing to minimize


		//    for (; ; )
		//    {
		//        int oldId = id;

		//        for (int i = 0; i < id; i++)
		//        {
		//            DfaState etalon = GetFirstInSet(states, i);

		//            bool splitted = false;
		//            foreach (var state in GetSet(states, i))
		//            {
		//                for (int ch = 0; ch <= byte.MaxValue; ch++)
		//                {
		//                    if (etalon.GetTransitedSetId(ch) != state.GetTransitedSetId(ch))
		//                    {
		//                        state.SetId = id;
		//                        splitted = true;
		//                        break;
		//                    }
		//                }
		//            }

		//            if (splitted)
		//            {
		//                id++;
		//                Console.Write("{0}\r", id);
		//            }
		//        }

		//        if (oldId == id)
		//            break;
		//    }

		//    // create minimized dfa

		//    var miniStates = new DfaState[id];
		//    for (int i = 0; i < id; i++)
		//    {
		//        if (i == start.SetId)
		//            miniStates[i] = start;
		//        else
		//            miniStates[i] = GetFirstInSet(states, i);
		//    }

		//    foreach (var state in miniStates)
		//    {
		//        for (int ch = 0; ch <= byte.MaxValue; ch++)
		//        {
		//            int transitedSetId = state.GetTransitedSetId((byte)ch);
		//            if (transitedSetId >= 0)
		//                state.SetTransition((byte)ch, miniStates[transitedSetId]);
		//            else
		//                if (state.Transition[ch] != null)
		//                    throw new InvalidProgramException();
		//        }
		//    }

		//    if (showProgress)
		//        Console.WriteLine("Done ({0})", id);

		//    return id;
		//}

		private static DfaState GetFirstInSet(List<DfaState> states, int setId)
		{
			foreach (var state in states)
				if (state.SetId == setId)
					return state;
			return null;
		}

		private static IEnumerable<DfaState> GetSet(List<DfaState> states, int setId)
		{
			foreach (var state in states)
				if (state.SetId == setId)
					yield return state;
		}

		#endregion

		#region Version #2

		public static int Minimize2(this DfaState start, bool showProgress)
		{
			if (showProgress)
				Console.WriteLine("Minimize DFA v.2");

			var states = new List<DfaState>();
			start.ForEach((state) => { state.SetId = 0; states.Add(state); });

			int id = 1;

			if (showProgress)
				Console.Write("Proccessing marks...\r");
			var startTime = DateTime.Now;

			foreach (var state in states)
			{
				if (state.HasMarks)
				{
					foreach (var state2 in states)
						if (state.IsSame(state2))
							if (state2.SetId > 0)
							{
								state.SetId = state2.SetId;
								break;
							}

					if (state.SetId < 1)
						state.SetId = id++;
				}
				else
					state.SetId = 0;
			}

			if (showProgress)
				Console.WriteLine("Marks: {0}\t\t", DateTime.Now - startTime);

			if (GetFirstInSet(states, 0) == null)
				return -1; // nothing to minimize

			for (int xid = 0; xid < id; )
			{
				xid = id;

				for (int i = 0; i < id; i++)
				{
					DfaState first = GetFirstInSet(states, i);

					bool splitted = false;
					foreach (var state in GetSet(states, i))
					{
						if (IsTransitedToSameSets(first, state) == false)
						{
							state.NewSetId = id;
							splitted = true;
						}
					}

					if (splitted)
					{
						foreach (var state in GetSet(states, i))
							state.SetId = state.NewSetId;

						id++;
						Console.Write("{0}\r", id);
					}
				}
			}

			if (showProgress)
				Console.Write("Create MiniDFA\r", id);

			CreateMiniDfa(start, states, id);

			if (showProgress)
				Console.WriteLine("Done ({0}, {1})\t\t\t\t", id, DateTime.Now - startTime);

			return id;
		}

		private static void CreateMiniDfa(DfaState start, List<DfaState> states, int id)
		{
			var miniStates = new DfaState[id];

			for (int i = 0; i < id; i++)
			{
				if (i == start.SetId)
					miniStates[i] = start;
				else
					miniStates[i] = GetFirstInSet(states, i);
			}

			foreach (var state in miniStates)
			{
				for (int ch = 0; ch <= byte.MaxValue; ch++)
				{
					int transitedSetId = state.GetTransitedSetId(ch);
					if (transitedSetId >= 0)
						state.SetTransition((byte)ch, miniStates[transitedSetId]);
					else
						if (state.Transition[ch] != null)
							throw new InvalidProgramException();
				}
			}
		}

		private static bool IsTransitedToSameSets(DfaState state1, DfaState state2)
		{
			for (int ch = 0; ch <= byte.MaxValue; ch++)
				if (state1.GetTransitedSetId(ch) != state2.GetTransitedSetId(ch))
					return false;
			return true;
		}

		#endregion

		#region Version #3

		public static int Minimize3(this DfaState start, bool showProgress)
		{
			if (showProgress)
				Console.WriteLine("Minimize DFA v.3");

			var states = new List<DfaState>(3000000);
			start.ForEach((state) => { state.SetId = 0; states.Add(state); });

			int id = 1;

			foreach (var state in states)
			{
				if (state.HasMarks)
				{
					foreach (var state2 in states)
						if (state2.HasMarks)
						{
							if (state.IsSame(state2))
								if (state2.SetId > 0)
								{
									state.SetId = state2.SetId;
									break;
								}
						}

					if (state.SetId < 1)
						state.SetId = id++;
				}
				else
					state.SetId = 0;
			}

			if (GetFirstInSet(states, 0) == null)
				return -1; // nothing to minimize


			DfaState[] firsts = null;
			int[] ids = null;

			for (int xid = 0; xid < id; )
			{
				xid = id;

				PrepareArrays(ref firsts, ref ids, id);

				foreach (var state in states)
				{
					if (firsts[state.SetId] == null)
						firsts[state.SetId] = state;
					else
					{
						if (IsTransitedToSameSets(firsts[state.SetId], state) == false)
						{
							if (ids[state.SetId] == 0)
								ids[state.SetId] = id++;

							state.NewSetId = ids[state.SetId];
						}
					}
				}

				foreach (var state in states)
					state.SetId = state.NewSetId;

				Console.Write("{0}\r", id);
			}

			if (showProgress)
				Console.Write("Create MiniDFA\r", id);

			CreateMiniDfa(start, states, id);

			if (showProgress)
				Console.WriteLine("Done ({0})\t\t\t\t", id);

			return id;
		}

		private static void PrepareArrays(ref DfaState[] firsts, ref int[] ids, int id)
		{
			if (firsts == null)
			{
				firsts = new DfaState[512];
				ids = new int[512];
			}
			else
			{
				if (id > firsts.Length)
				{
					firsts = new DfaState[id + 8192];
					ids = new int[id + 8192];
				}

				Array.Clear(firsts, 0, firsts.Length);
				Array.Clear(ids, 0, ids.Length);
			}
		}

		#endregion

		#region Version #4

		private static Dictionary<int, int> _setidToIndex = new Dictionary<int, int>(20000);

		private static DfaState GetFirstInSet4(List<DfaState> states, int setId)
		{
			if (_setidToIndex.ContainsKey(setId))
			{
				for (int i = _setidToIndex[setId]; i < states.Count; i++)
					if (states[i].SetId == setId)
						return states[i];
			}
			else
			{
				for (int i = 0; i < states.Count; i++)
					if (states[i].SetId == setId)
					{
						_setidToIndex.Add(setId, i);
						return states[i];
					}
			}

			return null;
		}

		private static IEnumerable<DfaState> GetSet4(List<DfaState> states, int setId)
		{
			if (_setidToIndex.ContainsKey(setId))
			{
				for (int i = _setidToIndex[setId]; i < states.Count; i++)
					if (states[i].SetId == setId)
						yield return states[i];
			}
			else
			{
				for (int i = 0; i < states.Count; i++)
					if (states[i].SetId == setId)
					{
						_setidToIndex.Add(setId, i);
						yield return states[i];
					}
			}
		}

		private static void MoveToSet(List<HashSet<DfaState>> sets, DfaState state, int setId)
		{
			while (sets.Count <= setId)
				sets.Add(new HashSet<DfaState>());

			sets[state.SetId].Remove(state);
			state.SetId = setId;
			sets[state.SetId].Add(state);
		}

		private static void CopyToSet(List<HashSet<DfaState>> sets, DfaState state, int setId)
		{
			while (sets.Count <= setId)
				sets.Add(new HashSet<DfaState>());

			state.SetId = setId;
			sets[state.SetId].Add(state);
		}

		private static DfaState GetOne(HashSet<DfaState> states)
		{
			foreach (var state in states)
				return state;
			return null;
		}

		public static int Minimize4(this DfaState start, bool showProgress)
		{
			var startTime = DateTime.Now;

			if (showProgress)
				Console.WriteLine("Minimize DFA v.4");

			var states = new List<DfaState>(3000000);
			var sets = new List<HashSet<DfaState>>(50000);
			sets.Add(new HashSet<DfaState>());

			start.ForEach((state) => { state.SetId = 0; sets[0].Add(state); states.Add(state); });

			if (showProgress)
				Console.WriteLine("Create list: {0}", DateTime.Now - startTime);

			int id = 1;

			if (showProgress)
				Console.Write("Proccessing marks...\r");

			var startTime2 = DateTime.Now;

			foreach (var state in sets[0])
			{
				if (state.AllMarks.Count > 0)
				{
					for (int i = 1; i < sets.Count; i++)
					{
						var state2 = GetOne(sets[i]);

						if (state.IsSame(state2))
						{
							CopyToSet(sets, state, state2.SetId);
							break;
						}
					}

					if (state.SetId == 0)
						CopyToSet(sets, state, id++);
				}
			}
			sets[0].RemoveWhere((state) => { return state.SetId != 0; });

			if (showProgress)
				Console.WriteLine("Marks: {0}\t\t", DateTime.Now - startTime2);

			for (int xid = 0; xid < id; )
			{
				xid = id;

				for (int i = 0; i < id; i++)
				{
					DfaState first = GetOne(sets[i]);

					bool splitted = false;
					foreach (var state in sets[i])
					{
						if (IsTransitedToSameSets(first, state) == false)
						{
							state.NewSetId = id;
							splitted = true;
						}
					}

					if (splitted)
					{
						foreach (var state in sets[i])
							CopyToSet(sets, state, state.NewSetId);
						sets[i].RemoveWhere((state) => { return state.SetId != i; });

						id++;
						Console.Write("{0}\r", id);
					}
				}
			}

			if (showProgress)
				Console.Write("Create MiniDFA\r", id);

			CreateMiniDfa(start, states, id);

			if (showProgress)
				Console.WriteLine("Done ({0}, {1})\t\t\t\t", id, DateTime.Now - startTime);

			return id;
		}

		#endregion

		#region Version #4b

		public static int Minimize4b(this DfaState start, bool showProgress)
		{
			var startTime = DateTime.Now;

			if (showProgress)
				Console.WriteLine("Minimize DFA v.4b");

			var states = new List<DfaState>(3000000);
			start.ForEachNR((state) => { states.Add(state); });

			var sets = new IntSet(states);

			if (showProgress)
				Console.WriteLine("Create list: {0}", DateTime.Now - startTime);

			int id = 1;

			if (showProgress)
				Console.Write("Proccessing marks...\r");

			var startTime2 = DateTime.Now;

			foreach (var state in sets.GetSet(0))
			{
				if (state.Value.AllMarks.Count > 0)
				{
					for (int i = 1; i < sets.Count; i++)
					{
						var state2 = sets.GetFirst(i);

						if (state.Value.IsSame(state2))
						{
							sets.MoveToSet(state, state2.SetId);
							break;
						}
					}

					if (state.Value.SetId == 0)
						sets.MoveToSet(state, id++);
				}
			}

			if (showProgress)
			{
				Console.WriteLine("Marks: {0}\t\t", DateTime.Now - startTime2);
				Console.WriteLine("Splitted by marks to {0} sets", sets.Count);
			}

			for (int xid = 0; xid < id; )
			{
				xid = id;

				for (int i = 0; i < id; i++)
				{
					DfaState first = sets.GetFirst(i);

					bool splitted = false;
					foreach (var state in sets.GetSet(i))
					{
						if (IsTransitedToSameSets4(first, state.Value) == false)
						{
							state.Value.NewSetId = id;
							splitted = true;
						}
					}

					if (splitted)
					{
						foreach (var state in sets.GetSet(i))
							sets.MoveToSet(state, state.Value.NewSetId);

						id++;
						Console.Write("{0}\r", id);
					}
				}
			}

			if (showProgress)
				Console.Write("Create MiniDFA\r", id);

			CreateMiniDfa4(start, states, sets);

			if (showProgress)
				Console.WriteLine("Done ({0}, {1})\t\t\t\t", id, DateTime.Now - startTime);

			return id;
		}

		private static void CreateMiniDfa4(DfaState start, List<DfaState> states, IntSet sets)
		{
			var miniStates = new DfaState[sets.Count];

			for (int i = 0; i < sets.Count; i++)
			{
				if (i == start.SetId)
					miniStates[i] = start;
				else
					miniStates[i] = sets.GetFirst(i);
			}

			foreach (var state in miniStates)
			{
				for (int ch = 0; ch <= byte.MaxValue; ch++)
				{
					int transitedSetId = state.GetTransitedSetId(ch);
					if (transitedSetId >= 0)
						state.SetTransition((byte)ch, miniStates[transitedSetId]);
					else
						if (state.Transition[ch] != null)
							throw new InvalidProgramException();
				}
			}
		}

		private static bool IsTransitedToSameSets4(DfaState state1, DfaState state2)
		{
			for (int ch = 0; ch <= byte.MaxValue; ch++)
			{
				if (state1.Transition[ch] != null)
				{
					if (state2.Transition[ch] != null)
					{
						if (state1.Transition[ch].SetId != state2.Transition[ch].SetId)
							return false;
					}
					else
						return false;
				}
				else
				{
					if (state2.Transition[ch] != null)
						return false;
				}
			}

			return true;
		}

		#endregion

		#region Version #5

		public static int Minimize5(this DfaState start, bool showProgress)
		{
			var startTime = DateTime.Now;

			if (showProgress)
				Console.WriteLine("Minimize DFA v.5");

			var states = new List<DfaState>(3000000);
			start.ForEachNR((state) => { states.Add(state); });

			var sets = new IntSet(states);

			if (showProgress)
				Console.WriteLine("Create list: {0}", DateTime.Now - startTime);

			if (showProgress)
				Console.Write("Proccessing marks...\r");

			var startTime2 = DateTime.Now;

			foreach (var state in sets.GetSet(0))
			{
				if (state.Value.AllMarks.Count > 0)
				{
					for (int i = 1; i < sets.Count; i++)
					{
						var state2 = sets.GetFirst(i);

						if (state.Value.IsSame(state2))
						{
							sets.MoveToSet(state, state2.SetId);
							break;
						}
					}

					if (state.Value.SetId == 0)
						sets.MoveToNewSet(state);
				}
			}

			sets.RemoveEmpty();

			if (showProgress)
			{
				Console.WriteLine("Marks: {0}\t\t", DateTime.Now - startTime2);
				Console.WriteLine("Splitted by marks to {0} sets", sets.Count);
			}

			for (int oldCount = -1; oldCount < sets.Count; )
			{
				oldCount = sets.Count;

				for (int i = 0; i < sets.Count; i++)
				{
					DfaState first = sets.GetFirst(i);

					bool splitted = false;
					foreach (var state in sets.GetSet(i))
					{
						if (IsTransitedToSameSets4(first, state.Value) == false)
						{
							state.Value.NewSetId = sets.NewSetId;
							splitted = true;
						}
					}

					if (splitted)
					{
						foreach (var state in sets.GetSet(i))
							sets.MoveToSet(state, state.Value.NewSetId);

						if (showProgress)
							Console.Write("{0}\r", sets.Count);
					}
				}
			}

			if (showProgress)
				Console.Write("Create MiniDFA\r");

			CreateMiniDfa4(start, states, sets);

			if (showProgress)
				Console.WriteLine("Done ({0}, {1})\t\t\t\t", sets.Count, DateTime.Now - startTime);

			return sets.Count;
		}

		#endregion
	}
}
