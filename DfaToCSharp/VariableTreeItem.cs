using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Fsm;

namespace DfaCompiler
{
	public class VariableInfo
		: MarkImpl
	{
		private string _shortname;

		public VariableInfo(IMark mark)
			: base(mark)
		{
			_shortname = GetShortName(Name);
		}

		public string ShortName
		{
			get { return _shortname; }
		}

		public static string GetShortName(string name)
		{
			int splitter = name.LastIndexOf('.');
			if (splitter < 0)
				return name;
			return name.Substring(splitter + 1);
		}
	}

	class VariableTreeItem
	{
		private string _name;
		private List<string> _begins;
		private List<string> _ends;
		private Dictionary<string, VariableTreeItem> _subitems;
		private Dictionary<string, List<string>> _enums;
		private Dictionary<string, int> _counts;
		private List<string> _decimals;

		private Dictionary<string, VariableInfo> _customs;
		private Dictionary<string, VariableInfo> _begins1;
		private Dictionary<string, VariableInfo> _ends1;
		private Dictionary<string, VariableInfo> _counts1;
		private Dictionary<string, VariableInfo> _bools;
		private Dictionary<string, VariableInfo> _decimals1;

		public VariableTreeItem(string name)
		{
			_name = name;
			_begins = new List<string>();
			_ends = new List<string>();
			_subitems = new Dictionary<string, VariableTreeItem>();
			_enums = new Dictionary<string, List<string>>();
			_counts = new Dictionary<string, int>();
			_decimals = new List<string>();

			_begins1 = new Dictionary<string, VariableInfo>();
			_ends1 = new Dictionary<string, VariableInfo>();
			_counts1 = new Dictionary<string, VariableInfo>();
			_bools = new Dictionary<string, VariableInfo>();
			_decimals1 = new Dictionary<string, VariableInfo>();
			_customs = new Dictionary<string, VariableInfo>();
		}

		public string Name
		{
			get { return _name; }
		}

		public List<string> Begins
		{
			get { return _begins; }
		}

		public List<string> Ends
		{
			get { return _ends; }
		}

		public Dictionary<string, List<string>> Enums
		{
			get { return _enums; }
		}

		public Dictionary<string, VariableInfo> Customs
		{
			get { return _customs; }
		}

		public Dictionary<string, VariableInfo> Begins1
		{
			get { return _begins1; }
		}

		public Dictionary<string, VariableInfo> Ends1
		{
			get { return _ends1; }
		}

		public Dictionary<string, VariableInfo> Counts
		{
			get { return _counts1; }
		}

		public Dictionary<string, VariableInfo> Bools
		{
			get { return _bools; }
		}

		public Dictionary<string, VariableInfo> Decimals1
		{
			get { return _decimals1; }
		}

		public List<string> Decimals
		{
			get { return _decimals; }
		}

		public IEnumerable<VariableTreeItem> Subitems
		{
			get { return _subitems.Values; }
		}

		public string GetName(string path)
		{
			int splitter = path.LastIndexOf('.');
			if (splitter < 0)
				return path;
			return path.Substring(splitter + 1);
		}

		public void AddBegin(string path)
		{
			var name = GetName(path);

			if (_begins.Contains(name) == false)
				_begins.Add(name);
		}

		public void AddEnds(string path)
		{
			var name = GetName(path);

			if (_ends.Contains(name) == false)
				_ends.Add(name);
		}

		public void AddEnum(string path, string value)
		{
			var name = GetName(path);

			List<string> enum1;
			if (_enums.TryGetValue(name, out enum1) == false)
			{
				enum1 = new List<string>();
				enum1.Add("None");
				_enums.Add(name, enum1);
			}

			if (enum1.Contains(value) == false)
				enum1.Add(value);
		}

		public void AddCount(string path, int max, int initial)
		{
			var name = GetName(path);

			if (_counts.ContainsKey(name))
			{
				if (_counts[name] < max)
					_counts[name] = max;
			}
			else
			{
				_counts.Add(name, max);
			}
		}

		//public void AddDecimal(string path)
		//{
		//    var name = GetName(path);

		//    if (_decimals.Contains(name) == false)
		//        _decimals.Add(name);
		//}

		public void AddVariables(DfaState state)
		{
			foreach (var mark in state.AllMarks)
				if (string.IsNullOrEmpty(mark.Name) == false)
					GetItem(mark.Name).AddVariable(mark);
		}

		public void AddVariable(IMark mark)
		{
			var shortname = VariableInfo.GetShortName(mark.Name);

			Dictionary<string, VariableInfo> dictionary = null;

			var mark1 = mark.Mark;

			if (mark1 == Marks.Custom)
			{
				if (mark.Type == "ByteArrayPart")
					mark1 = Marks.BeginRange;
				if (mark.Type == "bool")
					mark1 = Marks.Bool;
				if (mark.Type == "int")
					mark1 = Marks.Decimal;
			}

			switch (mark1)
			{
				case Marks.Custom:
					if (string.IsNullOrEmpty(mark.Type) == false)
						dictionary = _customs;
					break;
				case Marks.Const:
					AddEnum(mark.Name, mark.Value);
					break;
				case Marks.BeginRange:
					dictionary = _begins1;
					break;
				case Marks.EndRange:
					dictionary = _ends1;
					break;
				case Marks.Count:
					dictionary = _counts1;
					break;
				case Marks.Bool:
				case Marks.BoolEx:
					dictionary = _bools;
					break;
				case Marks.Decimal:
				case Marks.Hex:
					dictionary = _decimals1;
					break;
			}

			if (dictionary != null)
				if (dictionary.ContainsKey(shortname) == false)
					dictionary.Add(shortname, new VariableInfo(mark));
		}

		public VariableTreeItem GetItem(string path)
		{
			int splitter = path.IndexOf('.');
			if (splitter < 0)
				return this;

			var subitemName = path.Substring(0, splitter);

			VariableTreeItem subitem;
			if (_subitems.TryGetValue(subitemName, out subitem) == false)
			{
				subitem = new VariableTreeItem(subitemName);
				_subitems.Add(subitemName, subitem);
			}

			return subitem.GetItem(path.Substring(splitter + 1));
		}

		private string ReplaceBrackets(string source, string replace)
		{
			var regex = new Regex(@"\[([a-zA-Z0-1]+)\]");
			return regex.Replace(source, replace);
		}
	}
}