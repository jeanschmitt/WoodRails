using UnityEngine;

namespace WoodRails
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Rail))]
    public class SwitchRail : MonoBehaviour
    {
        public int selectedPath;

        private int _previousSelectedPath;
        private Rail _rail;

        // Start is called before the first frame update
        void Start()
        {
            _previousSelectedPath = selectedPath;

            _rail = GetComponent<Rail>();
        }

        // Update is called once per frame
        void Update()
        {
            if (selectedPath != _previousSelectedPath)
            {
                if (_rail.SelectPath(selectedPath))
                {
                    _previousSelectedPath = selectedPath;
                }
            }
        }
    }
}