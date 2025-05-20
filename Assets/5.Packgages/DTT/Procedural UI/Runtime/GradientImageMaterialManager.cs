using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DTT.UI.ProceduralUI
{
    /// <summary>
    /// The Cach class for the image and material's of the gradient.
    /// Used when not animating the gradient.
    /// </summary>
    public static class GradientImageMaterialManager
    {
        /// <summary>
        /// Internal material data.
        /// </summary>
        public class MaterialData
        {
            /// <summary>
            /// If the material is for an Unity image or for the rounded image.
            /// </summary>
            public bool isForUnityImage;
            /// <summary>
            /// The type of the material.
            /// </summary>
            public int type;
            /// <summary>
            /// The offset of the gradient.
            /// </summary>
            public Vector2 offset;
            /// <summary>
            /// The rotation and scale of the gradient.
            /// </summary>
            public Vector2 rotationScale;
            /// <summary>
            /// The texture for the gradient.
            /// </summary>
            public Texture2D texture;

            /// <summary>
            /// Creates a copy from the object.
            /// </summary>
            /// <returns>The copy.</returns>
            public MaterialData Copy()
            {
                return new MaterialData()
                {
                    isForUnityImage = isForUnityImage,
                    offset = offset,
                    rotationScale = rotationScale,
                    type = type,
                    texture = texture
                };
            }
        }
        
        /// <summary>
        /// The Gradient sprites.
        /// </summary>
        private static readonly Dictionary<Gradient, Texture2D> _sprites = new Dictionary<Gradient, Texture2D>();
        /// <summary>
        /// The Gradient materials.
        /// </summary>
        private static readonly Dictionary<MaterialData, Material> _materials = new Dictionary<MaterialData, Material>();

        /// <summary>
        /// Gets the sprite of the gradient.
        /// </summary>
        /// <param name="gradient">The gradient.</param>
        /// <returns>The sprite of the gradient.</returns>
        public static Texture2D GetTextureForCache(Gradient gradient)
        {
            if (gradient == null)
                return null;

            Gradient selectedGradient = _sprites.GetComparableKey(gradient, (Gradient a, Gradient b) =>
            {
                if (a.mode != b.mode) 
                    return false;
                if (a.colorKeys.Length != b.colorKeys.Length) 
                    return false;
                if (a.alphaKeys.Length != b.alphaKeys.Length) 
                    return false;

                for (int i = 0; i < a.colorKeys.Length; i++)
                {
                    if (a.colorKeys[i].color != b.colorKeys[i].color) 
                        return false;
                    if (a.colorKeys[i].time != b.colorKeys[i].time) 
                        return false;
                }

                for (int i = 0; i < a.alphaKeys.Length; i++)
                {
                    if (a.alphaKeys[i].alpha != b.alphaKeys[i].alpha) 
                        return false;
                    if (a.alphaKeys[i].time != b.alphaKeys[i].time) 
                        return false;
                }

                return true;
            });

            if (selectedGradient != null)
                return _sprites[selectedGradient];

            Texture2D texture = GetTextureFromParameters(gradient);
                
            //create new gradient so that the instance is different
            Gradient gradientCopy = new Gradient();
            gradientCopy.alphaKeys = gradient.alphaKeys;
            gradientCopy.colorKeys = gradient.colorKeys;
            gradientCopy.mode = gradient.mode;

            //add it to the cach
            _sprites.Add(gradientCopy, texture);
            return texture;
        }

        /// <summary>
        /// Gets a material from given material data.
        /// </summary>
        /// <param name="materialData">The material data.</param>
        /// <returns>The created / cached material.</returns>
        public static Material GetMaterialFromCache(MaterialData materialData)
        {
            if (materialData == null)
                return null;

            MaterialData selectedMaterialData = _materials.GetComparableKey(materialData, (MaterialData a, MaterialData b) =>
            {
                if (a == null || b == null) 
                    return false;
                if (a.isForUnityImage != b.isForUnityImage) 
                    return false;
                if (a.offset != b.offset) 
                    return false;
                if (a.rotationScale != b.rotationScale) 
                    return false;
                if (a.type != b.type)
                    return false;
                if (a.texture != b.texture)
                    return false;
                return true;
            });

            if(selectedMaterialData != null && _materials[selectedMaterialData] != null)
                return _materials[selectedMaterialData];

            Material material = GetMaterialFromParameters(materialData);
            _materials.Add(materialData.Copy(), material);
            return material;
        }

        /// <summary>
        /// Gets the material from its parameters.
        /// </summary>
        /// <param name="materialData">The data to use.</param>
        /// <param name="material">If there is a material to use.</param>
        /// <returns>The updated / created material.</returns>
        public static Material GetMaterialFromParameters(MaterialData materialData, Material material = null)
        {
            if(material == null)
                material = new Material(materialData.isForUnityImage ? RoundedImageAssetManager.gradientShader : RoundedImageAssetManager.roundingShaderGradient);

            material.SetInt("_Type", materialData.type);
            material.SetTextureOffset("_GradientTex", materialData.offset);
            material.SetTextureScale("_GradientTex", materialData.rotationScale);
            material.SetTexture("_GradientTex", materialData.texture);
            material.name = "Gradient";

            return material;
        }

        /// <summary>
        /// Gets the texture from the parameters.
        /// </summary>
        /// <param name="gradient">The gradient to create the texture from.</param>
        /// <param name="texture">The texture to overwrite.</param>
        /// <returns>The updated / created texture.</returns>
        public static Texture2D GetTextureFromParameters(Gradient gradient, Texture2D texture = null)
        {
            if(texture == null)
                texture = new Texture2D(1024, 1);

            for (int i = 0; i <= 1024; i++)
                texture.SetPixel(i, 0, gradient.Evaluate(i / 1024.0f));

            texture.Apply();

            return texture;
        }

        /// <summary>
        /// Clears the cache.
        /// </summary>
        public static void Clear()
        {
            _sprites.Clear();
            _materials.Clear();
        }
    }
}