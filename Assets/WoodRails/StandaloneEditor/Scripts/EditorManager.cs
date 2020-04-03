using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace WoodRails
{
    /// <summary>
    /// IDEA :
    /// Dans le fichier enregistré, il y a une liste des rails utilisés
    /// Faire le lien lors de l'importation dans Unity, avec proposition de
    /// lier les rails utilisés avec les prefabs du même nom déjà importés
    /// </summary>
    public class EditorManager : MonoBehaviour
    {
        #region Fields

        /// <summary>
        /// Caméra de la scène
        /// </summary>
        public EditorCamera editorCamera;

        /// <summary>
        /// Contrôleur de la palette de rails
        /// </summary>
        public PaletteController paletteController;

        /// <summary>
        /// Prefab de circuit
        /// </summary>
        public Circuit circuitPrefab;

        /// <summary>
        /// Liste de rails utilisables
        /// TODO : à remplacer par la lecture d'un dossier (cf RefreshPalette())
        /// </summary>
        public Rail[] rails;

        /// <summary>
        /// Parent des handles de la scène
        /// </summary>
        private Transform _handlesRoot;

        /// <summary>
        /// Objet éditeur de circuit commun à l'interface UnityEditor et Standalone
        /// </summary>
        private CircuitEditor _circuitEditor;

        //////////////////////////////////// Dossier de prefabs ////////////////////////////////////
        /// <summary>
        /// Chemin du dossier contenant les prefabs de rail
        /// 
        /// Les prefabs doivent tous être orientés de la même façon (avant vers les Z positifs)
        /// </summary>
        private string _prefabsPath = "Assets/";
        ////////////////////////////////////////////////////////////////////////////////////////////

        public CircuitEditor.AddingMode AddMode => _circuitEditor.AddMode;

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
        private Rail _railHandle;
        ////////////////////////////////////////////////////////////////////////////////////////////



        /// <summary>
        /// Rail sélectionné dans le circuit
        /// Passerelle avec la sélection dans le CircuitEditor
        /// </summary>
        private Rail Selection
        {
            get
            {
                if (_circuitEditor == null)
                {
                    return null;
                }

                return _circuitEditor.Selection;
            }
            set
            {
                if (_circuitEditor != null)
                {
                    _circuitEditor.Selection = value;
                }
            }
        }

        /// <summary>
        /// Mémorise la précédente sélection, mis à jour dans Update()
        /// </summary>
        private Rail _oldSelection;

        /// <summary>
        /// La caméra a-t-elle été initialisée
        /// </summary>
        private bool _cameraInitialized;


        private Vector2 _lastMousePos;


        /// <summary>
        /// Position de la caméra par défaut
        /// A une distance 1 de la cible en (0,0,0)
        /// </summary>
        private readonly Vector3 _cameraNormalizedResetPosition = new Vector3(-0.5f, 0.7f, -0.5f);

        /// <summary>
        /// Couleur du contour du rail sélectionné
        /// </summary>
        private readonly Color _selectionOutlineColor = new Color(0.15f, 0.77f, 0.67f);

        /// <summary>
        /// Épaisseur du contour du rail sélectionné
        /// </summary>
        private readonly float _selectionOutlineWidth = 3f;

        #endregion

        #region Unity Lifecycle Events

        // Exécuté avant Start()
        private void Awake()
        {
            _circuitEditor = new CircuitEditor(true);
        }

        // Start is called before the first frame update
        void Start()
        {
            // Création d'un circuit vide au lancement
            NewCircuit();

            _handlesRoot = new GameObject("_handlesRoot").transform;

            RefreshPalette();
        }

        // Update is called once per frame
        void Update()
        {
            // Initialisation de la caméra à l'ajout du premier rail
            if (!_cameraInitialized)
            {
                if (_circuitEditor.Circuit.RailsCount > 0)
                {
                    ResetCamera();
                    _cameraInitialized = true;
                }
            }




            // Inputs \\

            // Évènements à la souris restreint à l'extérieur de l'UI
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                Vector2 mousePos = Input.mousePosition;


                // Mouse move
                if (mousePos != _lastMousePos)
                {
                    if (_circuitEditor.AddMode == CircuitEditor.AddingMode.Scene && !editorCamera.IsMovingView)
                    {
                        // -----------------------------------------------------------------------------------
                        // Détection de la proximité du curseur avec une extrémité de rail libre
                        float lowestDistance = _maxDistanceSnap;

                        RailAnchor previouslySelectedAnchor = _focusedAnchor;

                        _focusedAnchor = null;


                        foreach (Rail rail in _circuitEditor.Circuit.GetRails())
                        {
                            foreach (RailAnchor anchor in rail.GetFreeConnections())
                            {
                                Vector2 guiPosition = editorCamera.Camera.WorldToScreenPoint(anchor.Position);

                                float distance = Vector2.Distance(mousePos, guiPosition);

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
                                    _railHandle = Instantiate(_circuitEditor.SelectedRail);
                                }

                                // Placement de ce rail selon l'anchor
                                _railHandle.transform.parent = _handlesRoot.transform;

                                _railHandle.GetComponent<Rail>().SetTransformFromAnchor(_focusedAnchor);
                            }
                            // On est sorti de la zone de sqélection, suppression du handle
                            else
                            {
                                DestroyRailHandle();
                            }
                        }
                    }



                    _lastMousePos = mousePos;
                }



                // Clic gauche
                if (Input.GetMouseButtonDown(0))
                {
                    // En mode scène, le clic gauche sert à l'ajout de rail
                    if (_circuitEditor.AddMode == CircuitEditor.AddingMode.Scene && !editorCamera.IsMovingView)
                    {
                        // Un rail est prêt à être placé
                        if (_railHandle && _focusedAnchor)
                        {
                            Rail newRail = _circuitEditor.AddRail(_focusedAnchor.Parent, _focusedAnchor);

                            if (newRail)
                            {
                                DestroyRailHandle();
                            }
                        }
                    }
                    // En mode clic, le clic gauche sert à la sélection
                    else if (_circuitEditor.AddMode == CircuitEditor.AddingMode.Click)
                    {
                        Ray ray = editorCamera.Camera.ScreenPointToRay(mousePos);


                        int layerMask = 1 << CircuitEditor.railLayer;


                        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
                        {
                            if (hit.collider.TryGetComponent(out Rail selectedRail))
                            {
                                // Le rail devient la nouvelle sélection
                                Selection = selectedRail;
                            }
                        }
                        else
                        {
                            // Clic dans le vide => déselection
                            Selection = null;
                        }
                    }
                }
            }
            

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // Prévoir de fermer des fe^netres etc

                // Si aucune fenêtre ouverte, déselection
                Selection = null;
            }

            // Les touches fléchées permettent de parcourir le rail via la sélection
            if (Selection)
            {
                if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.RightArrow))
                {
                    Rail nextRail = Selection.NextRail;

                    if (nextRail != null)
                    {
                        Vector3 translation = nextRail.transform.position - Selection.transform.position;

                        editorCamera.Camera.transform.Translate(translation, Space.World);

                        Selection = nextRail;
                    }
                }
                else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    Rail prevRail = Selection.PreviousRail;

                    if (prevRail != null)
                    {
                        Vector3 translation = prevRail.transform.position - Selection.transform.position;

                        editorCamera.Camera.transform.Translate(translation, Space.World);

                        Selection = prevRail;
                    }
                }
            }
            




            // Observateur de la modification de sélection
            if (Selection != _oldSelection)
            {
                SelectionChanged();

                _oldSelection = Selection;
            }
        }

        #endregion


        #region Public Methods

        /// <summary>
        /// Rafraichis la liste des rails
        ///
        /// TODO : permettre de lire les fichiers depuis un dossier externe
        /// Il faudra pour cela intégrer un parser de fichier 3D (ex .obj)
        /// </summary>
        public void RefreshPalette()
        {
            /*System.IO.SearchOption searchOption = (recursive) ? System.IO.SearchOption.AllDirectories : System.IO.SearchOption.TopDirectoryOnly;

            string[] railFiles = System.IO.Directory.GetFiles(_prefabsPath, "*.prefab", searchOption);

            foreach (string railPath in railFiles)
            {
                GameObject rail = AssetDatabase.LoadAssetAtPath(railPath, typeof(GameObject)) as GameObject;


                if (rail.GetComponent<Rail>() != null)
                {
                    _railPalette.Add(rail);
                }
            }*/

            List<Rail> railsPalette = new List<Rail>(rails);

            _circuitEditor.RailPalette = railsPalette;

            paletteController.RefreshPalette(railsPalette, OnRailButtonClick);
        }

        /// <summary>
        /// Action exécutée par un item de la palette
        /// </summary>
        /// <param name="railId"></param>
        public void OnRailButtonClick(int railId)
        {
            _circuitEditor.SelectedRailIndex = railId;
        }

        #endregion


        #region GUI Events

        /// <summary>
        /// Action de créer un nouveau circuit
        /// </summary>
        public void NewCircuit()
        {
            if (_circuitEditor.HasCircuit)
            {
                Debug.Log("Sécurité temporaire : circuit non enregistré");

                return;
            }
            _circuitEditor.Circuit = Instantiate(circuitPrefab, Vector3.zero, Quaternion.identity);
        }

        /// <summary>
        /// Définit le mode d'ajout de rail depuis la scène
        /// </summary>
        public void SetAddModeScene()
        {
            _circuitEditor.AddMode = CircuitEditor.AddingMode.Scene;
        }

        /// <summary>
        /// Définit le mode d'ajout de rail par clic sur la palette
        /// </summary>
        public void SetAddModePalette()
        {
            _circuitEditor.AddMode = CircuitEditor.AddingMode.Click;
        }

        /// <summary>
        /// Place la caméra automatiquement en fonction de l'étendue du circuit
        /// </summary>
        public void ResetCamera()
        {
            if (_circuitEditor.Circuit.transform.childCount == 0)
            {
                return;
            }

            //Bounds bounds = new Bounds(_circuitEditor.Circuit.transform.position, Vector3.zero);
            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);

            foreach (Renderer rend in _circuitEditor.Circuit.GetComponentsInChildren<Renderer>())
            {
                bounds.Encapsulate(rend.bounds);
            }

            Vector3 center = bounds.center;
            float extents = bounds.extents.magnitude;


            // Position / rotation
            editorCamera.Camera.transform.position = center + _cameraNormalizedResetPosition * extents * 2f;
            editorCamera.Camera.transform.LookAt(center);

            // Rapport 1000 far / near
            editorCamera.Camera.nearClipPlane = extents / 10;
            editorCamera.Camera.farClipPlane = extents * 100;

            editorCamera.Camera.GetComponent<EditorCamera>().ReferenceDistance = Vector3.Distance(editorCamera.Camera.transform.position, center);
        }

        /// <summary>
        /// Fermeture du circuit etc
        /// </summary>
        public void CheckConnections()
        {
            _circuitEditor.Circuit.CheckConnections();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Détruit le gizmo de prévisualisation d'ajout de rail
        /// </summary>
        private void DestroyRailHandle()
        {
            if (_railHandle != null)
            {
                Destroy(_railHandle.gameObject);
                _railHandle = null;
            }
        }

        /// <summary>
        /// Appelé lorsque la sélection de rail dans le circuit a changé
        /// </summary>
        private void SelectionChanged()
        {
            if (_oldSelection)
            {
                Destroy(_oldSelection.GetComponent<Outline>());
            }

            if (Selection)
            {
                Outline outlineComponent = Selection.gameObject.AddComponent<Outline>();

                outlineComponent.OutlineMode = Outline.Mode.OutlineAll;
                outlineComponent.OutlineColor = _selectionOutlineColor;
                outlineComponent.OutlineWidth = _selectionOutlineWidth;
            }
        }

        

        #endregion
    }
}