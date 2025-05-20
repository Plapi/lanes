using DTT.PublishingTools;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace DTT.UI.ProceduralUI.Editor
{
    /// <summary>
    /// The custom inspector for <see cref="GradientEffect"/>.
    /// </summary>
    [CustomEditor(typeof(GradientEffect))]
    [CanEditMultipleObjects]
    [DTTHeader("dtt.proceduralui")]
    public class GradientEditor : DTTInspector
    {
        /// <summary>
        /// The gradient effect object
        /// </summary>
        private GradientEffect _gradientEffect;
        
        /// <summary>
        /// All the effects currently selected.
        /// </summary>
        private GradientEffect[] _gradientEffects;

        /// <summary>
        /// The serialized object's for the gradient
        /// </summary>
        private GradientEffectSerializedProperties _serializedProperties;

        /// <summary>
        /// All the sections that inspector should draw.
        /// </summary>
        private readonly List<IDrawable> _sections = new List<IDrawable>();
        
        /// <summary>
        /// Initializes the sections.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            serializedObject.Update();

            // Obtain references.
            _gradientEffect = (GradientEffect)target;
            _gradientEffects = targets.Cast<GradientEffect>().ToArray();
            _gradientEffect.UpdateGradient();
            _serializedProperties = new GradientEffectSerializedProperties(serializedObject);

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

            // Draw all the sections.
            for (int i = 0; i < _sections.Count; i++)
                _sections[i].Draw();

            if (EditorGUI.EndChangeCheck() || 
                (Event.current.type == EventType.ValidateCommand &&
                 Event.current.commandName == "UndoRedoPerformed"))
            {
                serializedObject.ApplyModifiedProperties();
                foreach (GradientEffect effect in _gradientEffects)
                    effect.UpdateGradient();
            }
        }
        
        /// <summary>
        /// Creates the different sections that are in the inspector.
        /// </summary>
        private void CreateSections()
        {
            UnityAction repaintAction = new UnityAction(Repaint);

            // Create the Image Settings section.
            _sections.Add(new GradientSection(_gradientEffect, repaintAction,
                _serializedProperties
                ));
        }
    }
}