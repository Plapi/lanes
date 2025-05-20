using UnityEngine;

namespace DTT.UI.ProceduralUI
{
	/// <summary>
	/// Contains utility methods for the <see cref="Rect"/>.
	/// </summary>
	public static class RectUtilities
	{
		/// <summary>
		/// Returns the length of the shortest side of a <see cref="Rect"/>.
		/// </summary>
		/// <param name="rect">Rect given for calculation</param>
		/// <returns>Shortest length of the given rect.</returns>
		public static float GetShortLength(this Rect rect) => Mathf.Min(rect.width, rect.height);

		/// <summary>
		/// Returns the length of the longest side of a <see cref="Rect"/>.
		/// </summary>
		/// <param name="rect">Rect given for calculation</param>
		/// <returns>Longest length of the given rect.</returns>
		public static float GetLongLength(this Rect rect) => Mathf.Max(rect.width, rect.height);
	}
}