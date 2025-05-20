#if TEST_FRAMEWORK

using DTT.PublishingTools;
using NUnit.Framework;
using System.Collections;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace DTT.UI.ProceduralUI.Tests
{
    /// <summary>
    /// Class that contains tests for <see cref="GradientEffect"/>.
    /// </summary>
    public class Test_GradientEffect
    {
        /// <summary>
        /// Reference to the <see cref="GradientEffect"/> component in the test scene.
        /// </summary>
        private GradientEffect _gradientEffect;

        /// <summary>
        /// Sets up the testing environment by loading a predefined scene and getting the references from it.
        /// </summary>
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            string scenePath;
            if (DTTEditorConfig.GetAssetJson("dtt.proceduralui").assetStoreRelease)
                scenePath = "Assets/DTT/Procedural UI/Tests/Scenes/Test Scene.unity";
            else
                scenePath = "Packages/dtt.proceduralui/Tests/Scenes/Test Scene.unity";
            EditorSceneManager.LoadSceneInPlayMode(scenePath, new LoadSceneParameters(LoadSceneMode.Single));
            yield return new WaitForSeconds(0.1f);

            _gradientEffect = Object.FindObjectOfType<GradientEffect>();
            _gradientEffect.Gradient = null;
        }

        /// <summary>
        /// Expects batching to prevent changes from happening to the gradient properties.
        /// </summary>
        [Test]
        public void Test_BatchingBlocksProperties()
        {
            // Arrange.
            _gradientEffect.Batching = true;
            Gradient originalGradient = _gradientEffect.Gradient;
            GradientEffect.GradientType originalType = _gradientEffect.Type;
            float originalRotation = _gradientEffect.Rotation;
            float originalScale = _gradientEffect.Scale;
            Vector2 originalOffset = _gradientEffect.Offset;

            // Act.
            _gradientEffect.Gradient = new Gradient();
            _gradientEffect.Type = GradientEffect.GradientType.DIAMOND;
            _gradientEffect.Rotation = 180f;
            _gradientEffect.Scale = 3;
            _gradientEffect.Offset = new Vector2(2, 1);

            bool valueChanged = _gradientEffect.Gradient != originalGradient ||
                _gradientEffect.Type != originalType ||
                _gradientEffect.Rotation != originalRotation ||
                _gradientEffect.Scale != originalScale ||
                _gradientEffect.Offset != originalOffset;

            // Assert.
            Assert.IsFalse(valueChanged, "A property value was changed, despite batching being turned on.");
        }

        /// <summary>
        /// Expects the batching property to be set to false.
        /// </summary>
        [Test]
        public void Test_SetBatching()
        {
            // Arrange.
            _gradientEffect.Batching = true;

            // Act.
            _gradientEffect.Batching = false;

            // Assert.
            Assert.IsFalse(_gradientEffect.Batching);
        }

        /// <summary>
        /// Expects the gradient type to successfully be set to a new value when batching is off.
        /// </summary>
        [Test]
        public void Test_UpdateGradientType()
        {
            // Arrange.
            GradientEffect.GradientType originalType = _gradientEffect.Type;
            GradientEffect.GradientType newType = GradientEffect.GradientType.DIAMOND;
            _gradientEffect.Batching = false;

            // Act.
            _gradientEffect.Type = newType;
            bool propertyChanged = newType == _gradientEffect.Type &&
                _gradientEffect.Type != originalType;

            // Assert.
            Assert.IsTrue(propertyChanged, "The type of gradient wasn't changed.");
        }

        /// <summary>
        /// Expects the gradient to successfully be set to a new value when batching is off.
        /// </summary>
        [Test]
        public void Test_UpdateGradient()
        {
            // Arrange.
            _gradientEffect.Gradient = null;
            Gradient newGradient = new Gradient()
            {
                colorKeys = new[]
                {
                    new GradientColorKey()
                    {
                        color = Color.green
                    }
                }
            };
            _gradientEffect.Batching = false;

            // Act.
            _gradientEffect.Gradient = newGradient;
            bool propertyChanged = newGradient == _gradientEffect.Gradient;

            // Assert.
            Assert.IsTrue(propertyChanged, "The gradient wasn't changed.");
        }

        /// <summary>
        /// Expects the offset to successfully be set to a new value when batching is off.
        /// </summary>
        [Test]
        public void Test_UpdateOffset()
        {
            // Arrange.
            Vector2 originalOffset = _gradientEffect.Offset;
            Vector2 newOffset = new Vector2(4, -4);
            _gradientEffect.Batching = false;

            // Act.
            _gradientEffect.Offset = newOffset;
            bool propertyChanged = newOffset == _gradientEffect.Offset &&
                _gradientEffect.Offset != originalOffset;

            // Assert.
            Assert.IsTrue(propertyChanged, "The offset wasn't changed.");
        }

        /// <summary>
        /// Expects the rotation to successfully be set to a new value when batching is off.
        /// </summary>
        [Test]
        public void Test_UpdateRotation()
        {
            // Arrange.
            float originalRotation = _gradientEffect.Rotation;
            float newRotation = 180f;
            _gradientEffect.Batching = false;

            // Act.
            _gradientEffect.Rotation = newRotation;
            bool propertyChanged = newRotation == _gradientEffect.Rotation &&
                _gradientEffect.Rotation != originalRotation;

            // Assert.
            Assert.IsTrue(propertyChanged, "The rotation wasn't changed.");
        }

        /// <summary>
        /// Expects the scale to successfully be set to a new value when batching is off.
        /// </summary>
        [Test]
        public void Test_UpdateScale()
        {
            // Arrange.
            float originalScale = _gradientEffect.Scale;
            float newScale = 10f;
            _gradientEffect.Batching = false;

            // Act.
            _gradientEffect.Scale = newScale;
            bool propertyChanged = newScale == _gradientEffect.Scale &&
                _gradientEffect.Scale != originalScale;

            // Assert.
            Assert.IsTrue(propertyChanged, "The scale wasn't changed.");
        }
    }
}

#endif