#if UNITY_EDITOR
namespace DTT.UI.ProceduralUI
{
	/// <summary>
	/// The different modes used for rounding the corners.
	/// </summary>
	public enum RoundingCornerMode
	{
		/// <summary>
		/// Rounds all the corners at simultaneously.
		/// </summary>
		ALL = 0,

		/// <summary>
		/// Rounds all the corners independently.
		/// </summary>
		INDIVIDUAL = 1,

		/// <summary>
		/// Rounds all the corners based on a side.
		/// </summary>
		SIDE = 2,

		/// <summary>
		/// Rounds all the corners fully.
		/// </summary>
		UNIFORM = 3
	}
} 
#endif