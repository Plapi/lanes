#if UNITY_EDITOR

using DTT.PublishingTools;
using UnityEditor;

namespace DTT.UI.ProceduralUI
{
    /// <summary>
    /// Holds the operation for opening the ReadMe of this package.
    /// </summary>
    public class ProceduralUIReadMeOpener
    {
        /// <summary>
        /// Opens the ReadMe of this package.
        /// </summary>
        [MenuItem("Tools/DTT/ProceduralUI/ReadMe")]
        private static void OpenReadMe() => DTTEditorConfig.OpenReadMe("dtt.proceduralui");
    }
}
#endif