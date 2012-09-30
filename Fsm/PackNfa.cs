using System;
using System.Collections.Generic;

namespace Fsm
{
	public static class PackNfa
	{
		public static int Pack(this State start, bool showProgress)
		{
			int i, total = 0, count = 0;

			if (showProgress)
				Console.WriteLine("Pack NFA...");

			do
			{
				i = PackInternal(start);
				total += i;
				count++;

				if (showProgress)
					Console.Write("\rPacked by step: {0}\t", i);
			}
			while (i > 0);

			if (showProgress)
				Console.WriteLine("\rDone ({0}/{1}).\t\t\t", count, total);

			return total;
		}

		private static int PackInternal(State start)
		{
			var optimizes = new List<State>();

			start.ForEach(
				new HashSet<State>(),
				(curent, key, next) =>
				{
					if (curent.Transition.GetOne(State.Epsilon) != null)
						optimizes.Add(curent);
				});

			foreach (var state in optimizes)
			{
				var nexts = new List<State>();
				nexts.AddRange(state.Transition.Get(State.Epsilon));

				foreach (var next in nexts)
				{
					state.Transition.Remove(State.Epsilon, next);

					if (state != next)
					{
						state.Transition.AddAll(State.Epsilon, next.Transition.Get(State.Epsilon));
						for (int i = 0; i <= 255; i++)
						{
							byte key = (byte)i;
							state.Transition.AddAll(key, next.Transition.Get(key));
						}

						state.AddRangeMarks(next.AllMarks);
					}

					//state.PackedStates.Add(next.Id);
					//state.PackedStates.AddRange(next.PackedStates);
				}
			}

			return optimizes.Count;
		}
	}
}
