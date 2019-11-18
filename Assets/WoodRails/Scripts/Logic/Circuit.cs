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


#if UNITY_EDITOR
        /// <summary>
        /// Rails contenus dans le circuit
        /// </summary>
        public List<Rail> Rails = new List<Rail>();
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


        #endregion
    }
}