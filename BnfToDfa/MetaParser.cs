using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BnfToDfa
{
	class MetaParser
	{
		#region interface IInformation

		public interface IInformation
		{
			IEnumerable<string> Usings { get; }
			BnfGrammar.Mode Mode { get; }
		}

		#endregion
		#region class Information

		class Information
			: IInformation
		{
			private readonly List<string> usings;
			private BnfGrammar.Mode? mode;

			public Information()
			{
				usings = new List<string>();
			}

			public IEnumerable<string> Usings
			{
				get { return usings; }
			}

			public BnfGrammar.Mode Mode
			{
				get
				{
					if (mode.HasValue)
						return mode.Value;
					return BnfGrammar.Mode.Strict;
				}
				set
				{
					mode = value;
				}
			}

			public bool HasMode
			{
				get { return mode.HasValue; }
			}

			public void AddUsing(string @using)
			{
				usings.Add(@using);
			}
		}

		#endregion

		private readonly Regex regex;

		public MetaParser()
		{
			regex = new Regex("^;@(?<command>[a-z]+)\\s+\"(?<argument>.+?)\"", RegexOptions.Multiline);
		}

		public IInformation Parse(string bnf)
		{
			var result = new Information();

			foreach (Match match in regex.Matches(bnf))
			{
				var command = match.Groups["command"].Value;
				var argument = match.Groups["argument"].Value;

				switch (command)
				{
					case "using":
						result.AddUsing(argument);
						break;

					case "mode":
						if (result.HasMode)
							throw new Exception(@"Meta tag @mode specified twice");
						if (argument == "strict")
							result.Mode = BnfGrammar.Mode.Strict;
						else if (argument == "http-comaptible")
							result.Mode = BnfGrammar.Mode.HttpCompatible;
						else
							throw new Exception(string.Format(@"Unknow @mode specified: {0}", argument));
						break;
				}
			}

			return result;
		}
	}
}
