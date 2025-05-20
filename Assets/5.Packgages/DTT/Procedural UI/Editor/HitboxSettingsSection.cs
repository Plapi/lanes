using DTT.Utils.EditorUtilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DTT.UI.ProceduralUI.Editor
{
    /// <summary>
    /// Draws the hitbox settings in the inspector for <see cref="RoundedImage"/>.
    /// </summary>
    public class HitboxSettingsSection : Section<RoundedImage>
    {
        /// <summary>
        /// The GUI content for <see cref="HitboxSettingsSection"/>.
        /// </summary>
        private class HitboxSettingsContent : GUIContentCache
        {
            /// <summary>
            /// The content for the use of enabling/disabling hitboxes.
            /// </summary>
            public GUIContent HitboxEnabledOption => base[nameof(HitboxEnabledOption)];

            /// <summary>
            /// The content for the outside hitbox option.
            /// </summary>
            public GUIContent OutsideHitboxOption => base[nameof(OutsideHitboxOption)];

            /// <summary>
            /// The content for the inside hitbox option.
            /// </summary>
            public GUIContent InsideHitboxOption => base[nameof(InsideHitboxOption)];

            /// <summary>
            /// The content for the fill type warning.
            /// </summary>
            public GUIContent FillTypeWarning => base[nameof(FillTypeWarning)];

            /// <summary>
            /// The outside hitbox detection content.
            /// </summary>
            public GUIContent OutsideHitBoxDetection => base[nameof(OutsideHitBoxDetection)];

            /// <summary>
            /// The inside hitbox detection content.
            /// </summary>
            public GUIContent InsideHitBoxDetection => base[nameof(InsideHitBoxDetection)];

            /// <summary>
            /// The full rectangle area setting content.
            /// </summary>
            public GUIContent FullRectangleAreaSetting => base[nameof(FullRectangleAreaSetting)];

            /// <summary>
            /// The account for rounding setting content.
            /// </summary>
            public GUIContent AccountForRounding => base[nameof(AccountForRounding)];

            /// <summary>
            /// The no inside hitbox setting content.
            /// </summary>
            public GUIContent NoInsideHitBox => base[nameof(NoInsideHitBox)];

            /// <summary>
            /// The account for border fill setting content.
            /// </summary>
            public GUIContent AccountForBorderFill => base[nameof(AccountForBorderFill)];

            public HitboxSettingsContent()
            {
                Add(nameof(HitboxEnabledOption), () => new GUIContent("Raycast Target", "Do you want to be able to 'hit' " +
                    "this graphic with mouse events."));
                Add(nameof(OutsideHitboxOption), () => new GUIContent("Dynamic Outer Hitbox", "The hitbox for this image " +
                    "will take the outside boundaries into account."));
                Add(nameof(InsideHitboxOption), () => new GUIContent("Dynamic Inner Hitbox", "The hitbox for this image " +
                    "will take the inside boundaries into account"));
                Add(nameof(FillTypeWarning), () => new GUIContent("When using the filled image type, the cut-off areas are " +
                    "not taken into account for hit testing.", EditorGUIUtility.IconContent("console.warnicon.sml@2x").image));

                Add(nameof(OutsideHitBoxDetection), () => new GUIContent("Use Outside Hitbox"));
                Add(nameof(FullRectangleAreaSetting), () =>
                {
                    GUIContent content = new GUIContent("Full rectangle area");
                    content.tooltip = "Will ignore rounding.";
                    return content;
                });

                Add(nameof(AccountForRounding), () =>
                {
                    GUIContent content = new GUIContent("Account for rounding");
                    content.tooltip = "Will account for rounding amount of borders.";
                    return content;
                });

                Add(nameof(InsideHitBoxDetection), () => new GUIContent("Use Inside Hitbox"));
                Add(nameof(NoInsideHitBox), () =>
                {
                    GUIContent content = new GUIContent("No inside hitbox");
                    content.tooltip = "Will ignore border fill.";
                    return content;
                });

                Add(nameof(AccountForBorderFill), () =>
                {
                    GUIContent content = new GUIContent("Account for fill");
                    content.tooltip = "Will account for fill amount of borders.";
                    return content;
                });
            }
        }
        
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override string HeaderName => "Hitbox Settings";
        
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override bool OpenFoldoutOnEnter => true;
        
        /// <summary>
        /// The use of outside hitbox property of <see cref="RoundedImage"/>.
        /// </summary>
        private SerializedProperty _useHitboxOutside;

        /// <summary>
        /// The use of inside hitbox property of <see cref="RoundedImage"/>.
        /// </summary>
        private SerializedProperty _useHitboxInside;

        /// <summary>
        /// Whether to be able to fire raycasts on this graphic.
        /// </summary>
        private SerializedProperty _raycastTarget;

        /// <summary>
        /// The GUIContent for <see cref="HitboxSettingsSection"/>.
        /// </summary>
        private HitboxSettingsContent _content;
        
        /// <summary>
        /// Creates a new hitbox settings section.
        /// </summary>
        /// <param name="roundedImage">
        /// The rounded image instance to apply this to.
        /// </param>
        /// <param name="repaint">
        /// When called should repaint the inspector.
        /// </param>
        /// <param name="useHitboxOutside">
        /// The use of outside hitbox property of <see cref="RoundedImage"/>.
        /// </param>
        /// <param name="useHitboxInside">
        /// The use of inside hitbox property of <see cref="RoundedImage"/>.
        /// </param>
        public HitboxSettingsSection(
            RoundedImage roundedImage,
            UnityAction repaint,
            SerializedProperty useHitboxOutside,
            SerializedProperty useHitboxInside,
            SerializedProperty raycastTarget
        ) : base(roundedImage, repaint)
        {
            this._useHitboxOutside = useHitboxOutside;
            this._useHitboxInside = useHitboxInside;
            this._raycastTarget = raycastTarget;

            _content = new HitboxSettingsContent();
        }
        
        /// <summary>
        /// Draws the toggles to set hitbox settings.
        /// </summary>
        protected override void DrawSection()
        {
            _raycastTarget.boolValue = EditorGUILayout.ToggleLeft(_content.HitboxEnabledOption, _raycastTarget.boolValue);
            if (!_raycastTarget.boolValue)
                return;

            DrawOutsideHitboxSettings();
            DrawInsideHitboxSettings();

            // Shows a warning when using filled image type.
            if (_roundedImage.type == Image.Type.Filled && _raycastTarget.boolValue)
                EditorGUILayout.HelpBox(_content.FillTypeWarning);
        }

        /// <summary>
        /// Draws the outside hitbox settings.
        /// </summary>
        private void DrawOutsideHitboxSettings()
        {
            EditorGUILayout.Space();
            GUIContent[] displayOptions = new GUIContent[]
            {
                _content.FullRectangleAreaSetting,
                _content.AccountForRounding
            };

            int selectedIndex = _useHitboxOutside.boolValue ? 1 : 0;
            selectedIndex = EditorGUILayout.Popup(_content.OutsideHitBoxDetection, selectedIndex, displayOptions);

            // Update the boolean value based on the selected index.
            _useHitboxOutside.boolValue = selectedIndex == 1 ? true : false;

            // If the Outer Hitbox setting has been turned off
            // this makes sure the Inner Hitbox setting is also turned off.
            if (!_useHitboxOutside.boolValue)
                _useHitboxInside.boolValue = false;
        }

        /// <summary>
        /// Draws the inside hitbox settings.
        /// </summary>
        private void DrawInsideHitboxSettings()
        {
            if (_roundedImage.Mode == RoundingMode.BORDER)
            {
                GUIContent[] displayOptions = new GUIContent[]
                {
                    _content.NoInsideHitBox,
                    _content.AccountForBorderFill
                };

                EditorGUI.BeginDisabledGroup(!_useHitboxOutside.boolValue);

                int selectedIndex = _useHitboxInside.boolValue ? 1 : 0;
                selectedIndex = EditorGUILayout.Popup(_content.InsideHitBoxDetection, selectedIndex, displayOptions);

                // Update the boolean value based on the selected index.
                _useHitboxInside.boolValue = selectedIndex == 1 ? true : false;

                EditorGUI.EndDisabledGroup();
            }
        }
    }
}