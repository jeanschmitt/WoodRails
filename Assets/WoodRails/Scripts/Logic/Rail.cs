using System.Collections.Generic;
using UnityEngine;
using BansheeGz.BGSpline.Components;
using BansheeGz.BGSpline.Curve;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WoodRails
{
    [ExecuteInEditMode]
    public class Rail : MonoBehaviour
    {
        #region Public Fields
        
        /// <summary>
        /// Courbes BGCurve attachées au rail
        /// Chaque courbe représente un chemin orienté pour le véhicule.
        /// Pour la plupart des rails il n'y a donc qu'une courbe, sauf dans les aiguillages par exemple
        /// </summary>
        public RailPath[] Paths;
        
        #endregion


        #region Properties

        /// <summary>
        /// Renvoie le rail suivant actuellement sélectionné
        /// Aucune complexité dans le cas général, mais dans le cas d'un aiguillage il y a une certaine logique
        /// IDEA : dans le cas d'un aiguillage par exemple, la modification de comportement doit se faire via un autre composant
        /// </summary>
        public Rail NextRail
        {
            get
            {
                RailAnchor nextRailBegin = Path.end.Connection;

                if (nextRailBegin == null)
                {
                    return null;
                }

                return nextRailBegin.Parent;
            }
        }

        /// <summary>
        /// Renvoie le rail précédent actuellement sélectionné
        /// </summary>
        public Rail PreviousRail
        {
             get
             {
                RailAnchor prevRailEnd = Path.begin.Connection;

                if (prevRailEnd == null)
                {
                    return null;
                }

                return prevRailEnd.Parent;
            }
        }

        /// <summary>
        /// Composant BGCurve rattaché au rail actuel, définissant la courbe du rail
        /// </summary>
        public RailPath Path => Paths[CurrentPathIndex];

        /// <summary>
        /// Composant BGCurve rattaché au rail actuel, définissant la courbe du rail
        /// </summary>
        public BGCurve Curve => Path.Curve;

        /// <summary>
        /// Composant BGCcMath rattaché au rail actuel, permettant des opérations mathématiques sur la courbe
        /// du rail.
        /// </summary>
        public BGCcMath Math => Path.Math;

        /// <summary>
        /// Taille en mètres du rail
        /// </summary>
        public float PathLength { get; private set; }

        #endregion



        #region Private Fields

        // index de la courbe actuellement sélectionnée
        private int CurrentPathIndex
        {
            get => _currentPathIndex;
            set
            {
                _currentPathIndex = value;
                PathLength = Math.GetDistance();
            }
        }
        private int _currentPathIndex;

        #endregion


        #region Unity Lifecycle Events

        // Start is called before the first frame update
        void Start()
        {
            PathLength = Math.GetDistance();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Vérifie si un rail peut être ajouté à une extrémité donnée
        /// </summary>
        /// <param name="anchor"></param>
        /// <returns></returns>
        public bool CanAppendRail(RailAnchor anchor)
        {
            return anchor.Free;

            // TODO Gérer lorsqu'un rail est retiré

        }

        /// <summary>
        /// Vérifie si un rail peut être ajouté à une extrémité donnée
        /// </summary>
        /// <param name="point">Extrémité du rail</param>
        /// <returns></returns>
        public bool CanAppendRail(Endpoint point)
        {
            RailAnchor anchor = GetAnchorForEndpoint(point);

            if (anchor == null)
            {
                return false;
            }

            return CanAppendRail(anchor);
        }

        /// <summary>
        /// Vérifie si un rail peut être ajouté à une extrémité donnée
        /// </summary>
        /// <param name="boundary">Début ou fin de rail</param>
        /// <param name="path">Courbe correspondante</param>
        /// <returns></returns>
        public bool CanAppendRail(RAIL_BOUNDARY boundary, int path)
        {
            return CanAppendRail(new Endpoint(boundary, path));
        }



        /// <summary>
        /// Ajoute un rail à l'extrémité du rail courrant
        /// </summary>
        /// <param name="rail"></param>
        /// <param name="anchor"></param>
        /// <returns></returns>
        public bool AppendRail(Rail rail, RailAnchor anchor)
        {
            if (!GetFreeConnections().Contains(anchor))
            {
                Debug.LogError("Rail.AppendRail() : l'ancre donnée n'est pas dans le rail ou n'est pas libre");

                return false;
            }


            // Affectation du parent, position, et rotation
            rail.transform.parent = transform.parent;

            rail.SetTransformFromAnchor(anchor);


            anchor.Connect(rail.GetAnchorForPosition(anchor.Position));


            //CheckConnections();

            return true;
        }

        /// <summary>
        /// Ajoute un rail à l'extrémité du rail courrant
        /// </summary>
        /// <param name="rail">Rail à ajouter (doit être déjà instancié)</param>
        /// <param name="point">Extrémité du rail</param>
        /// <returns></returns>
        public bool AppendRail(Rail rail, Endpoint point)
        {
            RailAnchor anchor = GetAnchorForEndpoint(point);

            if (anchor == null)
            {
                return false;
            }

            return AppendRail(rail, anchor);
        }

        /// <summary>
        /// Ajoute un rail à l'extrémité du rail courrant
        /// </summary>
        /// <param name="rail">Rail à ajouter (doit être déjà instancié)</param>
        /// <param name="boundary">Début ou fin de rail</param>
        /// <param name="path">Courbe correspondante</param>
        /// <returns></returns>
        public bool AppendRail(Rail rail, RAIL_BOUNDARY boundary, int path)
        {
            return AppendRail(rail, new Endpoint(boundary, path));
        }



        /// <summary>
        /// Positionne le rail par rapport à l'extrémité de rail sélectionné
        /// </summary>
        /// <param name="anchor">Ancre dont la future position du rail courant dépend</param>
        public void SetTransformFromAnchor(RailAnchor anchor)
        {
            // La référence est une fin de rail (on place donc notre début de rail par rapport à elle)
            if (anchor.IsEnd)
            {
                transform.position = anchor.Position;
                transform.rotation = anchor.Rotation;
            }
            // La référence est un début de rail (on place donc notre fin de rail par rapport à elle)
            else
            {
                RailAnchor endAnchor = GetAnchorForEndpoint(new Endpoint(RAIL_BOUNDARY.RAIL_END, 0)); // <- !!!


                float angle = Vector3.SignedAngle(endAnchor.transform.forward, anchor.transform.forward, endAnchor.transform.up);


                transform.Translate(anchor.Position - endAnchor.Position);
                transform.RotateAround(endAnchor.Position, endAnchor.transform.up, angle);
            }
        }

        /// <summary>
        /// Établit les connections manquantes entre les rails
        /// Utile principalement pour la fermeture des circuits
        /// </summary>
        public void CheckConnections()
        {
            Circuit circuit = GetComponentInParent<Circuit>();

            if (circuit == null)
            {
                Debug.LogError("Rail.CheckConnections() : Le circuit de ce rail n'a pas été trouvé");
                return;
            }

            List<RailAnchor> freeConnections = GetFreeConnections();


            foreach (Rail otherRail in circuit.GetRails())
            {
                if (otherRail == this)
                {
                    continue;
                }

                foreach (RailAnchor otherAnchor in otherRail.GetFreeConnections())
                {
                    foreach (RailAnchor anchor in freeConnections)
                    {
                        if (anchor.Position == otherAnchor.Position)
                        {
                            anchor.Connect(otherAnchor);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Renvoie la liste des RailAnchor qui ne sont pas encore connectés
        /// </summary>
        /// <returns></returns>
        public List<RailAnchor> GetFreeConnections()
        {
            List<RailAnchor> freeAnchors = new List<RailAnchor>();

            foreach (RailAnchor anchor in GetAllAnchors())
            {
                if (anchor.Free)
                {
                    freeAnchors.Add(anchor);
                }
            }

            Debug.Log(freeAnchors.Count);

            return freeAnchors;
        }

        /// <summary>
        /// Renvoie la liste des connecteurs du rail
        /// </summary>
        /// <returns></returns>
        public List<RailAnchor> GetAllAnchors()
        {
            List<RailAnchor> anchors = new List<RailAnchor>();

            foreach (RailPath path in Paths)
            {
                anchors.Add(path.begin);
                anchors.Add(path.end);
            }

            return anchors;
        }


        /// <summary>
        /// Renvoie le RailAnchor correspondant à une position donnée
        /// </summary>
        /// <param name="position">Position demandée pour le RailANchor</param>
        /// <returns></returns>
        public RailAnchor GetAnchorForPosition(Vector3 position)
        {
            foreach (RailAnchor anchor in GetAllAnchors())
            {
                if (anchor.Position == position)
                {
                    return anchor;
                }
            }

            return null;
        }

        /// <summary>
        /// Renvoie l'index d'un chemin du rail selon l'objet RailPath correspondant
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public int GetPathIdFromPath(RailPath path)
        {
            for (int i = 0; i < Paths.Length; i++)
            {
                if(Paths[i] == path)
                {
                    return i;
                }
            }

            Debug.LogError("Rail.GetPathIdFromPath() : le chemin donné n'est pas présent dans le rail");

            return -1;
        }

        /// <summary>
        /// Fournit le connecteur lié à un certain Endpoint
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public RailAnchor GetAnchorForEndpoint(Endpoint point)
        {
            point.Path = (point.Path == -1) ? CurrentPathIndex : point.Path;

            if (point.Path >= Paths.Length || point.Path < 0)
            {
                Debug.LogError("Rail.GetAnchorForEndpoint(): La courbe demandée n'existe pas");

                return null;
            }

            RailPath concernedPath = Paths[point.Path];

            return point.Boundary == RAIL_BOUNDARY.RAIL_BEGIN ? concernedPath.begin : concernedPath.end;
        }

        /// <summary>
        /// Renvoie les bounds englobant les modèles contenus dans le rail
        /// </summary>
        /// <returns></returns>
        public Bounds GetBounds()
        {
            Bounds bounds;

            if (TryGetComponent(out Renderer rend))
            {
                bounds = rend.bounds;
            }
            else
            {
                bounds = new Bounds(transform.position, Vector3.zero);
            }

            foreach (Renderer childRenderer in GetComponentsInChildren<Renderer>())
            {
                bounds.Encapsulate(childRenderer.bounds);
            }

            

            return bounds;
        }

        /// <summary>
        /// Change le chemin sélectionné dans le rail
        /// </summary>
        /// <param name="pathIndex"></param>
        /// <returns></returns>
        public bool SelectPath(int pathIndex)
        {
            if (pathIndex >= Paths.Length || pathIndex < 0)
            {
                Debug.LogError("Rails.SelectCurve() : new index out of range");

                return false;
            }

            CurrentPathIndex = pathIndex;
            PathLength = Math.GetDistance();

            return true;
        }

        #endregion




        #region Structs

        /// <summary>
        /// Endpoint est la structure représentant une extrémité d'un chemin du rail
        /// Il est définit par l'index de la courbe correspondante (dans l'array Curve) et
        /// l'extrémité (begin, end).
        /// </summary>
        public struct Endpoint
        {
            public RAIL_BOUNDARY Boundary;
            public int Path;


            public static Endpoint Default = new Endpoint(RAIL_BOUNDARY.RAIL_END, -1);

            public Endpoint(RAIL_BOUNDARY boundary, int path)
            {
                Boundary = boundary;
                Path = path;
            }
        }

        #endregion
    }
}
