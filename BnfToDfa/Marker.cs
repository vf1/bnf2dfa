using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;
using Fsm;
using DfaCompiler;

namespace BnfToDfa
{
	class Marker
	{
		private Dictionary<string, ActionsDescription> marks;
		private Dictionary<string, Dictionary<string, ActionsDescription>> groups;
		private List<string> suppressWarning;

		public Marker()
		{
			marks = new Dictionary<string, ActionsDescription>();
			groups = new Dictionary<string, Dictionary<string, ActionsDescription>>();
			suppressWarning = new List<string>();
		}

		public void LoadSuppressWarning(string path)
		{
			suppressWarning.Clear();

			if (File.Exists(path))
			{
				var lines = File.ReadAllLines(path);

				foreach (var line in lines)
					if (string.IsNullOrEmpty(line) == false && line.Trim().StartsWith(@"//") == false)
						suppressWarning.Add(line.Trim());
			}
		}

		public void LoadMarks(string path)
		{
			marks.Clear();
			groups.Clear();

			string[] strings1 = new string[0];
			if (File.Exists(path))
				strings1 = File.ReadAllLines(path);
			else
				Console.WriteLine("Marks file not found: {0}", path);

			foreach (var mark in strings1)
			{
				if (mark.StartsWith("//") == false && string.IsNullOrEmpty(mark.Trim()) == false)
				{
					if (mark.StartsWith("."))
					{
						int arrow = mark.IndexOf("->", 1);

						if (arrow < 0)
						{
							Console.WriteLine("Error: Failed to parse group {0}", mark);
							throw new Exception();
						}

						int point = mark.IndexOf(".", 1, arrow);

						if (point < 0)
						{
							point = arrow;
							while (mark[point - 1] == ' ')
								point--;
						}

						var groupName = mark.Substring(1, point - 1);
						if (groups.ContainsKey(groupName) == false)
							groups.Add(groupName, new Dictionary<string, ActionsDescription>());

						AddMark(groups[groupName], mark.Substring(point));
					}
					else
					{
						AddMark(marks, mark);
					}
				}
			}
		}

		public IEnumerable<string> GetUnusedRules()
		{
			var unused = new List<string>();

			foreach (var pair in marks)
				if (pair.Value.IsEmpty == false && pair.Value.UseCount == 0)
				{
					if (suppressWarning.Contains(pair.Key) == false)
						unused.Add(pair.Key);
				}

			return unused;
		}

		public State MarkRuleHandler(State start, RulePath path)
		{
			ActionsDescription mark;
			if (marks.TryGetValue(path.Value, out mark) && mark.IsEmpty == false)
			{
				mark.UseCount++;

				foreach (var action in mark.Actions)
				{
					switch (action.Mark)
					{
						case Marks.Custom:
							start = State.MarkCustom(start, action.Args[0], action.Args[1], action.Args[2], action.Args[3]);
							break;
						case Marks.Const:
							start.MarkConst(action.Args[0], action.Args[1], int.Parse(action.Args[2]));
							break;
						case Marks.Range:
							start = State.MarkRange(start, action.Args[0], int.Parse(action.Args[1]), int.Parse(action.Args[2]));
							break;
						case Marks.BeginRange:
							start = State.MarkBeginRange(start, action.Args[0], action.Args[1] == "AtBegin", int.Parse(action.Args[2]));
							break;
						case Marks.EndRange:
						case Marks.EndRangeIfInvalid:
							start = State.MarkEndRange(start, action.Mark, action.Args[0], action.Args[1] == "AtBegin", int.Parse(action.Args[2]));
							break;
						case Marks.ContinueRange:
							start.MarkContinueRange(action.Args[0]);
							break;
						case Marks.Decimal:
							start.MarkDecimal(action.Args[0], action.Args[1], action.Args[2]);
							break;
						case Marks.Hex:
							start.MarkHex(action.Args[0], action.Args[1], action.Args[2]);
							break;
						case Marks.Count:
							start.MarkCount(action.Args[0], int.Parse(action.Args[1]), int.Parse(action.Args[2]));
							break;
						case Marks.Bool:
						case Marks.BoolEx:
						case Marks.BoolExNot:
							start.MarkBool(action.Mark, action.Args[0]);
							break;
						case Marks.ResetRange:
							start.MarkReset(action.Args[0]);
							break;
						case Marks.ResetRangeIfInvalid:
							start.MarkResetIfInvalid(action.Args[0]);
							break;
						default:
							throw new Exception();
					}
				}
			}

			return start;
		}

		private void AddMark(Dictionary<string, ActionsDescription> marks, string mark)
		{
			string markpath, description;
			int x = mark.IndexOf("->");
			if (x < 0)
			{
				markpath = mark;
				description = "";
			}
			else
			{
				markpath = mark.Substring(0, x).Trim();
				description = mark.Substring(x + 2).Trim();
			}

			ActionsDescription.Action[] extraActions = null;
			if (marks.ContainsKey(markpath))
			{
				Console.WriteLine("Warning: Duplicated mark {0}", markpath);
				extraActions = marks[markpath].Actions;
				marks.Remove(markpath);
			}

			var xmark = ActionsDescription.TryParse(description, markpath, extraActions);
			if (xmark == null)
			{
				throw new Exception(string.Format("Error: Failed to parse mark {0}", markpath));
			}
			else
			{
				if (xmark.Actions == null || xmark.Actions[0].Mark != Marks.Group)
					marks.Add(markpath, xmark);
				else
				{
					try
					{
						var group = groups[xmark.Actions[0].Args[1]];

						foreach (var item in group)
							marks.Add(markpath + item.Key, item.Value.Clone("?", xmark.Actions[0].Args[0]));
					}
					catch (KeyNotFoundException)
					{
						Console.WriteLine("Group not defined {0}", xmark.Actions[0].Args[0]);
						throw new Exception();
					}
				}
			}
		}
	}
}
