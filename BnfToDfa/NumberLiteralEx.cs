using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Irony;
using Irony.Ast;
using Irony.Parsing;

namespace BnfToDfa
{
	class NumberLiteralEx
		: NumberLiteral
	{
		public NumberLiteralEx(string name)
			: base(name)
		{
		}

		public NumberLiteralEx(string name, NumberOptions options, Type astNodeType)
			: base(name, options, astNodeType)
		{
		}

		public NumberLiteralEx(string name, NumberOptions options, AstNodeCreator astNodeCreator)
			: base(name, options, astNodeCreator)
		{
		}

		public NumberLiteralEx(string name, NumberOptions options)
			: base(name, options)
		{
		}

		public override IList<string> GetFirsts()
		{
			var result = base.GetFirsts() as StringList;

			if (IsSet(NumberOptions.Hex))
				result.AddRange(new string[] { "a", "b", "c", "d", "e", "f", "A", "B", "C", "D", "E", "F" });

			return result;
		}

		////Most numbers in source programs are just one-digit instances of 0, 1, 2, and maybe others until 9
		//// so we try to do a quick parse for these, without starting the whole general process
		//protected override Token QuickParse(ParsingContext context, ISourceStream source)
		//{
		//    if (IsSet(NumberOptions.DisableQuickParse)) return null;
		//    char current = source.PreviewChar;
		//    //it must be a digit followed by a whitespace or delimiter
		//    if (!char.IsDigit(current)) return null;
		//    if (!Grammar.IsWhitespaceOrDelimiter(source.NextPreviewChar))
		//        return null;
		//    int iValue = current - '0';
		//    object value = null;
		//    switch (DefaultIntTypes[0])
		//    {
		//        case TypeCode.Int32: value = iValue; break;
		//        case TypeCode.UInt32: value = (UInt32)iValue; break;
		//        case TypeCode.Byte: value = (byte)iValue; break;
		//        case TypeCode.SByte: value = (sbyte)iValue; break;
		//        case TypeCode.Int16: value = (Int16)iValue; break;
		//        case TypeCode.UInt16: value = (UInt16)iValue; break;
		//        default: return null;
		//    }
		//    source.PreviewPosition++;
		//    return source.CreateToken(this.OutputTerminal, value);
		//}
	}
}
