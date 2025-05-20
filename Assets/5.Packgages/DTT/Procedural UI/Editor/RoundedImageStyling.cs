using DTT.PublishingTools;
using DTT.Utils.EditorUtilities;
using UnityEditor;
using UnityEngine;

namespace DTT.UI.ProceduralUI.Editor
{
    /// <summary>
    /// Contains general styling elements for <see cref="RoundedImage"/>
    /// </summary>
    public class RoundedImageStyling : GUIStyleCache
    {
        
        /// <summary>
        /// The header style for fade group foldouts.
        /// </summary>
        public GUIStyle FoldoutHeaderStyle => base[nameof(FoldoutHeaderStyle)];

        /// <summary>
        /// The colours used for this styling. These adjust based on pro theme.
        /// </summary>
        private Color _textColor => EditorGUIUtility.isProSkin ? Color.white : new Color(0.1803922f, 0.1764706f, 0.1764706f);
        
        public RoundedImageStyling()
        {
            Add(nameof(FoldoutHeaderStyle), () =>
            {
                GUIStyle style = new GUIStyle(DTTGUI.styles.Label);
                style.font = DTTGUI.TitleFont;
                style.fontSize = 14;
                style.alignment = TextAnchor.MiddleLeft;
                style.padding = new RectOffset(10, 10, 0, 0);
                style.normal.textColor = _textColor;
                style.onNormal.textColor = _textColor;
                return style;
            });
        }
    }
}