using Fsm;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Fsm.Tests
{
	[TestClass()]
	public class MapTest
	{
		Map<byte?, int> _map;

		[TestInitialize()]
		public void Initialize()
		{
			_map = new Map<byte?, int>();
		}

		[TestMethod()]
		public void NullTest()
		{
			Assert.AreEqual(_map.Get(null).Count, 0);

			_map.Add(null, 1);
			_map.Add(null, 2);
			_map.Add(null, 3);

			Assert.AreEqual(_map.Get(null).Count, 3);
		}

		[TestMethod()]
		public void EnumeratorTest()
		{
			_map.Add(null, 1);
			_map.Add(null, 2);

			_map.Add(1, 11);
			_map.Add(1, 12);

			_map.Add(2, 21);
			_map.Add(2, 22);

			int i = 0;
			foreach (var pair in _map)
			{
				switch (i)
				{
					case 0:
						Assert.AreEqual<byte>(1, (byte)pair.Key);
						Assert.AreEqual(11, pair.Value);
						break;
					case 1:
						Assert.AreEqual<byte>(1, (byte)pair.Key);
						Assert.AreEqual(12, pair.Value);
						break;
					case 2:
						Assert.AreEqual<byte>(2, (byte)pair.Key);
						Assert.AreEqual(21, pair.Value);
						break;
					case 3:
						Assert.AreEqual<byte>(2, (byte)pair.Key);
						Assert.AreEqual(22, pair.Value);
						break;
					case 4:
						Assert.AreEqual<byte?>(null, pair.Key);
						Assert.AreEqual(1, pair.Value);
						break;
					case 5:
						Assert.AreEqual<byte?>(null, pair.Key);
						Assert.AreEqual(2, pair.Value);
						break;
				}

				i++;
			}
		}
	}
}
