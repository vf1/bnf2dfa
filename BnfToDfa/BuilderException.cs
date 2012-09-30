using System;
using System.Collections.Generic;
using Irony.Parsing;

namespace BnfToDfa
{
	class BuilderException
		: Exception
	{
		public BuilderException(string message)
			: base(message)
		{
		}

		public BuilderException(ParseTreeNode node, string message)
			: base(GetMessage(node, message))
		{
		}

		public BuilderException(ParseTreeNode node, string message, string details)
			: base(GetMessage(node, message) + @", Details: " + ((string.IsNullOrEmpty(details)) ? @"<null-or-empty>" : details))
		{
		}

		public static string GetMessage(ParseTreeNode node, string message)
		{
			return string.Format(@"ParseTreeNode: {0}, Error: {1}", node.FindTokenAndGetText(), message);
		}
	}
}
