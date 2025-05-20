using DTT.UI.ProceduralUI.Exceptions;
using DTT.Utils.Extensions;
using System;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using UnityEngine.Events;

namespace DTT.UI.ProceduralUI.Editor
{
    /// <summary>
    /// A section of the <see cref="RoundedImageEditor"/> inspector.
    /// A section is defined with a foldable fade header group.
    /// Additional content can be added using <see cref="DrawSection"/>.
    /// </summary>
    public abstract class Section<T> : IDrawable where T : MonoBehaviour
    {
        /// <summary>
        /// The name of the header of this section. This will be directly shown in the inspector.
        /// </summary>
        public abstract string HeaderName { get; }
        
        /// <summary>
        /// Reference to the target instance of this inspector.
        /// </summary>
        protected T _roundedImage;
        
        /// <summary>
        /// The animation boolean for folding the section.
        /// </summary>
        private AnimBool _foldoutAnimation;

        /// <summary>
        /// Whether to automatically open the foldout when opening the inspector.
        /// </summary>
        protected abstract bool OpenFoldoutOnEnter { get; }
        
        /// <summary>
        /// Base constructor for creating section.
        /// </summary>
        /// <param name="roundedImage">
        /// Reference to the target instance of this inspector.
        /// </param>
        /// <param name="repaint">
        /// Should repaint the inspector when called.
        /// </param>
        public Section(T roundedImage, UnityAction repaint)
        {
            this._roundedImage = roundedImage;
            this._foldoutAnimation = new AnimBool(OpenFoldoutOnEnter);
            _foldoutAnimation.valueChanged.AddListener(repaint);
        }
        
        /// <summary>
        /// Will draw the section.
        /// </summary>
        public void Draw()
        {
            DrawHeader();
            if (EditorGUILayout.BeginFadeGroup(_foldoutAnimation.faded))
                DrawSection();
            EditorGUILayout.EndFadeGroup();
        }
        
        /// <summary>
        /// Can be used to add additional layout to your section.
        /// </summary>
        protected abstract void DrawSection();

        /// <summary>
        /// Draws a slider that adjusts based on the passed unit.
        /// </summary>
        /// <param name="unit">The unit to draw a slider for.</param>
        /// <param name="currentValue">The current value of what should be slided between.</param>
        /// <param name="content">The content of the slider.</param>
        /// <returns>The new slider value.</returns>
        protected float DrawSlider(RoundingUnit unit, float currentValue, GUIContent content)
        {
            switch (unit)
            {
                case RoundingUnit.PERCENTAGE:
                    return EditorGUILayout.Slider(content, currentValue * 100, 0f, 100f) / 100;
                case RoundingUnit.WORLD:
                    float maxValue = _roundedImage.GetRectTransform().rect.GetShortLength() / 2;
                    maxValue = Mathf.Max(maxValue, 0);

                    return EditorGUILayout.Slider(content, currentValue, 0, maxValue);
                default:
                    string message = string.Format(ProceduralUIException.UNSUPPORTED_ROUNDING_UNIT_EXCEPTION_MESSAGE, unit);
                    throw new NotSupportedException(message);
            }
        }

        /// <summary>
        /// Draws a slider that adjusts based on the passed unit.
        /// </summary>
        /// <param name="unit">The unit to draw a slider for.</param>
        /// <param name="currentValue">The current value of what be should slided between.</param>
        /// <returns>The new slider value.</returns>
        protected float DrawSlider(RoundingUnit unit, float currentValue) => DrawSlider(unit, currentValue, new GUIContent());

        /// <summary>
        /// Draws a slider that adjusts based on the passed unit.
        /// </summary>
        /// <param name="unit">The unit to draw a slider for.</param>
        /// <param name="currentValue">The current value of what should be slided between.</param>
        /// <param name="label">The name of the slider.</param>
        /// <param name="tooltip">The tooltip that appears when hovering above the label.</param>
        /// <returns>The new slider value.</returns>
        protected float DrawSlider(RoundingUnit unit, float currentValue, string label, string tooltip) => DrawSlider(unit, currentValue, new GUIContent(label, tooltip));
        
        /// <summary>
        /// Draws the header label of section.
        /// </summary>
        private void DrawHeader()
        {
            EditorGUILayout.Space();

            _foldoutAnimation.target = EditorGUILayout.Foldout(_foldoutAnimation.target, string.Empty, true);

            Rect rect = GUILayoutUtility.GetLastRect();
            GUI.Label(rect, HeaderName, RoundedImageEditor.Styling.FoldoutHeaderStyle);

            EditorGUILayout.Space();
        }
    }
}