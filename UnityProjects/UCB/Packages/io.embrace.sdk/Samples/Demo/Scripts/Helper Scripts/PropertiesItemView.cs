using UnityEngine;
using UnityEngine.UI;

namespace EmbraceSDK.Demo
{
    /// <summary>
    /// Provides a UI that allows a user to add a key and value to a property.
    /// </summary>
    public class PropertiesItemView : MonoBehaviour
    {
        public InputField keyInput;
        public InputField valueInput;
        public Button deleteButton;
        public Button saveButton;
        [HideInInspector]
        public PropertiesController propertiesController;

        private void Start()
        {
            deleteButton.onClick.AddListener(HandleDeleteButtonClick);
            saveButton.onClick.AddListener(HandleEditButtonClick);
            keyInput.onEndEdit.AddListener(KeyOnEndEdit);
            valueInput.onEndEdit.AddListener(ValueOnEndEdit);
        }

        private void HandleDeleteButtonClick()
        {
            propertiesController.RemoveProperty(keyInput.text);
            Destroy(gameObject);
        }

        private void ValueOnEndEdit(string value)
        {
            propertiesController.UpdateProperties(keyInput.text, valueInput.text);
            saveButton.gameObject.SetActive(false);
            deleteButton.gameObject.SetActive(true);
        }

        private void KeyOnEndEdit(string value)
        {
            deleteButton.gameObject.SetActive(false);
            saveButton.gameObject.SetActive(true);
            keyInput.interactable = false;
        }

        private void HandleEditButtonClick()
        {
            propertiesController.UpdateProperties(keyInput.text, valueInput.text);
            saveButton.gameObject.SetActive(false);
            deleteButton.gameObject.SetActive(true);
        }

    }
}
