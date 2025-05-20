using System;
using UnityEngine;

namespace DTT.UI.ProceduralUI.Unsafe
{
	/// <summary>
	/// Contains methods that are relevant for encoding data.
	/// </summary>
	public static unsafe class Encoding
	{
		/// <summary>
		/// Encodes 2 floats into a single one. 
		/// <b>Only use values between 0 and 1.</b>
		/// <b>There will be a loss in precision.</b>
		/// </summary>
		/// <param name="a">First float.</param>
		/// <param name="b">Second float.</param>
		/// <returns>Packed float with values <paramref name="a"/> and <paramref name="b"/></returns>
		public static float EncodeFloats(float a, float b)
		{
			// Only range of 0 and 1 is allowed.
			if (a < 0 || a > 1)
				throw new ArgumentOutOfRangeException($"Can only use values for encoding between 0 and 1. Error value: {a}");
			if (b < 0 || b > 1)
				throw new ArgumentOutOfRangeException($"Can only use values for encoding between 0 and 1. Error value: {b}");

			// Gets a range of 0 to max unsigned integer.
			a *= UInt16.MaxValue;
			b *= UInt16.MaxValue;
			
			// Save this one as is.
			uint aInt = (UInt32)Mathf.FloorToInt(a);
			
			// Shift this value into the other side of the integer.
			uint bInt = ((UInt32)Mathf.FloorToInt(b)) << 16;
			
			// Combine both values. First 16 bits is 'a', second 16 bits is 'b'.
			uint combine = aInt | bInt;

			return UInt32ToSingle(combine);
		}

		/// <summary>
		/// Decodes a single float into two of them. 
		/// <b>Make sure the encoded value comes from the <see cref="EncodeFloats" method./></b>
		/// <b>There will be a loss in precision.</b>
		/// </summary>
		/// <param name="encoded">The encoded float to decode.</param>
		/// <returns>A tuple that has both floats contained in it.</returns>
		public static (float, float) DecodeFloats(float encoded)
		{
			uint value = SingleToUInt32(encoded);
			uint aInt = value & 0x0000ffff;
			uint bInt = (value & 0xffff0000) >> 16;

			Vector2 result = new Vector2(aInt, bInt) / 0x0000ffff;
			return (result.x, result.y);
		}
		
		/// <summary>
		/// Converts <paramref name="value"/> to an unsigned integer.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>The converted value.</returns>
		public static uint SingleToUInt32(float value) => *(uint*)(&value); // Reinterpret address of float as that of an unsigned integer.

		/// <summary>
		/// Converts <paramref name="value"/> to a float.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <returns>The converted value.</returns>
		public static float UInt32ToSingle(uint value) => *(float*)(&value); // Reinterpret address of an unsigned integer as that of a float.
	}
}