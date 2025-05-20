using UnityEngine;

namespace DTT.UI.ProceduralUI.Demo
{
    /// <summary>
    /// Example class of how to use the <see cref="GradientEffect"/> in code.
    /// </summary>
    public class GradientEffectExample : MonoBehaviour
    {
        /// <summary>
        /// Reference to the <see cref="GradientEffect"/> of this object.
        /// </summary>
        [SerializeField]
        private GradientEffect gradientEffect;

        /// <summary>
        /// Calls all possible methods and properties for the <see cref="GradientEffect"/>.
        /// </summary>
        private void Start()
        {
            // Gets the gradient component.
            gradientEffect = GetComponent<GradientEffect>();

            // Allows you to enable or disable batching.
            // To change the gradient properties, batching must be turned off.
            gradientEffect.Batching = false;

            // Set the type of the gradient to the given GradientType.
            gradientEffect.Type = GradientEffect.GradientType.LINEAR;

            // Set the gradient.
            gradientEffect.Gradient = new Gradient();

            // Set the offset of the gradient.
            gradientEffect.Offset = new Vector2(1, 0);

            // Set the rotation of the gradient.
            gradientEffect.Rotation = 180f;

            // Set the scale of the gradient.
            gradientEffect.Scale = 2f;

            // Updates the gradient.
            // This doesn't need to be called when properties are changed.
            gradientEffect.UpdateGradient();
        }
    }
}