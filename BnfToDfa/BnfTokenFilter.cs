using System;
using System.Collections.Generic;
using Irony;
using Irony.Parsing;

namespace BnfToDfa
{
	class BnfTokenFilter
		: TokenFilter
	{
		private readonly BnfGrammar grammar;

		public BnfTokenFilter(BnfGrammar grammar)
		{
			this.grammar = grammar;
		}

		public override IEnumerable<Token> BeginFiltering(ParsingContext context, IEnumerable<Token> tokens)
		{
			foreach (var token in tokens)
			{
				if (token.Terminal == grammar.Eof)
					yield return new Token(grammar.NewLine, new SourceLocation(), string.Empty, null);

				yield return token;
			}
		}

		public override void Reset()
		{
		}
	}
}
