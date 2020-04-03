using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WoodRails
{
    public class PaletteController : MonoBehaviour
    {
        public GameObject panel;
        public Transform paletteContent;

        public Button railIconPrefab;

        private bool _isPanelOpened;

        private const int _iconsSize = 150;

        #region Unity Lifecycle Events

        // Start is called before the first frame update
        void Start()
        {
            // Initialise la palette à masquée
            panel.SetActive(_isPanelOpened);
        }

        #endregion


        #region Public Methods

        /// <summary>
        /// Rafraichis la palette selont une liste des Rails modèles
        /// </summary>
        /// <param name="rails"></param>
        /// <param name="buttonCallback"></param>
        public void RefreshPalette(List<Rail> rails, System.Action<int> buttonCallback)
        {
            // Clear
            foreach (Transform child in paletteContent)
            {
                Destroy(child.gameObject);
            }

            Rail rail;
            for (int i = 0; i < rails.Count; i++)
            {
                rail = rails[i];

                Texture2D preview = RuntimePreviewGenerator.GenerateModelPreview(rail.transform, _iconsSize, _iconsSize);

                Button button = Instantiate(railIconPrefab, paletteContent);
                button.image.sprite = Sprite.Create(preview, new Rect(0, 0, _iconsSize, _iconsSize), new Vector2(0.5f, 0.5f));

                // local copy
                int buttonId = i;

                button.onClick.AddListener(() => buttonCallback(buttonId));
            }
        }

        #endregion


        #region GUI Events

        /// <summary>
        /// Affiche ou masque le panneau de la palette
        /// </summary>
        public void TogglePanel()
        {
            _isPanelOpened = !_isPanelOpened;

            panel.SetActive(_isPanelOpened);
        }

        #endregion
    }
}