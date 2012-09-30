using System;
using System.Text.RegularExpressions;

namespace DfaCompiler
{
	public partial class Generator
	{
		private static Regex _brackets = new Regex(@"\[([a-zA-Z0-9]+)\]");
		private static Regex _brackets2 = new Regex(@"\[(?<count>[a-zA-Z0-9]+)\]");
		//private static Regex _global = new Regex(@"(?<name>[a-zA-Z0-9]+)\<(?<type>[a-zA-Z0-9]+)\>");//|\<(?<type>[a-zA-Z0-9]+)\>(?<name>[a-zA-Z0-9]+)
		private static Regex _global = new Regex(@"\<(?<type>[a-zA-Z0-9]+)\>");

		private bool HasBrackets(string name)
		{
			return _brackets.IsMatch(name);
		}

		private string RemoveBrackets(string name)
		{
			return _brackets.Replace(name, "");
		}

		private bool IsGlobal(string name)
		{
			return _global.IsMatch(name);
		}

		private string GetGlobalType(string name)
		{
			var match = _global.Match(name);

			if (match.Success)
				return match.Groups["type"].Value;

			throw new ArgumentException();
		}

		private string GetGlobalVarname(string name)
		{
			bool found = false;

			for (; ; )
			{
				var match = _global.Match(name);

				if (match.Success)
				{
					name = name.Replace("<" + match.Groups["type"].Value + ">", "");
					found = true;
				}
				else
					break;
			}

			if (found)
				return name;

			throw new ArgumentException();
		}

		private string RemoveExtraInfo(string varname)
		{
			if (IsGlobal(varname))
				return GetGlobalVarname(varname);
			return varname;
		}

		private string GetVarname(string varname, string prefix)
		{
			varname = RemoveExtraInfo(varname);

			int point = varname.LastIndexOf(".") + 1;
			varname = varname.Insert(point, prefix);

			return AddCountPrefix(varname);
		}

		private string GetCounter(string name)
		{
			var m = _brackets2.Match(name);
			if (m.Success)
				return m.Groups["count"].Value;
			return "";
		}

		private string CountersToParams(string name)
		{
			string params1 = "";

			var matches = _brackets2.Matches(name);

			foreach (Match m in matches)
				params1 += ", int " + ToLowerBegin(m.Groups["count"].Value);

			return params1;
		}

		private string ToLowerBegin(string x)
		{
			return char.ToLower(x[0]) + x.Substring(1, x.Length - 1);
		}

		private string GetCountComparation(string varname)
		{
			var matches = _brackets2.Matches(varname);

			string result = "";
			foreach (Match m in matches)
			{
				if (result != "")
					result += " && ";
				result += "Count." + m.Groups["count"].Value + " < Max." + m.Groups["count"].Value;
			}

			return result;
		}

		private string AddCountPrefix(string varname)
		{
			return _brackets2.Replace(varname, new MatchEvaluator((match) =>
			{
				int point1 = match.Value.LastIndexOf(".");
				return match.Value.Insert((point1 < 0) ? 1 : point1 + 1, "Count.");
			}));
		}
	}
}
