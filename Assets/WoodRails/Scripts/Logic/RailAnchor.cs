using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WoodRails
{
    [ExecuteInEditMode]
    public class RailAnchor : MonoBehaviour
    {
        #region Fields

        /// <summary>
        /// Rail auquel le RailAnchor est attaché
        /// </summary>
        [HideInInspector]
        public Rail Parent;

        /// <summary>
        /// RailAnchor connecté au RailAnchor actuel
        /// </summary>
        [SerializeField]
        public RailAnchor Connection;

        /// <summary>
        /// True si le RailAnchor est en fin de rail
        /// à remplacer par un enum + custom inspector
        /// </summary>
        public bool IsEnd = true;

        #endregion


        #region Properties

        /// <summary>
        /// Rail connecté
        /// </summary>
        public Rail ConnectedRail => Connection.Parent;

        /// <summary>
        /// Position du RailAnchor
        /// </summary>
        public Vector3 Position => transform.position; // world

        /// <summary>
        /// Rotation du RailAnchor
        /// </summary>
        public Quaternion Rotation => transform.rotation;

        /// <summary>
        /// Rnvoie si le RailAnchor est connecté ou pas
        /// </summary>
        public bool Free => Connection == null;


        #endregion


        #region Unity Lifecycle Events

        // Start is called before the first frame update
        void Start()
        {
            Parent = GetComponentInParent<Rail>();

            if (Parent == null)
            {
                Debug.LogError("Le parent d'un RailConnection doit être un rail");
            }
        }

        #endregion


        #region Public Methods

        /// <summary>
        /// Connecte le RailAnchor à un autre
        /// </summary>
        /// <param name="withAnchor"></param>
        /// <param name="notifyOther"></param>
        public void Connect(RailAnchor withAnchor, bool notifyOther = true)
        {
            if (notifyOther)
            {
                withAnchor.NotifyConnection(this);
            }

#if UNITY_EDITOR
            // https://answers.unity.com/questions/155370/edit-an-object-in-unityeditor-editorwindow.html
            // Nécessaire d'utiliser SerializedObject pour conserver la valeur après un play
            var serializedAnchor = new SerializedObject(this);
            var connectionProp = serializedAnchor.FindProperty("Connection");

            connectionProp.objectReferenceValue = withAnchor;

            serializedAnchor.ApplyModifiedProperties();
#else
            Connection = withAnchor;
#endif
        }

        /// <summary>
        /// Réplique la connection sur l'autre RailAnchor
        /// </summary>
        /// <param name="withAnchor"></param>
        public void NotifyConnection(RailAnchor withAnchor)
        {
            Connect(withAnchor, false);
        }

        #endregion
    }
}