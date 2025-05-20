using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace DTT.UI.ProceduralUI.Editor
{
    /// <summary>
    /// Displays the section in the inspector for <see cref="GradientEffect"/> where the user 
    /// can select what type of gradient he/she wants to use.
    /// </summary>
    public class GradientSection : Section<GradientEffect>
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override string HeaderName => "Gradient";
        
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override bool OpenFoldoutOnEnter => true;
        
        /// <summary>
        /// The batching property of <see cref="GradientEffect"/>.
        /// </summary>
        private SerializedProperty _batching;

        /// <summary>
        /// The type property of <see cref="GradientEffect"/>.
        /// </summary>
        private SerializedProperty _type;

        /// <summary>
        /// The gradient property of <see cref="GradientEffect"/>.
        /// </summary>
        private SerializedProperty _gradient;

        /// <summary>
        /// The offset property of <see cref="GradientEffect"/>.
        /// </summary>
        private SerializedProperty _offset;

        /// <summary>
        /// The rotation property of <see cref="GradientEffect"/>.
        /// </summary>
        private SerializedProperty _rotation;

        /// <summary>
        /// The scale property of <see cref="GradientEffect"/>.
        /// </summary>
        private SerializedProperty _scale;
        
        /// <summary>
        /// Creates a new gradient section.
        /// </summary>
        /// <param name="GradientEffect">
        /// The Gradient instance to apply this to.
        /// </param>
        /// <param name="repaint">
        /// When called should repaint the inspector.
        /// </param>
        /// <param name="type">
        /// The type property of <see cref="GradientEffect"/>.
        /// </param>
        /// <param name="gradient">
        /// The gradient property of <see cref="GradientEffect"/>.
        /// </param>
        /// <param name="offset">
        /// The offset property of <see cref="GradientEffect"/>.
        /// </param>
        /// <param name="rotation">
        /// The rotation of <see cref="GradientEffect"/>.
        /// </param>
        /// <param name="scale">
        /// The scale of <see cref="GradientEffect"/>.
        /// </param>
        public GradientSection(
            GradientEffect GradientEffect,
            UnityAction repaint,
            GradientEffectSerializedProperties properties
        ) : base(GradientEffect, repaint)
        {
            this._type = properties.type;
            this._gradient = properties.gradient;
            this._offset = properties.offset;
            this._rotation = properties.rotation;
            this._scale = properties.scale;
            this._batching = properties.batching;
        }
        
        /// <summary>
        /// Draws the toolbar where the user can select their option, and draws the selected option.
        /// </summary>
        protected override void DrawSection()
        {
            EditorGUILayout.PropertyField(_batching);
            EditorGUILayout.PropertyField(_type);
            EditorGUILayout.PropertyField(_gradient);
            EditorGUILayout.PropertyField(_offset);
            EditorGUILayout.PropertyField(_rotation);
            EditorGUILayout.PropertyField(_scale);
        }
    }
}