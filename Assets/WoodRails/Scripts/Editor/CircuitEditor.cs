using UnityEngine;
using UnityEditor;

namespace WoodRails
{
    [CustomEditor(typeof(Circuit))]
    public class CircuitEditor : Editor
    {
        public override void OnInspectorGUI() //2
        {
            base.DrawDefaultInspector();

            GUILayout.Space(20f);

            if (GUILayout.Button("Ouvrir l'éditeur de circuit"))
            {
                CircuitWindow.ShowWindow(target as Circuit);
            }
        }
    }
}