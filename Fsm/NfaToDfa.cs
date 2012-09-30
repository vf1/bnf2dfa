using System;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using SocketServers;
using System.Diagnostics;

namespace Fsm
{
	public static class NfaToDfa
	{
		public static HashSet<State> GetEclosure(this IEnumerable<State> states)
		{
			HashSet<State> eclosure = new HashSet<State>();

			foreach (var state in states)
				eclosure.UnionWith(state.GetCachedEclosure());

			return eclosure;
		}

		public static HashSet<State> GetMove(this IEnumerable<State> states, byte? ch)
		{
			HashSet<State> move = new HashSet<State>();

			foreach (var state in states)
				move.UnionWith(state.Transition.Get(ch));

			return move;
		}

		public static int[] GetSortedMoveEclosureIds(this IEnumerable<State> states, byte? ch)
		{
			var eclosure = new HashSet<int>();

			foreach (var state in states)
				foreach (var move in state.Transition.Get(ch))
					move.GetEclosureId(eclosure);


			var ids = new int[eclosure.Count];

			int i = 0;
			foreach (var id in eclosure)
				ids[i++] = id;

			Array.Sort<int>(ids);

			return ids;
		}

		//public static DfaState ToDfa(this State start, out int dfaCount, bool showProgress)
		//{
		//    if (showProgress)
		//    {
		//        Console.WriteLine("NFA to DFA Converting...");
		//        Console.WriteLine("{0}", DateTime.Now);
		//    }

		//    var dfaStates = new Dictionary<int[], DfaState>(new IntArrayComparer());
		//    var notMarkedStates = new Queue<DfaState>();

		//    var dfaStart = new DfaState(start.Eclosure());
		//    dfaStates.Add(dfaStart.GetNfaIds(), dfaStart);
		//    notMarkedStates.Enqueue(dfaStart);

		//    int i = 0;
		//    int time = Environment.TickCount;
		//    while (notMarkedStates.Count > 0)
		//    {
		//        DfaState t = notMarkedStates.Dequeue();

		//        if (showProgress && i++ % 100 == 0)
		//        {
		//            int time1 = Environment.TickCount;
		//            Console.Write("{2}\t({3})\t\t{0}\t\t{1}          ", dfaStates.Count, notMarkedStates.Count, dfaStates.Count - notMarkedStates.Count, time1 - time);
		//            Console.Write("\r");
		//            time = time1;
		//        }

		//        if (t != null)
		//        {
		//            var allChars = new HashSet<byte>();
		//            foreach (var nfaState in t.NfaStates)
		//                allChars.UnionWith(
		//                    nfaState.Transition.GetNotNullKeys<byte>((nb) => { return (byte)nb; }));

		//            foreach (var char1 in allChars)
		//            {
		//                var u = new DfaState(t.NfaStates.GetMove(char1).GetEclosure());

		//                var uUnique = u.GetNfaIds();
		//                DfaState u1;
		//                if (dfaStates.TryGetValue(uUnique, out u1))
		//                {
		//                    u = u1;
		//                }
		//                else
		//                {
		//                    dfaStates.Add(uUnique, u);
		//                    notMarkedStates.Enqueue(u);
		//                }

		//                t.AddTransition(char1, u);
		//            }

		//            if (i % 10000 == 0)
		//            {
		//                GC.Collect();
		//                GC.WaitForPendingFinalizers();
		//            }
		//        }
		//    }

		//    dfaCount = dfaStates.Count;

		//    if(showProgress)
		//        Console.WriteLine("Done\t\t\t\t\t\t\t\t\t\t\t\t\t\t");

		//    return dfaStart;
		//}

		public static IEnumerable<State> GetMove2(this IEnumerable<State> states, byte? ch)
		{
			foreach (var state in states)
				foreach (var move in state.Transition.Get(ch))
					yield return move;
		}

		public static DfaState ToDfa2(this State start, out int dfaCount, bool showProgress)
		{
			DfaState dfaStart;

			if (showProgress)
			{
				Console.WriteLine("NFA to DFA v.2 Converting...");
				Console.WriteLine("{0}", DateTime.Now);
			}

			var dfaStates = new Dictionary<int[], DfaState>(3000000, new IntArrayComparer());
			var notMarkedStates = new Queue<DfaState>(500000);

			dfaStart = new DfaState(DfaState.GetNfaIds(start.Eclosure()));
			dfaStates.Add(dfaStart.NfaIds, dfaStart);
			notMarkedStates.Enqueue(dfaStart);

			int i = 0;
			int time = Environment.TickCount;
			var allChars = new bool[byte.MaxValue + 1];

			GC.Collect();
			long memStart = GC.GetTotalMemory(true);

			while (notMarkedStates.Count > 0)
			{
				DfaState t = notMarkedStates.Dequeue();

				if (showProgress && i++ % 100 == 0)
				{
					int memPerState = (int)((GC.GetTotalMemory(true) - memStart) / (long)dfaStates.Count);
					int time1 = Environment.TickCount;
					Console.Write("{2}\t({3} ms)\t\t{0} ({4} b)\t\t{1}\t\t", dfaStates.Count, notMarkedStates.Count, dfaStates.Count - notMarkedStates.Count, time1 - time, memPerState);
					Console.Write("\r");
					time = time1;
				}

				if (t != null)
				{
					for (int j = 0; j <= byte.MaxValue; j++)
						allChars[j] = false;
					foreach (var nfaState in t.NfaStates)
						nfaState.Transition.ForEachNotNullKeys((nb) => { allChars[(int)nb] = true; });

					for (int j = 0; j <= byte.MaxValue; j++)
					{
						if (allChars[j])
						{
							byte char1 = (byte)j;

							var states = t.NfaStates.GetMove2(char1).GetEclosure();

							var uUnique = DfaState.GetNfaIds(states);

							DfaState u;
							if (dfaStates.TryGetValue(uUnique, out u) == false)
							{
								u = new DfaState(uUnique);
								dfaStates.Add(uUnique, u);
								notMarkedStates.Enqueue(u);
							}

							t.AddTransition(char1, u);
						}
					}

					if (i % 1000 == 0)
					{
						GC.Collect();
						GC.WaitForFullGCComplete();
						GC.WaitForPendingFinalizers();
					}
				}
			}

			dfaCount = dfaStates.Count;

			if (showProgress)
				Console.WriteLine("Done ({0})\t\t\t\t\t\t\t\t", dfaCount);

			return dfaStart;
		}


		public static DfaState ToDfa3(this State start, out int dfaCount, bool showProgress)
		{
			DfaState dfaStart;

			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();

			if (showProgress)
			{
				Console.WriteLine("NFA to DFA v.3 Converting...");
				Console.WriteLine("{0}", DateTime.Now);
			}

			var dfaStates = new Dictionary<int[], DfaState>(3000000, new IntArrayComparer());
			var dfaStatesSync = new object();
			var notMarkedStates = new Queue<DfaState>(500000);
			var notMarkedSync = new object();

			dfaStart = new DfaState(DfaState.GetNfaIds(start.Eclosure()));
			dfaStates.Add(dfaStart.NfaIds, dfaStart);
			notMarkedStates.Enqueue(dfaStart);

			Thread[] threads = new Thread[Environment.ProcessorCount * 2];

			for (int i = 0; i < threads.Length; i++)
			{
				threads[i] = new Thread(ToDfaThread);
				threads[i].Start(new ThreadParams(i == 0, showProgress, dfaStates,
					dfaStatesSync, notMarkedStates, notMarkedSync));

				if (i == 0)
				{
					for (int j = 0; j < 10; j++)
					{
						Thread.Sleep(1000);
						if (threads[0].IsAlive == false)
							break;
					}
					if (threads[0].IsAlive == false)
						break;
				}
			}

			for (int i = 0; i < threads.Length; i++)
				if (threads[i] != null)
					threads[i].Join();

			dfaCount = dfaStates.Count;
			if (showProgress)
			{
				stopwatch.Stop();
				Console.WriteLine("Done (States: {0}, Elapsed: {1})\t\t\t\t", dfaCount, stopwatch.Elapsed);
			}

			return dfaStart;
		}

		private class ThreadParams
		{
			public ThreadParams(bool main, bool showProgress, Dictionary<int[], DfaState> dfaStates, object dfaStatesSync, Queue<DfaState> notMarkedStates, object notMarkedSync)
			{
				this.Main = main;
				this.ShowProgress = main && showProgress;
				this.DfaStates = dfaStates;
				this.DfaStatesSync = dfaStatesSync;
				this.NotMarkedStates = notMarkedStates;
				this.NotMarkedSync = notMarkedSync;
			}

			public bool Main;
			public bool ShowProgress;
			public Dictionary<int[], DfaState> DfaStates;
			public object DfaStatesSync;
			public Queue<DfaState> NotMarkedStates;
			public object NotMarkedSync;
		}

		private static void ToDfaThread(Object stateInfo)
		{
			var p = stateInfo as ThreadParams;

			int i = 0;
			int time = Environment.TickCount;
			var allChars = new bool[byte.MaxValue + 1];

			long memStart = 0;
			if (p.Main)
			{
				GC.Collect();
				GC.WaitForFullGCComplete();
				GC.WaitForPendingFinalizers();
				memStart = GC.GetTotalMemory(true);
			}
			else
			{
				Thread.Sleep(20000);
			}

			int memPerState = 0;
			for (; ; i++)
			{
				DfaState t = null;
				lock (p.NotMarkedSync)
				{
					if (p.NotMarkedStates.Count <= 0)
						break;
					t = p.NotMarkedStates.Dequeue();
				}

				if (p.ShowProgress && (i % 100) == 0)
				{
					if ((i % 25000) == 0 && i > 0)
						memPerState = (int)((GC.GetTotalMemory(true) - memStart) / (long)p.DfaStates.Count);
					int time1 = Environment.TickCount;
					Console.Write("{2}\t({3} ms)\t\t{0} ({4} b)\t\t{1}  ", p.DfaStates.Count, p.NotMarkedStates.Count, p.DfaStates.Count - p.NotMarkedStates.Count, time1 - time, memPerState);
					Console.Write("\r");
					time = time1;
				}

				if (t != null)
				{
					for (int j = 0; j <= byte.MaxValue; j++)
						allChars[j] = false;
					foreach (var nfaState in t.NfaStates)
						nfaState.Transition.ForEachNotNullKeys((nb) => { allChars[(int)nb] = true; });

					for (int j = 0; j <= byte.MaxValue; j++)
					{
						if (allChars[j])
						{
							byte char1 = (byte)j;

							var uUnique = t.NfaStates.GetSortedMoveEclosureIds(char1);

							DfaState u;
							lock (p.DfaStatesSync)
							{
								if (p.DfaStates.TryGetValue(uUnique, out u) == false)
								{
									u = new DfaState(uUnique);
									p.DfaStates.Add(uUnique, u);
									lock (p.NotMarkedSync)
										p.NotMarkedStates.Enqueue(u);
								}
							}

							t.AddTransition(char1, u);
						}
					}

					if (p.Main)
					{
						if (i % 10000 == 0)
						{
							GC.Collect();
							GC.WaitForFullGCComplete();
							GC.WaitForPendingFinalizers();
						}
					}
				}
			}
		}

		/////////////////////////////////////////////////////////////////original v.3 //////////////////////////////////////////

		//private static void ToDfaThread(Object stateInfo)
		//{
		//    var p = stateInfo as ThreadParams;

		//    int i = 0;
		//    int time = Environment.TickCount;
		//    var allChars = new bool[byte.MaxValue + 1];

		//    long memStart = 0;
		//    if (p.Main)
		//    {
		//        GC.Collect();
		//        GC.WaitForFullGCComplete();
		//        GC.WaitForPendingFinalizers();
		//        memStart = GC.GetTotalMemory(true);
		//    }
		//    else
		//    {
		//        Thread.Sleep(20000);
		//    }

		//    int memPerState = 0;
		//    for (; ; i++)
		//    {
		//        DfaState t = null;
		//        lock (p.NotMarkedSync)
		//        {
		//            if (p.NotMarkedStates.Count <= 0)
		//                break;
		//            t = p.NotMarkedStates.Dequeue();
		//        }

		//        if (p.Main &&  (i % 100) == 0)
		//        {
		//            if ((i % 25000) == 0 && i > 0)
		//                memPerState = (int)((GC.GetTotalMemory(true) - memStart) / (long)p.DfaStates.Count);
		//            int time1 = Environment.TickCount;
		//            Console.Write("{2}\t({3} ms)\t\t{0} ({4} b)\t\t{1}  ", p.DfaStates.Count, p.NotMarkedStates.Count, p.DfaStates.Count - p.NotMarkedStates.Count, time1 - time, memPerState);
		//            Console.Write("\r");
		//            time = time1;
		//        }

		//        if (t != null)
		//        {
		//            for (int j = 0; j <= byte.MaxValue; j++)
		//                allChars[j] = false;
		//            foreach (var nfaState in t.NfaStates)
		//                nfaState.Transition.ForEachNotNullKeys((nb) => { allChars[(int)nb] = true; });

		//            for (int j = 0; j <= byte.MaxValue; j++)
		//            {
		//                if (allChars[j])
		//                {
		//                    byte char1 = (byte)j;

		//                    var states = t.NfaStates.GetMove2(char1).GetEclosure();

		//                    var uUnique = DfaState.GetNfaIds(states);

		//                    DfaState u;
		//                    lock (p.DfaStatesSync)
		//                    {
		//                        if (p.DfaStates.TryGetValue(uUnique, out u) == false)
		//                        {
		//                            u = new DfaState(uUnique);
		//                            p.DfaStates.Add(uUnique, u);
		//                            lock (p.NotMarkedSync)
		//                                p.NotMarkedStates.Enqueue(u);
		//                        }
		//                    }

		//                    t.AddTransition(char1, u);
		//                }
		//            }

		//            if (p.Main)
		//            {
		//                if (i % 10000 == 0)
		//                {
		//                    GC.Collect();
		//                    GC.WaitForFullGCComplete();
		//                    GC.WaitForPendingFinalizers();
		//                }
		//            }
		//        }
		//    }
		//}
	}
}
