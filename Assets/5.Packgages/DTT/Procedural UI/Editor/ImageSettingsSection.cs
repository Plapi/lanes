using DTT.Utils.EditorUtilities;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace DTT.UI.ProceduralUI.Editor
{
    /// <summary>
    /// Displays the section in the inspector for <see cref="RoundedImage"/> where the user 
    /// can adjust the image settings.
    /// </summary>
    public class ImageSettingsSection : Section<RoundedImage>
    {
        /// <summary>
        /// All the related serialized properties for <see cref="ImageSettingsSection"/>.
        /// </summary>
        public struct ImageSerializedProperties
        {
            /// <summary>
            /// Related to <see cref="Image.type"/>.
            /// </summary>
            public SerializedProperty type;

            /// <summary>
            /// Related to <see cref="Image.fillMethod"/>.
            /// </summary>
            public SerializedProperty fillMethod;

            /// <summary>
            /// Related to <see cref="Image.fillOrigin"/>.
            /// </summary>
            public SerializedProperty fillOrigin;

            /// <summary>
            /// Related to <see cref="Image.fillClockwise"/>.
            /// </summary>
            public SerializedProperty fillClockwise;

            /// <summary>
            /// Related to <see cref="Image.fillAmount"/>.
            /// </summary>
            public SerializedProperty fillAmount;

            /// <summary>
            /// Related to <see cref="Image.sprite"/>.
            /// </summary>
            public SerializedProperty sprite;

            /// <summary>
            /// Related to <see cref="Image.color"/>.
            /// </summary>
            public SerializedProperty color;

            /// <summary>
            /// Related to <see cref="Image.useSpriteMesh"/>.
            /// </summary>
            public SerializedProperty useSpriteMesh;

            /// <summary>
            /// Related to <see cref="Image.preserveAspect"/>.
            /// </summary>
            public SerializedProperty preserveAspect;

            /// <summary>
            /// Related to the falloff distance serialized property of <see cref="RoundedImage"/>.
            /// </summary>
            public SerializedProperty falloffDistance;

            /// <summary>
            /// Related to <see cref="Image.material"/>.
            /// </summary>
            public SerializedProperty material;

            /// <summary>
            /// Related to <see cref="Image.maskable"/>.
            /// </summary>
            public SerializedProperty maskable;
        }

        /// <summary>
        /// A truncated version of <see cref="Image.Type"/> 
        /// where the unnecessary entries for <see cref="ImageSettingsSection"/> were removed.
        /// </summary>
        private enum TruncatedImageType
        {
            /// <summary>
            /// Related to <see cref="Image.Type.Simple"/>.
            /// </summary>
            Simple = 0,

            /// <summary>
            /// Related to <see cref="Image.Type.Filled"/>.
            /// </summary>
            Filled = 3,
        }

        /// <summary>
        /// The GUI content for <see cref="ImageSettingsSection"/>.
        /// </summary>
        private class ImageSettingsContent : GUIContentCache
        {
            /// <summary>
            /// The GUIContent for the distance falloff slider.
            /// </summary>
            public GUIContent FalloffSlider => base[nameof(FalloffSlider)];

            /// <summary>
            /// The GUIContent for the sprite field, where the user can assign an image.
            /// </summary>
            public GUIContent SpriteField => base[nameof(SpriteField)];

            /// <summary>
            /// The GUIContent for the warning when no sprite is assigned.
            /// </summary>
            public GUIContent NoSpriteWarning => base[nameof(NoSpriteWarning)];

            /// <summary>
            /// The GUIContent for the info bubble if no sprite is assigned for preserve aspect.
            /// </summary>
            public GUIContent CantUsePreserveAspect => base[nameof(CantUsePreserveAspect)];

            /// <summary>
            /// The GUIContent for whether the user wants to use clockwise on filled image types.
            /// </summary>
            public GUIContent Clockwise => base[nameof(Clockwise)];

            /// <summary>
            /// The GUIContent for the image type selection.
            /// </summary>
            public GUIContent ImageType => base[nameof(ImageType)];

            /// <summary>
            /// The GUIContent for the image type selection.
            /// </summary>
            public GUIContent PreserveAspectToggle => base[nameof(PreserveAspectToggle)];

            public ImageSettingsContent()
            {
                Add(nameof(FalloffSlider), () => new GUIContent("Fall-off Distance", "The amount of anti-aliasing that is applied."));
                Add(nameof(SpriteField), () => EditorGUIUtility.TrTextContent("Source Image"));
                Add(nameof(NoSpriteWarning), () => new GUIContent("A filled image won't have effect when there is no sprite assigned.", EditorGUIUtility.IconContent("console.warnicon.sml@2x").image));
                Add(nameof(CantUsePreserveAspect), () => new GUIContent("When no sprite is assigned preserve aspect can't be used.", EditorGUIUtility.IconContent("console.infoicon.sml@2x").image));
                Add(nameof(Clockwise), () => EditorGUIUtility.TrTextContent("Clockwise"));
                Add(nameof(ImageType), () => EditorGUIUtility.TrTextContent("Image Type"));
                Add(nameof(PreserveAspectToggle), () => EditorGUIUtility.TrTextContent("Preserve Aspect"));
            }
        }
        
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override string HeaderName => "Image Settings";
        
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override bool OpenFoldoutOnEnter => true;
        
        /// <summary>
        /// Contains all the properties relevant to draw <see cref="ImageSettingsSection"/>
        /// </summary>
        private ImageSerializedProperties _serializedProperties;

        /// <summary>
        /// Whether filled section should be foldedout.
        /// </summary>
        private bool _showFilled;

        /// <summary>
        /// Whether the sprite attached to the image component is atlas packed or not.
        /// </summary>
        private bool _isAtlasPacked;

        /// <summary>
        /// The GUIContent for <see cref="ImageSettingsSection"/>.
        /// </summary>
        private ImageSettingsContent _content;
        
        /// <summary>
        /// Creates a new section for the image settings.
        /// </summary>
        /// <param name="roundedImage">
        /// The rounded image instance to apply this to.
        /// </param>
        /// <param name="repaint">
        /// When called repaints the inspector. 
        /// Used for animating with <see cref="EditorGUILayout.BeginFadeGroup(float)"/>.
        /// </param>
        /// <param name="serializedProperties">
        /// Contains all the properties relevant to draw <see cref="ImageSettingsSection"/>
        /// </param>
        public ImageSettingsSection(
            RoundedImage roundedImage,
            UnityAction repaint,
            ImageSerializedProperties serializedProperties
        ) : base(roundedImage, repaint)
        {
            this._serializedProperties = serializedProperties;

            _content = new ImageSettingsContent();

            if (_roundedImage.sprite != null)
                _isAtlasPacked = SpriteUtility.IsAtlasPacked(_roundedImage.sprite);
        }
        
        /// <summary>
        /// Draws the section for the image settings; 
        /// sprite field, colour field, image type and relevant settings, 
        /// optionally falloff distance.
        /// </summary>
        protected override void DrawSection()
        {
#if !UNITY_2020_2_OR_NEWER 
            if (TryDrawSpriteWarningLayout())
                return;
#endif
            Material mat = (Material)_serializedProperties.material.objectReferenceValue;
            if (mat != null && !RoundedImageAssetManager.RoundingShaders.Contains(mat.shader))
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("The material you're using does not make use of the RoundedCorners shader.", MessageType.Error);
                DrawMaterialField();
                return;
            }

            DrawSpriteField();
            DrawColorField();
            DrawMaterialField();
            DrawTypeField();
            DrawPreserveAspectField();
            DrawMaskableField();
            DrawFalloffField();
        }
        
        /// <summary>
        /// Draws the colour property field.
        /// </summary>
        private void DrawColorField() => EditorGUILayout.PropertyField(_serializedProperties.color);

        /// <summary>
        /// Draws the field for the material.
        /// </summary>
        private void DrawMaterialField() => EditorGUILayout.PropertyField(_serializedProperties.material);

        /// <summary>
        /// Draws the fall off field slider.
        /// </summary>
        private void DrawFalloffField()
        {
            float currentFalloff = _serializedProperties.falloffDistance.floatValue;
            float newFalloff = DrawSlider(RoundingUnit.WORLD, currentFalloff, _content.FalloffSlider);
            _serializedProperties.falloffDistance.floatValue = newFalloff;
        }

        private void DrawPreserveAspectField()
        {
            bool condition = _serializedProperties.sprite.objectReferenceValue == null;
            EditorGUI.BeginDisabledGroup(condition);
            EditorGUILayout.PropertyField(_serializedProperties.preserveAspect, _content.PreserveAspectToggle);
            EditorGUI.EndDisabledGroup();
            if (condition)
                EditorGUILayout.HelpBox(_content.CantUsePreserveAspect);
        }

        /// <summary>
        /// Draws the maskable toggle.
        /// </summary>
        private void DrawMaskableField() => EditorGUILayout.PropertyField(_serializedProperties.maskable);

        /// <summary>
        /// Draws the image selection field.
        /// </summary>

        private void DrawSpriteField()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_serializedProperties.sprite, _content.SpriteField);
            if (EditorGUI.EndChangeCheck())
            {
                _roundedImage.DisableSpriteOptimizations();
                if (_serializedProperties.sprite.objectReferenceValue != null)
                    _isAtlasPacked = SpriteUtility.IsAtlasPacked((Sprite)_serializedProperties.sprite.objectReferenceValue);
                else
                    _isAtlasPacked = false;
            }
        }

        /// <summary>
        /// Draws the image type selection field and relevant settings for the selected option.
        /// <remark>
        /// Code mostly used from the UnityEditor UI. 
        /// It has been truncated for custom usage.
        /// </remark>
        /// </summary>
        private void DrawTypeField()
        {
            var truncatedImageType = (TruncatedImageType)_serializedProperties.type.enumValueIndex;
            var type = EditorGUILayout.EnumPopup(_content.ImageType, truncatedImageType);
            _serializedProperties.type.enumValueIndex = (int)(TruncatedImageType)type;

            ++EditorGUI.indentLevel;
            {
                bool isFilledType = truncatedImageType == TruncatedImageType.Filled;
                bool usesMultipleValues = !_serializedProperties.type.hasMultipleDifferentValues;
                _showFilled = usesMultipleValues && isFilledType;

                if (_showFilled)
                {
                    if (_serializedProperties.sprite.objectReferenceValue == null)
                        EditorGUILayout.HelpBox(_content.NoSpriteWarning);

                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(_serializedProperties.fillMethod);
                    if (EditorGUI.EndChangeCheck())
                        _serializedProperties.fillOrigin.intValue = 0;

                    Image.FillMethod fillMethodType = (Image.FillMethod)_serializedProperties.fillMethod.enumValueIndex;
                    DrawFillType(fillMethodType);

                    EditorGUILayout.PropertyField(_serializedProperties.fillAmount);
                    if (fillMethodType > Image.FillMethod.Vertical)
                        EditorGUILayout.PropertyField(_serializedProperties.fillClockwise, _content.Clockwise);
                }
            }
            --EditorGUI.indentLevel;
        }

        private void DrawFillType(Image.FillMethod fillMethodType)
        {
            int index;
            switch (fillMethodType)
            {
                case Image.FillMethod.Horizontal:
                    var originHorizontal = (Image.OriginHorizontal)_serializedProperties.fillOrigin.intValue;
                    index = Convert.ToInt32(EditorGUILayout.EnumPopup("Fill Origin", originHorizontal));
                    break;
                case Image.FillMethod.Vertical:
                    var originVertical = (Image.OriginVertical)_serializedProperties.fillOrigin.intValue;
                    index = Convert.ToInt32(EditorGUILayout.EnumPopup("Fill Origin", originVertical));
                    break;
                case Image.FillMethod.Radial90:
                    var origin90 = (Image.Origin90)_serializedProperties.fillOrigin.intValue;
                    index = Convert.ToInt32(EditorGUILayout.EnumPopup("Fill Origin", origin90));
                    break;
                case Image.FillMethod.Radial180:
                    var origin180 = (Image.Origin180)_serializedProperties.fillOrigin.intValue;
                    index = Convert.ToInt32(EditorGUILayout.EnumPopup("Fill Origin", origin180));
                    break;
                case Image.FillMethod.Radial360:
                    var origin360 = (Image.Origin360)_serializedProperties.fillOrigin.intValue;
                    index = Convert.ToInt32(EditorGUILayout.EnumPopup("Fill Origin", origin360));
                    break;
                default:
                    return;
            }
            _serializedProperties.fillOrigin.intValue = index;
        }

        /// <summary>
        /// Tries drawing a warning layout related to incorrect usage of the rounded image sprite.
        /// </summary>
        /// <returns>Whether the warning layout was drawn.</returns>
        private bool TryDrawSpriteWarningLayout()
        {
            Sprite sprite = _roundedImage.sprite;
            if (sprite == null || SpriteUtility.IsSpriteFromUnity(sprite))
                return false;

            string message;
            if (SpriteUtility.IsImportedWithMultipleSpriteMode(sprite))
            {
                message = "Sprites that are set to 'Sprite Mode: Multiple', are not supported in Unity versions older than 2020.2.";
                DrawWarning();
                return true;
            }
            else if (_roundedImage.sprite.packed)
            {
                message = "You are using a sprite packed by Unity's legacy packer, this is not supported.";
                DrawWarning();
                return true;
            }
            else if (_isAtlasPacked)
            {
                message = "Sprites that use a sprite atlas are not supported in Unity versions older than 2020.2.";
                DrawWarning();
                return true;
            }

            return false;

            void DrawWarning()
            {
                DrawSpriteField();
                EditorGUILayout.HelpBox(message, MessageType.Error);
            }
        }
    }
}