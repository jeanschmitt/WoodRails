using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BansheeGz.BGSpline.Components;
using BansheeGz.BGSpline.Curve;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WoodRails
{
    [RequireComponent(typeof(BGCurve))]
    [RequireComponent(typeof(BGCcMath))]
    [ExecuteInEditMode]
    public class Rail : MonoBehaviour
    {
        #region Public Fields
        

        /// <summary>
        /// Liste des rails suivant le rail actuel
        /// À part pour des aiguillages et autres, cette liste ne contient qu'un et un seul élément
        /// </summary>
        [SerializeField]
        public List<Rail> NextRails;// = new List<Rail>();

        /// <summary>
        /// Liste des rails précédant le rail actuel
        /// À part pour des aiguillages et autres, cette liste ne contient qu'un et un seul élément
        /// </summary>
        [SerializeField]
        public List<Rail> PreviousRails;// = new List<Rail>();

        /// <summary>
        /// Composant BGCurve rattaché au rail actuel, définissant la courbe du rail
        /// </summary>
        public BGCurve Curve;
        
        /// <summary>
        /// Composant BGCcMath rattaché au rail actuel, permettant des opérations mathématiques sur la courbe
        /// du rail.
        /// </summary>
        [HideInInspector]
        public BGCcMath Math;


        /// <summary>
        /// Extrémité du rail
        /// </summary>
        public enum RAIL_BOUNDARY
        {
            RAIL_END,
            RAIL_BEGIN
        }

        

        #endregion

        #region Properties

        /// <summary>
        /// Taille en mètres du rail
        /// </summary>
        public float PathLength { get { return _pathLength; } }

        /// <summary>
        /// Renvoie le rail suivant actuellement sélectionné
        /// Aucune complexité dans le cas général, mais dans le cas d'un aiguillage il y a une certaine logique
        /// IDEA : dans le cas d'un aiguillage par exemple, la modification de comportement doit se faire via un autre composant
        /// NOTE : ce cas montre l'intérêt des composants enfants comme dans Unreal Engine
        /// </summary>
        public Rail Next
        {
            get
            {
                return NextRails.Count > 0 ? NextRails[_nextRailIndex] : null;
            }
            set
            {
                int index = NextRails.IndexOf(value);
                if (index < 0) // l'objet j'a pas été trouvé
                {
                    NextIndex = 0;
                }
                else
                {
                    NextIndex = index;
                }
            }
        }

        /// <summary>
        /// Renvoie le rail précédent actuellement sélectionné
        /// </summary>
        public Rail Previous
        {
            get
            {
                return PreviousRails.Count > 0 ? PreviousRails[_previousRailIndex] : null;
            }
            set
            {
                int index = PreviousRails.IndexOf(value);
                if (index < 0) // l'objet j'a pas été trouvé
                {
                    PreviousIndex = 0;
                }
                else
                {
                    PreviousIndex = index;
                }
            }
        }

        

        /// <summary>
        /// Index du rail suivant actuellement sélectionné
        /// </summary>
        public int NextIndex
        {
            get
            {
                return _nextRailIndex;
            }
            set
            {
                if (value < NextRails.Count)
                {
                    _nextRailIndex = value;
                }
                else
                {
                    Debug.Log("Rails.NextIndex.set : new index out of range");
                }
            }
        } 

        /// <summary>
        /// Index du rail précédent actuellement sélectionné
        /// </summary>
        public int PreviousIndex
        {
            get
            {
                return _previousRailIndex;
            }
            set
            {
                if (value < PreviousRails.Count)
                {
                    _previousRailIndex = value;
                }
                else
                {
                    Debug.Log("Rails.PreviousIndex.set : new index out of range");
                }
            }
        } 

        #endregion

        #region Private Fields

        // Longueur du rail
        private float _pathLength;

        // Index des rails suivants et précédents actuellement sélectionnés
        private int _nextRailIndex = 0;
        private int _previousRailIndex = 0;

        #endregion

        
        #region Unity Lifecycle Events
        // Start is called before the first frame update
        void Start()
        {
            Math = Curve.GetComponent<BGCcMath>();
            _pathLength = Math.GetDistance();
        }
        #endregion

        #region Public Methods



#if UNITY_EDITOR
        /// <summary>
        /// Ajoute un rail au rail courant
        /// </summary>
        /// <param name="prefab">Prefab du rail à ajouter</param>
        /// <param name="railBoundary">Ajouter à la fin ou au début du rail</param>
        /// <returns></returns>
        public Rail AppendRail(GameObject prefab, RAIL_BOUNDARY railBoundary = RAIL_BOUNDARY.RAIL_END)
        {
            Math = Curve.GetComponent<BGCcMath>();

            Vector3 tangentEnd;
            Vector3 positionEnd;

            if (railBoundary == RAIL_BOUNDARY.RAIL_BEGIN)
            {
                positionEnd = Math.CalcPositionAndTangentByDistanceRatio(0.0f, out tangentEnd);

                tangentEnd *= -1;
            }
            else// if (railBoundary == RAIL_BOUNDARY.RAIL_END)
            {
                positionEnd = Math.CalcPositionAndTangentByDistanceRatio(1.0f, out tangentEnd);
            }


            GameObject newRail = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            //GameObject newRail = Instantiate(prefab);

            newRail.transform.parent = transform.parent;
            newRail.transform.position = positionEnd;
            newRail.transform.rotation = Quaternion.LookRotation(tangentEnd);

            Rail railComponent = newRail.GetComponent<Rail>();
            
            
            railComponent.PreviousRails.Add(GetComponent<Rail>());
            NextRails.Add(railComponent);

            return railComponent;
        }
#endif

        #endregion

        #region Private Methods

        #endregion
    }
}
