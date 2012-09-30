using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Fsm
{
	public enum Marks
	{
		None,
		Range,
		BeginRange,
		EndRange,
		ContinueRange,
		ResetRangeIfInvalid,
		ResetRange,
		EndRangeIfInvalid,
		Const,
		Count,
		Decimal,
		Hex,
		Bool,
		BoolEx,
		BoolExNot,
		Chars,
		Group,
		Final,
		Custom,
		Service1,
		Service2,
	}

	public interface IMark
		: IEquatable<IMark>
	{
		Marks Mark { get; }
		string Name { get; }
		int Priority { get; }
		string Value { get; }
		int Max { get; }
		string Default { get; }
		int Offset { get; }
		bool IsSame(IMark mark);

		string Type { get; } // non compared
	}

	public class MarkEqualityComparer
		: IEqualityComparer<IMark>
	{
		public bool Equals(IMark x, IMark y)
		{
			return x.IsSame(y);
		}

		public int GetHashCode(IMark x)
		{
			return
				(int)x.Mark ^
				(string.IsNullOrEmpty(x.Name) ? 0 : x.Name.GetHashCode()) ^
				(string.IsNullOrEmpty(x.Value) ? 0 : x.Value.GetHashCode()) ^
				x.Max ^
				(string.IsNullOrEmpty(x.Default) ? 0 : x.Default.GetHashCode()) ^
				x.Priority ^
				x.Offset;
		}
	}

	public class MarkImpl
		: IMark
	{
		private Marks _mark = Marks.None;
		private string _name;
		private string _value;
		private int _max;
		private string _default;
		private int _priority;
		private int _offset;
		private string _type;

		public MarkImpl()
		{
		}

		public MarkImpl(IMark mark)
		{
			CopyFrom(mark);
		}

		public MarkImpl(Marks mark)
		{
			Mark = mark;
		}

		public MarkImpl(Marks mark, string name)
		{
			Mark = mark;
			Name = name;
		}

		public Marks Mark
		{
			get { return _mark; }
			set
			{
				if (_mark != value)
				{
					if (_mark != Marks.None)
						throw new InvalidOperationException("IMark.Mark already has value");
					_mark = value;
				}
			}
		}

		public string Name
		{
			get { return _name; }
			set
			{
				if (_name != value)
				{
					if (_name != null)
						throw new InvalidOperationException("IMark.Name already has value");
					_name = value;
				}
			}
		}

		public string Value
		{
			get { return _value; }
			set { _value = value; }
		}

		public int Max
		{
			get { return _max; }
			set { _max = value; }
		}

		// it looks like this property double Value
		public string Default
		{
			get { return _default; }
			set { _default = value; }
		}

		public int Priority
		{
			get { return _priority; }
			set { _priority = value; }
		}

		public int Offset
		{
			get { return _offset; }
			set { _offset = value; }
		}

		public string Type
		{
			get { return _type; }
			set { _type = value; }
		}

		public bool Equals(IMark other)
		{
			return
				(
					this == other
				) ||
				(
					Mark == other.Mark &&
					Name == other.Name &&
					Value == other.Value &&
					Max == other.Max &&
					Default == other.Default &&
					Priority == other.Priority &&
					Offset == other.Offset
				);
		}

		public override bool Equals(object obj)
		{
			IMark other = obj as IMark;
			return other != null && Equals(other);
		}

		public override int GetHashCode()
		{
			return
				(int)Mark ^
				(string.IsNullOrEmpty(Name) ? 0 : Name.GetHashCode()) ^
				(string.IsNullOrEmpty(Value) ? 0 : Value.GetHashCode()) ^
				Max ^
				(string.IsNullOrEmpty(Default) ? 0 : Default.GetHashCode()) ^
				Priority ^
				Offset;
		}

		[Obsolete("Should be removed after testing, use Equals")]
		public bool IsSame(IMark mark)
		{
			return
				(
					this == mark
				) ||
				(
					Mark == mark.Mark &&
					Name == mark.Name &&
					Value == mark.Value &&
					Max == mark.Max &&
					Default == mark.Default &&
					Priority == mark.Priority &&
					Offset == mark.Offset
				);
		}

		public void CopyFrom(IMark imark)
		{
			Mark = imark.Mark;
			Name = imark.Name;
			Priority = imark.Priority;
			Value = imark.Value;
			Max = imark.Max;
			Default = imark.Default;
			Offset = imark.Offset;
			Type = imark.Type;
		}

		//#region IXmlSerializable

		//public void WriteXml(XmlWriter writer)
		//{
		//    writer.WriteStartElement("Hellow");
		//    writer.WriteEndElement();
		//}

		//public void ReadXml(XmlReader reader)
		//{
		//}

		//public XmlSchema GetSchema()
		//{
		//    return null;
		//}

		//#endregion
	}
}
