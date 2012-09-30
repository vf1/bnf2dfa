using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Fsm;

namespace BnfToDfa
{
	class Writer
	{
		public static void Write(DfaState start, string fileName)
		{
			var dfa = new Dfa(start);

			XmlSerializer xs = new XmlSerializer(typeof(Dfa));
			using (Stream s = File.Create(fileName))
				xs.Serialize(s, dfa);
		}
	}
}
