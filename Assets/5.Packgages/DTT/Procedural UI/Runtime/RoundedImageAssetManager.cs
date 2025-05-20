using UnityEngine;

namespace DTT.UI.ProceduralUI
{
	/// <summary>
	/// Manages the assets for <see cref="RoundedImage"/>
	/// </summary>
	public static class RoundedImageAssetManager
	{
		/// <summary>
		/// The shader used for filled Rounded Images that have a border.
		/// </summary>
		public static Shader[] RoundingShaders => new[] { _roundingShader, roundingShaderGradient };

		/// <summary>
		/// The shader used for filled Rounded Images that have a border & gradient.
		/// </summary>
		public static Shader roundingShaderGradient;

		/// <summary>
		/// The shader used for the gradient.
		/// </summary>
		public static Shader gradientShader;
		
		/// <summary>
		/// Cached material used for Rounded Images.
		/// </summary>
		private static Material _material;

		/// <summary>
		/// The shader used for filled Rounded Images that have a border.
		/// </summary>
		private static Shader _roundingShader;
		
		/// <summary>
		/// The shader name of the border shader.
		/// Can be used in <see cref="Shader.Find(string)"/>
		/// </summary>
		public const string ROUNDING_SHADER_NAME = "UI/RoundedCorners/RoundedCorners";

		/// <summary>
		/// The shader name of the border shader.
		/// Can be used in <see cref="Shader.Find(string)"/>
		/// </summary>
		public const string ROUNDING_GRADIENT_SHADER_NAME = "UI/RoundedCorners/RoundedCornersGradient";

		/// <summary>
		/// The shader name of the border shader.
		/// Can be used in <see cref="Shader.Find(string)"/>
		/// </summary>
		public const string GRADIENT_SHADER_NAME = "UI/RoundedCorners/Gradient";
		
		/// <summary>
		/// Find the relevant shader and initialize the cached material.
		/// </summary>
		static RoundedImageAssetManager()
		{
			_roundingShader = Shader.Find(ROUNDING_SHADER_NAME);
			roundingShaderGradient = Shader.Find(ROUNDING_GRADIENT_SHADER_NAME);
			gradientShader = Shader.Find(GRADIENT_SHADER_NAME);
			_material = new Material(_roundingShader);
		}
		
		/// <summary>
		/// Get the rounding material.
		/// </summary>
		/// <returns>The material that rounds according to the mode.</returns>
		public static Material GetRoundingMaterial()
		{
			if (_material == null)
				_material = new Material(_roundingShader);

			return _material;
		}
	}
}