using UnityEngine;
using UnityEngine.UI;

namespace EmbraceSDK.Demo
{
    public class SceneButton : MonoBehaviour
    {
        public Text buttonTitle;
        private Button button;
        public string SceneName { get; set; } = "Demo Home";

        public void Start()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(HandleButtonClick);
        }

        public void HandleButtonClick()
        {
            SceneSelector.instance.LoadScene(SceneName);
        }
    }
}