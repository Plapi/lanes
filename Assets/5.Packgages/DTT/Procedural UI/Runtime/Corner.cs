using UnityEngine;

namespace DTT.UI.ProceduralUI
{
    /// <summary>
    /// Mappings for the corners used by <see cref="RoundedImage"/>.
    /// </summary>
    public enum Corner
    {
        /// <summary>
        /// The top left corner.
        /// </summary>
        [InspectorName("Top Left")]
        TOP_LEFT = 0,

        /// <summary>
        /// The top right corner.
        /// </summary>
        [InspectorName("Top Right")]
        TOP_RIGHT = 1,

        /// <summary>
        /// The bottom left corner.
        /// </summary>
        [InspectorName("Bottom Left")]
        BOTTOM_LEFT = 2,

        /// <summary>
        /// The bottom right corner.
        /// </summary>
        [InspectorName("Bottom Right")]
        BOTTOM_RIGHT = 3,
    }
}