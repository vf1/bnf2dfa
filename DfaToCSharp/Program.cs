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
			if (args.Length < 3)
			{
				Console.WriteLine("Usage: DfaToCSharp.exe dfa_file_name.xml c#_namespace c#_class_name");
				return -1;
			}

			var dfaXml = args[0];
			var nameSpace = args[1];
			var className = args[2];

			Console.WriteLine("Load DFA file: {0}", dfaXml);
			var dfa = Read(dfaXml);

			Console.WriteLine("Generate C# Source Code");
			Console.WriteLine(" DFA Table file:        {0}.dfa", nameSpace);
			Console.WriteLine(" Not Optimized Version: {0}NO.cs", className);
			var generator1 = new Generator(OptimizationMode.NoOptimization);
			generator1.Generate(className, className, nameSpace, dfa.Start, true);

			Console.WriteLine(" Optimized Version #1:  {0}O1.cs", className);
			var generator2 = new Generator(OptimizationMode.SingleStatic);
			generator2.Generate(className + "Optimized", className, nameSpace, dfa.Start, false);

			Console.WriteLine(" Optimized Version #2:  {0}O2.cs", className);
			var generator3 = new Generator(OptimizationMode.IndexedArray);
			generator3.Generate(className + "Optimized2", className, nameSpace, dfa.Start, false);

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
