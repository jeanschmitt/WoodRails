using System.Collections.Generic;
using UnityEngine;
using BansheeGz.BGSpline.Components;

namespace WoodRails
{
    [ExecuteInEditMode]
    public class AdaptiveRail : MonoBehaviour
    {
        #region Fields

        // MeshFilter du rail à adapter
        public MeshFilter meshFilter;

        /// <summary>
        /// Composant BGCcMath de la courbe du rail
        /// À remplacer par un array dans le futur
        /// </summary>
        public BGCcMath curveMath;


        // Modèle d'origine du rail
        private Mesh _originalMesh;

        // Clone du mesh, modifié
        private Mesh _clonedMesh;

        // Buffer des vertices
        [HideInInspector]
        public Vector3[] vertices;


        [HideInInspector]
        public int[] beginVertices;

        [HideInInspector]
        public int[] endVertices;


        // Offset des vertices de début de rail avec le début de la BGCurve
        private Dictionary<int, Vector3> _beginVerticesOffset = new Dictionary<int, Vector3>();

        // Offset des vertices de fin de rail avec le fin de la BGCurve
        private Dictionary<int, Vector3> _endVerticesOffset = new Dictionary<int, Vector3>();

        private Vector3 _beginInitialTangent;

        private Vector3 _endInitialTangent;

        private bool _initialStateSet;

        private MeshSelection _renderedMesh;

        #endregion

        #region Properties

        /// <summary>
        /// Le mesh d'origine a-t-il été cloné ?
        /// </summary>
        public bool IsCloned { get; private set; }

        #endregion

        #region Enums

        public enum MeshSelection
        {
            Original,
            Cloned
        }

        #endregion

        #region Unity Lifecycle Events

        // Start is called before the first frame update
        private void Start()
        {
            PrepareMeshes();
        }

        #endregion

        #region Events

        /// <summary>
        /// Appelé lorsque la courbe a changé
        /// </summary>
        public void CurveChanged()
        {
            if (_initialStateSet && _renderedMesh == MeshSelection.Cloned)
            {
                RebuildMesh();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Définit quel maillage est affiché (original ou cloné)
        /// </summary>
        /// <param name="selection"></param>
        public void SetRenderedMesh(MeshSelection selection)
        {
            switch (selection)
            {
                case MeshSelection.Original:
                    vertices = _originalMesh.vertices;
                    meshFilter.mesh = _originalMesh;
                    _renderedMesh = MeshSelection.Original;
                    break;
                case MeshSelection.Cloned:
                    vertices = _clonedMesh.vertices;
                    meshFilter.mesh = _clonedMesh;
                    _renderedMesh = MeshSelection.Cloned;
                    break;
            }
        }

        /// <summary>
        /// Retransforme tout le mesh
        ///
        /// Attention : ne marche pas si on scale le modèle
        /// </summary>
        public void RebuildMesh()
        {
            if (!_initialStateSet)
            {
                Debug.LogError("L'état initial du mesh n'a pas été défini");
                return;
            }

            if (_renderedMesh != MeshSelection.Cloned)
            {
                return;
            }

            // TODO : les rails courbes en utilisant les Sections des BGCurve
            // curveMath.SectionParts nombre de parties

            // World Space
            // Les tangentes sont normées
            Vector3 newBeginPoint = curveMath.CalcPositionAndTangentByDistanceRatio(0.0f, out Vector3 newBeginTangent);
            Vector3 newEndPoint = curveMath.CalcPositionAndTangentByDistanceRatio(1.0f, out Vector3 newEndTangent);


            // Début du rail
            Quaternion beginRotation = Quaternion.FromToRotation(_beginInitialTangent, newBeginTangent);

            foreach (int b in beginVertices)
            {
                Vector3 rotatedVector = beginRotation * _beginVerticesOffset[b];
                Vector3 newLocalPoint = meshFilter.transform.InverseTransformPoint(newBeginPoint + rotatedVector);
                vertices[b] = newLocalPoint;
            }


            // Fin du rail
            Quaternion endRotation = Quaternion.FromToRotation(_endInitialTangent, newEndTangent);

            foreach (int e in endVertices)
            {
                Vector3 rotatedVector = endRotation * _endVerticesOffset[e];
                Vector3 newLocalPoint = meshFilter.transform.InverseTransformPoint(newEndPoint + rotatedVector);
                vertices[e] = newLocalPoint;
            }

            ApplyEditing();
        }

        /// <summary>
        /// Renvoie les index des points à la position donnée
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public List<int> GetVerticesAtPos(Vector3 position)
        {
            List<int> relatedVertices = new List<int>();

            for (int v = 0; v < vertices.Length; v++)
            {
                if (vertices[v] == position)
                {
                    relatedVertices.Add(v);
                }
            }

            return relatedVertices;
        }

        /// <summary>
        /// Initialise les vecteurs entre le point d'origine début/fin de la
        /// courbe et les points de références définis grâce à l'inspecteur.
        /// </summary>
        public void InitVerticesOffset()
        {
            if (_renderedMesh != MeshSelection.Original)
            {
                Debug.LogError("Pour définir l'état initial, il faut être en mode modèle original");
            }


            // World space
            Vector3 beginCurvePoint = curveMath.CalcPositionAndTangentByDistanceRatio(0.0f, out _beginInitialTangent);
            Vector3 endCurvePoint = curveMath.CalcPositionAndTangentByDistanceRatio(1.0f, out _endInitialTangent);


            _beginVerticesOffset.Clear();
            foreach (int b in beginVertices)
            {
                Vector3 meshPosToWorld = meshFilter.transform.TransformPoint(vertices[b]);

                _beginVerticesOffset.Add(b, meshPosToWorld - beginCurvePoint);
            }

            _endVerticesOffset.Clear();
            foreach (int e in endVertices)
            {
                Vector3 meshPosToWorld = meshFilter.transform.TransformPoint(vertices[e]);

                _endVerticesOffset.Add(e, meshPosToWorld - endCurvePoint);
            }

            _initialStateSet = true;
        }

        /// <summary>
        /// Rétablit le modèle cloné en fonction du modèle d'origine
        /// </summary>
        public void Reset()
        {
            if (!IsCloned)
            {
                PrepareMeshes();
            }
            if (_clonedMesh != null && _originalMesh != null)
            {
                _clonedMesh.vertices = _originalMesh.vertices;
                _clonedMesh.triangles = _originalMesh.triangles;
                _clonedMesh.normals = _originalMesh.normals;
                _clonedMesh.uv = _originalMesh.uv;
                meshFilter.mesh = _clonedMesh;

                vertices = _clonedMesh.vertices;
                //_triangles = _clonedMesh.triangles;
            }

            beginVertices = null;
            endVertices = null;
            _beginVerticesOffset.Clear();
            _endVerticesOffset.Clear();
            _initialStateSet = false;

            SetRenderedMesh(MeshSelection.Cloned);
        }

        /// <summary>
        /// Applique les modifications sur le tableau de vertices
        /// sur le modèle cloné et recalcule les normales
        /// </summary>
        public void ApplyEditing()
        {
            _clonedMesh.vertices = vertices;
            _clonedMesh.RecalculateNormals();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Crée un modèle cloné du modèle d'origine et prépare les tableaux
        /// de vertices et triangles
        /// </summary>
        private void PrepareMeshes()
        {
            _originalMesh = meshFilter.sharedMesh;

            _clonedMesh = new Mesh
            {
                name = "clone",
                vertices = _originalMesh.vertices,
                triangles = _originalMesh.triangles,
                normals = _originalMesh.normals,
                uv = _originalMesh.uv
            };

            meshFilter.mesh = _clonedMesh;

            vertices = _clonedMesh.vertices;
            //_triangles = _clonedMesh.triangles;

            IsCloned = true;

            SetRenderedMesh(MeshSelection.Cloned);
        }

        #endregion
    }
}