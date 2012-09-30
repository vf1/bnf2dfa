using Fsm;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Fsm.Tests
{
	[TestClass()]
	public class DfaStateTest
	{
		//[TestMethod()]
		//public void ToTableTest()
		//{
		//    DfaState start = new DfaState(new HashSet<State>() { new State(), new State(), });
		//    DfaState state2 = new DfaState(new HashSet<State>() { new State(), new State(), });
		//    DfaState state3 = new DfaState(new HashSet<State>() { new State(), new State() { IsEndRange = true, } });

		//    start.Transition.Add(1, start);
		//    start.Transition.Add(2, state2);
		//    start.Transition.Add(3, state3);

		//    state2.Transition.Add(5, start);
		//    state2.Transition.Add(6, state2);
		//    state2.Transition.Add(7, state3);

		//    state3.Transition.Add(5, start);
		//    state3.Transition.Add(6, state2);
		//    state3.Transition.Add(7, state3);

		//    uint[,] dfa;
		//    start.ToTable(out dfa);

		//    Assert.AreEqual<uint>(0xffffffff, dfa[0, 0]);
		//    Assert.AreEqual<uint>(0x00000000, dfa[0, 1]);
		//    Assert.AreEqual<uint>(0x00000001, dfa[0, 2]);
		//    Assert.AreEqual<uint>(0x80000002, dfa[0, 3]);
		//    Assert.AreEqual<uint>(0xffffffff, dfa[0, 4]);

		//    Assert.AreEqual<uint>(0xffffffff, dfa[1, 4]);
		//    Assert.AreEqual<uint>(0x00000000, dfa[1, 5]);
		//    Assert.AreEqual<uint>(0x00000001, dfa[1, 6]);
		//    Assert.AreEqual<uint>(0x80000002, dfa[1, 7]);
		//    Assert.AreEqual<uint>(0xffffffff, dfa[1, 8]);
		//}
	}
}
