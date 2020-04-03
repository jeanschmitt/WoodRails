using UnityEngine;
using UnityEngine.UI;

namespace WoodRails
{
    public class ToolsController : MonoBehaviour
    {
        public EditorManager manager;

        public Button sceneModeButton;
        public Button paletteModeButton;

        public Color normalTint = Color.white;
        public Color selectedTint = Color.grey;


        private CircuitEditor.AddingMode _lastAddingMode;


        private void Start()
        {
            SetButtonsTint(manager.AddMode);

            _lastAddingMode = manager.AddMode;
        }

        private void Update()
        {
            if (manager.AddMode != _lastAddingMode)
            {
                SetButtonsTint(manager.AddMode);

                _lastAddingMode = manager.AddMode;
            }
        }

        /// <summary>
        /// Définit le mode d'ajout de rail depuis la scène
        /// </summary>
        public void SetAddModeScene()
        {
            manager.SetAddModeScene();
        }

        /// <summary>
        /// Définit le mode d'ajout de rail par clic sur la palette
        /// </summary>
        public void SetAddModePalette()
        {
            manager.SetAddModePalette();
        }

        /// <summary>
        /// Adapte la couleur des boutons selon celui sélectionné
        /// </summary>
        /// <param name="addMode"></param>
        private void SetButtonsTint(CircuitEditor.AddingMode addMode)
        {
            switch (addMode)
            {
                case CircuitEditor.AddingMode.Click:
                    sceneModeButton.image.color = normalTint;
                    paletteModeButton.image.color = selectedTint;
                    break;

                case CircuitEditor.AddingMode.Scene:
                    paletteModeButton.image.color = normalTint;
                    sceneModeButton.image.color = selectedTint;
                    break;
            }
        }
    }
}