using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace WoodRails
{
    [CustomEditor(typeof(AdaptiveRail))]
    public class AdaptiveRailEditor : Editor
    {
        #region Fields

        private AdaptiveRail _mesh;

        private Transform _handleTransform;
        private Quaternion _handleRotation;

        // État de la sélection
        // -1 : pas de sélection
        // 0 : sélection des beginVertices
        // 1 : sélection des endVertices
        private int _selectionState = -1;

        // Vertices sélectionnés pour le début de la courbe
        private HashSet<int> _selectedBeginVertices = new HashSet<int>();

        // Vertices sélectionnés pour la fin de la courbe
        private HashSet<int> _selectedEndVertices = new HashSet<int>();

        private bool _showVertices;

        #endregion

        #region Properties

        private bool ShowVertices { get { return _showVertices || _selectionState != -1; } }

        #endregion

        #region Unity Lifecycle Events

        // Custom Inspector
        public override void OnInspectorGUI()
        {
            //
            GUILayout.Label("Default Inspector");

            DrawDefaultInspector();

            _mesh = target as AdaptiveRail;

            //
            GUILayout.Space(10f);
            GUILayout.Label("Vertices affectation");

            if (GUILayout.Button("Sélectionner les vertices"))
            {
                if (_selectionState == -1)
                {
                    _selectionState = 0; // par défaut

                    _mesh.SetRenderedMesh(AdaptiveRail.MeshSelection.Original);

                    // On récupère les liens des vertices venant de l'objet
                    _selectedBeginVertices = _mesh.beginVertices == null ? new HashSet<int>() : new HashSet<int>(_mesh.beginVertices);
                    _selectedEndVertices = _mesh.endVertices == null ? new HashSet<int>() : new HashSet<int>(_mesh.endVertices);
                }
                else
                {
                    _selectionState = -1;

                    _mesh.SetRenderedMesh(AdaptiveRail.MeshSelection.Cloned);
                }

                // Actualise la vue
                SceneView.RepaintAll();
            }

            // En mode de sélection
            if (_selectionState != -1)
            {
                string[] choices = { "Begin", "End" };
                _selectionState = GUILayout.SelectionGrid(_selectionState, choices, 2);

                if (GUILayout.Button("Apply selection"))
                {
                    ApplySelection();
                }
            }

            if (GUILayout.Button("Link with curve"))
            {
                _mesh.InitVerticesOffset();
            }

            //
            GUILayout.Space(10f);
            GUILayout.Label("Actions");

            if (GUILayout.Button("Reset Mesh"))
            {
                _mesh.Reset();
                _selectedBeginVertices.Clear();
                _selectedEndVertices.Clear();
            }

            if (GUILayout.Button("Show vertices"))
            {
                _showVertices = !_showVertices;

                SceneView.RepaintAll();
            }
        }

        // Appelé à chaque affichage de la SceneView
        private void OnSceneGUI()
        {
            _mesh = target as AdaptiveRail;

            if (ShowVertices)
            {
                ShowHandles();
            }
        }

        #endregion

        #region Private Methods

        // Affiche les points de sélection des vertices
        private void ShowHandles()
        {
            _handleTransform = _mesh.meshFilter.transform;
            _handleRotation = (Tools.pivotRotation == PivotRotation.Local) ? _handleTransform.rotation : Quaternion.identity;

            for (int i = 0; i < _mesh.vertices.Length; i++)
            {
                ShowHandle(i);
            }
        }

        // Affiche le point de sélection pour un vertex
        private void ShowHandle(int index)
        {
            const float handleSize = .004f;

            Vector3 point = _handleTransform.TransformPoint(_mesh.vertices[index]);

            // Couleur du point
            if (_selectedBeginVertices.Contains(index))
            {
                Handles.color = Color.red;
            }
            else if (_selectedEndVertices.Contains(index))
            {
                Handles.color = Color.yellow;
            }
            else
            {
                Handles.color = Color.blue;
            }

            // En train de sélectionner
            if (_selectionState != -1)
            {
                if (Handles.Button(point, _handleRotation, handleSize, handleSize, Handles.SphereHandleCap))
                {
                    SelectVertex(index);
                }
            }
            // Pas en état de sélection
            else
            {
                Handles.SphereHandleCap(index, point, _handleRotation, handleSize, EventType.Repaint);
            }
        }

        // Sélectionne un vertex ainsi que tous ceux superposés avec lui
        private void SelectVertex(int index)
        {
            if (_selectionState == -1)
            {
                return;
            }

            foreach (int vIndex in _mesh.GetVerticesAtPos(_mesh.vertices[index]))
            {
                if (_selectionState == 0)
                {
                    // Supprime de l'autre liste si le point est dedans
                    _selectedEndVertices.Remove(vIndex);

                    // Ajoute l'élément (les index sont uniques grâce au HashSet)
                    _selectedBeginVertices.Add(vIndex);
                }
                else
                {
                    // Supprime de l'autre liste si le point est dedans
                    _selectedBeginVertices.Remove(vIndex);

                    // Ajoute l'élément (les index sont uniques grâce au HashSet)
                    _selectedEndVertices.Add(vIndex);
                }
            }
        }

        // Répercute la sélection sur l'objet AdaptiveRail
        private void ApplySelection()
        {
            _mesh.beginVertices = new int[_selectedBeginVertices.Count];
            _selectedBeginVertices.CopyTo(_mesh.beginVertices);

            _mesh.endVertices = new int[_selectedEndVertices.Count];
            _selectedEndVertices.CopyTo(_mesh.endVertices);

            //_mesh.RebuildMesh();
        }

        #endregion
    }
}