using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace EmbraceSDK.Demo
{
    /// <summary>
    /// This demo demonstrates how to use the Moments API. For more info please see our documentation.
    /// https://embrace.io/docs/unity/features/performance-monitoring/
    /// </summary>
    public class MomentsDemo : DemoBase
    {
        public Image image;
        public Button cropButton;
        public Button abandonmentButton;
        public Button MomentTestButton;
        public Toggle allowScreenshotToggle;
        public Slider slider;
        public PropertiesController propertiesController;
        public void Start()
        {
            cropButton.onClick.AddListener(HandleCropClick);
            abandonmentButton.onClick.AddListener(HandleAbandonmentClick);
            MomentTestButton.onClick.AddListener(HandleStartMomentClick);
        }

        public void HandleCropClick()
        {
            Embrace.Instance.StartMoment(DemoConstants.MOMENT_CROP_IMAGE);
            Texture2D texture = image.sprite.texture;
            texture = CropToCircle(texture.height, texture.width, texture.width / 2, texture.width / 2, texture.height / 2, texture);
            image.sprite = Sprite.Create(texture, new Rect(0,0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);

            Embrace.Instance.EndMoment(DemoConstants.MOMENT_CROP_IMAGE);
        }

        public void HandleStartMomentClick()
        {
            Embrace.Instance.StartMoment(DemoConstants.MOMENT_TEST, DemoConstants.TEST_ID, allowScreenshotToggle.isOn, propertiesController.properties);
            StartCoroutine(EndMoment());
        }

        private IEnumerator EndMoment()
        {
            yield return new WaitForSeconds(slider.value);
            Embrace.Instance.EndMoment(DemoConstants.MOMENT_TEST, DemoConstants.TEST_ID, propertiesController.properties);
        }

        private void HandleAbandonmentClick()
        {
            Embrace.Instance.StartMoment(DemoConstants.MOMENT_ABANDONMENT);
            Application.Quit();
            Embrace.Instance.EndMoment(DemoConstants.MOMENT_ABANDONMENT);
        }

        // Crops texture into a circle, this is used to help demonstrate moments.
        private Texture2D CropToCircle(int h, int w, float r, float cx, float cy, Texture2D sourceTex)
        {
            Color[] c = sourceTex.GetPixels(0, 0, sourceTex.width, sourceTex.height);
            Texture2D b = new Texture2D(h, w);
            for (int i = (int)(cx - r); i < cx + r; i++)
            {
                for (int j = (int)(cy - r); j < cy + r; j++)
                {
                    float dx = i - cx;
                    float dy = j - cy;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    if (d <= r)
                        b.SetPixel(i - (int)(cx - r), j - (int)(cy - r), sourceTex.GetPixel(i, j));
                    else
                        b.SetPixel(i - (int)(cx - r), j - (int)(cy - r), Color.clear);
                }
            }
            b.Apply();
            return b;
        }
    }
}
