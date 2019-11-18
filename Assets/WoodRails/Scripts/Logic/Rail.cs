using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BansheeGz.BGSpline.Components;
using BansheeGz.BGSpline.Curve;

namespace WoodRails
{
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
        /// Nombre maximum de rails suivants possibles
        /// </summary>
        public int MaxNextRails = 1;

        /// <summary>
        /// Nombre maximum de rails précédents possibles
        /// </summary>
        public int MaxPreviousRails = 1;

        /// <summary>
        /// Courbes de Bézier rattachées au rail
        /// Ne doit contenir qu'un élément, sauf dans le cas d'aiguillages etc
        /// </summary>
        public BGCurve[] Curves = {};
        
        #endregion

        #region Private Fields

        // Index de la courbe courrante
        private int _currentCurveIndex = 0;

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


        /// <summary>
        /// Composant BGCurve rattaché au rail actuel, définissant la courbe du rail
        /// </summary>
        public BGCurve Curve
        {
            get
            {
                return Curves.Length > 0 ? Curves[_currentCurveIndex] : null;
            }
            set
            {
                int index = System.Array.IndexOf(Curves, value);;
                if (index < 0) // l'objet j'a pas été trouvé
                {
                    CurveIndex = 0;
                }
                else
                {
                    CurveIndex = index;
                }
            }
        }

        /// <summary>
        /// Index de la curve actuellement sélectionnée
        /// </summary>
        public int CurveIndex
        {
            get
            {
                return _currentCurveIndex;
            }
            set
            {
                if (value < Curves.Length)
                {
                    _currentCurveIndex = value;
                    _pathLength = Math.GetDistance();
                }
                else
                {
                    Debug.Log("Rails.CurveIndex.set : new index out of range");
                }
            }
        }

        /// <summary>
        /// Composant BGCcMath rattaché au rail actuel, permettant des opérations mathématiques sur la courbe
        /// du rail.
        /// </summary>
        [HideInInspector]
        public BGCcMath Math
        {
            get
            {
                // Optimisable
                return Curve.GetComponent<BGCcMath>();
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
            _pathLength = Math.GetDistance();
        }
        #endregion


        #region Public Methods

        /// <summary>
        /// Ajoute un rail suivant, si possible
        /// </summary>
        /// <param name="newRail">Rail à ajouter</param>
        /// <returns></returns>
        public bool AddNextRail(Rail newRail)
        {
            if (NextRails.Count < MaxNextRails)
            {
                NextRails.Add(newRail);

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Ajoute un rail précédent, si possible
        /// </summary>
        /// <param name="newRail">Rail à ajouter</param>
        /// <returns></returns>
        public bool AddPreviousRail(Rail newRail)
        {
            if (PreviousRails.Count < MaxPreviousRails)
            {
                PreviousRails.Add(newRail);

                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion
    }
}
