using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Threading;
using Fsm;

namespace DfaCompiler
{
	public enum OptimizationMode
	{
		NoOptimization,
		SingleStatic,
		IndexedArray,
	}

	public partial class Generator
	{
		private TextWriter _main;
		private BinaryWriter _table;
		private VariableTreeItem _varibalesTree;
		private OptimizationMode _optimized;
		private string _beginEndStructName;

		public Generator(OptimizationMode optimized)
		{
			_optimized = optimized;

			switch (optimized)
			{
				case OptimizationMode.NoOptimization:
					_beginEndStructName = "ByteArrayPart";
					break;
				case OptimizationMode.SingleStatic:
					_beginEndStructName = "BeginEnd";
					break;
				case OptimizationMode.IndexedArray:
					_beginEndStructName = "BeginEndIndex";
					break;
			}
		}

		public void ParseDeclaration(DfaState dfa)
		{
			_varibalesTree = new VariableTreeItem("root");

			dfa.ForEachNR((state) =>
			{
				_varibalesTree.AddVariables(state);
			});
		}

		public void ParseDeclaration(Dictionary<string, ActionsDescription> descriptions)
		{
			_varibalesTree = new VariableTreeItem("root");

			foreach (var descr in descriptions)
			{
				if (descr.Value.Actions != null)
				{
					foreach (var action1 in descr.Value.Actions)
					{
						var item = _varibalesTree.GetItem(action1.Args[0]);

						var mark = new MarkImpl()
						{
							Mark = action1.Mark,
							Name = action1.Args[0],
						};

						if (action1.Mark == Marks.Const)
							mark.Value = action1.Args[1];

						item.AddVariable(mark);

						if (action1.Mark == Marks.BeginRange)
						{
							item.AddVariable(
								new MarkImpl()
								{
									Mark = Marks.EndRange,
									Name = action1.Args[0],
								});
						}
					}
				}
			}
		}

		private void GenerateVaribales(Dictionary<string, VariableInfo> vars, string type1, string wrapper)
		{
			if (vars.Count > 0)
			{
				_main.WriteLine("public struct {0}s", wrapper);
				_main.WriteLine("{");

				GenerateVaribales(vars, type1);

				_main.WriteLine("}");

				_main.WriteLine("public {0}s {0};", wrapper);
			}
		}

		private void GenerateVaribales(Dictionary<string, VariableInfo> vars, string type1)
		{
			foreach (var var1 in vars)
				GenerateVaribale(var1.Value.ShortName, type1);
		}

		private void GenerateCustomVaribales(Dictionary<string, VariableInfo> vars)
		{
			foreach (var var1 in vars)
				GenerateVaribale(var1.Value.ShortName, var1.Value.Type);
		}

		private void GenerateVaribale(string shortName, string type1)
		{
			_main.Write("public ");
			_main.Write(type1);
			if (HasBrackets(shortName))
				_main.Write("[]");
			_main.Write(" ");
			_main.Write(RemoveBrackets(shortName));
			_main.Write(";");
			_main.WriteLine();
		}

		private void GenerateConstsMax(Dictionary<string, VariableInfo> vars, string type1)
		{
			if (vars.Count > 0)
			{
				_main.WriteLine("public class Max");
				_main.WriteLine("{");

				foreach (var var1 in vars)
				{
					_main.Write("public ");
					_main.Write(type1);
					_main.Write(" ");
					_main.Write(var1.Value.ShortName);
					_main.Write(" = ");
					_main.Write(var1.Value.Max);
					_main.Write(";");
					_main.WriteLine();
				}

				_main.WriteLine("}");
			}
		}

		//public void GenerateLoadTables(string filename, string namespace1, int states)
		//{
		//    using (_main = File.CreateText(filename + ".LoadTable.cs"))
		//    {
		//        _main.WriteLine("using System;");
		//        _main.WriteLine("using System.Text;");
		//        _main.WriteLine("using System.IO;");
		//        _main.WriteLine("using System.IO.Compression;");
		//        _main.WriteLine("using Server.Memory;");
		//        _main.WriteLine();

		//        _main.WriteLine("namespace {0}", namespace1);
		//        _main.WriteLine("{");

		//        _main.WriteLine("public partial class {0}", filename);
		//        _main.WriteLine(":IDefaultValue");
		//        _main.WriteLine("{");

		//        GenerateStateConsts(states+1);
		//        GenerateLoadFunction3(states, false);

		//        _main.WriteLine("}");
		//        _main.WriteLine("}");

		//        _main.Flush();
		//    }
		//}

		protected void GenerateStateConsts(int count)
		{
			_main.WriteLine("#region enum States");

			for (int i = 0; i < count; i++)
				_main.WriteLine("const int State{0} = {0};", i);

			_main.WriteLine("#endregion");
		}

		private void WriteIfOptimizedStatement()
		{
			switch (_optimized)
			{
				case OptimizationMode.NoOptimization:
					_main.WriteLine("#if (!OPTIMIZED && !OPTIMIZED2)");
					break;
				case OptimizationMode.SingleStatic:
					_main.WriteLine("#if OPTIMIZED");
					break;
				case OptimizationMode.IndexedArray:
					_main.WriteLine("#if OPTIMIZED2");
					break;
			}
		}

		public void Generate(string filename, string classname, string namespace1, DfaState dfa, bool writeDfaFile)
		{
			using (_main = File.CreateText(filename + ".cs"))
			using (_table = new BinaryWriter((writeDfaFile) ?
					new DeflateStream(
						new BufferedStream(File.Create(namespace1 + ".dfa"), 65536), CompressionMode.Compress)
					: Stream.Null))
			{

				if (dfa != null)
					ParseDeclaration(dfa);

				WriteIfOptimizedStatement();

				_main.WriteLine("using System;");
				_main.WriteLine("using System.Text;");
				_main.WriteLine("using System.IO;");
				_main.WriteLine("using System.IO.Compression;");
				_main.WriteLine("using Base.Message;");
				if (_optimized == OptimizationMode.IndexedArray)
					_main.WriteLine("using System.Threading;");
				_main.WriteLine();

				_main.WriteLine("namespace {0}", namespace1);
				_main.WriteLine("{");

				GenerateEnums(_varibalesTree, new List<string>());
				GenerateGlobalStructs(_varibalesTree, new List<string>());

				_main.WriteLine("public partial class {0}", classname);
				if (_optimized == OptimizationMode.IndexedArray)
					_main.WriteLine(":IDisposable");
				_main.WriteLine("{");

				if (_optimized == OptimizationMode.SingleStatic)
				{
					_main.WriteLine("[ThreadStatic]");
					_main.WriteLine("public static byte[] Bytes;");

					_main.WriteLine("/// <summary>");
					_main.WriteLine("/// ATT: must be call each time when thread get control for optimized version");
					_main.WriteLine("/// </summary>");
					_main.WriteLine("public void SetArray(byte[] bytes)");
					_main.WriteLine("{");
					_main.WriteLine("Bytes=bytes;");
					_main.WriteLine("}");
				}
				else if (_optimized == OptimizationMode.IndexedArray)
				{
					_main.WriteLine("internal static byte[][] bytes = new byte[32][];");
					_main.WriteLine("private int index;");

					_main.WriteLine("public void SetArray(byte[] bytes1)");
					_main.WriteLine("{");
					_main.WriteLine("lock (sync)");
					_main.WriteLine("bytes[index]=bytes1;");
					_main.WriteLine("}");

					_main.WriteLine("public {0}()", classname);
					_main.WriteLine("{");
					_main.WriteLine("AcquireArraySlot();");
					_main.WriteLine("}");

					_main.WriteLine("~{0}()", classname);
					_main.WriteLine("{");
					_main.WriteLine("ReleaseArraySlot();");
					_main.WriteLine("}");

					_main.WriteLine("public void Dispose()");
					_main.WriteLine("{");
					_main.WriteLine("ReleaseArraySlot();");
					_main.WriteLine("GC.SuppressFinalize(this);");
					_main.WriteLine("index=-1;");
					_main.WriteLine("}");
				}

				_main.WriteLine("public bool Final;");
				_main.WriteLine("public bool IsFinal { get { return Final; }}");

				_main.WriteLine("public bool Error;");
				_main.WriteLine("public bool IsError { get { return Error; }}");

				_main.WriteLine("private int state;");
				_main.WriteLine("private int boolExPosition;");

				GenerateVariables(_varibalesTree, true);

				int countStates = 0;
				if (dfa != null)
					dfa.ForEachNR((state) => { state.Index = countStates++; });
				else
					countStates = 1;

				GenerateStateConsts(countStates + 1);
				GenerateTables(dfa, countStates);
				GenerateLoadFunction3(countStates, dfa == null);
				GenerateParseMethod(dfa, countStates);
				GenerateGetHexDigitFunction();

				_main.WriteLine("}");
				_main.WriteLine("}");

				_main.WriteLine("#endif");

				_main.Flush();
				_table.Flush();
			}
		}

		private void GenerateVariables(VariableTreeItem item)
		{
			GenerateVariables(item, false);
		}

		private void GenerateVariables(VariableTreeItem item, bool sipMessageReaderClass)
		{
			GenerateConstsMax(item.Counts, "const int");

			GenerateVaribales(item.Counts, "int", "Count");
			GenerateCustomVaribales(item.Customs);
			GenerateVaribales(item.Begins1, _beginEndStructName);
			//GenerateVaribales(item.Decimals1, "int");
			GenerateCustomVaribales(item.Decimals1);
			GenerateVaribales(item.Bools, "bool");

			foreach (var pair in item.Enums)
				GenerateVaribale(pair.Key, RemoveBrackets(pair.Key) + "s");

			foreach (var subitem in item.Subitems)
			{
				string structName;

				if (IsGlobal(subitem.Name))
				{
					structName = GetGlobalType(subitem.Name);
				}
				else
				{
					structName = RemoveBrackets(subitem.Name) + "Struct";

					_main.WriteLine("public partial struct {0}", structName);
					_main.WriteLine("{");

					GenerateVariables(subitem);

					_main.WriteLine("}");
				}

				GenerateVaribale(RemoveExtraInfo(subitem.Name), structName);
			}

			GenerateInitializers(item, sipMessageReaderClass);

			if (_optimized == OptimizationMode.NoOptimization)
				GenerateSetArrayFunction(item);
		}

		private void GenerateSetArrayFunction(VariableTreeItem item)
		{
			GenerateSetXFunction(item, "SetArray", "byte[]", "Bytes");
		}

		private void GenerateSetXFunction(VariableTreeItem item, string functionName, string fieldType, string field)
		{
			var argName = field.ToLower();

			_main.WriteLine("public void " + functionName + "(" + fieldType + " " + argName + ")");
			_main.WriteLine("{");

			foreach (var subitem in item.Subitems)
			{
				if (HasBrackets(subitem.Name))
				{
					var name = RemoveExtraInfo(RemoveBrackets(subitem.Name));
					var counter = GetCounter(subitem.Name);

					_main.WriteLine("for(int i=0; i<Max.{0}; i++)", counter);
					_main.WriteLine("{0}[i]." + functionName + "(" + argName + ");", name);
				}
				else
				{
					_main.WriteLine("{0}." + functionName + "(" + argName + ");", RemoveExtraInfo(subitem.Name));
				}
			}

			foreach (var begin in item.Begins1)
			{
				if (HasBrackets(begin.Value.ShortName))
				{
					var name = RemoveExtraInfo(RemoveBrackets(begin.Value.ShortName));
					var counter = GetCounter(begin.Value.ShortName);

					_main.WriteLine("for(int i=0; i<Max.{0}; i++)", counter);
					_main.WriteLine("{0}[i]." + field + " = " + argName + ";", name);
				}
				else
				{
					_main.WriteLine("{0}." + field + " = " + argName + ";", begin.Value.ShortName);
				}
			}

			_main.WriteLine("}");
		}

		private void GenerateInitializers(Dictionary<string, VariableInfo> vars)
		{
			foreach (var var1 in vars)
				GenerateInitializer(var1.Value.ShortName, var1.Value.Type, "", var1.Value.Default);
		}

		private void GenerateInitializers(Dictionary<string, VariableInfo> vars, string type1, string prefix, string value)
		{
			foreach (var var1 in vars)
				GenerateInitializer(var1.Value.ShortName, type1, prefix, value);
		}

		private void GenerateInitializers(Dictionary<string, VariableInfo> vars, string type1, string prefix)
		{
			foreach (var var1 in vars)
				GenerateInitializer(var1.Value.ShortName, type1, prefix, var1.Value.Default);
		}

		private void GenerateInitializer(string shortName, string type1, string prefix, string value)
		{
			if (value.StartsWith(".") == false)
				value = " = " + value;

			if (HasBrackets(shortName))
			{
				var name = RemoveExtraInfo(RemoveBrackets(shortName));
				var counter = GetCounter(shortName);

				_main.WriteLine("if({0}{1}==null) {0}{1}=new {2}[Max.{3}];", prefix, name, type1, counter);
				_main.WriteLine("for(int i=0; i<Max.{2}; i++) {0}{1}[i]{3};", prefix, name, counter, value);
			}
			else
			{
				_main.Write(prefix);
				_main.Write(shortName);
				_main.Write(value);
				_main.Write(";");
				_main.WriteLine();
			}
		}

		//private void GenerateInitializers(VariableTreeItem item)
		//{
		//    GenerateInitializers(item, false);
		//}

		private string GetSetDefauleValueCall()
		{
			return (_optimized == OptimizationMode.IndexedArray) ? "SetDefaultValue(index)" : "SetDefaultValue()";
		}

		private void GenerateInitializers(VariableTreeItem item, bool isRootClass)
		{
			_main.WriteLine("partial void OnSetDefaultValue();");

			if (isRootClass || _optimized != OptimizationMode.IndexedArray)
				_main.WriteLine("public void SetDefaultValue()");
			else
				_main.WriteLine("public void SetDefaultValue(int index)");
			_main.WriteLine("{");

			if (isRootClass)
			{
				if (_optimized == OptimizationMode.SingleStatic)
				{
					_main.WriteLine("Bytes=null;");
				}
				else if (_optimized == OptimizationMode.IndexedArray)
				{
					_main.WriteLine("lock (sync)");
					_main.WriteLine("bytes[index]=null;");
				}
			}

			foreach (var subitem in item.Subitems)
			{
				if (HasBrackets(subitem.Name))
				{
					var name = RemoveExtraInfo(RemoveBrackets(subitem.Name));
					var counter = GetCounter(subitem.Name);

					var structName = IsGlobal(subitem.Name) ?
						GetGlobalType(subitem.Name) : RemoveBrackets(subitem.Name) + "Struct";

					_main.WriteLine("if({0} == null)", name);
					_main.WriteLine("{");
					_main.WriteLine("{0} = new {2}[Max.{1}];", name, counter, structName);
					_main.WriteLine("for(int i=0; i<Max.{0}; i++)", counter);
					_main.WriteLine("{0}[i] = new {1}();", name, structName);
					_main.WriteLine("}");

					_main.WriteLine("for(int i=0; i<Max.{0}; i++)", counter);
					_main.WriteLine("{0}[i]." + GetSetDefauleValueCall() + ";", name);
				}
				else
				{
					_main.WriteLine("{0}." + GetSetDefauleValueCall() + ";", RemoveExtraInfo(subitem.Name));
				}
			}

			if (isRootClass)
			{
				_main.WriteLine("Final = false;");
				_main.WriteLine("Error = false;");
				_main.WriteLine("state = State0;");
				_main.WriteLine("boolExPosition = int.MinValue;");
			}

			foreach (var pair in item.Enums)
			{
				var name = RemoveBrackets(pair.Key);
				GenerateInitializer(pair.Key, name + "s", "", name + "s.None");
			}

			GenerateInitializers(item.Begins1, _beginEndStructName, "", "." + GetSetDefauleValueCall());

			GenerateInitializers(item.Counts, "int", "Count.");
			//GenerateInitializers(item.Decimals1, "int", "", "int.MinValue");
			GenerateInitializers(item.Decimals1);
			GenerateInitializers(item.Bools, "bool", "", "false");

			_main.WriteLine("OnSetDefaultValue();");

			_main.WriteLine("}");
		}

		#region Experimental - wrong way!

		//private void GenerateInitializers(VariableTreeItem item, bool isRootClass)
		//{
		//    if (_optimized == OptimizationMode.IndexedArray)
		//    {
		//        GenerateInitializersIndexArrayOptimized_Part1(item, isRootClass);
		//        GenerateInitializersIndexArrayOptimized_Part2(item, isRootClass);
		//    }
		//    else
		//        GenerateInitializersClassic(item, isRootClass);
		//}

		//private void GenerateInitializersClassic(VariableTreeItem item, bool sipMessageReaderClass_Root)
		//{
		//    _main.WriteLine("partial void OnSetDefaultValue();");

		//    _main.WriteLine("public void SetDefaultValue()");
		//    _main.WriteLine("{");

		//    if (sipMessageReaderClass_Root)
		//    {
		//        if (_optimized == OptimizationMode.SingleStatic)
		//        {
		//            _main.WriteLine("Bytes=null;");
		//        }
		//        else if (_optimized == OptimizationMode.IndexedArray)
		//        {
		//            _main.WriteLine("if(bytes[index]!=null)");
		//            _main.WriteLine("lock (sync)");
		//            _main.WriteLine("bytes[index]=null;");
		//        }
		//    }

		//    foreach (var subitem in item.Subitems)
		//    {
		//        if (HasBrackets(subitem.Name))
		//        {
		//            var name = RemoveExtraInfo(RemoveBrackets(subitem.Name));
		//            var counter = GetCounter(subitem.Name);

		//            var structName = IsGlobal(subitem.Name) ?
		//                GetGlobalType(subitem.Name) : RemoveBrackets(subitem.Name) + "Struct";

		//            _main.WriteLine("if({0} == null)", name);
		//            _main.WriteLine("{");
		//            _main.WriteLine("{0} = new {2}[Max.{1}];", name, counter, structName);
		//            _main.WriteLine("for(int i=0; i<Max.{0}; i++)", counter);
		//            _main.WriteLine("{0}[i] = new {1}();", name, structName);
		//            _main.WriteLine("}");

		//            _main.WriteLine("for(int i=0; i<Max.{0}; i++)", counter);
		//            _main.WriteLine("{0}[i].SetDefaultValue();", name);
		//        }
		//        else
		//        {
		//            _main.WriteLine("{0}.SetDefaultValue();", RemoveExtraInfo(subitem.Name));
		//        }
		//    }

		//    if (item == _varibalesTree)
		//    {
		//        _main.WriteLine("Final = false;");
		//        _main.WriteLine("Error = false;");
		//        _main.WriteLine("state = State0;");
		//        _main.WriteLine("boolExPosition = int.MinValue;");
		//    }

		//    foreach (var pair in item.Enums)
		//    {
		//        var name = RemoveBrackets(pair.Key);
		//        GenerateInitializer(pair.Key, name + "s", "", name + "s.None");
		//    }

		//    //foreach(var begin in item.Begins1)
		//    //    _main.WriteLine("{0}.SetDefaultValue();", begin.Value.ShortName);
		//    GenerateInitializers(item.Begins1, _beginEndStructName, "", ".SetDefaultValue()");

		//    GenerateInitializers(item.Counts, "int", "Count.");
		//    GenerateInitializers(item.Decimals1, "int", "", "int.MinValue");
		//    GenerateInitializers(item.Bools, "bool", "", "false");

		//    _main.WriteLine("OnSetDefaultValue();");

		//    _main.WriteLine("}");
		//}

		//private void GenerateInitializersIndexArrayOptimized_Part1(VariableTreeItem item, bool isRootClass)
		//{
		//    _main.WriteLine("internal void Initialize(" + (isRootClass ? "" : "int index") + ")");
		//    _main.WriteLine("{");

		//    foreach (var subitem in item.Subitems)
		//    {
		//        if (HasBrackets(subitem.Name))
		//        {
		//            var name = RemoveExtraInfo(RemoveBrackets(subitem.Name));
		//            var counter = GetCounter(subitem.Name);

		//            var structName = IsGlobal(subitem.Name) ?
		//                GetGlobalType(subitem.Name) : RemoveBrackets(subitem.Name) + "Struct";

		//            _main.WriteLine("{0} = new {2}[Max.{1}];", name, counter, structName);
		//            _main.WriteLine("for(int i=0; i<Max.{0}; i++)", counter);
		//            _main.WriteLine("{0}[i].Initialize(index);", name);
		//        }
		//        else
		//        {
		//            _main.WriteLine("{0}.Initialize(index);", RemoveExtraInfo(subitem.Name));
		//        }
		//    }

		//    if (isRootClass)
		//    {
		//        _main.WriteLine("Final = false;");
		//        _main.WriteLine("Error = false;");
		//        _main.WriteLine("state = State0;");
		//        _main.WriteLine("boolExPosition = int.MinValue;");
		//    }

		//    foreach (var pair in item.Enums)
		//    {
		//        var name = RemoveBrackets(pair.Key);
		//        GenerateInitializer(pair.Key, name + "s", "", name + "s.None");
		//    }

		//    GenerateInitializers(item.Begins1, _beginEndStructName, "", ".SetDefaultValue(index)");

		//    GenerateInitializers(item.Counts, "int", "Count.");
		//    GenerateInitializers(item.Decimals1, "int", "", "int.MinValue");
		//    GenerateInitializers(item.Bools, "bool", "", "false");

		//    _main.WriteLine("}");
		//}

		//private void GenerateInitializersIndexArrayOptimized_Part2(VariableTreeItem item, bool isRootClass)
		//{
		//    _main.WriteLine("partial void OnSetDefaultValue();");

		//    _main.WriteLine("public void SetDefaultValue()");
		//    _main.WriteLine("{");

		//    if (isRootClass)
		//    {
		//        _main.WriteLine("if(isInitialized==false)");
		//        _main.WriteLine("{");
		//        _main.WriteLine("isInitialized=true;");
		//        _main.WriteLine("Initialize();");
		//        _main.WriteLine("}");
		//        _main.WriteLine("else");
		//        _main.WriteLine("{");
		//    }

		//    if (isRootClass)
		//    {
		//        _main.WriteLine("if(bytes[index]!=null)");
		//        _main.WriteLine("lock (sync)");
		//        _main.WriteLine("bytes[index]=null;");
		//    }

		//    foreach (var subitem in item.Subitems)
		//    {
		//        if (HasBrackets(subitem.Name))
		//        {
		//            var name = RemoveExtraInfo(RemoveBrackets(subitem.Name));
		//            var counter = GetCounter(subitem.Name);

		//            var structName = IsGlobal(subitem.Name) ?
		//                GetGlobalType(subitem.Name) : RemoveBrackets(subitem.Name) + "Struct";

		//            _main.WriteLine("for(int i=0; i<Max.{0}; i++)", counter);
		//            _main.WriteLine("{0}[i].SetDefaultValue();", name);
		//        }
		//        else
		//        {
		//            _main.WriteLine("{0}.SetDefaultValue();", RemoveExtraInfo(subitem.Name));
		//        }
		//    }

		//    if (isRootClass)
		//    {
		//        _main.WriteLine("Final = false;");
		//        _main.WriteLine("Error = false;");
		//        _main.WriteLine("state = State0;");
		//        _main.WriteLine("boolExPosition = int.MinValue;");
		//    }

		//    foreach (var pair in item.Enums)
		//    {
		//        var name = RemoveBrackets(pair.Key);
		//        GenerateInitializer(pair.Key, name + "s", "", name + "s.None");
		//    }

		//    GenerateInitializers(item.Begins1, _beginEndStructName, "", ".SetDefaultValue()");

		//    GenerateInitializers(item.Counts, "int", "Count.");
		//    GenerateInitializers(item.Decimals1, "int", "", "int.MinValue");
		//    GenerateInitializers(item.Bools, "bool", "", "false");

		//    if (isRootClass)
		//        _main.WriteLine("}");
		//    _main.WriteLine("OnSetDefaultValue();");
		//    _main.WriteLine("}");
		//}

		#endregion

		private void GenerateEnums(VariableTreeItem item, List<string> enums)
		{
			foreach (var enum1 in item.Enums)
			{
				if (enums.Contains(enum1.Key) == false)
				{
					GenerateEnum(enum1);
					enums.Add(enum1.Key);
				}
			}

			foreach (var subitem in item.Subitems)
				GenerateEnums(subitem, enums);
		}

		private void GenerateEnum(KeyValuePair<string, List<string>> pair)
		{
			_main.WriteLine("public enum {0}s", RemoveBrackets(pair.Key));
			_main.WriteLine("{");

			foreach (var name in pair.Value)
				_main.WriteLine("{0},", name);

			_main.WriteLine("}");
		}


		private void GenerateGlobalStructs(VariableTreeItem item, List<string> structs)
		{
			foreach (var subitem in item.Subitems)
			{
				if (IsGlobal(subitem.Name))
				{
					var globalType = GetGlobalType(subitem.Name);

					if (structs.Contains(globalType) == false)
					{
						GenerateGlobalStruct(subitem);
						structs.Add(globalType);
					}
				}
			}

			foreach (var subitem in item.Subitems)
				GenerateGlobalStructs(subitem, structs);
		}

		private void GenerateGlobalStruct(VariableTreeItem item)
		{
			_main.WriteLine("public partial struct {0}", RemoveBrackets(GetGlobalType(item.Name)));
			_main.WriteLine("{");

			GenerateVariables(item);

			_main.WriteLine("}");
		}
	}
}
