using System;
using System.Collections.Generic;

namespace Fsm
{
	public class State
		//: MarkImpl
		: IIndexed
	{
		public static readonly byte? Epsilon = null;

		private Map<byte?, State> _transition;
		private List<IMark> _allMarks;

		public State()
		{
			Indexer<State>.Add(this);

			_transition = new Map<byte?, State>();
			_allMarks = new List<IMark>();
		}

		public State(byte? c, State s)
			: this()
		{
			_transition.Add(c, s);
		}

		public State(byte? c, State s1, State s2)
			: this()
		{
			_transition.Add(c, s1);
			_transition.Add(c, s2);
		}

		#region IIndexed

		public Int32 Id
		{
			get;
			private set;
		}

		void IIndexed.SetId(int id)
		{
			Id = id;
		}

		#endregion

		#region Marks

		public IEnumerable<IMark> AllMarks
		{
			get
			{
				return _allMarks;

				//if (Mark != Marks.None)
				//    yield return this;

				//foreach (var mark in _allMarks)
				//    yield return mark;
			}
		}

		public void AddMark(Marks mark)
		{
			_allMarks.Add(new MarkImpl(mark));
		}

		public void AddMark(IMark mark)
		{
			_allMarks.Add(mark);
		}

		public void AddRangeMarks(IEnumerable<IMark> marks)
		{
			_allMarks.AddRange(marks);
		}

		public void RemoveMark(IMark mark)
		{
			_allMarks.Remove(mark);
		}

		public int RemoveAllMarks(Predicate<IMark> predicate)
		{
			return _allMarks.RemoveAll(predicate);
		}

		//private List<int> packedStates = new List<int>();

		//public List<int> PackedStates
		//{
		//    get { return packedStates; }
		//}

		#endregion

		public Map<byte?, State> Transition
		{
			get { return _transition; }
		}

		public static int MaxId
		{
			get { return Indexer<State>.MaxId; }
		}

		#region Legacy

		//public bool IsBeginRange
		//{
		//    get { return Mark == Marks.BeginRange; }
		//    //	set { if (value) Mark = Marks.BeginRange; }
		//}

		//public bool IsEndRange
		//{
		//    get { return Mark == Marks.EndRange; }
		//    //	set { if (value) Mark = Marks.EndRange; }
		//}

		//public string RangeName
		//{
		//    get { return Name; }
		//    //	set { Name = value; }
		//}

		//public bool IsConst
		//{
		//    get { return Mark == Marks.Const; }
		//}

		//public string ConstName
		//{
		//    get { if (IsConst) return Name; else return null; }
		//    set
		//    {
		//        if (string.IsNullOrEmpty(value) == false)
		//        {
		//            Name = value;
		//            Mark = Marks.Const;
		//        }
		//    }
		//}

		//public string ConstValue
		//{
		//    get { return Value; }
		//    set { Value = value; }
		//}

		//public int ConstPriority
		//{
		//    get { return Priority; }
		//    set { Priority = value; }
		//}

		//public bool IsCount
		//{
		//    get { return Mark == Marks.Count; }
		//}

		//public string CountName
		//{
		//    get { if (IsCount) return Name; else return null; }
		//    //set
		//    //{
		//    //    if (string.IsNullOrEmpty(value) == false)
		//    //    {
		//    //        Name = value;
		//    //        Mark = Marks.Count;
		//    //    }
		//    //}
		//}

		//public int CountMax
		//{
		//    get { return Max; }
		//    //set { Max = value; }
		//}

		//public bool IsFinal
		//{
		//    get { return Mark == Marks.Final; }
		//    set { if (value) Mark = Marks.Final; }
		//}

		#endregion

		public string Tag
		{
			get;
			set;
		}

		public static State MarkCustom(State begin1, string name, string select, string custom, string type)
		{
			switch (select)
			{
				case "Begin":

					begin1.AddMark(new MarkImpl(Marks.Custom, name) { Value = custom, Type = type, });

					return begin1;

				//return new State(Epsilon, begin1)
				//{
				//    Mark = Marks.Custom,
				//    Name = name,
				//    Value = custom,
				//    Type = type,
				//};

				case "End":

					begin1.FindEnd().AddMark(new MarkImpl(Marks.Custom, name) { Value = custom, Type = type, });

					//var end2 = new State()
					//{
					//    Mark = Marks.Custom,
					//    Name = name,
					//    Value = custom,
					//    Type = type,
					//};

					//begin1.FindEnd().Transition.Add(Epsilon, end2);

					return begin1;
			}

			throw new Exception("Not implemented");
		}

		public static void MarkRange(ref State begin1, string name)
		{
			begin1 = MarkRange(begin1, name, 0, 0);
		}

		public static State MarkRange(State begin1, string name, int lookup, int beginOffset)
		{
			//State begin2;

			if (lookup == 0)
			{
				//begin2 = new State(Epsilon, begin1);
				////begin2.IsBeginRange = true;
				////begin2.RangeName = name;
				//begin2.Mark = Marks.BeginRange;
				//begin2.Name = name;
				//begin2.Offset = beginOffset;

				begin1.AddMark(new MarkImpl(Marks.BeginRange, name) { Offset = beginOffset, });
			}
			else if (lookup == 1)
			{
				//begin2 = begin1;
				//begin2.MarkNext(name, Marks.BeginRange, beginOffset);
				begin1.MarkNext(name, Marks.BeginRange, beginOffset);
			}
			else
				throw new ArgumentOutOfRangeException("lookup", "Must be 0 or 1, other value is not supported.");



			begin1.FindEnd().AddMark(new MarkImpl(Marks.EndRange, name));

			return begin1;

			//var end2 = new State();
			////end2.IsEndRange = true;
			////end2.RangeName = name;
			//end2.Mark = Marks.EndRange;
			//end2.Name = name;

			//var end1 = begin1.FindEnd();
			//end1.Transition.Add(Epsilon, end2);

			//return begin2;
		}

		public static State MarkBeginRange(State begin1, string name, bool atBegin, int offset)
		{
			if (atBegin)
			{
				begin1.AddMark(new MarkImpl(Marks.BeginRange, name) { Offset = offset, });

				return begin1;

				//return new State(Epsilon, begin1)
				//{
				//    Mark = Marks.BeginRange,
				//    Name = name,
				//    Offset = offset,
				//};
			}
			else
			{
				begin1.FindEnd().AddMark(new MarkImpl(Marks.BeginRange, name) { Offset = offset, });

				//var end2 = new State()
				//{
				//    Mark = Marks.BeginRange,
				//    Name = name,
				//    Offset = offset,
				//};

				//begin1.FindEnd().Transition.Add(Epsilon, end2);

				return begin1;
			}
		}

		public static State MarkEndRange(State begin1, Marks mark, string name, bool atBegin, int offset)
		{
			if (atBegin)
			{
				begin1.AddMark(new MarkImpl()
					{
						Mark = mark,
						Name = name,
						Offset = offset,
					});

				return begin1;

				//return new State(Epsilon, begin1)
				//{
				//    Mark = mark,
				//    Name = name,
				//    Offset = offset,
				//};
			}
			else
			{
				begin1.FindEnd().AddMark(new MarkImpl(mark, name) { Offset = offset, });

				//var end2 = new State()
				//{
				//    Mark = mark,
				//    Name = name,
				//    Offset = offset,
				//};

				//begin1.FindEnd().Transition.Add(Epsilon, end2);

				return begin1;
			}
		}

		public void MarkConst(string name, string value, int priority)
		{
			FindEnd().AddMark(new MarkImpl(Marks.Const, name) { Value = value, Priority = priority, });

			//var end2 = new State();
			//end2.ConstName = name;
			//end2.ConstValue = value;
			//end2.ConstPriority = priority;

			//var end1 = FindEnd();
			//end1.Transition.Add(Epsilon, end2);
		}

		public void MarkDecimal(string name, string type, string defaultValue)
		{
			//MarkEach(name, Marks.Decimal);
			MarkEach(new MarkImpl(Marks.Decimal)
			{
				Name = name,
				Type = type,
				Default = defaultValue,
			});
		}

		public void MarkHex(string name, string type, string defaultValue)
		{
//			MarkEach(name, Marks.Hex);
			MarkEach(new MarkImpl(Marks.Hex)
			{
				Name = name,
				Type = type,
				Default = defaultValue,
			});
		}

		//public void MarkResetIfInvalid(string name)
		//{
		//    MarkEach(name, Marks.ResetRangeIfInvalid);
		//}

		public void MarkResetIfInvalid(string name)
		{
			FindEnd().AddMark(new MarkImpl() { Name = name, Mark = Marks.ResetRangeIfInvalid, });
			//FindEnd().Transition.Add(Epsilon,
			//    new State() { Name = name, Mark = Marks.ResetRangeIfInvalid, });
		}

		public void MarkReset(string name)
		{
			FindEnd().AddMark(new MarkImpl() { Name = name, Mark = Marks.ResetRange, });
			//FindEnd().Transition.Add(Epsilon,
			//    new State() { Name = name, Mark = Marks.ResetRange, });
		}

		public void MarkContinueRange(string name)
		{
			MarkEach(name, Marks.ContinueRange);
		}

		public static State Substract(State nfa1, State nfa2)
		{
			nfa1.FindEnd().AddMark(Marks.Service1);
			nfa2.FindEnd().AddMark(Marks.Service2);

			int count;
			var dfa1 = nfa1.ToDfa3(out count, false);
			var dfa2 = nfa2.ToDfa3(out count, false);

			dfa1.Minimize(false);
			dfa2.Minimize(false);

			var error = new DfaState(new[] { new State().Id, });
			for (int i = 0; i <= 255; i++)
				error.AddTransition((byte)i, error);
			dfa2.ForEachNR((state) =>
			{
				if (state != error)
				{
					for (int i = 0; i <= 255; i++)
					{
						byte key = (byte)i;
						if (state.Transition[i] == null)
							state.AddTransition(key, error);
					}
				}
			});



			var nfa3 = DfaIntersect.Intersect(dfa1, dfa2).ToNfa2();


			var ends = new List<State>();
			nfa3.ForEach((state) =>
				{
					bool s1 = false, s2 = false;

					state.RemoveAllMarks((mark) =>
					{
						if (mark.Mark == Marks.Service1)
							s1 = true;

						if (mark.Mark == Marks.Service2)
							s2 = true;

						return mark.Mark == Marks.Service1 || mark.Mark == Marks.Service2;
					});

					if (s1 == true && s2 == false)
						ends.Add(state);
				});


			var end = new State();
			foreach (var item in ends)
				item.Transition.Add(Epsilon, end);


			return nfa3;
		}

		//public static State MarkAlternativesAsError(State start)
		//{
		//    start.FindEnd().Transition.Add(Epsilon, new State() { Mark = Marks.Service, });

		//    int count;
		//    var dfa = start.ToDfa3(out count, false);
		//    var nfa = dfa.ToNfa2();

		//    var ends = new List<State>();
		//    nfa.ForEach((state) =>
		//        {
		//            foreach (var id in state.PackedStates)
		//            {
		//                var packed = Indexer<State>.Get(id);
		//                if (packed.Mark == Marks.Service)
		//                {
		//                    state.PackedStates.Remove(id);
		//                    ends.Add(state);
		//                    break;
		//                }
		//            }
		//        });

		//    var end = new State();
		//    foreach (var item in ends)
		//        item.Transition.Add(Epsilon, end);


		//    //var ends2 = nfa.FindEnds();

		//    var error = new State() { Mark = Marks.Error, };
		//    for (int i = 0; i <= 255; i++)
		//        error.Transition.Add((byte)i, error);

		//    nfa.ForEach((state) =>
		//    {
		//        if (state != error && state != end && state != nfa && ends.Contains(state) == false)
		//        {
		//            for (int i = 0; i <= 255; i++)
		//            {
		//                byte key = (byte)i;
		//                if (state.Transition.IsEmpty(key))
		//                    state.Transition.Add(key, error);
		//            }
		//        }
		//    });

		//    //var ends3 = nfa.FindEnds();

		//    return nfa;
		//}

		protected void MarkEach(string name, Marks markType)
		{
			var eclosure = Eclosure();
			var end = FindEnd();

			ForEach((state) =>
			{
				if (eclosure.Contains(state) == false)
				{
					state.AddMark(new MarkImpl(markType, name));

					//var mark = new State(Epsilon, state)
					//{
					//    Name = name,
					//    Mark = markType,
					//};

					//state.Transition.Add(Epsilon, mark);
				}
			});

			end.Transition.Add(Epsilon, new State());
		}

		protected void MarkEach(IMark mark)
		{
			var eclosure = Eclosure();
			var end = FindEnd();

			ForEach((state) =>
			{
				if (eclosure.Contains(state) == false)
					state.AddMark(mark);
			});

			end.Transition.Add(Epsilon, new State());
		}

		protected void MarkNext(string name, Marks markType, int offset)
		{
			var proccessed = new HashSet<State>();
			var eclosure = Eclosure();
			var end = FindEnd();

			foreach (var first in eclosure)
			{
				foreach (var pair in first.Transition)
				{
					if (pair.Key != null)
					{
						var next = pair.Value;
						if (proccessed.Contains(next) == false)
						{
							proccessed.Add(next);

							var mark = new MarkImpl()
							{
								Name = name,
								Mark = markType,
								Offset = offset,
							};

							next.AddMark(mark);

							//var mark = new State(Epsilon, next)
							//{
							//    Name = name,
							//    Mark = markType,
							//    Offset = offset,
							//};

							//next.Transition.Add(Epsilon, mark);
						}
					}
				}
			}

			end.Transition.Add(Epsilon, new State());
		}

		public void MarkCount(string name, int max, int default1)
		{
			var mark = new MarkImpl();
			mark.Mark = Marks.Count;
			mark.Name = name;
			mark.Max = max;
			mark.Default = default1.ToString();

			var end1 = FindEnd();
			end1.AddMark(mark);

			//var end2 = new State();
			////end2.CountName = name;
			////end2.CountMax = max;
			//end2.Mark = Marks.Count;
			//end2.Name = name;
			//end2.Max = max;
			//end2.Default = default1;

			//var end1 = FindEnd();
			//end1.Transition.Add(Epsilon, end2);
		}

		public void MarkBool(Marks mark, string name)
		{
			var mark2 = new MarkImpl()
			{
				Mark = mark,
				Name = name,
			};

			var end1 = FindEnd();
			end1.AddMark(mark2);

			//var end2 = new State()
			//{
			//    Mark = mark,
			//    Name = name,
			//};

			//var end1 = FindEnd();
			//end1.Transition.Add(Epsilon, end2);
		}

		public static State MarkBeginCount(State begin1, string name, int max)
		{
			begin1.AddMark(new MarkImpl()
				{
					Mark = Marks.Count,
					Name = name,
					Max = max,
				});

			return begin1;

			//return new State(Epsilon, begin1)
			//{
			//    Mark = Marks.Count,
			//    Name = name,
			//    Max = max,
			//};
		}

		public void MarkFinal()
		{
			FindEnd().AddMark(new MarkImpl(Marks.Final));

			//var end2 = new State()
			//{
			//    Mark = Marks.Final,
			//};

			//var end1 = FindEnd();
			//end1.Transition.Add(Epsilon, end2);
		}

		public State GetNextOne(byte? c)
		{
			foreach (var next in _transition.Get(c))
				return next;
			return null;
		}

		public State Clone()
		{
			return Clone(new Dictionary<int, State>());
		}

		public HashSet<State> FindEnds()
		{
			var ends = new HashSet<State>();
			FindEnds(new HashSet<int>(), ends);
			return ends;
		}

		public State FindEnd()
		{
			foreach (var end in FindEnds())
				return end;
			throw new InvalidProgramException();
		}

		public int Count()
		{
			return Count(new HashSet<int>());
		}

		public HashSet<State> Eclosure()
		{
			var eclosure = new HashSet<State>();
			Eclosure(eclosure);
			return eclosure;
		}

		private HashSet<State> _cachedEclosure;

		internal HashSet<State> GetCachedEclosure()
		{
			if (_cachedEclosure == null)
				_cachedEclosure = Eclosure();
			return _cachedEclosure;
		}

		private void Eclosure(HashSet<State> eclosure)
		{
			eclosure.Add(this);

			foreach (var state in Transition.Get(Epsilon))
				if (eclosure.Contains(state) == false)
					state.Eclosure(eclosure);
		}

		public void GetEclosureId(HashSet<int> eclosure)
		{
			eclosure.Add(this.Id);

			foreach (var state in Transition.Get(Epsilon))
				if (eclosure.Contains(state.Id) == false)
					state.GetEclosureId(eclosure);
		}

		////////////////////////////////////////////////////////////////////

		//public void AddPacked(State next)
		//{
		//    int oldsize = packedStates.Length;
		//    int length = packedStates.Length + 1 + next.PackedStates.Length;
		//
		//    Array.Resize<int>(ref packedStates, length);
		//    packedStates[oldsize] = next.Id;
		//    Array.Copy(next.PackedStates, 0, packedStates, oldsize + 1, next.PackedStates.Length);
		//}

		////////////////////////////////////////////////////////////////////

		/// <summary>
		/// 
		/// </summary>
		public static State Repeat(int repeat1, int repeat2, State state1)
		{
			if (repeat1 == -1 && repeat2 == -1)
				return -state1;

			if (repeat1 == -1)
				repeat1 = 0;

			State result = new State();
			for (int i = 0; i < repeat1; i++)
				result = result + state1;

			if (repeat2 != -1)
			{
				for (int i = repeat1; i < repeat2; i++)
					result = result + !state1;
			}
			else
			{
				result = result + -state1;
			}

			return result;
		}

		/// <summary>
		/// Example: arg, arg, arg
		/// </summary>
		public static State NoCloneRepeatBy(State item, State separator)
		{
			separator.FindEnd().Transition.Add(State.Epsilon, item);

			var itemEnd = item.FindEnd();
			itemEnd.Transition.Add(State.Epsilon, new State());
			itemEnd.Transition.Add(State.Epsilon, separator);

			return item;
		}

		/// <summary>
		/// [...] rule -> NoCloneOption
		/// </summary>
		public static State operator !(State state)
		{
			return NoCloneOption(state.Clone());
		}

		public static State NoCloneOption(State start)
		{
			var end = new State();
			var oldEnd = start.FindEnd();

			oldEnd.Transition.Add(Epsilon, end);
			start.Transition.Add(Epsilon, end);

			return start;
		}

		/// <summary>
		/// +(...) rule
		/// </summary>
		public static State operator +(State state)
		{
			return NoClonePlus(state.Clone());
		}

		public static State NoClonePlus(State state)
		{
			var start = state;

			var end = new State();
			var oldEnd = start.FindEnd();

			oldEnd.Transition.Add(Epsilon, start);
			oldEnd.Transition.Add(Epsilon, end);

			return start;
		}

		/// <summary>
		/// *(...) rule
		/// </summary>
		public static State operator -(State state)
		{
			return NoCloneStar(state.Clone());
		}

		public static State NoCloneStar(State state)
		{
			var oldStart = state;

			var end = new State();
			var start = new State(Epsilon, oldStart, end);
			var oldEnd = start.FindEnd();

			oldEnd.Transition.Add(Epsilon, oldStart);
			oldEnd.Transition.Add(Epsilon, end);

			return start;
		}

		/// <summary>
		/// Concatanation
		/// </summary>
		public static State operator +(State state1, State state2)
		{
			var start = state1.Clone();
			start.FindEnd().Transition.Add(Epsilon, state2.Clone());
			return start;
		}

		public static State operator +(String string1, State state2)
		{
			return Thompson.Create(string1) + state2;
		}

		public static State operator +(State state1, String string2)
		{
			return state1 + Thompson.Create(string2);
		}

		public static State operator +(int byte1, State state2)
		{
			return Thompson.Create(byte1) + state2;
		}

		public static State operator +(State state1, int byte2)
		{
			return state1 + Thompson.Create(byte2);
		}

		public static State NoCloneConcatanation(params State[] sequence)
		{
			for (int i = sequence.Length - 1; i >= 1; i--)
				sequence[i - 1].FindEnd().Transition.Add(Epsilon, sequence[i]);

			return sequence[0];
		}

		/// <summary>
		/// Alternation
		/// </summary>
		public static State operator |(State state1, State state2)
		{
			var start = new State();
			start.Transition.Add(Epsilon, state1.Clone());
			start.Transition.Add(Epsilon, state2.Clone());

			var end = new State();
			foreach (var oldend in start.FindEnds())
				oldend.Transition.Add(Epsilon, end);

			return start;
		}

		public static State operator |(String string1, State state2)
		{
			return Thompson.Create(string1) | state2;
		}

		public static State operator |(State state1, String string2)
		{
			return state1 | Thompson.Create(string2);
		}

		public static State operator |(int byte1, State state2)
		{
			return Thompson.Create(byte1) | state2;
		}

		public static State operator |(State state1, int byte2)
		{
			return state1 | Thompson.Create(byte2);
		}

		public static State NoCloneAlternation(params State[] alternations)
		{
			if (alternations.Length > 1)
			{
				var start = new State();
				var end = new State();

				foreach (var item in alternations)
				{
					start.Transition.Add(Epsilon, item);
					foreach (var oldend in item.FindEnds())
						oldend.Transition.Add(Epsilon, end);
				}

				return start;
			}

			return alternations[0];
		}

		/// <summary>
		/// Create from string
		/// </summary>
		public static implicit operator State(string string1)
		{
			return Thompson.Create(string1);
		}

		/// <summary>
		/// Create from byte[]
		/// </summary>
		public static implicit operator State(byte[] bytes)
		{
			return Thompson.Create(bytes);
		}

		/// <summary>
		/// Create from int (byte)
		/// </summary>
		public static implicit operator State(int byte1)
		{
			return Thompson.Create(byte1);
		}

		private State Clone(Dictionary<int, State> proccessed)
		{
			var copy = new State();
			//copy.CopyIMarkFrom(this);
			copy.Tag = Tag;

			proccessed.Add(Id, copy);

			foreach (var pair in Transition)
			{
				State nextState;
				if (proccessed.TryGetValue(pair.Value.Id, out nextState) == false)
					nextState = pair.Value.Clone(proccessed);
				copy.Transition.Add(pair.Key, nextState);
			}

			//if (packedStates != null)
			//    copy.packedStates = new List<int>(packedStates);

			if (_allMarks != null)
				copy._allMarks = new List<IMark>(_allMarks);

			return copy;
		}

		//public void CopyIMarkFrom(IMark imark)
		//{
		//    CopyFrom(imark);
		//}

		private void FindEnds(HashSet<int> proccessed, HashSet<State> ends)
		{
			proccessed.Add(Id);

			bool hasTransition = false;
			foreach (var pair in Transition)
			{
				hasTransition = true;

				if (proccessed.Contains(pair.Value.Id) == false)
					pair.Value.FindEnds(proccessed, ends);
			}


			if (hasTransition == false)
				ends.Add(this);
		}

		private int Count(HashSet<int> proccessed)
		{
			proccessed.Add(Id);

			int count = 1;
			foreach (var pair in Transition)
				if (proccessed.Contains(pair.Value.Id) == false)
					count += pair.Value.Count(proccessed);

			return count;
		}

		public void ForEach(HashSet<State> proccessed, Action<State, byte?, State> action)
		{
			foreach (var pair in Transition)
				if (proccessed.Contains(pair.Value) == false)
				{
					proccessed.Add(this);

					action(this, pair.Key, pair.Value);

					pair.Value.ForEach(proccessed, action);
				}
		}

		private void ForEach(Action<State> action)
		{
			var all = new HashSet<State>();
			ForEach(all);

			foreach (var item in all)
				action(item);
		}

		private void ForEach(HashSet<State> proccessed)
		{
			proccessed.Add(this);

			foreach (var pair in Transition)
				if (proccessed.Contains(pair.Value) == false)
					pair.Value.ForEach(proccessed);
		}
	}
}
