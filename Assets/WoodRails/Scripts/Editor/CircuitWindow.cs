using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections;
using System.Collections.Generic;
using BansheeGz.BGSpline.Components;
using BansheeGz.BGSpline.Curve;


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
        /// Si cet attribut est modifié directement, il faut mettre à jour _serializedCircuit
        /// </summary>
        private int _selectedCircuitIndex = 0;

        /// <summary>
        /// SerializedObject du Circuit actuel, nécessaire pour le modifier dans l'éditeur
        /// </summary>
        private SerializedObject _serializedCircuit;

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

                _serializedCircuit = new SerializedObject(_sceneCircuits[_selectedCircuitIndex]);
            }
        }


        /// <summary>
        /// Extrémité du rail
        /// </summary>
        public enum RAIL_BOUNDARY
        {
            RAIL_END,
            RAIL_BEGIN
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

            // Mise à jour de l'objet sérialisé
            _serializedCircuit = new SerializedObject(Circuit);

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

            GameObject newRail = AppendRail(_railPalette[railIndex], selectedRail);
            Rail newRailComp = newRail.GetComponent<Rail>();

            // Met à jour la version sérialisée du circuit
            _serializedCircuit.Update();

            CheckRailConnections(newRailComp);

            // Ajoute le nouveau rail à la liste des rails du circuit

             // pas besoin !!! chercher parmi les enfants
            var circuitRails = _serializedCircuit.FindProperty("Rails");
            int newRailIndex = circuitRails.arraySize++;
            circuitRails.GetArrayElementAtIndex(newRailIndex).objectReferenceValue = newRailComp;
            _serializedCircuit.ApplyModifiedProperties();

            // Sélectionne le nouveau rail
            GameObject[] newSelection = { newRail };
            Selection.objects = newSelection;

            Undo.RegisterCreatedObjectUndo(newRail, "Add Rail");
        }


        /// <summary>
        /// Ajoute un rail à la suite d'un autre
        /// 
        /// Note : empêcher l'ajout de rail à un rail déjà plein
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="toRail"></param>
        /// <param name="railBoundary"></param>
        /// <returns></returns>
        private GameObject AppendRail(GameObject prefab, Rail toRail = null, RAIL_BOUNDARY railBoundary = RAIL_BOUNDARY.RAIL_END)
        {
            GameObject newRail = PrefabUtility.InstantiatePrefab(prefab) as GameObject;


            // Créé après un rail existant
            if (toRail != null)
            {
                BGCcMath math = toRail.Curve.GetComponent<BGCcMath>();

                Vector3 tangentEnd;
                Vector3 positionEnd;

                // Placement au début ou à la fin du rail
                if (railBoundary == RAIL_BOUNDARY.RAIL_BEGIN)
                {
                    positionEnd = math.CalcPositionAndTangentByDistanceRatio(0.0f, out tangentEnd);

                    tangentEnd *= -1;
                }
                else// if (railBoundary == RAIL_BOUNDARY.RAIL_END)
                {
                    positionEnd = math.CalcPositionAndTangentByDistanceRatio(1.0f, out tangentEnd);
                }

                // Affectation du parent, position, et rotation
                newRail.transform.parent = toRail.transform.parent;
                newRail.transform.position = positionEnd;
                newRail.transform.rotation = Quaternion.LookRotation(tangentEnd);


                Rail newRailComp = newRail.GetComponent<Rail>();
                

                // Ajout dans les tableaux de rail suivant et précédent
                newRailComp.PreviousRails.Add(toRail);

                // https://answers.unity.com/questions/155370/edit-an-object-in-unityeditor-editorwindow.html
                // Nécessaire d'utiliser SerializedObject pour conserver la valeur après un play
                // Mais pas dans la ligne précédente apparement
                var serializedToRail = new SerializedObject(toRail);
                var nextRailsProp = serializedToRail.FindProperty("NextRails");

                int nextRailsSize = nextRailsProp.arraySize++;
                nextRailsProp.GetArrayElementAtIndex(nextRailsSize).objectReferenceValue = newRailComp;

                serializedToRail.ApplyModifiedProperties();
                //
            }
            // Créé à la racine du circuit édité
            else
            {
                newRail.transform.parent = Circuit.transform;
                newRail.transform.position = Circuit.transform.position;
            }

            return newRail;
        }

        /// <summary>
        /// Vérifie si le rail ajouté peut être connecté à un autre rail déjà posé
        /// </summary>
        /// <param name="rail">Rail à vérifier</param>
        private void CheckRailConnections(Rail rail)
        {
            // _serializedCircuit.Update() est déjà appelé plus tôt

            foreach (var curve in rail.Curves)
            {

            }

            var rails = _serializedCircuit.FindProperty("Rails");
            // non ! chercher parmi les enfants du circuit

            for (int i = 0; i < rails.arraySize; i++)
            {
                Rail r = rails.GetArrayElementAtIndex(i).objectReferenceValue as Rail; // à vérifier


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