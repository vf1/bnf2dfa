using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Fsm;
using DfaCompiler;

namespace DfaToCSharp
{
	class Program
	{
		static int Main(string[] args)
		{
			try
			{
				if (args.Length < 3)
				{
					Console.WriteLine("Usage: DfaToCSharp.exe dfa_file_name.xml c#_namespace c#_class_name [optimization]");
					Console.WriteLine("   Optimization: --WO  - without optimization");
					Console.WriteLine("                 --O1  - optimization #1");
					Console.WriteLine("                 --O2  - optimization #2");
					return -1;
				}

				var dfaXml = args[0];
				var nameSpace = args[1];
				var className = args[2];
				var optimization = (args.Length >= 4) ? args[3] : "--WO";

				Console.WriteLine("Load DFA file: {0}", dfaXml);
				var dfa = Read(dfaXml);

				Console.WriteLine("Generate C# Source Code");


				Console.WriteLine(" DFA Table file: {0}.dfa", nameSpace);
				Console.WriteLine(" Source file: {0}.cs", className);

				if (optimization == "--WO")
				{
					Console.WriteLine(" Not Optimized Version");
					var generator1 = new Generator(OptimizationMode.NoOptimization);
					generator1.Generate(className, className, nameSpace, dfa.Start, true);
				}
				else if (optimization == "--O1")
				{
					Console.WriteLine(" Optimization: Version #1");
					var generator2 = new Generator(OptimizationMode.SingleStatic);
					generator2.Generate(className, className, nameSpace, dfa.Start, false);

				}
				else if (optimization == "--O2")
				{
					Console.WriteLine(" Optimization: Version #2");
					var generator3 = new Generator(OptimizationMode.IndexedArray);
					generator3.Generate(className, className, nameSpace, dfa.Start, false);
				}
				else
				{
					throw new Exception("Unknow optimization mode selected: " + optimization);
				}
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine();
				Console.WriteLine("Error {0}", ex.Message);
				Console.ResetColor();
				return -1;
			}

			return 0;
		}

		private static Dfa Read(string filename)
		{
			XmlSerializer xs = new XmlSerializer(typeof(Dfa));

			using (Stream s = File.OpenRead(filename))
				return (Dfa)xs.Deserialize(s);
		}
	}
}
