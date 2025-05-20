using DTT.Utils.EditorUtilities;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace DTT.UI.ProceduralUI.Editor
{
    /// <summary>
    /// Displays the section in the inspector for <see cref="RoundedImage"/> where the user 
    /// can toggle between <see cref="RoundingUnit"/>.
    /// This also handles conversion between the different values on change.
    /// </summary>
    public class UnitSettingsSection : Section<RoundedImage>
    {
        /// <summary>
        /// The GUI content for <see cref="UnitSettingsSection"/>.
        /// </summary>
        private class UnitSettingsContent : GUIContentCache
        {
            /// <summary>
            /// The toolbar option to convert to percentages.
            /// </summary>
            public GUIContent PercentageOption => base[nameof(PercentageOption)];

            /// <summary>
            /// The toolbar option to convert to world units.
            /// </summary>
            public GUIContent WorldUnitOption => base[nameof(WorldUnitOption)];

            /// <summary>
            /// All the toolbar options in <see cref="UnitSettingsSection"/>.
            /// </summary>
            public GUIContent[] ToolbarOptions => new GUIContent[] { PercentageOption, WorldUnitOption };

            public UnitSettingsContent()
            {
                Add(nameof(PercentageOption), () => new GUIContent("Percentage", "All parameters will be using percentages."));
                Add(nameof(WorldUnitOption), () => new GUIContent("World Units", "All parameters will be using world units."));
            }
        }
        
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override string HeaderName => "Unit Settings";
        
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override bool OpenFoldoutOnEnter => true;
        
        /// <summary>
        /// The selected unit property of <see cref="RoundedImage"/>.
        /// </summary>
        private SerializedProperty _selectedUnit;

        /// <summary>
        /// The border thickness property of <see cref="RoundedImage"/>.
        /// </summary>
        private SerializedProperty _borderThickness;

        /// <summary>
        /// The rounding amount property of <see cref="RoundedImage"/>.
        /// </summary>
        private SerializedProperty _roundingAmount;

        /// <summary>
        /// The GUIContent for this section.
        /// </summary>
        private UnitSettingsContent _content;
        
        /// <summary>
        /// Creates a new unit settings section.
        /// </summary>
        /// <param name="roundedImage">
        /// The rounded image instance to apply this to.
        /// </param>
        /// <param name="repaint">
        /// When called should repaint the inspector.
        /// </param>
        /// <param name="selectedUnit">
        /// The selected unit property of <see cref="RoundedImage"/>.
        /// </param>
        /// <param name="borderThickness">
        /// The border thickness property of <see cref="RoundedImage"/>.
        /// </param>
        /// <param name="roundingAmount">
        /// The rounding amount property of <see cref="RoundedImage"/>.
        /// </param>
        public UnitSettingsSection(RoundedImage roundedImage, UnityAction repaint,
            SerializedProperty selectedUnit,
            SerializedProperty borderThickness,
            SerializedProperty roundingAmount
        ) : base(roundedImage, repaint)
        {
            this._selectedUnit = selectedUnit;
            this._borderThickness = borderThickness;
            this._roundingAmount = roundingAmount;

            _content = new UnitSettingsContent();
        }
        
        /// <summary>
        /// Draws the section for converting between units.
        /// </summary>
        protected override void DrawSection()
        {
            int previousIndex = _selectedUnit.enumValueIndex;
            int selectedUnitIndex = DrawToolbar();

            if (previousIndex != selectedUnitIndex)
                ConvertUnits((RoundingUnit)selectedUnitIndex);
        }
        
        /// <summary>
        /// Draws the toolbar where the user can select their desired unit.
        /// </summary>
        /// <returns>The selected option.</returns>
        private int DrawToolbar()
        {
            int currentIndex = _selectedUnit.enumValueIndex;
            return _selectedUnit.enumValueIndex = GUILayout.Toolbar(currentIndex, _content.ToolbarOptions);
        }

        /// <summary>
        /// Converts the values of the <see cref="RoundedImage"/> to <paramref name="to"/>.
        /// </summary>
        /// <param name="to">The unit to convert to.</param>
        private void ConvertUnits(RoundingUnit to)
        {
            float shortLength = _roundedImage.rectTransform.rect.GetShortLength();
            switch (to)
            {
                case RoundingUnit.PERCENTAGE:
                    _borderThickness.floatValue /= _roundedImage.rectTransform.rect.GetShortLength();
                    _borderThickness.floatValue *= 2;
                    for (int i = 0; i < _roundingAmount.arraySize; i++)
                    {
                        _roundingAmount.GetArrayElementAtIndex(i).floatValue /= shortLength;
                        _roundingAmount.GetArrayElementAtIndex(i).floatValue *= 2;
                    }
                    break;
                case RoundingUnit.WORLD:
                    _borderThickness.floatValue *= _roundedImage.rectTransform.rect.GetShortLength() / 2;
                    for (int i = 0; i < _roundingAmount.arraySize; i++)
                        _roundingAmount.GetArrayElementAtIndex(i).floatValue *= shortLength / 2;

                    break;
                default:
                    throw new NotSupportedException($"Rounding unit {to} is not supported for conversion.");
            }
        }
    }
}