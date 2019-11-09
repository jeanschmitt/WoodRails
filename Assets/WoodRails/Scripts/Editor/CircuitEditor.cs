using UnityEngine;
using UnityEditor;

namespace WoodRails
{
    public class CircuitEditor : EditorWindow
    {
        // The window is selected if it already exists, else it's created.
        [MenuItem("Circuit/Edit Circuit")]
        private static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(CircuitEditor));
        }

        // Called to draw the MapEditor windows.
        private void OnGUI()
        {
            
        }

        // Does the rendering of the map editor in the scene view.
        private void OnSceneGUI(SceneView sceneView)
        {

        }

        void OnFocus()
        {
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI; // Just in case
            SceneView.onSceneGUIDelegate += this.OnSceneGUI;
        }

        void OnDestroy()
        {
            SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
        }
    }
}