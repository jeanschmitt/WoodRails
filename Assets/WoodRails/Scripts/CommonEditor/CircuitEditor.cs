using System.Collections.Generic;
using UnityEngine;

namespace WoodRails
{
    public class CircuitEditor
    {
        #region Fields & Properties

        private readonly bool _isStandalone;

        public static int railLayer = 16;

        // Circuits
        public Circuit Circuit { get; set; }

        public bool HasCircuit { get => Circuit != null; }
        //

        [HideInInspector]
        public Rail Selection;

        //////////////////////////////////// Mode d'ajout //////////////////////////////////////////
        public AddingMode AddMode { get; set; } = AddingMode.Click;
        ////////////////////////////////////////////////////////////////////////////////////////////


        //////////////////////////////////// Palette de rails //////////////////////////////////////
        // Palette des rails utilisables par l'éditeur
        [SerializeField]
        public List<Rail> RailPalette { get; set; }

        /// <summary>
        /// Index du rail sélectionné dans la palette
        /// </summary>
        [SerializeField]
        public int SelectedRailIndex
        {
            get => _selectedRailIndex;
            set
            {
                int oldSelection = _selectedRailIndex;
                
                _selectedRailIndex = value;

                HandleRailSelection(oldSelection);
            }
        }
        private int _selectedRailIndex;

        /// <summary>
        /// Rail sélectionné dans la palette
        /// </summary>
        public Rail SelectedRail => RailPalette[SelectedRailIndex];
        ////////////////////////////////////////////////////////////////////////////////////////////

        #endregion

        #region Enums

        public enum AddingMode
        {
            Click,
            Scene
        }

        #endregion

        #region

        public CircuitEditor(bool isStandalone = false)
        {
            _isStandalone = isStandalone;
        }

        /// <summary>
        /// Ajoute un rail
        /// </summary>
        /// <param name="toRail"></param>
        /// <param name="anchor"></param>
        /// <returns></returns>
        public Rail AddRail(Rail toRail = null, RailAnchor anchor = null)
        {
            if (Circuit == null)
            {
                Debug.LogError("Circuit non créé");
                return null;
            }

            Rail newRail = Circuit.AddRail(SelectedRail, toRail, anchor);

            if (!newRail)
            {
                Debug.LogError("Impossible d'ajouter un rail à cet endroit");

                return null;
            }

            // En standalone, ajoute un boxcollider pour la sélection
            if (_isStandalone)
            {
                Bounds railBounds = newRail.GetBounds();

                BoxCollider railCollider = newRail.gameObject.AddComponent<BoxCollider>();

                railCollider.center = newRail.transform.InverseTransformPoint(railBounds.center);
                railCollider.size = railBounds.size;

                newRail.gameObject.layer = railLayer;
            }

            Selection = newRail;

            // TODO undo system
            //Undo.RegisterCreatedObjectUndo(newRail, "Add Rail");

            return newRail;
        }

        /// <summary>
        /// Ajoute un rail
        /// </summary>
        /// <param name="toRail"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public Rail AddRail(Rail toRail, Rail.Endpoint point)
        {
            if (toRail != null)
            {
                RailAnchor anchor = toRail.GetAnchorForEndpoint(point);

                if (anchor != null)
                {
                    return AddRail(toRail, anchor);
                }
            }

            return AddRail();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Action appelée lorsqu'un rail de la palette est sélectionné
        /// </summary>
        /// <param name="oldSelection"></param>
        private void HandleRailSelection(int oldSelection)
        {
            if (AddMode == AddingMode.Click)
            {
                // TODO : Ajout au début ou a la fin du rail
                AddRail(Selection, Rail.Endpoint.Default);
            }
            else if (AddMode == AddingMode.Scene)
            {
                // La sélection a changé
                if (oldSelection != _selectedRailIndex)
                {

                }
            }
        }

        #endregion
    }
}