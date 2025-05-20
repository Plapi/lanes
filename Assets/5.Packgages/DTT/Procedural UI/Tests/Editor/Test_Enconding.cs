#if TEST_FRAMEWORK

using DTT.UI.ProceduralUI.Unsafe;
using NUnit.Framework;
using System;

namespace DTT.UI.ProceduralUI.Tests
{
	/// <summary>
	/// Tests the <see cref="Encoding"/> helper class.
	/// </summary>
	public class Test_Encoding
	{
		/// <summary>
		/// Tests the encoding and decoding of floats.
		/// It expects that the result of decoding an encoded float returns the same as input with a precision of <c>0.0001</c>.
		/// </summary>
		[Test]
		public void Test_EncodeAndDecodeFloats()
		{
			float a = 0.7f;
			float b = 0.3f;
			(float, float) result = Encoding.DecodeFloats(Encoding.EncodeFloats(a, b));

			Assert.AreEqual(result.Item1, a, 0.0001f, "The first decoded float doesn't match the input.");
			Assert.AreEqual(result.Item2, b, 0.0001f, "The first decoded float doesn't match the input.");
		}

		/// <summary>
		/// Tests the exceptions thrown with using out of range values.
		/// Expects the <see cref="ArgumentOutOfRangeException"/> to be thrown when using inputs outside the range of 0 to 1.
		/// </summary>
		[Test]
		public void Test_EncodeFloatsOutOfRangeValues()
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => Encoding.EncodeFloats(-1, 0), 
				"No out of range exception thrown with negative value");
			Assert.Throws<ArgumentOutOfRangeException>(() => Encoding.EncodeFloats(0, 10), 
				"No out of range exception thrown with value larger than one.");
		}
	}
}
#endif