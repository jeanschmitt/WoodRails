using UnityEditor;
using UnityEngine;
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
    /// - Au clic √
    /// - Par sélection puis ajout sur la scène bout à bout avec un autre rail √
    /// 
    /// Notes
    /// - Prévoir d'ajouter un rail pas directement à la suite d'un autre rail du circuit ?
    /// 
    /// Pour un pathfollower :
    /// quand on le bouge sur la scène, se rattache automatiquement à un rail proche
    /// </summary>
    public class CircuitWindow : EditorWindow
    {
        #region Attributes & Properties

        /// <summary>
        /// GameObject parent des futurs GameObjects créés uniquement pour l'éditeur
        /// </summary>
        private GameObject _editorGameObject;

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
        /// Si cet attribut est modifié directement, il faut mettre à jour _serializedCircuit
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

        /// <summary>
        /// Mémorise le dernier tool utilisé (transform, rotate, scale, ...)
        /// Lors de l'entrée en mode In Scene
        /// </summary>
        private Tool _lastTool;



        //                          \\
        // Mode d'ajout "In Scene"  \\
        //                          \\

        /// <summary>
        /// Distance de snap des rails, en unité de l'écran GUI (et non de la scène 3D)
        /// </summary>
        private const float _maxDistanceSnap = 30.0f;

        /// <summary>
        /// RailAnchor actuellement sélectionné (souris au dessus)
        /// </summary>
        private RailAnchor _focusedAnchor;

        /// <summary>
        /// Handle de l'indicateur de rail à créer
        /// </summary>
        private GameObject _railHandle;

        /// <summary>
        /// Consomme le prochain MouseUp
        /// Utilisé après un MouseDown réussi
        /// </summary>
        private bool _consumeNextMouseUp = false;
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
        [MenuItem("Circuit/Ouvrir l'éditeur")]
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
        public static void ShowWindow(Circuit circuit)
        {
            CircuitWindow window = CircuitWindow.ShowWindow();

            if (circuit)
            {
                window.Circuit = circuit;
            }
        }

        /*
        /// <summary>
        /// Affiche la fenêtre depuis le menu contextuel d'un circuit
        /// </summary>
        /// <param name="command">Commande de menu</param>
        [MenuItem("CONTEXT/Circuit/Ouvrir l'éditeur")]
        public static void ShowWindow(MenuCommand command)
        {
            Circuit circuit = (Circuit)command.context;

            ShowWindow(circuit);
        }*/

        /// <summary>
        /// Exécuté lors de lu focus de la fenêtre
        /// </summary>
        public void OnEnable()
        {
            if (!_editorGameObject)
            {
                // Pas de Instantiate() pour un GameObject Empty
                _editorGameObject = new GameObject
                {
                    name = "_circuitEditor"
                };
            }

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

                int assetsStringIndex = path.IndexOf("Assets/", System.StringComparison.Ordinal);

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

            int previousAddModeIndex = _addModeIndex;
            _addModeIndex = GUILayout.SelectionGrid(_addModeIndex, _addModes, 2);

            // Gestion des évènements dans la SceneView
            if (_addModeIndex != previousAddModeIndex)
            {
                if (_addModeIndex == 1)
                {
                    EnterSceneEditing();
                }
                else
                {
                    ExitSceneEditing();
                }
            }
            




            //GUILayout.Space(20f);
            
            // Palette de rails
            if (_addModeIndex == 0) // Clic
            {
                int selection = GUILayout.SelectionGrid(-1, _railIcons.ToArray(), _railsPerRow);

                if (selection != -1)
                {
                    // Ajout d'un rail via la palette de sélection
                    AddRail(selection);
                }
            }
            else if(_addModeIndex == 1) // in scene
            {
                int previousSelectedRail = _selectedRailIndex;
                _selectedRailIndex = GUILayout.SelectionGrid(_selectedRailIndex, _railIcons.ToArray(), _railsPerRow);

                // Changement dans la sélection
                if (previousSelectedRail != _selectedRailIndex)
                {
                    DestroyRailHandle();
                    SceneView.lastActiveSceneView.Focus();
                }
            }

            if (GUILayout.Button("Check connections"))
            {
                Circuit.CheckConnections();
            }
        }

        /// <summary>
        /// Entre dans le mode édition depuis la SceneView
        /// Désactive l'outil actuellement sélectionné et focus la SceneView
        /// </summary>
        private void EnterSceneEditing()
        {
            SceneView.duringSceneGui += OnSceneGUI;
            SceneView.lastActiveSceneView.Focus();

            _lastTool = Tools.current;
            Tools.current = Tool.None;
        }

        /// <summary>
        /// Quitte le mode édition depuis la SceneView
        /// Rétablit l'outil précédement sélectionné et détruit l'éventuel Gismo de rail
        /// </summary>
        private void ExitSceneEditing()
        {
            SceneView.duringSceneGui -= OnSceneGUI;

            DestroyRailHandle();

            _focusedAnchor = null;

            Tools.current = _lastTool;
        }


        /// <summary>
        /// Gestion des évènements dans la SceneView
        /// </summary>
        /// <param name="sceneView">SceneView courante</param>
        private void OnSceneGUI(SceneView sceneView)
        {
            if (_addModeIndex == 1)
            {
                if (Event.current.type == EventType.MouseMove)
                {
                    // -----------------------------------------------------------------------------------
                    // Détection de la proximité du curseur avec une extrémité de rail libre
                    float lowestDistance = _maxDistanceSnap;

                    RailAnchor previouslySelectedAnchor = _focusedAnchor;

                    _focusedAnchor = null;

                    
                    foreach (Rail rail in Circuit.GetRails())
                    {
                        foreach (RailAnchor anchor in rail.GetFreeConnections())
                        {
                            Vector2 guiPosition = HandleUtility.WorldToGUIPoint(anchor.Position);

                            float distance = Vector2.Distance(Event.current.mousePosition, guiPosition);

                            if (distance < lowestDistance)
                            {
                                _focusedAnchor = anchor;
                                lowestDistance = distance;
                            }
                        }
                    }
                    // -----------------------------------------------------------------------------------



                    // La sélection a changé
                    if (previouslySelectedAnchor != _focusedAnchor)
                    {
                        // Changement de la sélection d'anchor
                        if (_focusedAnchor != null)
                        {
                            if (_railHandle == null)
                            {
                                _railHandle = Instantiate(_railPalette[_selectedRailIndex]);
                            }

                            // Placement de ce rail selon l'anchor
                            _railHandle.transform.parent = _editorGameObject.transform;

                            _railHandle.GetComponent<Rail>().SetTransformFromAnchor(_focusedAnchor);
                        }
                        // On est sorti de la zone de sqélection, suppression du handle
                        else
                        {
                            DestroyRailHandle();
                        }
                    }

                    

                    // Consomme l'Event
                    Event.current.Use();
                }
                else if (Event.current.type == EventType.MouseDown)
                {
                    // Un rail est prêt à être placé
                    if (_railHandle && _focusedAnchor)
                    {
                        // à bouger -------
                        GameObject newRail = AddRail(_selectedRailIndex, _focusedAnchor.Parent, _focusedAnchor);

                        if (newRail)
                        {
                            // Consomme l'Event
                            Event.current.Use();
                            _consumeNextMouseUp = true;

                            DestroyRailHandle();
                        }
                    }
                }
                else if (Event.current.type == EventType.MouseUp)
                {
                    if (_consumeNextMouseUp)
                    {
                        // Consomme l'Event
                        Event.current.Use();
                        
                        _consumeNextMouseUp = false;
                    }
                }
            }
        }

        /// <summary>
        /// Détruit le gizmo de prévisualisation d'ajout de rail
        /// </summary>
        private void DestroyRailHandle()
        {
            if (_railHandle != null)
            {
                DestroyImmediate(_railHandle);
                _railHandle = null;
            }
        }

        /// <summary>
        /// Exécuté lorsque la fenêtre est détruite
        /// </summary>
        private void OnDestroy()
        {
            ExitSceneEditing();

            if (_editorGameObject)
            {
                DestroyImmediate(_editorGameObject);
                _editorGameObject = null;
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
        private GameObject AddRail(int railIndex)
        {
            // Récupération de la sélection actuelle
            GameObject selected = Selection.activeGameObject;
            Rail selectedRail = null;

            if (selected)
            {
                selectedRail = selected.GetComponent<Rail>();
                // null si l'objet sélectionné n'est pas un rail

                if (!selectedRail)
                {
                    // Recherche parmi les parents
                    selectedRail = selected.GetComponentInParent<Rail>();
                }

                if (selectedRail != null)
                {
                    // Can be null
                    RailAnchor anchor = selectedRail.GetAnchorForEndpoint(Rail.Endpoint.Default);

                    return AddRail(railIndex, selectedRail, anchor);
                }
            }

            return AddRail(railIndex, null, null);
        }


        /// <summary>
        /// Ajoute un rail et le définit comme sélection actuelle
        /// </summary>
        /// <param name="railIndex"></param>
        /// <param name="toRail"></param>
        /// <param name="anchor"></param>
        /// <returns></returns>
        public GameObject AddRail(int railIndex, Rail toRail, RailAnchor anchor)
        {
            // TODO : changer le type
            GameObject newRail = Circuit.AddRail(_railPalette[railIndex].GetComponent<Rail>(), toRail, anchor).gameObject;

            if (newRail)
            {
                // Sélectionne le nouveau rail
                GameObject[] newSelection = { newRail };
                Selection.objects = newSelection;

                Undo.RegisterCreatedObjectUndo(newRail, "Add Rail");

                return newRail;
            }
            else
            {
                Debug.LogError("Impossible d'ajouter un rail à cet endroit");

                return null;
            }
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