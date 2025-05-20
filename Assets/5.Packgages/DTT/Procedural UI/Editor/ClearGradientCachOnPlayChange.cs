using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DTT.UI.ProceduralUI.Editor
{
    /// <summary>
    /// Static class for capturing unity play mode changed event.
    /// </summary>
    [InitializeOnLoadAttribute]
    public static class ClearGradientCachOnPlayChange
    {
        /// <summary>
        /// Register an event handler when the class is initialized.
        /// </summary>
        static ClearGradientCachOnPlayChange()
        {
            EditorApplication.playModeStateChanged += ClearCachePlayModeState; 
            EditorSceneManager.sceneOpened += SceneLoaded;
        }
        
        /// <summary>
        /// Clears the cache when switching play modes.
        /// </summary>
        /// <param name="state"> The state it changes to.</param>
        private static void ClearCachePlayModeState(PlayModeStateChange state) => GradientImageMaterialManager.Clear();

        /// <summary>
        /// Clears the cache if the scene changed.
        /// </summary>
        private static void SceneLoaded(Scene scene, OpenSceneMode mode) => GradientImageMaterialManager.Clear();
    }
}