using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WoodRails
{
    public class Circuit : MonoBehaviour
    {
        #region Properties

        /// <summary>
        /// Nombre de rails dans le circuit
        /// </summary>
        public int RailsCount => transform.childCount;

        #endregion

        #region Public Methods

        /// <summary>
        /// Ajoute un rail au circuit
        /// TODO : éviter la redondance toRail = anchor.Parent
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="toRail"></param>
        /// <param name="anchor"></param>
        /// <returns></returns>
        public Rail AddRail(Rail prefab, Rail toRail = null, RailAnchor anchor = null)
        {
            // Vérification si on ajoute à un rail existant
            if (toRail != null && anchor != null)
            {
                // Si le rail possède déjà un rail à cet endroit
                if (!toRail.CanAppendRail(anchor))
                {
                    return null;
                }
            }

            Rail newRail;

#if UNITY_EDITOR
            newRail = PrefabUtility.InstantiatePrefab(prefab) as Rail; // TODO tester le cast

#else
            newRail = Instantiate(prefab);
#endif

            // Créé à la suite d'un rail existant
            if (toRail)
            {
                toRail.AppendRail(newRail, anchor);
            }
            // Créé au centre du circuit (par défaut)
            else
            {
                newRail.transform.parent = transform;
                newRail.transform.localPosition = Vector3.zero;
            }


            return newRail;
        }

        /// <summary>
        /// Ajoute un rail au circuit
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="toRail"></param>
        /// <param name="railBoundary"></param>
        /// <param name="pathIndex"></param>
        /// <returns></returns>
        public Rail AddRail(Rail prefab, Rail toRail = null, RAIL_BOUNDARY railBoundary = RAIL_BOUNDARY.RAIL_END, int pathIndex = -1)
        {
            return AddRail(prefab, toRail, new Rail.Endpoint(railBoundary, pathIndex));
        }

        /// <summary>
        /// Ajoute un rail au circuit
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="toRail"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public Rail AddRail(Rail prefab, Rail toRail, Rail.Endpoint point)
        {
            if (toRail != null)
            {
                RailAnchor anchor = toRail.GetAnchorForEndpoint(point);

                if (anchor != null)
                {
                    return AddRail(prefab, toRail, anchor);
                }
            }

            return AddRail(prefab, null, null);
        }

        /// <summary>
        /// Vérifie les connections des rails du circuit
        /// </summary>
        public void CheckConnections()
        {
            foreach (Rail rail in GetRails())
            {
                rail.CheckConnections();
            }
        }

        /// <summary>
        /// Renvoie la liste des rails du circuit
        /// </summary>
        /// <returns></returns>
        public Rail[] GetRails()
        {
            return GetComponentsInChildren<Rail>();
        }

        #endregion
    }
}