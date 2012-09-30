using Fsm;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Fsm.Tests
{
	[TestClass()]
	public class StateTest
	{
		State _state1;
		State _state2;
		State _state3;
		State _state4;

		[TestInitialize()]
		public void Initialize()
		{
			_state1 = new State(1, new State(2, new State()));

			_state2 = new State();
			_state2.Transition.Add(1, _state2);
			_state2.Transition.Add(2, new State(3, new State(4, new State())));

			_state3 = new State(1, new State(2, new State() { Tag = "end", }));
			_state3.Transition.Add(12, new State() { Tag = "end", });

			_state4 = new State(null, new State(null, new State(null, new State())));
			_state4.Transition.Add(null, new State(null, new State()));
		}

		[TestMethod()]
		public void CountTest()
		{
			Assert.AreEqual(_state1.Count(), 3);
			Assert.AreEqual(_state2.Count(), 4);
		}

		[TestMethod()]
		public void CloneTest()
		{
			var state1 = _state1.Clone();
			Assert.AreEqual(state1.Transition.Get(1).Count, 1);
			Assert.AreNotEqual(state1.GetNextOne(1).GetNextOne(2), null);
			Assert.AreEqual(state1.Count(), 3);

			var state2 = _state2.Clone();
			Assert.AreNotEqual(state2.GetNextOne(1).GetNextOne(1).GetNextOne(1).GetNextOne(1).GetNextOne(1), null);
			Assert.AreNotEqual(state2.GetNextOne(2).GetNextOne(3).GetNextOne(4), null);
			Assert.AreEqual(state2.GetNextOne(2).GetNextOne(3).GetNextOne(4).GetNextOne(5), null);
			Assert.AreEqual(state2.Count(), 4);
		}

		[TestMethod()]
		public void FindEndTest()
		{
			Assert.AreEqual(1, _state1.FindEnds().Count);
			Assert.AreEqual(1, _state2.FindEnds().Count);
			Assert.AreEqual(2, _state3.FindEnds().Count);

			foreach (var end in _state3.FindEnds())
				Assert.AreEqual("end", end.Tag);
		}

		[TestMethod()]
		public void EclosureTest()
		{
			Assert.AreEqual(6, _state4.Eclosure().Count);
		}
	}
}
