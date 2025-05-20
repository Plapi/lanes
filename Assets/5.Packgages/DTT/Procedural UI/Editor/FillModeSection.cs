using DTT.Utils.EditorUtilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace DTT.UI.ProceduralUI.Editor
{
    /// <summary>
    /// Displays the section in the inspector for <see cref="RoundedImage"/> where the user 
    /// can toggle between <see cref="RoundingMode"/>.
    /// </summary>
    public class FillModeSection : Section<RoundedImage>
    {
        /// <summary>
        /// The GUI content for <see cref="FillModeSection"/>.
        /// </summary>
        private class FillModeContent : GUIContentCache
        {
            /// <summary>
            /// The content for the fill option.
            /// </summary>
            public GUIContent FillOption => base[nameof(FillOption)];

            /// <summary>
            /// The content for the border option.
            /// </summary>
            public GUIContent BorderOption => base[nameof(BorderOption)];

            /// <summary>
            /// All the toolbar options in <see cref="FillModeSection"/>.
            /// </summary>
            public GUIContent[] ToolbarOptions => new GUIContent[] { FillOption, BorderOption };

            public FillModeContent()
            {
                Add(nameof(FillOption), () => new GUIContent("Fill", "Fills the whole image"));
                Add(nameof(BorderOption), () => new GUIContent("Border", "Makes the inside invisible"));
            }
        }
        
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override string HeaderName => "Fill Mode";
        
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override bool OpenFoldoutOnEnter => true;
        
        /// <summary>
        /// The border thickness property of <see cref="RoundedImage"/>.
        /// </summary>
        private SerializedProperty _borderThickness;

        /// <summary>
        /// The mode property of <see cref="RoundedImage"/>.
        /// </summary>
        private SerializedProperty _mode;

        /// <summary>
        /// The selected unit property of <see cref="RoundedImage"/>.
        /// </summary>
        private SerializedProperty _selectedUnit;

        /// <summary>
        /// The GUIContent for <see cref="FillModeSection"/>.
        /// </summary>
        private FillModeContent _content;
        
        /// <summary>
        /// Creates a new fillmode section.
        /// </summary>
        /// <param name="roundedImage">
        /// The rounded image instance to apply this to.
        /// </param>
        /// <param name="repaint">
        /// When called should repaint the inspector.
        /// </param>
        /// <param name="borderThickness">
        /// The border thickness property of <see cref="RoundedImage"/>.
        /// </param>
        /// <param name="mode">
        /// The mode property of <see cref="RoundedImage"/>.
        /// </param>
        /// <param name="selectedUnit">
        /// The selected unit property of <see cref="RoundedImage"/>.
        /// </param>
        public FillModeSection(
            RoundedImage roundedImage,
            UnityAction repaint,
            SerializedProperty borderThickness,
            SerializedProperty mode,
            SerializedProperty selectedUnit
        ) : base(roundedImage, repaint)
        {
            this._borderThickness = borderThickness;
            this._mode = mode;
            this._selectedUnit = selectedUnit;

            _content = new FillModeContent();
        }
        
        /// <summary>
        /// Draws the fill mode section where the user can 
        /// select how they want to fill their rounded image.
        /// </summary>
        protected override void DrawSection()
        {
            _mode.enumValueIndex = GUILayout.Toolbar(_mode.enumValueIndex, _content.ToolbarOptions);

            // Show border amount when border rounding mode is enabled.
            if ((RoundingMode)_mode.enumValueIndex == RoundingMode.BORDER)
                _borderThickness.floatValue = DrawSlider((RoundingUnit)_selectedUnit.enumValueIndex, _borderThickness.floatValue);
        }
    }
}