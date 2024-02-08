using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace EmbraceSDK.Demo
{
    /// <summary>
    /// UI that allows users to add properties to Embrace method calls in Demo.
    /// </summary>
    public class PropertiesController : MonoBehaviour
    {
        public Button viewButton;
        public Button addButton;
        public Text propertyCount;
        public GameObject propertiesOverlay;
        public Transform propertiesContent;
        public GameObject propertyItemPrefab;
        public Dictionary<string, string> properties = new Dictionary<string, string>();

        private void Start()
        {
            viewButton.onClick.AddListener(HandleViewButtonClick);
            addButton.onClick.AddListener(HandleAddButtonClick);
            Clear();
            HandleAddButtonClick();
            propertyCount.text = "0";
            propertiesOverlay.SetActive(false);
        }

        public void UpdateProperties(string key, string value)
        {
            if (properties.ContainsKey(key))
            {
                properties[key] = value;
            }
            else
            {
                properties.Add(key, value);
                int result = Int32.Parse(propertyCount.text);
                propertyCount.text = (result + 1).ToString();
            }
        }

        public void RemoveProperty(string key)
        {
            properties.Remove(key);
            int result = Int32.Parse(propertyCount.text);
            propertyCount.text = (result - 1).ToString();
        }

        private void HandleAddButtonClick()
        {
            propertiesOverlay.SetActive(true);
            GameObject go = Instantiate(propertyItemPrefab, propertiesContent);
            PropertiesItemView item = go.GetComponent<PropertiesItemView>();
            item.propertiesController = this;
        }

        private void HandleViewButtonClick()
        {
            propertiesOverlay.SetActive(!propertiesOverlay.activeSelf);
            if (propertiesOverlay.activeSelf)
            {
                viewButton.GetComponentInChildren<Text>().text = "Hide";
                if (propertiesContent.childCount == 0)
                    HandleAddButtonClick();
            }
            else
            {
                viewButton.GetComponentInChildren<Text>().text = "View";
            }
        }

        private void Clear()
        {
            foreach (Transform child in propertiesContent)
            {
                Destroy(child);
            }
        }
    }

}
