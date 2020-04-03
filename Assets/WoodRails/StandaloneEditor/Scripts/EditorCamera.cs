using UnityEngine;
using UnityEngine.EventSystems;

namespace WoodRails
{
    [RequireComponent(typeof(Camera))]
    public class EditorCamera : MonoBehaviour
    {
        // Link to camera
        public Camera Camera { get; private set; }

        // Sensibilities
        public float panSensibility = .2f;
        public float rotateSensibility = 10f;
        public float zoomSensibility = .1f;


        // Mouse Buttons
        private bool _rotateButtonDown;
        private bool _panButtonDown;

        private const float SCROLL_EPSILON = .0001f;

        public float ReferenceDistance
        {
            get => _referenceDistance;
            set
            {
                _currentDistance = value;
                _minDistance = value / 10;
                _maxDistance = value * 100;
                _referenceDistance = value;
            }
        }
        private float _referenceDistance;

        private float _currentDistance;
        private float _minDistance;
        private float _maxDistance;

        /// <summary>
        /// Indique si la caméra est en phase de déplacement (bouton droit ou molette enfoncé)
        /// </summary>
        public bool IsMovingView => _rotateButtonDown || _panButtonDown;




        // Start is called before the first frame update
        void Start()
        {
            Camera = GetComponent<Camera>();
        }


        // Update is called once per frame
        void Update()
        {
            // Seul le premier clic est pris en compte
            if (!_rotateButtonDown && !_panButtonDown && !EventSystem.current.IsPointerOverGameObject())
            {
                if (Input.GetMouseButtonDown(1)) // droite
                {
                    _rotateButtonDown = true;
                }
                else if (Input.GetMouseButtonDown(2)) // milieu
                {
                    _panButtonDown = true;
                }
            }
            else
            {
                // Si un des boutons est relaché on enlève les deux effets
                if (Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2))
                {
                    _rotateButtonDown = false;
                    _panButtonDown = false;
                }
            }


            // Mouvement pan et rotate
            if (_rotateButtonDown || _panButtonDown)
            {
                float deltaX = Input.GetAxis("Mouse X");
                float deltaY = Input.GetAxis("Mouse Y");

                if (_rotateButtonDown)
                {
                    transform.Rotate(Vector3.left * deltaY * rotateSensibility, Space.Self);
                    transform.Rotate(Vector3.up * deltaX * rotateSensibility, Space.World);
                }
                else if (_panButtonDown)
                {
                    Vector3 translateVector = Vector3.left * deltaX + Vector3.down * deltaY;
                    transform.Translate(translateVector * panSensibility);
                    // TODO : sensibilité dépendante de la distance
                }
            }

            // Zoom
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (System.Math.Abs(scroll) > SCROLL_EPSILON && !EventSystem.current.IsPointerOverGameObject())
            {
                // à remplacer par un Transform et une distance
                // (se décale sur de violents coups de molette)
                //Debug.Log(_currentDistance);
                float forwardTranslation = scroll * _currentDistance * zoomSensibility;

                float newDistance = _currentDistance - forwardTranslation;

                if (newDistance < _minDistance)
                {
                    transform.Translate(Vector3.forward * (_minDistance - _currentDistance));

                    _currentDistance = _minDistance;
                }
                else if (newDistance > _maxDistance)
                {
                    transform.Translate(Vector3.forward * (_currentDistance - _maxDistance));

                    _currentDistance = _maxDistance;
                }
                else
                {
                    _currentDistance = newDistance;

                    transform.Translate(Vector3.forward * forwardTranslation);
                }
            }
        }
    }
}