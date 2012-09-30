using Fsm;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fsm.Tests
{
	[TestClass()]
	public class NfaToDfaTest
	{
		[TestMethod()]
		public void NfaToDfaTest1()
		{
			var state9 = new State() { Tag = "9", };
			var state8 = new State(1, state9) { Tag = "8", };
			var state7 = new State(1, state8) { Tag = "7", };
			var state6 = new State(State.Epsilon, state7) { Tag = "6", };
			var state5 = new State(State.Epsilon, state6) { Tag = "5", };
			var state4 = new State(2, state5) { Tag = "4", };
			var state3 = new State(State.Epsilon, state6) { Tag = "3", };
			var state2 = new State(1, state3) { Tag = "2", };
			var state1 = new State(State.Epsilon, state2, state4) { Tag = "1", };
			state6.Transition.Add(State.Epsilon, state1);
			var state0 = new State(State.Epsilon, state1, state7) { Tag = "0", };

			int count;
			var a = state0.ToDfa3(out count, false);
			var b = a.Transition[1];
			var c = a.Transition[2];
			var d = b.Transition[1];

			Assert.AreEqual("{0,1,2,4,7}", a.ToString());
			Assert.AreEqual("{1,2,3,4,6,7,8}", b.ToString());
			Assert.AreEqual("{1,2,4,5,6,7}", c.ToString());
			Assert.AreEqual("{1,2,3,4,6,7,8,9}", d.ToString());

			//Assert.AreEqual(2, a.Transition.Count);
			Assert.AreEqual(b, a.Transition[1]);
			Assert.AreEqual(c, a.Transition[2]);

			//Assert.AreEqual(2, b.Transition.Count);
			Assert.AreEqual(d, b.Transition[1]);
			Assert.AreEqual(c, b.Transition[2]);

			//Assert.AreEqual(2, c.Transition.Count);
			Assert.AreEqual(b, c.Transition[1]);
			Assert.AreEqual(c, c.Transition[2]);

			//Assert.AreEqual(2, d.Transition.Count);
			Assert.AreEqual(d, d.Transition[1]);
			Assert.AreEqual(c, d.Transition[2]);
		}
	}
}
