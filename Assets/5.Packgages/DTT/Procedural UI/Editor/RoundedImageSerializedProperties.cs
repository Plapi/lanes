using DTT.Utils.EditorUtilities;
using UnityEditor;

namespace DTT.UI.ProceduralUI.Editor
{
	/// <summary>
	/// Contains all the serialized field names of <see cref="RoundedImage"/>.
	/// Can be used for getting serialized properties 
	/// via <see cref="UnityEditor.SerializedProperty.FindPropertyRelative(string)"/>.
	/// </summary>
	public class RoundedImageSerializedProperties : SerializedPropertyCache
	{
		/// <summary>
		/// Name of the rounding mode property of <see cref="RoundedImage"/>.
		/// </summary>
		public SerializedProperty roundingMode => base[nameof(roundingMode)];

		/// <summary>
		/// Name of the rounding amount property of <see cref="RoundedImage"/>.
		/// </summary>
		public SerializedProperty roundingAmount => base[nameof(roundingAmount)];

		/// <summary>
		/// Name of the corner mode property of <see cref="RoundedImage"/>.
		/// </summary>
		public SerializedProperty borderThickness => base[nameof(borderThickness)];

		/// <summary>
		/// Name of the border thickness property of <see cref="RoundedImage"/>.
		/// </summary>
		public SerializedProperty useHitboxOutside => base[nameof(useHitboxOutside)];

		/// <summary>
		/// Name of the hitbox usage outside property of <see cref="RoundedImage"/>.
		/// </summary>
		public SerializedProperty useHitboxInside => base[nameof(useHitboxInside)];

		/// <summary>
		/// Name of the hitbox usage inside property of <see cref="RoundedImage"/>.
		/// </summary>
		public SerializedProperty distanceFalloff => base[nameof(distanceFalloff)];

		/// <summary>
		/// Name of the distance falloff property of <see cref="RoundedImage"/>.
		/// </summary>
		public SerializedProperty selectedUnit => base[nameof(selectedUnit)];

		/// <summary>
		/// Name of the selected unit property of <see cref="RoundedImage"/>.
		/// </summary>
		public SerializedProperty cornerMode => base[nameof(cornerMode)];

		/// <summary>
		/// Name of the side property of <see cref="RoundedImage"/>.
		/// </summary>
		public SerializedProperty side => base[nameof(side)];

		/// <summary>
		/// Name of the image type property of <see cref="RoundedImage"/>.
		/// </summary>
		public SerializedProperty m_Type => base[nameof(m_Type)]; 

		/// <summary>
		/// Name of the fill method property of <see cref="RoundedImage"/>.
		/// </summary>
		public SerializedProperty m_FillMethod => base[nameof(m_FillMethod)]; 

		/// <summary>
		/// Name of the origin fill property of <see cref="RoundedImage"/>.
		/// </summary>
		public SerializedProperty m_FillOrigin => base[nameof(m_FillOrigin)]; 

		/// <summary>
		/// Name of the clockwise fill property of <see cref="RoundedImage"/>.
		/// </summary>
		public SerializedProperty m_FillClockwise => base[nameof(m_FillClockwise)]; 

		/// <summary>
		/// Name of the fill amount property of <see cref="RoundedImage"/>.
		/// </summary>
		public SerializedProperty m_FillAmount => base[nameof(m_FillAmount)]; 

		/// <summary>
		/// Name of the sprite property of <see cref="RoundedImage"/>.
		/// </summary>
		public SerializedProperty m_Sprite => base[nameof(m_Sprite)]; 

		/// <summary>
		/// Name of the color property of <see cref="RoundedImage"/>.
		/// </summary>
		public SerializedProperty m_Color => base[nameof(m_Color)]; 

		/// <summary>
		/// Name of the color property of <see cref="RoundedImage"/>.
		/// </summary>
		public SerializedProperty m_RaycastTarget => base[nameof(m_RaycastTarget)]; 

		/// <summary>
		/// Name of the color property of <see cref="RoundedImage"/>.
		/// </summary>
		public SerializedProperty m_PreserveAspect => base[nameof(m_PreserveAspect)]; 

		/// <summary>
		/// Name of the material property of <see cref="RoundedImage"/>.
		/// </summary>
		public SerializedProperty m_Material => base[nameof(m_Material)]; 

		/// <summary>
		/// Name of the maskable property of <see cref="RoundedImage"/>.
		/// </summary>
		public SerializedProperty m_Maskable => base[nameof(m_Maskable)]; 
		
		public RoundedImageSerializedProperties(SerializedObject serializedObject) : base(serializedObject) { }
	}
}