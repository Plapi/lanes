using DTT.UI.ProceduralUI.Exceptions;
using DTT.Utils.EditorUtilities;
using DTT.Utils.Extensions;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace DTT.UI.ProceduralUI.Editor
{
    /// <summary>
    /// Displays the section in the inspector for <see cref="RoundedImage"/> where the user 
    /// can select what type of rounding he/she wants to use.
    /// </summary>
    public class CornerTargetModeSection : Section<RoundedImage>
    {
        /// <summary>
        /// The GUI content for <see cref="CornerTargetModeSection"/>.
        /// </summary>
        private class CornerTargetModeContent : GUIContentCache
        {
            /// <summary>
            /// The option round all at once.
            /// </summary>
            public GUIContent AllOption => base[nameof(AllOption)];

            /// <summary>
            /// The option to round every corner individually.
            /// </summary>
            public GUIContent IndividualOption => base[nameof(IndividualOption)];

            /// <summary>
            /// The option to round based on a side.
            /// </summary>
            public GUIContent SideOption => base[nameof(SideOption)];

            /// <summary>
            /// The option to have it automatically all rounded.
            /// </summary>
            public GUIContent UniformOption => base[nameof(UniformOption)];

            /// <summary>
            /// The info message that displays when uniform is selected.
            /// </summary>
            public GUIContent UniformInfoMessage => base[nameof(UniformInfoMessage)];

            /// <summary>
            /// All the options for the <see cref="CornerTargetModeSection"/>.
            /// </summary>
            public GUIContent[] ToolbarOptions => new GUIContent[] { AllOption, IndividualOption, SideOption, UniformOption };

            public CornerTargetModeContent()
            {
                Add(nameof(AllOption), () => new GUIContent("All", "Targets all corners"));
                Add(nameof(IndividualOption), () => new GUIContent("Individual", "Targets individual corners"));
                Add(nameof(SideOption), () => new GUIContent("Side", "Target sides"));
                Add(nameof(UniformOption), () => new GUIContent("Uniform", "Everything will be 100% rounded"));
                Add(nameof(UniformInfoMessage), () => new GUIContent("Uniform mode will automatically round your image to be as rounded as possible.", EditorGUIUtility.IconContent("console.infoicon.sml@2x").image));
            }
        }
        
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override string HeaderName => "Corner Target Mode";
        
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override bool OpenFoldoutOnEnter => true;
        
        /// <summary>
        /// The corner mode property of <see cref="RoundedImage"/>.
        /// </summary>
        private SerializedProperty _cornerMode;

        /// <summary>
        /// The rounding amount property of <see cref="RoundedImage"/>.
        /// </summary>
        private SerializedProperty _roundingAmount;

        /// <summary>
        /// The selected unit property of <see cref="RoundedImage"/>.
        /// </summary>
        private SerializedProperty _selectedUnit;

        /// <summary>
        /// The side property of <see cref="RoundedImage"/>.
        /// </summary>
        private SerializedProperty _side;

        /// <summary>
        /// All the GUIContent for <see cref="CornerTargetModeSection"/>.
        /// </summary>
        private CornerTargetModeContent _content;
        
        /// <summary>
        /// Creates a new corner target mode section.
        /// </summary>
        /// <param name="roundedImage">
        /// The rounded image instance to apply this to.
        /// </param>
        /// <param name="repaint">
        /// When called should repaint the inspector.
        /// </param>
        /// <param name="cornerMode">
        /// The corner mode property of <see cref="RoundedImage"/>.
        /// </param>
        /// <param name="roundingAmount">
        /// The rounding amount property of <see cref="RoundedImage"/>.
        /// </param>
        /// <param name="selectedUnit">
        /// The selected unit property of <see cref="RoundedImage"/>.
        /// </param>
        /// <param name="side">
        /// The side property of <see cref="RoundedImage"/>.
        /// </param>
        public CornerTargetModeSection(
            RoundedImage roundedImage,
            UnityAction repaint,
            SerializedProperty cornerMode,
            SerializedProperty roundingAmount,
            SerializedProperty selectedUnit,
            SerializedProperty side
        ) : base(roundedImage, repaint)
        {
            this._cornerMode = cornerMode;
            this._roundingAmount = roundingAmount;
            this._selectedUnit = selectedUnit;
            this._side = side;

            _content = new CornerTargetModeContent();
        }
        
        /// <summary>
        /// Draws the toolbar where the user can select their option, and draws the selected option.
        /// </summary>
        protected override void DrawSection()
        {
            _cornerMode.enumValueIndex = GUILayout.Toolbar(_cornerMode.enumValueIndex, _content.ToolbarOptions);

            // Based on selected toolbar index, will draw correct additional section.
            switch ((RoundingCornerMode)_cornerMode.enumValueIndex)
            {
                case RoundingCornerMode.ALL:
                    DrawAllMode();
                    break;
                case RoundingCornerMode.INDIVIDUAL:
                    DrawIndividualMode();
                    break;
                case RoundingCornerMode.SIDE:
                    DrawSideMode();
                    break;
                case RoundingCornerMode.UNIFORM:
                    DrawUniformMode();
                    break;
                default:
                    throw new NotSupportedException($"The corner target mode " +
                        $"{(RoundingCornerMode)_cornerMode.enumValueIndex} is not supported.");
            }
        }
        
        /// <summary>
        /// Draws the inspector section for when <see cref="RoundingCornerMode.UNIFORM"/> is selected.
        /// </summary>
        private void DrawUniformMode()
        {
            for (int i = 0; i < _roundingAmount.arraySize; i++)
            {
                float targetValue;
                switch ((RoundingUnit)_selectedUnit.enumValueIndex)
                {
                    case RoundingUnit.PERCENTAGE:
                        // Set to 100% percent.
                        targetValue = 1;
                        break;
                    case RoundingUnit.WORLD:
                        targetValue = _roundedImage.rectTransform.rect.GetShortLength() / 2;
                        break;
                    default:
                        RoundingUnit unsupportedUnit = (RoundingUnit)_selectedUnit.enumValueIndex;
                        string message = string.Format(ProceduralUIException.UNSUPPORTED_ROUNDING_UNIT_EXCEPTION_MESSAGE, unsupportedUnit);
                        throw new NotSupportedException(message);
                }
                _roundingAmount.GetArrayElementAtIndex(i).floatValue = targetValue;
            }
            EditorGUILayout.HelpBox(_content.UniformInfoMessage);
        }

        /// <summary>
        /// Draws the inspector section for when <see cref="RoundingCornerMode.SIDE"/> is selected.
        /// </summary>
        private void DrawSideMode()
        {
            EditorGUILayout.BeginHorizontal();

            // Set the selected side mode based on an enum popup.
            RoundingSide prevSide = (RoundingSide)_side.enumValueIndex;
            _side.enumValueIndex = (int)(RoundingSide)EditorGUILayout.EnumPopup(prevSide);

            // Retrieve the corners from the users selected side mode.
            (Corner, Corner) corners = RoundedImageEditor.SideToCorners((RoundingSide)_side.enumValueIndex);

            // Get the rounding from one of the corners.
            float rounding = _roundingAmount.GetArrayElementAtIndex((int)corners.Item1).floatValue;

            // If previous side doesn't match the current we use the 
            // rounding from the previous side to maintain the users rounding.
            if ((int)prevSide != _side.enumValueIndex)
            {
                var previousUsedCorners = RoundedImageEditor.SideToCorners(prevSide);
                rounding = _roundingAmount.GetArrayElementAtIndex((int)previousUsedCorners.Item2).floatValue;
            }

            float outputRounding = DrawSlider((RoundingUnit)_selectedUnit.enumValueIndex, rounding);
            for (int i = 0; i < _roundingAmount.arraySize; i++)
            {
                // Applies rounding from previous side to new side.
                bool indexMatchesUsedSide = i == (int)corners.Item1 || i == (int)corners.Item2;
                _roundingAmount.GetArrayElementAtIndex(i).floatValue = indexMatchesUsedSide ? outputRounding : 0;
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws the inspector section for when <see cref="RoundingCornerMode.ALL"/> is selected.
        /// </summary>
        private void DrawAllMode()
        {
            float rounding = _roundingAmount.GetArrayElementAtIndex(0).floatValue;
            rounding = DrawSlider((RoundingUnit)_selectedUnit.enumValueIndex, rounding);
            for (int i = 0; i < _roundingAmount.arraySize; i++)
                _roundingAmount.GetArrayElementAtIndex(i).floatValue = rounding;
        }

        /// <summary>
        /// Draws the inspector section for when <see cref="RoundingCornerMode.INDIVIDUAL"/> is selected.
        /// </summary>
        private void DrawIndividualMode()
        {
            RoundingUnit unit = (RoundingUnit)_selectedUnit.enumValueIndex;
            string tooltip = unit == RoundingUnit.PERCENTAGE ?
                  "Rounding of all corners in percentage."
                : "Rounding of all corners in world units.";
            for (int i = 0; i < _roundingAmount.arraySize; i++)
            {
                float rounding = DrawSlider(unit, _roundingAmount.GetArrayElementAtIndex(i).floatValue, $"{(Corner)i}".FromAllCapsToReadableFormat(), tooltip);

                _roundingAmount.GetArrayElementAtIndex(i).floatValue = rounding;
            }
        }
    }
}