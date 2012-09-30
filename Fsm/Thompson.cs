using System;
using System.Collections.Generic;
using System.Text;

namespace Fsm
{
	public static class Thompson
	{
		public static readonly byte? Epsilon = null;

		public static State Create(char from, char to)
		{
			return Create(Convert.ToByte(from), Convert.ToByte(to));
		}

		public static State Create(byte from, byte to)
		{
			State start = new State();
			State end = new State();

			//for (byte i = from; i <= to; i++)
			//    start.Transition.Add(i,
			//        new State(Epsilon, end));

			for (int i = from; i <= to; i++)
				start.Transition.Add((byte)i, end);

			return start;
		}

		public static State Create(byte[] sequence1, byte[] sequence2)
		{
			var start = new State();
			
			var state1 = start;
			for (int i = 0; i < sequence1.Length; i++)
			{
				var state2 = new State();

				state1.Transition.Add(sequence1[i], state2);
				if (sequence2 != null && sequence1[i] != sequence2[i])
					state1.Transition.Add(sequence2[i], state2);

				state1 = state2;
			}

			return start;
		}

		public static State Create(byte byte1)
		{
			return Create(new byte[] { byte1 }, null);
		}

		public static State Create(int byte1)
		{
			return Create(new byte[] { (byte)byte1 }, null);
		}

		public static State Create(byte[] sequence1)
		{
			return Create(sequence1, null);
		}

		public static State Create(string nocharcase)
		{
			var utf = new UTF8Encoding();
			return Create(utf.GetBytes(nocharcase.ToLower()), utf.GetBytes(nocharcase.ToUpper()));
		}

		//public static State MakeStarRepeat(this State start1)
		//{
		//    var oldstart = start1.Clone();

		//    var end = new State();
		//    var start = new State(Epsilon, oldstart, end);
		//    var oldEnd = start.FindEnd();

		//    oldEnd.Transition.Add(Epsilon, oldstart);
		//    oldEnd.Transition.Add(Epsilon, end);

		//    return start;
		//}

		//public static State MakePlusRepeat(this State start1)
		//{
		//    var start = start1.Clone();

		//    var end = new State();
		//    var oldEnd = start.FindEnd();

		//    oldEnd.Transition.Add(Epsilon, start);
		//    oldEnd.Transition.Add(Epsilon, end);

		//    return start;
		//}

		//public static State MakeSequence(this State start1, State start2)
		//{
		//    var start11 = start1.Clone();

		//    start11.FindEnd().Transition.Add(Epsilon, start2.Clone());
	
		//    return start11;
		//}

		//public static State MakeAlternation(this State start1, State start2)
		//{
		//    var start = new State();
		//    start.Transition.Add(Epsilon, start1.Clone());
		//    start.Transition.Add(Epsilon, start2.Clone());

		//    var end = new State();
		//    foreach (var oldend in start.FindEnds())
		//        oldend.Transition.Add(Epsilon, end);

		//    return start;
		//}
	}
}
