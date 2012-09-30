using System;
using System.Collections.Generic;

namespace BnfToDfa
{
	struct RulePath
	{
		private readonly string value;

		public RulePath(string value)
		{
			this.value = value;
		}

		public string Value
		{
			get { return value; }
		}

		public static RulePath operator +(RulePath rulePath, string extension)
		{
			return new RulePath(rulePath.Value + "." + extension);
		}
	}
}
