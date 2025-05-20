using DTT.Utils.EditorUtilities;
using UnityEditor;

namespace DTT.UI.ProceduralUI.Editor
{
	/// <summary>
	/// Contains all the serialized field names of <see cref="GradientEditor"/>.
	/// Can be used for getting serialized properties 
	/// via <see cref="UnityEditor.SerializedProperty.FindPropertyRelative(string)"/>.
	/// </summary>
	public class GradientEffectSerializedProperties : SerializedPropertyCache
	{
		/// <summary>
		/// Name of the type property of <see cref="GradientEffect"/>.
		/// </summary>
		public SerializedProperty type => base[nameof(type)];

		/// <summary>
		/// Name of the gradient property of <see cref="GradientEffect"/>.
		/// </summary>
		public SerializedProperty gradient => base[nameof(gradient)];

		/// <summary>
		/// Name of the offset property of <see cref="GradientEffect"/>.
		/// </summary>
		public SerializedProperty offset => base[nameof(offset)];

		/// <summary>
		/// Name of the rotation property of <see cref="GradientEffect"/>.
		/// </summary>
		public SerializedProperty rotation => base[nameof(rotation)];

		/// <summary>
		/// Name of the scale property of <see cref="GradientEffect"/>.
		/// </summary>
		public SerializedProperty scale => base[nameof(scale)];
		/// <summary>
		/// Name of the scale property of <see cref="GradientEffect"/>.
		/// </summary>
		public SerializedProperty batching => base[nameof(batching)];
		
		public GradientEffectSerializedProperties(SerializedObject serializedObject) : base(serializedObject) { }
	}
}