using System.Collections;
using EmbraceSDK.Demo;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EmbraceSDK.Tests
{
    public class PlayPropertyTests : PlayTestBase
    {
        private readonly string[] _scenesWithProperties =
        {
            DemoConstants.SCENE_NAME_LOGS,
            DemoConstants.SCENE_NAME_MOMENTS
        };

        /// <summary>
        /// Tests the View/Hide functionality of the custom properties view.
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator TestShowAndHide()
        {
            foreach (var sceneName in _scenesWithProperties)
            {
                yield return LoadScene(sceneName, waitSeconds: .25f);

                yield return PressButton(DemoConstants.BUTTON_NAME_PROPERTIES_VIEW);

                yield return null;

                GameObject propertiesGO = GameObject.Find(DemoConstants.GAMEOBJECT_NAME_PROPERTIES_VIEW);

                Assert.AreEqual(propertiesGO.activeInHierarchy, true);

                yield return PressButton(DemoConstants.BUTTON_NAME_PROPERTIES_VIEW);

                yield return null;

                Assert.AreEqual(propertiesGO.activeInHierarchy, false);
            }

            Cleanup();
        }

        /// <summary>
        /// Tests the Add functionality of the properties view
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator TestAddProperty()
        {
            foreach (var sceneName in _scenesWithProperties)
            {
                yield return LoadScene(sceneName, waitSeconds: .25f);

                int propertiesCount = 3;

                // Add property item views.
                // Subtracting one since the property view already contains one by default.
                for (int i = 0; i < propertiesCount - 1; i++)
                {
                    yield return PressButton(DemoConstants.BUTTON_NAME_PROPERTIES_ADD);
                }

                PropertiesItemView[] itemViews = GameObject.FindObjectsOfType<PropertiesItemView>();

                Assert.AreEqual(itemViews.Length, propertiesCount);
            }

            Cleanup();
        }

        /// <summary>
        /// Tests the Save functionality of the property item views
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator TestSaveProperty()
        {
            foreach (var sceneName in _scenesWithProperties)
            {
                yield return LoadScene(sceneName, waitSeconds: .25f);

                int propertiesCount = 3;

                // Add property item views.
                // Subtracting one since the property view already contains one by default.
                for (int i = 0; i < propertiesCount - 1; i++)
                {
                    yield return PressButton(DemoConstants.BUTTON_NAME_PROPERTIES_ADD);
                }

                yield return null;

                PropertiesItemView[] itemViews = GameObject.FindObjectsOfType<PropertiesItemView>();

                // Fill in the property views with unique key value pairs
                for (int i = 0; i < propertiesCount; i++)
                {
                    PropertiesItemView itemView = itemViews[i];
                    itemView.keyInput.text = $"{DemoConstants.TEST_KEY}{i}";
                    itemView.valueInput.text = $"{DemoConstants.TEST_MESSAGE}{i}";
                    itemView.saveButton.onClick.Invoke();
                }

                yield return null;

                // Check if the properties controller dictionary contains the same amount of properties
                PropertiesController controller = GameObject.FindObjectOfType<PropertiesController>();
                Assert.AreEqual(controller.properties.Count, itemViews.Length);
            }

            Cleanup();
        }

        /// <summary>
        /// Tests the Delete functionality of the property item views
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator TestDeleteProperty()
        {
            foreach (var sceneName in _scenesWithProperties)
            {
                yield return LoadScene(sceneName, waitSeconds: .25f);

                int propertyCount = 3;

                // Add property item views.
                // Subtracting one since the property view already contains one by default.
                for (int i = 0; i < propertyCount - 1; i++)
                {
                    yield return PressButton(DemoConstants.BUTTON_NAME_PROPERTIES_ADD);
                }

                PropertiesItemView[] itemViews = GameObject.FindObjectsOfType<PropertiesItemView>();

                // Save the property items
                foreach (var itemView in itemViews)
                {
                    itemView.saveButton.onClick.Invoke();
                }

                yield return null;

                // Do amount of item views match the expected amount?
                Assert.AreEqual(itemViews.Length, propertyCount);

                // Delete the property items
                foreach (var itemView in itemViews)
                {
                    itemView.deleteButton.onClick.Invoke();
                }

                yield return null;

                itemViews = GameObject.FindObjectsOfType<PropertiesItemView>();

                // There should be no more property views.
                Assert.AreEqual(itemViews.Length, 0);
            }

            Cleanup();
        }

        /// <summary>
        /// Tests the data in the property view controller dictionary
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator TestPropertiesDictionary()
        {
            foreach (var sceneName in _scenesWithProperties)
            {
                yield return LoadScene(sceneName, waitSeconds: .25f);

                // Add 3 property item views
                for (int i = 0; i < 3; i++)
                {
                    yield return PressButton(DemoConstants.BUTTON_NAME_PROPERTIES_ADD);
                }

                PropertiesItemView[] itemViews = GameObject.FindObjectsOfType<PropertiesItemView>();

                // Populate the properties with distinct keys and values and invoke Save to add them to the controller dictionary
                for (int i = 0; i < itemViews.Length; i++)
                {
                    PropertiesItemView itemView = itemViews[i];
                    itemView.keyInput.text = $"{DemoConstants.TEST_KEY}{i}";
                    itemView.valueInput.text = $"{DemoConstants.TEST_MESSAGE}{i}";
                    itemView.saveButton.onClick.Invoke();
                    yield return null;
                }

                PropertiesController controller = GameObject.FindObjectOfType<PropertiesController>();

                // Check if the pairs in the property item views matches the controller's dictionary pairs
                foreach (var itemView in itemViews)
                {
                    string keyText = itemView.keyInput.text;
                    string valueText = itemView.valueInput.text;
                    Assert.IsTrue(controller.properties.ContainsKey(keyText));
                    Assert.AreEqual(controller.properties[keyText], valueText);
                }
            }

            Cleanup();
        }
    }
}
