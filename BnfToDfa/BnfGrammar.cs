using System;
using System.Collections.Generic;
using System.Linq;
using Irony.Parsing;

namespace BnfToDfa
{
	public class BnfGrammar
		: Grammar
	{
		public enum Mode
		{
			Strict,
			HttpCompatible,
		}

		public BnfGrammar()
			//: this(Mode.HttpCompatible)
			: this(Mode.Strict)
		{
		}

		public BnfGrammar(Mode mode)
		{
			var alternationChar = (mode == Mode.Strict) ? "/" : "|";

			var rulename = new IdentifierTerminal("rulename", "-_", null);
			var funcname = new IdentifierTerminal("funcname", ".", null);
			var newrulename = new IdentifierTerminal("newrulename", "-_", null);
			//var comment1 = new CommentTerminal("comment", "/*", "*/");
			var comment = new CommentTerminal("comment", ";", "\n");
			var bindig1 = new NumberLiteral("bindig", NumberOptions.Binary | NumberOptions.IntOnly);
			var bindig2 = new NumberLiteral("bindig", NumberOptions.Binary | NumberOptions.IntOnly);
			var hexdig1 = new NumberLiteral("hexdig", NumberOptions.Hex | NumberOptions.IntOnly);
			var hexdig2 = new NumberLiteral("hexdig", NumberOptions.Hex | NumberOptions.IntOnly);
			var decdig1 = new NumberLiteral("decvalue", NumberOptions.IntOnly);
			var decdig2 = new NumberLiteral("decvalue", NumberOptions.IntOnly);
			var charval = new StringLiteral("charval", "\"", StringOptions.NoEscapes);
			var repeat1 = new NumberLiteral("repeat1", NumberOptions.IntOnly);
			var repeat2 = new NumberLiteral("repeat2", NumberOptions.IntOnly);
			var minus = ToTerm("-", "minus");
			var point = ToTerm(".", "point");

			bindig1.AddPrefix("b", NumberOptions.None);
			hexdig1.AddPrefix("x", NumberOptions.None);
			decdig1.AddPrefix("d", NumberOptions.None);

			//base.NonGrammarTerminals.Add(comment1);
			base.NonGrammarTerminals.Add(comment);


			// NON TERMINALS
			var numval = new NonTerminal("numval");

			var hexval = new NonTerminal("hexval");
			var hexvalp = new NonTerminal("hexvalpoint");
			var hexvalps = new NonTerminal("hexvalpointstar");

			var binval = new NonTerminal("binval");
			var binvalp = new NonTerminal("binvalpoint");
			var binvalps = new NonTerminal("binvalpointstar");

			var decval = new NonTerminal("decval");
			var decvalp = new NonTerminal("decvalpoint");
			var decvalps = new NonTerminal("decvalpointstar");

			var rule = new NonTerminal("rule");
			var rulelist = new NonTerminal("rulelist");
			var alternation = new NonTerminal("alternation");
			var concatenation = new NonTerminal("concatenation");
			var subtraction = new NonTerminal("subtraction");
			var repetition = new NonTerminal("repetition");
			var repeat = new NonTerminal("repeat");
			var element = new NonTerminal("element");
			var elements = new NonTerminal("elements");
			var group = new NonTerminal("group");
			var option = new NonTerminal("option");
			var func = new NonTerminal("func");
			var funcarg = new NonTerminal("funcarg");
			var funcargs = new NonTerminal("funcargs");

			// RULES
			hexval.Rule = hexdig1 + (hexvalps | (minus + hexdig2) | Empty);
			hexvalp.Rule = point + hexdig2;
			hexvalps.Rule = MakePlusRule(hexvalps, hexvalp);

			binval.Rule = bindig1 + (binvalps | (minus + bindig2) | Empty);
			binvalp.Rule = point + bindig2;
			binvalps.Rule = MakePlusRule(binvalps, binvalp);

			decval.Rule = decdig1 + (decvalps | (minus + decdig2) | Empty);
			decvalp.Rule = point + decdig2;
			decvalps.Rule = MakePlusRule(decvalps, decvalp);

			numval.Rule = ToTerm("%") + (binval | hexval | decval);

			BnfExpression rp = ToTerm("*");
			if (mode == Mode.HttpCompatible)
				rp = rp | "#";

			repeat.Rule = ((repeat1) | ((repeat1 | Empty) + rp + (repeat2 | Empty)));
			group.Rule = ToTerm("(") + alternation + ")";
			option.Rule = ToTerm("[") + alternation + "]";

			funcarg.Rule = alternation;
			funcargs.Rule = MakePlusRule(funcargs, ToTerm(","), funcarg);
			func.Rule = ToTerm("{") + funcname + "," + funcargs + "}";

			alternation.Rule = MakePlusRule(alternation, ToTerm(alternationChar), subtraction);
			subtraction.Rule = MakePlusRule(subtraction, ToTerm("&!"), concatenation);
			concatenation.Rule = MakePlusRule(concatenation, repetition);

			repetition.Rule = (Empty | repeat) + element;
			element.Rule = rulename | group | option | numval | charval | func;

			elements.Rule = alternation;
			rule.Rule = NewLineStar + newrulename + (ToTerm("=") | ToTerm("=" + alternationChar)) + elements + NewLinePlus;
			rulelist.Rule = MakeStarRule(rulelist, rule);

			base.Root = rulelist;
		}

		public override void CreateTokenFilters(LanguageData language, TokenFilterList filters)
		{
			filters.Add(new BnfTokenFilter(this));
		}

		public override void SkipWhitespace(ISourceStream source)
		{
			while (!source.EOF())
			{
				switch (source.PreviewChar)
				{
					case '\r':
					case '\v':
					case ' ':
					case '\t':
						break;

					case '\n':
						if (source.NextPreviewChar != ' ' &&
							source.NextPreviewChar != '\t' &&
							source.NextPreviewChar != '\n' &&
							source.NextPreviewChar != '\r' &&
							source.NextPreviewChar != '\v' &&
							source.NextPreviewChar != ';')
							return;
						break;

					default:
						return;
				}

				source.PreviewPosition++;
			}
		}
	}
}
