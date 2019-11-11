using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections;
using System.Collections.Generic;


namespace WoodRails
{
    /// <summary>
    /// Fenêtre d'édition de circuit
    /// 
    /// Organisation :
    /// - Choix du circuit à éditer √
    /// - Choix du dossier avec les prefabs de rail √
    /// - Modes d'édition (au clic, (glisser-déposer) ou sélection puis ajout) √
    /// 
    /// Modes d'ajout :
    /// - Au clic
    /// - Par sélection puis ajout sur la scène bout à bout avec un autre rail
    /// 
    /// Notes
    /// - Prévoir d'ajouter un rail pas directement à la suite d'un autre rail du circuit ?
    /// 
    /// Pour un pathfollower :
    /// quand on le bouge sur la scène, se rattache automatiquement à un rail proche
    /// </summary>
    public class CircuitWindow : EditorWindow
    {
        #region Private Fields

        //////////////////////////////////// Sélection du circuit //////////////////////////////////
        /// <summary>
        /// Liste des circuits présents dans la scène
        /// </summary>
        private List<Circuit> _sceneCircuits = new List<Circuit>();

        /// <summary>
        /// Liste des noms des circuits de la scène
        /// </summary>
        private List<string> _sceneCircuitsNames = new List<string>();

        /// <summary>
        /// Index du circuit sélectionné
        /// </summary>
        private int _selectedCircuitIndex = 0;

        /// <summary>
        /// Circuit en cours d'édition
        /// Attention : synchronisé avec l'ID, donc pour l'instant on ne peut pas éditer de circuit n'étant pas dans la scène
        /// </summary>
        public Circuit Circuit
        {
            get
            {
                return _sceneCircuits[_selectedCircuitIndex];
            }
            set
            {
                int index = _sceneCircuits.IndexOf(value);
                if (index < 0) // l'objet j'a pas été trouvé
                {
                    _selectedCircuitIndex = 0;
                    Debug.LogError("Le circuit n'a pas été trouvé dans la scène");
                }
                else
                {
                    _selectedCircuitIndex = index;
                }
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////




        //////////////////////////////////// Dossier de prefabs ////////////////////////////////////
        /// <summary>
        /// Chemin du dossier contenant les prefabs de rail
        /// 
        /// Les prefabs doivent tous être orientés de la même façon (avant vers les Z positifs)
        /// </summary>
        private string _prefabsPath = "Assets/";
        ////////////////////////////////////////////////////////////////////////////////////////////
        
        


        //////////////////////////////////// Mode d'ajout //////////////////////////////////////////
        /// <summary>
        /// Modes d'ajout de rail
        /// </summary>
        private string[] _addModes = {"Clic", "In Scene"};

        /// <summary>
        /// Mode d'ajout de rail sélectionné
        /// </summary>
        private int _addModeIndex = 0;
        ////////////////////////////////////////////////////////////////////////////////////////////


        //////////////////////////////////// Palette de rails //////////////////////////////////////
        /// <summary>
        /// Nombre de rails affichés par ligne
        /// </summary>
        private const int _railsPerRow = 5;

        // Palette des rails utilisables par l'éditeur
        [SerializeField]
        private List<GameObject> _railPalette = new List<GameObject>();

        // Icones des rails de la palette
        private List<GUIContent> _railIcons = new List<GUIContent>();

        /// <summary>
        /// Rail sélectionné dans la palette
        /// </summary>
        [SerializeField]
        private int _selectedRailIndex;
        ////////////////////////////////////////////////////////////////////////////////////////////

        

        #endregion





        /// <summary>
        /// Affiche la fenêtre
        /// </summary>
        [MenuItem("Circuit/Open Editor")]
        public static CircuitWindow ShowWindow()
        {
            CircuitWindow window = ScriptableObject.CreateInstance(typeof(CircuitWindow)) as CircuitWindow;
            window.titleContent = new GUIContent("Circuit Editor");
            // La fenêtre reste au premier plan lorsqu'elle perd le focus
            window.ShowUtility();

            window.RefreshCircuitsList();

            window.SetCurrentCircuitFromSelection();

            return window;
        }

        /// <summary>
        /// Affiche la fenêtre avec un circuit préselectionné
        /// </summary>
        /// <param name="circuit">Circuit à éditer</param>
        public static void ShowWindow(Circuit circuit = null)
        {
            CircuitWindow window = CircuitWindow.ShowWindow();

            if (circuit)
            {
                window.Circuit = circuit;
            }
        }

        /// <summary>
        /// Exécuté lors de lu focus de la fenêtre
        /// </summary>
        public void OnEnable()
        {
            // Each editor window contains a root VisualElement object
            /*VisualElement root = rootVisualElement;

            // VisualElements objects can contain other VisualElement following a tree hierarchy.
            VisualElement label = new Label("Hello World! From C#");
            root.Add(label);*/

            RefreshPalette();
        }

        /// <summary>
        /// Affiche le contenu de la fenêtre
        /// </summary>
        private void OnGUI()
        {
            // Sélection du circuit à éditer
            GUILayout.Label("Circuit à éditer :");
            _selectedCircuitIndex = EditorGUILayout.Popup(_selectedCircuitIndex, _sceneCircuitsNames.ToArray());

            GUILayout.Space(20f);
            
            // sélectionner dossier de prefabs
            GUILayout.Label("Dossier de prefabs :");
            GUILayout.BeginHorizontal();
            GUILayout.Label(_prefabsPath);
            if (GUILayout.Button("Sélectionner..."))
            {
                string path = EditorUtility.OpenFolderPanel("Prefabs Directory", _prefabsPath, "");

                int assetsStringIndex = path.IndexOf("Assets/");

                if (assetsStringIndex != -1)
                {
                    _prefabsPath = path.Substring(assetsStringIndex);

                    RefreshPalette();
                }
                else
                {
                    Debug.LogError("Le dossier sélectionné doit être dans le dossier Assets !");
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(20f);

            // Mode d'ajout de prefabs

            _addModeIndex = GUILayout.SelectionGrid(_addModeIndex, _addModes, 2);



            //GUILayout.Space(20f);
            
            // Palette de rails
            if (_addModeIndex == 0) // Clic
            {
                int selection = GUILayout.SelectionGrid(-1, _railIcons.ToArray(), _railsPerRow);

                if (selection != -1)
                {
                    AddRail(selection);
                }
            }
            else if(_addModeIndex == 1) // in scene
            {
                _selectedRailIndex = GUILayout.SelectionGrid(_selectedRailIndex, _railIcons.ToArray(), _railsPerRow);
            }
        }


        /// <summary>
        /// Définit la circuit en train d'être édité en fonction de la sélection.
        /// 
        /// Si la sélection est un Circuit, il le prend comme circuit actuel.
        /// Si la sélection est un Rail, il prend son circuit parent comme circuit actuel.
        /// 
        /// Sinon, Circuit reste null
        /// </summary>
        public void SetCurrentCircuitFromSelection()
        {
            GameObject selection = Selection.activeGameObject;

            if (selection != null)
            {
                if (selection.GetComponent<Circuit>() != null)
                {
                    Circuit = selection.GetComponent<Circuit>();
                }
                else if (selection.GetComponent<Rail>() != null)
                {
                    Circuit parentCircuit = selection.GetComponentInParent<Circuit>();

                    if (parentCircuit != null)
                    {
                        Circuit = parentCircuit;
                    }
                }
            }
        }


        /// <summary>
        /// Ajoute un rail et le définit comme sélection actuelle
        /// </summary>
        /// <param name="railIndex">Index du prefab à ajouter, depuis la palette de rails</param>
        private void AddRail(int railIndex)
        {
            // Récupération de la sélection actuelle
            GameObject selected = Selection.activeGameObject;
            Rail selectedRail = null;

            if (selected)
            {
                selectedRail = selected.GetComponent<Rail>();
                // null si l'objet sélectionné n'est pas un rail
            }

            Rail newRail = Circuit.AddRail(_railPalette[railIndex], selectedRail);

            GameObject[] newSelection = {newRail.gameObject};
            Selection.objects = newSelection;

            Undo.RegisterCreatedObjectUndo(newRail.gameObject, "Add Rail");

            //////////////////////////
            // Penser à vérifier la fermeture automatique du circuit
            //////////////////////////
        }


        /// <summary>
        /// Met à jour la liste des circuits de la scène
        /// Cette méthode étant lente, il faut l'appeler le moins possible
        /// </summary>
        private void RefreshCircuitsList()
        {
            _sceneCircuits.Clear();

            var foundObjects = FindObjectsOfType<Circuit>();

            foreach (Object obj in foundObjects)
            {
                Circuit circuit = obj as Circuit;
                _sceneCircuits.Add(circuit);
                _sceneCircuitsNames.Add(obj.name);
            }
        }

        /// <summary>
        /// Met à jour le contenu de la palette de rails
        /// </summary>
        /// <param name="recursive">Recherche dans les sous-dossiers ou non</param>
        private void RefreshPalette(bool recursive = true)
        {
            _railPalette.Clear();

            System.IO.SearchOption searchOption = (recursive) ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly;

            string[] railFiles = System.IO.Directory.GetFiles(_prefabsPath, "*.prefab", searchOption);

            foreach (string railPath in railFiles)
            {
                GameObject rail = AssetDatabase.LoadAssetAtPath(railPath, typeof(GameObject)) as GameObject;


                if (rail.GetComponent<Rail>() != null)
                {
                    _railPalette.Add(rail);
                }
            }

            // Après rafraichissement des rails, il est nécessaire de changer les icones
            ReloadIcons();
        }

        /// <summary>
        /// Recharge les icones de la palette de rails
        /// </summary>
        private void ReloadIcons()
        {
            _railIcons.Clear();

            foreach (GameObject rail in _railPalette)
            {
                Texture2D texture = AssetPreview.GetAssetPreview(rail);

                GUIContent buttonImage = (texture) ? new GUIContent(texture) : new GUIContent(rail.name);

                buttonImage.tooltip = rail.name;
                // ecrire nom
                _railIcons.Add(buttonImage);
            }
        }
    }
}