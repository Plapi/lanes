#if UNITY_EDITOR
using UnityEngine;

namespace DTT.UI.ProceduralUI
{
	/// <summary>
	/// Mappings for a side of <see cref="RoundedImage"/>.
	/// </summary>
	public enum RoundingSide
	{
		[InspectorName("Top")]
		TOP = 0,
		[InspectorName("Right")]
		RIGHT = 1,
		[InspectorName("Bottom")]
		BOTTOM = 2,
		[InspectorName("Left")]
		LEFT = 3,
	}
} 
#endif