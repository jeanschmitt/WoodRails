﻿using UnityEngine;


namespace WoodRails
{
    public class PathFollower : MonoBehaviour
    {
        #region Public Fields

        /// <summary>
        /// Vitesse de l'objet circulant sur un rail
        /// Unité par défaut de Unity (m/s)
        /// </summary>
        public float Speed = 0.5f;

        #endregion

        #region Properties

        /// <summary>
        /// Rail sur lequel est rattaché l'objet
        /// </summary>
        public Rail AttachedRail
        {
            get
            {
                return _attachedRail;
            }
            set
            {
                _attachedRail = value;
                _currentRailLength = value.PathLength;
            }
        }

        #endregion

        #region Private Fields

        // Rail actuellement rattaché
        [SerializeField]
        private Rail _attachedRail;

        // Distance est la position de l'élément par rapport au rail attaché
        private float _distance;

        // Taille du rail actuellement rattaché
        private float _currentRailLength;

        #endregion



        #region Unity Lifecycle Events

        // Start is called before the first frame update
        void Start()
        {
            if (_attachedRail)
            {
                _currentRailLength = _attachedRail.PathLength;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (_attachedRail)
            {
                _distance += Speed * Time.deltaTime;

                CheckRailEnd();

                // Mise à jour de la position
                Vector3 tangent;

                transform.position = _attachedRail.Math.CalcPositionAndTangentByDistance(_distance, out tangent);

                transform.rotation = Quaternion.LookRotation(tangent);
            }
        }

        #endregion

        #region Private Methods

        // Vérifie si on a atteint le bout du rail, et dans ce cas positionne l'objet sur le rail suivant
        private void CheckRailEnd()
        {
            if (_distance >= _currentRailLength && Speed > 0)
            {
                if (_attachedRail.NextRail) // Changement de rail
                {
                    _distance -= _currentRailLength;
                    ChangeRail(_attachedRail.NextRail);
                }
                else if (_attachedRail.Curve.Closed) // Bouclage du rail
                {
                    _distance -= _currentRailLength;
                }
            }
            else if (_distance <= 0f && Speed < 0) {
                if (_attachedRail.PreviousRail) // Changement de rail
                {
                    ChangeRail(_attachedRail.PreviousRail);
                    _distance += _currentRailLength;
                }
                else if (_attachedRail.Curve.Closed) // Bouclage du rail
                {
                    _distance += _currentRailLength;
                }
            }
        }

        // Changement de rail
        private void ChangeRail(Rail newRail)
        {
            AttachedRail = newRail;
        }

        #endregion
    }
}