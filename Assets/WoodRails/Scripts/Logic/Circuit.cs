using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WoodRails
{
    public class Circuit : MonoBehaviour
    {
        #region Public Fields




        #endregion

        #region Private Fields

        /// <summary>
        /// Rails contenus dans le circuit
        /// </summary>
        private List<Rail> _rails = new List<Rail>();


        /// <summary>
        /// Rail actuel
        /// </summary>
        private Rail _currentRail;

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
                GameObject newRail = Instantiate(prefab, transform);

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