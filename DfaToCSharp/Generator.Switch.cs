using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using Fsm;

namespace DfaCompiler
{
	public partial class Generator
	{
		private void GenerateParseMethod(DfaState dfa, int errorState)
		{
			_main.WriteLine("partial void OnBeforeParse();");
			_main.WriteLine("partial void OnAfterParse();");

			_main.WriteLine("#region int Parse(..)");

			_main.WriteLine("public bool ParseAll(ArraySegment<byte> data)");
			_main.WriteLine("{");
			_main.WriteLine("return ParseAll(data.Array, data.Offset, data.Count);");
			_main.WriteLine("}");

			_main.WriteLine("public bool ParseAll(byte[] bytes, int offset, int length)");
			_main.WriteLine("{");
			_main.WriteLine("int parsed = 0;");
			_main.WriteLine("do");
			_main.WriteLine("{");
			_main.WriteLine("Final = false;");
			_main.WriteLine("parsed += Parse(bytes, offset + parsed, length - parsed);");
			_main.WriteLine("} while (parsed < length && IsFinal);");
			_main.WriteLine("return IsFinal;");
			_main.WriteLine("}");

			_main.WriteLine("public int Parse(ArraySegment<byte> data)");
			_main.WriteLine("{");
			_main.WriteLine("return Parse(data.Array, data.Offset, data.Count);");
			_main.WriteLine("}");

			_main.WriteLine("public int Parse(byte[] bytes, int offset, int length)");
			_main.WriteLine("{");
			_main.WriteLine("OnBeforeParse();");

			if (dfa != null)
			{
				_main.WriteLine("int i = offset;");

				GenerateSwitch(dfa, errorState, SwitchMode.JumpOnly);

				//_main.WriteLine("if (state == State{0})", errorState);
				//_main.WriteLine("goto exit1;");

				_main.WriteLine("i++;");

				_main.WriteLine("int end = offset + length;");
				_main.WriteLine("for( ; i < end; i++)");
				_main.WriteLine("{");

				GenerateSwitch(dfa, errorState, SwitchMode.ActionJump);

				_main.WriteLine("}");

				GenerateSwitch(dfa, errorState, SwitchMode.ActionOnly);

				_main.WriteLine("exit1: ;");
				_main.WriteLine("OnAfterParse();");
				_main.WriteLine("return i - offset;");
			}
			else
			{
				_main.WriteLine("OnAfterParse();");
				_main.WriteLine("return 0;");
			}

			_main.WriteLine("}");

			_main.WriteLine("#endregion");
		}

		enum SwitchMode
		{
			JumpOnly,
			ActionOnly,
			ActionJump
		}

		private void GenerateSwitch(DfaState dfa, int errorState, SwitchMode mode)
		{
			_main.WriteLine("switch(state)");
			_main.WriteLine("{");

			dfa.ForEachNR((state) =>
			{
				_main.WriteLine("case State{0}:", state.Index);

				if (mode == SwitchMode.ActionJump || mode == SwitchMode.ActionOnly)
				{
					foreach (var nfa1 in state.AllMarks)
					{
						if (nfa1.Mark == Marks.ResetRange)
						{
							var name = GetVarname(nfa1.Name, "");
							// #1 Do NOT use SetDefaultValue, it clears bytes too -> wrong!
							// #1 Should to create special method for this
							// #2 Ok for IndexArray optimimized 
							_main.WriteLine("{0}." + GetSetDefauleValueCall() + ";", name);
						}

						if (nfa1.Mark == Marks.ResetRangeIfInvalid)
						{
							var name = GetVarname(nfa1.Name, "");
							_main.WriteLine("if({0}.End <0) {0}.Begin = int.MinValue;", name);
						}

						if (nfa1.Mark == Marks.Custom)
						{
							var name = GetVarname(nfa1.Name, "");
							_main.WriteLine(nfa1.Value.Replace("Var", name));
						}
					}

					foreach (var nfa1 in state.AllMarks)//.NfaStates)
					{
						if (nfa1.Mark == Marks.Count)
							_main.WriteLine("{0}++;", GetVarname(nfa1.Name, "Count."));
					}

					foreach (var mark in state.AllMarks)
					{
						if (mark.Mark == Marks.ContinueRange)
						{
							var ifv = GetCountComparation(RemoveExtraInfo(mark.Name));
							if (ifv != "")
								ifv += " && ";
							_main.WriteLine("if({1}{0}.End == i-1) {0}.End = i;", GetVarname(mark.Name, ""), ifv);
						}
					}

					foreach (var nfa1 in state.AllMarks)//.NfaStates)
					{
						switch (nfa1.Mark)
						{
							case Marks.BeginRange:
							case Marks.EndRange:
							case Marks.EndRangeIfInvalid:

								var varName = GetVarname(nfa1.Name, "") +
									((nfa1.Mark == Marks.BeginRange) ? ".Begin" : ".End");

								var condition = GetCountComparation(RemoveExtraInfo(nfa1.Name));
								if (nfa1.Mark != Marks.EndRange)
								{
									if (condition != "")
										condition += " && ";
									condition = varName + " < 0";
								}

								if (condition != "")
									_main.Write("if({0})", condition);

								_main.Write("{0} = i", varName);

								if (nfa1.Offset != 0)
									_main.Write("{0} {1}", (nfa1.Offset > 0) ? "+" : "-", Math.Abs(nfa1.Offset));

								_main.WriteLine(";");

								break;


							case Marks.BoolEx:
								_main.WriteLine("boolExPosition = i;");
								goto case Marks.Bool;
							case Marks.Bool:
								_main.WriteLine("{0} = true;", GetVarname(nfa1.Name, ""));
								break;

							case Marks.BoolExNot:
								_main.WriteLine("if(boolExPosition == i-1) {0} = false;", GetVarname(nfa1.Name, ""));
								break;


							case Marks.Final:
								_main.WriteLine("Final = true;");
								break;
						}
					}

					//if (mode == SwitchMode.ActionJump || mode == SwitchMode.ActionOnly)
					//{
					if (state.HasMarks)
					{
						foreach (var decimal1 in state.Decimals)
							_main.WriteLine("{0} = ({0} << 1) * 5 + bytes[i - 1] - 48;", GetVarname(decimal1.Name, ""));

						foreach (var hex1 in state.Hexes)
							_main.WriteLine("{0} = ({0} << 4) + AsciiCodeToHex[bytes[i - 1]];", GetVarname(hex1.Name, ""));
					}
					//}

					if (state.Consts.Count > 0)
					{
						foreach (var pair in state.ConstNameValues)
						{
							var ifv = GetCountComparation(RemoveExtraInfo(pair.Key));

							if (ifv != "")
								_main.Write("if(" + ifv + ") ");

							_main.WriteLine("{0} = {1}s.{2};",
								AddCountPrefix(RemoveExtraInfo(pair.Key)),
								RemoveBrackets(VariableInfo.GetShortName(pair.Key)),
								pair.Value);
						}
					}
				}

				if (state.IsFinal)
				{
					if (mode == SwitchMode.JumpOnly)
					{
						_main.WriteLine("state = table{0}[bytes[i]];", state.Index);
						_main.WriteLine("break;");
					}
					else
					{
						_main.WriteLine("goto exit1;");
					}
				}
				else
				{
					if (mode == SwitchMode.ActionJump || mode == SwitchMode.JumpOnly)
						_main.WriteLine("state = table{0}[bytes[i]];", state.Index);
					_main.WriteLine("break;");
				}

			}); // ForEach state

			_main.WriteLine("case State{0}:", errorState);
			if (mode == SwitchMode.ActionJump || mode == SwitchMode.ActionOnly)
				_main.WriteLine("i--;");
			_main.WriteLine("Error = true;");
			_main.WriteLine("goto exit1;");

			_main.WriteLine("}");
		}

		private void GenerateTables(DfaState dfa, int errorState)
		{
			if (dfa != null)
			{
				_main.WriteLine("#region States Tables");

				dfa.ForEachNR((state) =>
					{
						_main.WriteLine("private static int[] table{0};", state.Index);

						int next;
						DfaState nextState;
						for (int i = 0; i <= byte.MaxValue; i++)
						{
							next = errorState;
							nextState = state.Transition[i];
							if (nextState != null)
								next = nextState.Index;
							_table.Write((Int32)next);
						}
					});

				_main.WriteLine("#endregion");
			}
			else
				_main.WriteLine("// NO TABLES");
		}

		private void GenerateLoadFunction3(int count, bool empty, string @namespace)
		{
			_main.WriteLine("#region void LoadTables(..)");

			_main.WriteLine("public static void LoadTables()");
			_main.WriteLine("{");
			_main.WriteLine("LoadTables(\"\");");
			_main.WriteLine("}");

			_main.WriteLine("public static void LoadTables(string path)");
			_main.WriteLine("{");

			if (empty == false)
			{
				_main.WriteLine("const int maxItems = byte.MaxValue + 1;");
				_main.WriteLine("const int maxBytes = sizeof(Int32) * maxItems;");

				_main.WriteLine("using (var reader = new DeflateStream(File.OpenRead(path+\"\\\\{0}.dfa\"), CompressionMode.Decompress))", @namespace);
				_main.WriteLine("{");
				_main.WriteLine("byte[] buffer = new byte[maxBytes];");

				for (int i = 0; i < count; i++)
				{
					_main.WriteLine("table{0} = new int[maxItems];", i);
					_main.WriteLine("reader.Read(buffer, 0, buffer.Length);");
					_main.WriteLine("Buffer.BlockCopy(buffer, 0, table{0}, 0, maxBytes);", i);
				}

				_main.WriteLine("}");
			}

			_main.WriteLine("}");

			_main.WriteLine("#endregion");
		}

		//private void GenerateLoadFunction(int count, bool empty)
		//{
		//    _main.WriteLine("public void LoadTables(string path)");
		//    _main.WriteLine("{");

		//    if (empty == false)
		//    {
		//        _main.WriteLine("const int maxItems = byte.MaxValue + 1;");
		//        _main.WriteLine("const int maxBytes = sizeof(Int32) * maxItems;");

		//        _main.WriteLine("using (var reader = new DeflateStream(File.OpenRead(path), CompressionMode.Decompress))");
		//        _main.WriteLine("{");
		//        _main.WriteLine("byte[] buffer = new byte[maxBytes];");
		//        _main.WriteLine("Int32[] intBuffer = new Int32[maxItems];");

		//        for (int i = 0; i < count; i++)
		//        {
		//            _main.WriteLine("reader.Read(buffer, 0, buffer.Length);");
		//            _main.WriteLine("Buffer.BlockCopy(buffer, 0, intBuffer, 0, maxBytes);", i);

		//            _main.WriteLine("table{0} = new States[maxItems];", i);
		//            _main.WriteLine("for (int i = 0; i < maxItems; i++)");
		//            _main.WriteLine("table{0}[i] = (States)intBuffer[i];", i);
		//        }

		//        _main.WriteLine("}");

		//        //_main.WriteLine("using (var reader = new DeflateStream(File.OpenRead(path), CompressionMode.Decompress))");
		//        //_main.WriteLine("{");
		//        //_main.WriteLine("byte[] buffer = new byte[sizeof(Int32)];");

		//        //for (int i = 0; i < count; i++)
		//        //{
		//        //    _main.WriteLine("table{0} = new States[byte.MaxValue + 1];", i);
		//        //    _main.WriteLine("for (int i = 0; i <= byte.MaxValue; i++)");
		//        //    _main.WriteLine("{");
		//        //    _main.WriteLine("reader.Read(buffer, 0, buffer.Length);");
		//        //    _main.WriteLine("table{0}[i] = (States)BitConverter.ToInt32(buffer, 0);", i);
		//        //    _main.WriteLine("}");
		//        //}
		//        //_main.WriteLine("}");
		//    }

		//    _main.WriteLine("}");
		//}

		private void GenerateGetHexDigitFunction()
		{
			_main.WriteLine("public static readonly byte[] AsciiCodeToHex = new byte[256] {");
			_main.WriteLine("0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,");
			_main.WriteLine("0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,");
			_main.WriteLine("0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,");
			_main.WriteLine("0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,");
			_main.WriteLine("0xff, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,");
			_main.WriteLine("0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,");
			_main.WriteLine("0xff, 0x0a, 0x0b, 0x0c, 0x0d, 0x0e, 0x0f, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,");
			_main.WriteLine("0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,");
			_main.WriteLine("0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,");
			_main.WriteLine("0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,");
			_main.WriteLine("0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,");
			_main.WriteLine("0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,");
			_main.WriteLine("0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,");
			_main.WriteLine("0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,");
			_main.WriteLine("0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,");
			_main.WriteLine("0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,");
			_main.WriteLine("};");


			//_main.WriteLine("private int GetHexDigit(byte ch)");
			//_main.WriteLine("{");
			//_main.WriteLine("switch (ch)");
			//_main.WriteLine("{");
			//_main.WriteLine("case (byte)'0':");
			//_main.WriteLine("return 0;");
			//_main.WriteLine("case (byte)'1':");
			//_main.WriteLine("return 1;");
			//_main.WriteLine("case (byte)'2':");
			//_main.WriteLine("return 2;");
			//_main.WriteLine("case (byte)'3':");
			//_main.WriteLine("return 3;");
			//_main.WriteLine("case (byte)'4':");
			//_main.WriteLine("return 4;");
			//_main.WriteLine("case (byte)'5':");
			//_main.WriteLine("return 5;");
			//_main.WriteLine("case (byte)'6':");
			//_main.WriteLine("return 6;");
			//_main.WriteLine("case (byte)'7':");
			//_main.WriteLine("return 7;");
			//_main.WriteLine("case (byte)'8':");
			//_main.WriteLine("return 8;");
			//_main.WriteLine("case (byte)'9':");
			//_main.WriteLine("return 9;");
			//_main.WriteLine("}");

			//_main.WriteLine("switch (ch)");
			//_main.WriteLine("{");
			//_main.WriteLine("case (byte)'a':");
			//_main.WriteLine("return 10;");
			//_main.WriteLine("case (byte)'b':");
			//_main.WriteLine("return 11;");
			//_main.WriteLine("case (byte)'c':");
			//_main.WriteLine("return 12;");
			//_main.WriteLine("case (byte)'d':");
			//_main.WriteLine("return 13;");
			//_main.WriteLine("case (byte)'e':");
			//_main.WriteLine("return 14;");
			//_main.WriteLine("case (byte)'f':");
			//_main.WriteLine("return 15;");
			//_main.WriteLine("}");

			//_main.WriteLine("switch (ch)");
			//_main.WriteLine("{");
			//_main.WriteLine("case (byte)'A':");
			//_main.WriteLine("return 10;");
			//_main.WriteLine("case (byte)'B':");
			//_main.WriteLine("return 11;");
			//_main.WriteLine("case (byte)'C':");
			//_main.WriteLine("return 12;");
			//_main.WriteLine("case (byte)'D':");
			//_main.WriteLine("return 13;");
			//_main.WriteLine("case (byte)'E':");
			//_main.WriteLine("return 14;");
			//_main.WriteLine("case (byte)'F':");
			//_main.WriteLine("return 15;");
			//_main.WriteLine("}");

			//_main.WriteLine("throw new ArgumentOutOfRangeException(string.Format(\"GetHexDigit: {0} is not hex digit\", ch));");
			//_main.WriteLine("}");
		}
	}
}