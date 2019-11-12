using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WoodRails
{
    public class Circuit : MonoBehaviour
    {
        #region Public Fields




        #endregion

        #region Private Fields


#if UNITY_EDITOR
        /// <summary>
        /// Rails contenus dans le circuit
        /// </summary>
        private List<Rail> _rails = new List<Rail>();


        /// <summary>
        /// Rail actuel
        /// </summary>
        private Rail _currentRail;
#endif

        #endregion

        #region Unity Lifecycle Events

        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        
#if UNITY_EDITOR
        public Rail AddRail(GameObject prefab, Rail afterRail = null)
        {
            Rail prevRail = (afterRail) ? afterRail : _currentRail;

            if (prevRail != null)
            {
                _currentRail = prevRail.AppendRail(prefab);
            }
            else
            {
                GameObject newRail = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                //GameObject newRail = Instantiate(prefab);

                newRail.transform.parent = transform;
                newRail.transform.position = transform.position;

                Rail railComponent = newRail.GetComponent<Rail>();

                _rails.Add(railComponent);
                _currentRail = railComponent;
            }

            return _currentRail;
        }
#endif

        #endregion
    }
}