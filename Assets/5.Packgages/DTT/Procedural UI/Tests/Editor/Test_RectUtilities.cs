#if TEST_FRAMEWORK

using NUnit.Framework;
using UnityEngine;

namespace DTT.UI.ProceduralUI.Tests
{
	/// <summary>
	/// Tests the extension methods inside the <see cref="RectUtilities"/> class.
	/// </summary>
	public class Test_RectUtilities
	{
		/// <summary>
		/// Tests that the long length is returned from the <see cref="RectUtilities.GetLongLength(Rect)"/>.
		/// Expects the input long length is returned.
		/// </summary>
		[Test]
		public void TestLongLength()
		{
			float shortLength = 50;
			float longLength = 100;
			Rect rect = new Rect(0, 0, longLength, shortLength);
			Assert.AreEqual(longLength, RectUtilities.GetLongLength(rect), 
				"The returned long length doesn't match the one expected.");
		}

		/// <summary>
		/// Tests that the short length is returned from the <see cref="RectUtilities.GetShortLength(Rect)(Rect)"/>.
		/// Expects the input short length is returned.
		/// </summary>
		[Test]
		public void TestShortLength()
		{
			float shortLength = 50;
			float longLength = 100;
			Rect rect = new Rect(0, 0, longLength, shortLength);
			Assert.AreEqual(shortLength, RectUtilities.GetShortLength(rect),
				"The returned short length doesn't match the one expected.");
		}
	}
}
#endif