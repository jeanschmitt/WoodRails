﻿using System;
using UnityEngine;
using BansheeGz.BGSpline.Components;
using BansheeGz.BGSpline.Curve;

namespace WoodRails
{
    [RequireComponent(typeof(BGCurve))]
    [RequireComponent(typeof(BGCcMath))]
    public class Rail : MonoBehaviour
    {
        #region Public Fields
        

        /// <summary>
        /// Liste des rails suivant le rail actuel
        /// À part pour des aiguillages et autres, cette liste ne contient qu'un et un seul élément
        /// </summary>
        public Rail[] NextRails;

        /// <summary>
        /// Liste des rails précédant le rail actuel
        /// À part pour des aiguillages et autres, cette liste ne contient qu'un et un seul élément
        /// </summary>
        public Rail[] PreviousRails;

        /// <summary>
        /// Composant BGCurve rattaché au rail actuel, définissant la courbe du rail
        /// </summary>
        [HideInInspector]
        public BGCurve Curve;
        
        /// <summary>
        /// Composant BGCcMath rattaché au rail actuel, permettant des opérations mathématiques sur la courbe
        /// du rail.
        /// </summary>
        [HideInInspector]
        public BGCcMath Math;

        

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
                return NextRails.Length > 0 ? NextRails[_nextRailIndex] : null;
            }
            set
            {
                int index = Array.IndexOf(NextRails, value);
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
                return PreviousRails.Length > 0 ? PreviousRails[_previousRailIndex] : null;
            }
            set
            {
                int index = Array.IndexOf(PreviousRails, value);
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
                if (value < NextRails.Length)
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
                if (value < PreviousRails.Length)
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
            Curve = GetComponent<BGCurve>();
            Math = GetComponent<BGCcMath>();
            _pathLength = Math.GetDistance();
        }
        #endregion

        #region Private Methods

        #endregion
    }
}