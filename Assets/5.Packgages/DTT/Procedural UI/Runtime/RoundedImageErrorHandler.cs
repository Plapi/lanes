using DTT.UI.ProceduralUI.Exceptions;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace DTT.UI.ProceduralUI
{
    /// <summary>
    /// Checks for relevant errors and throws an exception if necessary.
    /// </summary>
    public class RoundedImageErrorHandler
    {
        /// <summary>
        /// Whether there are any errors active.
        /// </summary>
        public bool AnyErrors
        {
            get
            {
                try
                {
                    CheckForErrors();
                    return false;
                }
                catch
                {
                    return true;
                }
            }
        }
        
        /// <summary>
        /// The RoundedImage we're inspecting for errors.
        /// </summary>
        private RoundedImage _roundedImage;

        /// <summary>
        /// Is part of prefab scene.
        /// </summary>
        private readonly bool _isPartOfPrefabScene;
        
        /// <summary>
        /// Creates a new instance of <see cref="RoundedImageErrorHandler"/>.
        /// </summary>
        /// <param name="roundedImage">
        /// The rounded image we're check for errors.
        /// </param>
        public RoundedImageErrorHandler(RoundedImage roundedImage)
        {
            this._roundedImage = roundedImage;
#if UNITY_EDITOR
#pragma warning disable 0618
            // Check whether the component is part of a prefab scene.
            _isPartOfPrefabScene = _roundedImage.gameObject.scene.name == PrefabUtility.FindPrefabRoot(_roundedImage.gameObject).name;
#pragma warning restore
#endif
        }
        
        /// <summary>
        /// Checks for any errors that are active in the selected <see cref="RoundedImage"/> instance.
        /// If there is an error it will be thrown. 
        /// All exceptions thrown will be derived from <see cref="ProceduralUIException"/>.
        /// </summary>
        public void CheckForErrors()
        {
            bool isPartOfPrefabPreviewOrPrefabScene = false;
#if UNITY_EDITOR
            isPartOfPrefabPreviewOrPrefabScene = PrefabUtility.IsPartOfPrefabAsset(_roundedImage) || _isPartOfPrefabScene;
#endif

            // Trigger exceptions that can cause the rounded image not to work
            // when it is not part of a prefab preview or prefab scene.
            if (_roundedImage.canvas == null && !isPartOfPrefabPreviewOrPrefabScene)
                throw new CanvasMissingException();

            if (!isPartOfPrefabPreviewOrPrefabScene)
            {
                if (!_roundedImage.canvas.additionalShaderChannels.HasFlag(AdditionalCanvasShaderChannels.TexCoord1))
                    throw new TexCoord1MissingException(_roundedImage.canvas);
                if (!_roundedImage.canvas.additionalShaderChannels.HasFlag(AdditionalCanvasShaderChannels.TexCoord2))
                    throw new TexCoord2MissingException(_roundedImage.canvas);
                if (!_roundedImage.canvas.additionalShaderChannels.HasFlag(AdditionalCanvasShaderChannels.TexCoord3))
                    throw new TexCoord3MissingException(_roundedImage.canvas);
            }

            if (Shader.Find(RoundedImageAssetManager.ROUNDING_SHADER_NAME) == null)
                throw new RoundingShaderNotFoundException();
        }

        /// <summary>
        /// Fixes errors, causing the rounded image not to work, that can be fixed.
        /// </summary>
        public void FixFixableErrors()
        {
            IFixableCanvasException[] fixables = GetCurrentFixableErrors();
            foreach (IFixableCanvasException fixable in fixables)
                fixable.Fix();
        }
        
        /// <summary>
        /// Returns an array of currently fixable errors.
        /// </summary>
        /// <returns>An array of currently fixable errors.</returns>
        private IFixableCanvasException[] GetCurrentFixableErrors()
        {
            List<IFixableCanvasException> errors = new List<IFixableCanvasException>();

            if (!_roundedImage.canvas.additionalShaderChannels.HasFlag(AdditionalCanvasShaderChannels.TexCoord1))
                errors.Add(new TexCoord1MissingException(_roundedImage.canvas));
            if (!_roundedImage.canvas.additionalShaderChannels.HasFlag(AdditionalCanvasShaderChannels.TexCoord2))
                errors.Add(new TexCoord2MissingException(_roundedImage.canvas));
            if (!_roundedImage.canvas.additionalShaderChannels.HasFlag(AdditionalCanvasShaderChannels.TexCoord3))
                errors.Add(new TexCoord3MissingException(_roundedImage.canvas));

            return errors.ToArray();
        }
    }
}