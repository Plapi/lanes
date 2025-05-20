using UnityEngine;

namespace DTT.UI.ProceduralUI
{
	/// <summary>
	/// The fill mode used for rounding the image.
	/// </summary>
	public enum RoundingMode
	{
		/// <summary>
		/// Fills the entire rounded image with a solid colour.
		/// </summary>
		[InspectorName("Fill")]
		FILL = 0,

		/// <summary>
		/// Creates a parameterized border the user can tweak.
		/// </summary>
		[InspectorName("Border")]
		BORDER = 1,
	}
} 