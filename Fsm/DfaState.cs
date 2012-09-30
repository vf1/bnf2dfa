using System;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

namespace Fsm
{
	public class DfaState
		: IIndexed
	{
		private Indexer<DfaState>.Array _trasition;
		private Indexer<State>.Array _nfaStates;

		internal DfaState()
		{
			Indexer<DfaState>.Add(this);

			_trasition = new Indexer<DfaState>.Array(byte.MaxValue + 1);
			_nfaStates = null;
		}

		internal DfaState(Int32[] nfaIds)
		{
			Indexer<DfaState>.Add(this);

			_trasition = new Indexer<DfaState>.Array(byte.MaxValue + 1);
			_nfaStates = new Indexer<State>.Array(nfaIds);
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

		internal IEnumerable<State> NfaStates
		{
			get { return _nfaStates.Items; }
		}

		internal int[] NfaIds
		{
			get { return _nfaStates.Ids; }
		}

		#region Transition

		public void AddTransition(byte char1, DfaState state)
		{
			if (_trasition[char1] != null)
				throw new InvalidProgramException("Transition already exist in DFA state");
			_trasition[char1] = state;
		}

		public void SetTransition(byte char1, DfaState state)
		{
			if (_trasition[char1].SetId != state.SetId)
				throw new InvalidProgramException("Invlid Transition");
			_trasition[char1] = state;
		}

		public Indexer<DfaState>.Array Transition
		{
			get { return _trasition; }
		}

		public DfaState GetTransited(byte ch)
		{
			return _trasition[ch];
		}

		#endregion

		public int Index
		{
			get;
			set;
		}

		#region NFA Properties

		private List<IMark> _marks;
		private List<IMark> _consts;
		private List<IMark> _decimals;
		private List<IMark> _hexes;
		private List<IMark> _chars;
		private List<IMark> _begins;
		private bool? _isFinal;

		private List<IMark> GetMarks(Marks markType)
		{
			if (HasMarks)
				return AllMarks
					.Where<IMark>((mark) => { return mark.Mark == markType; })
					.ToList<IMark>();
			return new List<IMark>();
		}

		public List<IMark> Consts
		{
			get
			{
				if (_consts == null)
					_consts = GetMarks(Marks.Const);
				return _consts;
			}
		}

		public List<IMark> Decimals
		{
			get
			{
				if (_decimals == null)
					_decimals = GetMarks(Marks.Decimal);
				return _decimals;
			}
		}

		public List<IMark> Hexes
		{
			get
			{
				if (_hexes == null)
					_hexes = GetMarks(Marks.Hex);
				return _hexes;
			}
		}

		public List<IMark> Chars
		{
			get
			{
				if (_chars == null)
					_chars = GetMarks(Marks.Chars);
				return _chars;
			}
		}

		public List<IMark> BeginRanges
		{
			get
			{
				if (_begins == null)
					_begins = GetMarks(Marks.BeginRange);
				return _begins;
			}
		}

		public bool IsFinal
		{
			get
			{
				if (_isFinal == null)
					_isFinal = AllMarks.Any<IMark>((mark) => { return mark.Mark == Marks.Final; });

				return (bool)_isFinal;
			}
		}

		public bool HasMarks
		{
			get
			{
				if (_marks != null)
					return _marks.Count > 0;
				foreach (var state in AllMarks)//NfaStates)
					if (state.Mark != Marks.None)
						return true;
				return false;
			}
		}

		public List<IMark> AllMarks
		{
			get
			{
				if (_marks == null)
				{
					var allMarks = new List<IMark>();
					//allMarks.AddRange(NfaStates.Cast<IMark>());
					foreach (var nfaState in NfaStates)
						allMarks.AddRange(nfaState.AllMarks);
					////foreach (var id in nfaState.PackedStates)
					////allMarks.Add(Indexer<State>.Get(id));


					var priorities = new Dictionary<string, int>();
					foreach (var mark in allMarks)//NfaStates)
					{
						if (mark.Mark != Marks.None)
						{
							int maxPriority = int.MaxValue;
							string markName = mark.Mark + ":" + mark.Name;

							if (priorities.TryGetValue(markName, out maxPriority) == false || maxPriority > mark.Priority)
								priorities[markName] = mark.Priority;
						}
					}



					_marks = allMarks//NfaStates
						//.Cast<IMark>()
						.Where<IMark>((mark) => { return mark.Mark != Marks.None; })
						.Where<IMark>((mark) =>
							{ return priorities[mark.Mark + ":" + mark.Name] == mark.Priority; })
						.OrderBy<IMark, string>((mark) => { return mark.Mark + ":" + mark.Name; })
						.Distinct<IMark>(new MarkEqualityComparer())
						//.Select<IMark, IMark>((mark) =>
						//    {
						//        var state = new State();
						//        state.CopyIMarkFrom(mark);
						//        return state;
						//    })
						.ToList<IMark>();
				}

				return _marks;
			}
		}

		internal void SetMarks(List<IMark> marks)
		{
			this._marks = marks;
		}

		#endregion

		public bool IsSame(DfaState state)
		{
			if (AllMarks.Count != state.AllMarks.Count)
				return false;

			for (int i = 0; i < AllMarks.Count; i++)
				if (AllMarks[i].IsSame(state.AllMarks[i]) == false)
					return false;

			return true;
		}

		public IEnumerable<KeyValuePair<string, string>> ConstNameValues
		{
			get
			{
				var priority = new Dictionary<string, int>();

				foreach (var mark in AllMarks)
					if (mark.Mark == Marks.Const)
					{
						if (priority.ContainsKey(mark.Name))
						{
							if (priority[mark.Name] > mark.Priority)
								priority[mark.Name] = mark.Priority;
						}
						else
							priority[mark.Name] = mark.Priority;
					}

				var result = new List<KeyValuePair<string, string>>();

				foreach (var mark in AllMarks)
					if (mark.Mark == Marks.Const)
					{
						if (priority[mark.Name] == mark.Priority)
							result.Add(new KeyValuePair<string, string>(mark.Name, mark.Value));
					}

				return result;

				//var priority = new Dictionary<string, int>();

				//foreach (var nfaState in NfaStates)
				//    if (nfaState.IsConst)
				//    {
				//        if (priority.ContainsKey(nfaState.ConstName))
				//        {
				//            if (priority[nfaState.ConstName] > nfaState.ConstPriority)
				//                priority[nfaState.ConstName] = nfaState.ConstPriority;
				//        }
				//        else
				//            priority[nfaState.ConstName] = nfaState.ConstPriority;
				//    }

				//var result = new List<KeyValuePair<string, string>>();

				//foreach (var nfaState in NfaStates)
				//    if (nfaState.IsConst)
				//    {
				//        if (priority[nfaState.ConstName] == nfaState.ConstPriority)
				//            result.Add(new KeyValuePair<string, string>(nfaState.ConstName, nfaState.ConstValue));
				//    }

				//return result;
			}
		}

		#region Intersection

		public static DfaState Intersect(DfaState start1, DfaState start2)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region SetId - for DFA minimize

		private int _setId;

		internal int SetId
		{
			get { return _setId; }
			set
			{
				_setId = value;
				NewSetId = value;
			}
		}

		internal int NewSetId
		{
			get;
			set;
		}

		internal int GetTransitedSetId(int ch)
		{
			if (ch > byte.MaxValue || ch < byte.MinValue)
				throw new ArgumentOutOfRangeException("ch must be a byte value");
			return GetTransitedSetId((byte)ch);
		}

		internal int GetTransitedSetId(byte ch)
		{
			if (_trasition[ch] != null)
				return _trasition[ch].SetId;

			return -1;
		}

		#endregion

		#region NFA IDs, ToString()

		public static int[] GetNfaIds(ICollection<State> states)
		{
			var ids = new int[states.Count];

			int i = 0;
			foreach (var state in states)
				ids[i++] = state.Id;

			Array.Sort<int>(ids);

			return ids;
		}

		public override string ToString()
		{
			var tags = new List<string>();
			foreach (var state in NfaStates)
				tags.Add(string.IsNullOrEmpty(state.Tag) ? state.Id.ToString() : state.Tag);
			tags.Sort();

			bool comma = false;
			string result = "{";
			foreach (var tag in tags)
			{
				result += (comma ? "," : "") + tag;
				comma = true;
			}
			return result + "}";
		}

		#endregion

		#region ForEach

		public void ForEach(Action<DfaState> action)
		{
			ForEach(this, new HashSet<DfaState>(), action);
		}

		private static void ForEach(DfaState state, HashSet<DfaState> proccessed, Action<DfaState> action)
		{
			action(state);

			proccessed.Add(state);

			for (int i = 0; i <= byte.MaxValue; i++)
				if (state._trasition[i] != null)
				{
					if (proccessed.Contains(state._trasition[i]) == false)
						ForEach(state._trasition[i], proccessed, action);
				}
		}

		#endregion

		#region ForEachNR - no recursion

		public void ForEachNR(Action<DfaState> action)
		{
			var proccessed = new HashSet<DfaState>();
			var queued = new HashSet<DfaState>();
			var queue = new Queue<DfaState>();

			queued.Add(this);
			queue.Enqueue(this);

			while (queue.Count > 0)
			{
				var state = queue.Dequeue();

				queued.Remove(state);
				proccessed.Add(state);

				action(state);

				for (int i = 0; i <= byte.MaxValue; i++)
					if (state._trasition[i] != null)
					{
						if (proccessed.Contains(state._trasition[i]) == false)
						{
							if (queued.Contains(state._trasition[i]) == false)
							{
								queued.Add(state._trasition[i]);
								queue.Enqueue(state._trasition[i]);
							}
						}
					}
			}
		}

		#endregion
	}
}
