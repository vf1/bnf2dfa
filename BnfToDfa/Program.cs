using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Reflection;
using Irony.Parsing;
using Fsm;

namespace BnfToDfa
{
	class Program
	{
		static int Main(string[] args)
		{
			try
			{
				if (args.Length < 2 || args.Length > 4)
				{
					Console.WriteLine("Usage:");
					Console.WriteLine("  BnfToDfa.exe bnf_file.bnf mark_file.mrk root_rule_name [no_warning_file.nwr]");
					Console.WriteLine("-or-");
					Console.WriteLine("  BnfToDfa.exe bnf_file.bnf root_rule_name");
					return -1;
				}

				var bnfFile = args[0];
				var mrkFile = (args.Length >= 3) ? args[1] : null;
				var rootRule = (args.Length >= 3) ? args[2] : args[1];
				var nwrFile = (args.Length >= 4) ? args[3] : null;
				var dfaFile = Path.GetFileNameWithoutExtension(bnfFile) + ".xml";


				var builder = LoadBnfFiles(bnfFile);


				var marker = new Marker();
				if (mrkFile != null)
				{
					Console.WriteLine("Load marks file: {0}", mrkFile);
					marker.LoadMarks(mrkFile);
				}

				if (nwrFile != null)
				{
					Console.WriteLine("Load 'No warning' file: {0}", nwrFile);
					marker.LoadSuppressWarning(nwrFile);
				}


				Console.Write("Build NFA");
				var nfa = builder.CreateNfa(rootRule, marker.MarkRuleHandler);
				Console.WriteLine(", max NFA state id: {0}", Fsm.State.MaxId);


				foreach (var unused in marker.GetUnusedRules())
					Console.WriteLine("UNUSED: {0}", unused);


				PackNfa.Pack(nfa, true);


				int count;
				var dfa = nfa.ToDfa3(out count, true);
				Console.WriteLine("DFA Complied States: {0}", count);


				var minCount = dfa.Minimize(true);
				Console.WriteLine("Minimized DFA States: {0}", minCount);


				Console.WriteLine("Write DFA file: {0}", dfaFile);
				Writer.Write(dfa, dfaFile);
			}
			catch (Exception ex)
			{
				Console.WriteLine();
				Console.WriteLine("Error {0}", ex.Message);
				return -1;
			}

			return 0;
		}

		private static Builder LoadBnfFiles(string fileName)
		{
			var result = new Builder();

			LoadBnfFile(fileName, result);

			return result;
		}

		private static void LoadBnfFile(string fileName, Builder mainBuilder)
		{
			Console.WriteLine("Parse BNF file: {0}", fileName);


			var bnf = File.ReadAllText(fileName);


			var metaParser = new MetaParser();
			var meta = metaParser.Parse(bnf);


			var oprimizedBnf = Optimize(bnf);


			var parser = new Parser(new BnfGrammar(meta.Mode));
			var tree = parser.Parse(oprimizedBnf, fileName);
			if (tree.Status == ParseTreeStatus.Error)
			{
				throw new Exception((tree.ParserMessages.Count > 0)
					? string.Format("{0}, in {3} file at line {1}, column {2}", tree.ParserMessages[0].Message, tree.ParserMessages[0].Location.Line, tree.ParserMessages[0].Location.Column, fileName)
					: string.Format(@"Unknow error in BNF file {0}", fileName));
			}


			var builder = new Builder(tree, mainBuilder);
			builder.BuildExpressions();


			foreach (var @using in meta.Usings)
				LoadBnfFile(@using, mainBuilder);
		}

		static string Optimize(string xbnf)
		{
			var repeatBy = new Regex(@"(?<item>[A-Za-z0-9\-_]+)\s+\*\((?<separator>[A-Za-z0-9\-_]+)\s+\k<item>\)");

			return repeatBy.Replace(xbnf, "{RepeatBy, ${item}, ${separator}}");
		}
	}
}
