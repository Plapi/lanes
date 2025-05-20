using DTT.PublishingTools;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace DTT.UI.ProceduralUI.Editor
{
    /// <summary>
    /// The custom inspector for <see cref="RoundedImage"/>.
    /// </summary>
    [CustomEditor(typeof(RoundedImage)), CanEditMultipleObjects]
    [DTTHeader("dtt.proceduralui")]
    public class RoundedImageEditor : DTTInspector
    {
        /// <summary>
        /// Contains the styles used by the editor.
        /// </summary>
        public static RoundedImageStyling Styling { get; private set; } = new RoundedImageStyling();
        
        /// <summary>
        /// All the sections that inspector should draw.
        /// </summary>
        private List<IDrawable> _sections = new List<IDrawable>();

        /// <summary>
        /// Reference to the target of the inspector.
        /// </summary>
        private RoundedImage _roundedImage;

        /// <summary>
        /// The section of the inspector that displays information to the user about active errors.
        /// </summary>
        private ErrorHandlingSection _errorHandlingSection;

        /// <summary>
        /// The serialized properties of <see cref="RoundedImage"/>.
        /// </summary>
        private RoundedImageSerializedProperties _serializedProperties;
        
        /// <summary>
        /// Initializes the sections.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            // Obtain references.
            _roundedImage = (RoundedImage)target;
            _serializedProperties = new RoundedImageSerializedProperties(serializedObject);

            CreateSections();
        }
        
        /// <summary>
        /// Draws the inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();

            // If there are any errors active, draw the error section to notify user.
            bool anyErrors = _roundedImage.ErrorHandler.AnyErrors;
            if (anyErrors)
                _errorHandlingSection.Draw();

            // If there are any errors active, disable the editor. 
            EditorGUI.BeginDisabledGroup(anyErrors);

            // Draw all the sections.
            for (int i = 0; i < _sections.Count; i++)
                _sections[i].Draw();

            EditorGUI.EndDisabledGroup();

            if (EditorGUI.EndChangeCheck())
                serializedObject.ApplyModifiedProperties();
        }
        
        /// <summary>
        /// Returns a tuple of the corners related to a certain side.
        /// </summary>
        /// <param name="side">The side to get the corners of.</param>
        /// <returns>The corners from the given side.</returns>
        public static (Corner, Corner) SideToCorners(RoundingSide side)
        {
            switch (side)
            {
                case RoundingSide.TOP:
                    return (Corner.TOP_LEFT, Corner.TOP_RIGHT);
                case RoundingSide.LEFT:
                    return (Corner.TOP_LEFT, Corner.BOTTOM_LEFT);
                case RoundingSide.BOTTOM:
                    return (Corner.BOTTOM_LEFT, Corner.BOTTOM_RIGHT);
                case RoundingSide.RIGHT:
                    return (Corner.TOP_RIGHT, Corner.BOTTOM_RIGHT);
                default:
                    throw new NotSupportedException($"This side is not support for side rounding: {side}");
            }
        }

        /// <summary>
        /// Adds the Rounded image in the "Create Gameobject" menu.
        /// </summary>
        [MenuItem("GameObject/UI/Rounded Image")]
        public static void RoundedImageMenuItem()
        {
            RoundedImage roundedImage = new GameObject().AddComponent<RoundedImage>();
            roundedImage.gameObject.layer = LayerMask.NameToLayer("UI");
            roundedImage.name = "Rounded Image";
            if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponentInParent<Canvas>() != null)
            {
                roundedImage.transform.SetParent(Selection.activeGameObject.transform, false);
            }
            else
            {
                if (FindObjectOfType<Canvas>() == null)
                    EditorApplication.ExecuteMenuItem("GameObject/UI/Canvas");

                Canvas canvas = FindObjectOfType<Canvas>();

                roundedImage.transform.SetParent(canvas.transform, false);
            }
            Selection.activeGameObject = roundedImage.gameObject;
        }
        
        /// <summary>
        /// Creates the different sections that are in the inspector.
        /// </summary>
        private void CreateSections()
        {
            // Define all the serialized properties for the Image Settings.
            var imageProperties = new ImageSettingsSection.ImageSerializedProperties
            {
                type = _serializedProperties.m_Type,
                fillMethod = _serializedProperties.m_FillMethod,
                fillOrigin = _serializedProperties.m_FillOrigin,
                fillClockwise = _serializedProperties.m_FillClockwise,
                fillAmount = _serializedProperties.m_FillAmount,
                sprite = _serializedProperties.m_Sprite,
                color = _serializedProperties.m_Color,
                preserveAspect = _serializedProperties.m_PreserveAspect,
                falloffDistance = _serializedProperties.distanceFalloff,
                material = _serializedProperties.m_Material,
                maskable = _serializedProperties.m_Maskable
            };

            UnityAction repaintAction = new UnityAction(Repaint);

            // Create the Image Settings section.
            _sections.Add(new ImageSettingsSection(_roundedImage, repaintAction, imageProperties));

            // Create the Unit Settings section.
            _sections.Add(new UnitSettingsSection(_roundedImage, repaintAction,
                _serializedProperties.selectedUnit,
                _serializedProperties.borderThickness,
                _serializedProperties.roundingAmount
            ));

            // Create the Corner Target Mode section.
            _sections.Add(new CornerTargetModeSection(_roundedImage, repaintAction,
                _serializedProperties.cornerMode,
                _serializedProperties.roundingAmount,
                _serializedProperties.selectedUnit,
                _serializedProperties.side
            ));

            // Create the Fill Mode section.
            _sections.Add(new FillModeSection(_roundedImage, repaintAction,
                _serializedProperties.borderThickness,
                _serializedProperties.roundingMode,
                _serializedProperties.selectedUnit
            ));

            // Create the Hitbox Settings section.
            _sections.Add(new HitboxSettingsSection(_roundedImage, repaintAction,
                _serializedProperties.useHitboxOutside,
                _serializedProperties.useHitboxInside,
                _serializedProperties.m_RaycastTarget
            ));

            _errorHandlingSection = new ErrorHandlingSection(_roundedImage.ErrorHandler);
        }
    }
}