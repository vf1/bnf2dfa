using System;
using System.Collections.Generic;
using System.Text;
using Irony.Parsing;
using Fsm;

namespace BnfToDfa
{
	partial class Builder
	{
		private readonly ParseTree tree;
		private readonly Dictionary<string, AlternationExpression> rules;
		private Func<State, RulePath, State> markAction;

		public Builder(ParseTree tree)
		{
			this.tree = tree;

			this.rules = new Dictionary<string, AlternationExpression>();
		}

		public State CreateNfa(string rootName, Func<State, RulePath, State> markAction)
		{
			State nfa;

			try
			{
				this.markAction = markAction;

				if (rules.ContainsKey(rootName) == false)
					throw new BuilderException(@"@Builder: root rule not found");

				nfa = rules[rootName].GetNfa(new RulePath(rootName));
			}
			finally
			{
				this.markAction = null;
			}

			nfa.MarkFinal();

			return nfa;
		}

		public void AddExpressions(Builder other)
		{
			foreach (var pair in other.rules)
			{
				if (rules.ContainsKey(pair.Key))
					throw new BuilderException(string.Format(@"Rule |{0}| exist in two different files", pair.Key));
				rules.Add(pair.Key, pair.Value);
			}
		}

		protected State OnMarkRule(State start, RulePath path)
		{
			if (markAction != null)
				return markAction(start, path);
			return start;
		}

		public void BuildExpressions()
		{
			foreach (var child in tree.Root.ChildNodes)
				BuildRuleExpressions(child);
		}

		private void BuildRuleExpressions(ParseTreeNode node)
		{
			var ruleName = node.FindTokenAndGetText();
			var rule = rules.ContainsKey(ruleName) ? rules[ruleName] : rules[ruleName] = new AlternationExpression();

			ParseTreeNode elements = null;
			foreach (var child in node.ChildNodes)
				if (child.Term.Name == "elements")
					elements = child;

			rule.Add(
				BuildAlternationExpression(elements.ChildNodes[0]));
		}

		private IExpression BuildExpression(ParseTreeNode node)
		{
			return BuildAlternationExpression(node);
		}

		private IExpression BuildAlternationExpression(ParseTreeNode node)
		{
			int count = node.ChildNodes.Count;

			if (count <= 0)
			{
				throw new BuilderException(node, @"Invalid alternation expression, no child nodes");
			}
			else if (count == 1)
			{
				return BuildSubtractionExpression(node.ChildNodes[0]);
			}
			else
			{
				var alternation = new AlternationExpression();

				foreach (var child in node.ChildNodes)
					alternation.Add(BuildSubtractionExpression(child));

				return alternation;
			}
		}

		private IExpression BuildSubtractionExpression(ParseTreeNode node)
		{
			int count = node.ChildNodes.Count;

			if (count == 1)
			{
				return BuildConcatanationExpression(node.ChildNodes[0]);
			}
			else if (count == 2)
			{
				return new SubtractionExpression(
					BuildConcatanationExpression(node.ChildNodes[0]),
					BuildConcatanationExpression(node.ChildNodes[1]));
			}
			else
			{
				throw new BuilderException(node, @"Invalid subtraction expression, too many args");
			}
		}

		private IExpression BuildConcatanationExpression(ParseTreeNode node)
		{
			int count = node.ChildNodes.Count;

			if (count <= 0)
			{
				throw new BuilderException(node, @"Invalid concatanation rule, no child nodes");
			}
			else if (count == 1)
			{
				return BuildRepeationExpression(node.ChildNodes[0]);
			}
			else
			{
				var concatanation = new ConcatanationExpression();

				foreach (var child in node.ChildNodes)
					concatanation.Add(BuildRepeationExpression(child));

				return concatanation;
			}
		}

		private IExpression BuildRepeationExpression(ParseTreeNode node)
		{
			if (node.ChildNodes[0].ChildNodes.Count == 0)
			{
				return BuildElementExpression(node.ChildNodes[1]);
			}
			else
			{
				string repeatChar = "*";
				int repeat1 = -1, repeat2 = -1;
				var repeat = node.ChildNodes[0].ChildNodes[0];

				if (repeat.ChildNodes.Count == 1)
				{
					repeat2 = repeat1 = int.Parse(repeat.ChildNodes[0].FindTokenAndGetText());
				}
				else if (repeat.ChildNodes.Count >= 3)
				{
					if (repeat.ChildNodes[0].ChildNodes.Count != 0)
						repeat1 = int.Parse(repeat.ChildNodes[0].FindTokenAndGetText());

					repeatChar = repeat.ChildNodes[1].FindTokenAndGetText();

					if (repeat.ChildNodes[2].ChildNodes.Count != 0)
						repeat2 = int.Parse(repeat.ChildNodes[2].FindTokenAndGetText());
				}

				switch (repeatChar)
				{
					case "*":
						return new RepeationExpression(repeat1, repeat2, BuildElementExpression(node.ChildNodes[1]));

					case "#":
						if (repeat2 != -1)
							throw new NotImplementedException();
						return new RepeationByExpression(repeat1, repeat2, BuildElementExpression(node.ChildNodes[1]), BuildRuleLinkExpression("COMMA"));

					default:
						throw new NotImplementedException();
				}
			}
		}

		private IExpression BuildElementExpression(ParseTreeNode node)
		{
			switch (node.ChildNodes[0].Term.Name)
			{
				case "rulename":
					return BuildRuleLinkExpression(node.ChildNodes[0].FindTokenAndGetText());
				case "group":
					return BuildExpression(node.ChildNodes[0].ChildNodes[1]);
				case "option":
					return new OptionExpression(BuildExpression(node.ChildNodes[0].ChildNodes[1]));
				case "numval":
					return BuildNumvalExpression(node.ChildNodes[0]);
				case "charval":
					return BuildCharvalExpression(node.ChildNodes[0]);
				case "func":
					return BuildFuncCallExpression(node.ChildNodes[0]);
				default:
					throw new InvalidProgramException();
			}
		}

		private IExpression BuildRuleLinkExpression(string name)
		{
			return new RuleLinkExpression(this, name, rules);
		}

		private IExpression BuildNumvalExpression(ParseTreeNode node)
		{
			if (node.ChildNodes[1].ChildNodes[0].Term.Name == "hexval")
			{
				var node2 = node.ChildNodes[1].ChildNodes[0];
				var hex1 = Convert.ToByte("0" + node2.ChildNodes[0].FindTokenAndGetText(), 16);

				//if (node2.ChildNodes.Count == 1)
				//{
				//    return new NumvalExpression(hex1);
				//}
				//else
				// ?????????????????????????????????????????????????????????????

				{
					var operation = node2.ChildNodes[1].FindTokenAndGetText();

					if (operation == "-")
					{
						return new NumvalExpression(hex1,
							Convert.ToByte("0x" + node2.ChildNodes[1].ChildNodes[node2.ChildNodes[1].ChildNodes.Count - 1].FindTokenAndGetText(), 16));
					}
					else if (operation == ".")
					{
						var expression = new NumvalExpression();

						var hexvalpointstart = node2.ChildNodes[1].ChildNodes[0];

						expression.Add(hex1);
						foreach (var hexval1 in hexvalpointstart.ChildNodes)
							expression.Add(Convert.ToByte("0x" + hexval1.ChildNodes[node2.ChildNodes[1].ChildNodes.Count - 1].FindTokenAndGetText(), 16));

						return expression;
					}
					else
					{
						return new NumvalExpression(hex1);
					}
					//else
					//{
					//    throw new SemanticErrorException(node, @"Unknow operation in Numval expression", operation);
					//}
				}
			}

			throw new InvalidProgramException("Dec and Binary const not implemented");
		}

		private IExpression BuildCharvalExpression(ParseTreeNode node)
		{
			var charval = node.FindTokenAndGetText();//.Replace("\\", "\\\\");
			charval = charval.Substring(1, charval.Length - 2);
			return new CharvalExpression(this, charval);
		}

		private IExpression BuildFuncCallExpression(ParseTreeNode node)
		{
			var name = node.ChildNodes[1].FindTokenAndGetText();

			var expression = new FuncCallExpression(name);

			foreach (var argument in node.ChildNodes[3].ChildNodes)
			{
				expression.AddArgument(
					BuildExpression(argument.ChildNodes[0]));
			}

			return expression;
		}

		//-----------------------------------------

		public void CreateDefinedRulesList(ParseTreeNode node, List<string> rulenames)
		{
			foreach (var child in node.ChildNodes)
				CreateDefinedRulesList(child, rulenames);

			if (node.Term.Name == "newrulename")
				rulenames.Add(node.FindTokenAndGetText());
		}

		public void CreateUsedRulesList(ParseTreeNode node, List<string> rulenames)
		{
			foreach (var child in node.ChildNodes)
				CreateUsedRulesList(child, rulenames);

			if (node.Term.Name == "rulename")
			{
				var rulename = node.FindTokenAndGetText();

				if (rulenames.Find((fromList) => { return fromList == rulename; }) == null)
					rulenames.Add(node.FindTokenAndGetText());
			}
		}
	}
}
