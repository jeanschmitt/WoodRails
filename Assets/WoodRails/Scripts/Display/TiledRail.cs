using System.Collections.Generic;
using UnityEngine;
using BansheeGz.BGSpline.Components;

namespace WoodRails
{
    [RequireComponent(typeof(Rail))]
    public class TiledRail : MonoBehaviour
    {
        #region Public Fields

        /// <summary>
        /// Prefab utilisé pour effectuer le rendu du rail.
        /// Le modèle 3D doit être adapté à la répétition suivant son axe Z
        /// </summary>
        public GameObject Tile;

        /// <summary>
        /// Taille d'une tile suivant son axe Z
        /// </summary>
        public float TileSize;

        /// <summary>
        /// Si une tile doit être affichée à une distance plus grande que la taille du BGCurve,
        /// elle est rebouclée à l'origine de la courbe.
        /// Particulièrement utile lorsqu'offset est différend de 0, et lorsque la courbe est bouclée.
        /// Légère optimisation quand réglé à false
        /// </summary>
        public bool LoopTiles = false;

        /// <summary>
        /// Définit si les positions doivent être recalculées dès qu'un changement est observé
        /// </summary>
        public bool AutoUpdatePositions = true;

        #endregion


        #region Properties

        /// <summary>
        /// Décalage des tiles par rapport à l'origine (distance 0 du BGCurve)
        /// </summary>
        public float Offset
        {
            get
            {
                return _offset;
            }
            set
            {
                _offset = value;
                _needsUpdate = true;
            }
        }

        #endregion


        #region Private Fields

        // Composant BGCcMath contenu dans path
        private BGCcMath _math;

        // Taille totale de la curve
        private float _pathLength;

        // Décalage des tiles par rapport à l'origine (distance 0 du BGCurve)
        private float _offset = 0.0f;

        // Définit si les positions doivent être mises à jour
        private bool _needsUpdate = false;

        // Compensation au niveau du scale des tiles pour remplir sans trou le BGCurve.
        private float _compensatedScale = 1.0f;

        // Nouvelle taille des tiles après compensation
        private float _compensatedSize;

        // Tableau contenant les tiles
        private List<GameObject> _tiles = new List<GameObject>();

        #endregion


        #region Unity Lifecycle Events


        // Start is called before the first frame update
        void Start()
        {
            _math = GetComponent<Rail>().Math;

            Populate();
        }

        // Update is called once per frame
        void Update()
        {
            if (AutoUpdatePositions)
            {
                UpdatePositions();
            }
        }

        #endregion


        #region Public Methods

        /// <summary>
        /// Met à jour les positions des tiles
        /// </summary>
        private void UpdatePositions()
        {
            if (_needsUpdate)
            {
                float offset = _offset;
                for (int i = 0; i < _tiles.Count; i++)
                {
                    // Bouclage des tiles
                    if (LoopTiles)
                    {
                        if (offset > _pathLength)
                        {
                            offset -= _pathLength;
                        }
                        else if (offset < 0.0f)
                        {
                            offset += _pathLength;
                        }
                    }

                    Vector3 tangent;

                    GameObject tile = _tiles[i];

                    tile.transform.position = _math.CalcPositionAndTangentByDistance(offset, out tangent);
                    //this is a version for 3D. For 2D, comment this line and uncomment the next one
                    tile.transform.rotation = Quaternion.LookRotation(tangent);

                    offset += _compensatedSize;
                }
            }
        }

        #endregion


        #region Private Methods

        // Remplis le GameObject avec des tiles suivant la BGCurve
        private void Populate()
        {
            _pathLength = _math.GetDistance();

            if (System.Math.Abs(TileSize) < 0.00001f)
            {
                Debug.LogError("TileSize must be != 0.0f");
            }

            int tilesCount = (int) Mathf.Floor(_pathLength / TileSize);

            if (tilesCount == 0)
            {
                // Pas de tile, sortie
                return;
            }

            // Compensation au niveau du scale des tiles
            _compensatedScale = _pathLength / tilesCount / TileSize;
            _compensatedSize = TileSize * _compensatedScale;

            float tileOffset = 0.0f;

            for (int i = 0; i < tilesCount; i++)
            {
                AddTile(tileOffset);
                tileOffset += _compensatedSize;
            }

            // Premier placement des tiles
            _needsUpdate = true;
            UpdatePositions();
        }

        // Instancie une tile, enfante de l'objet courant
        private void AddTile(float tileOffset)
        {
            GameObject tile = Instantiate(Tile);
            tile.transform.parent = transform;
            tile.transform.localScale = new Vector3(1.0f, 1.0f, _compensatedScale);
            _tiles.Add(tile);
        }

        #endregion
    }
}