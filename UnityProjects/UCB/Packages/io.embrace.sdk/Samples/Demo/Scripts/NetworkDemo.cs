using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace EmbraceSDK.Demo
{
    public class NetworkDemo : DemoBase
    {
        [Header("Unity Web Request Buttons")]
        public Button unityGetButton;
        public Button unityPostButton;
        public Button unityPutButton;
        public Button unityDeleteButton;
        public Button unityErrorButton;

        private void Start()
        {
            unityGetButton.onClick.AddListener(HandleGet);
            unityPostButton.onClick.AddListener(HandlePost);
            unityPutButton.onClick.AddListener(HandlePut);
            unityDeleteButton.onClick.AddListener(HandleDelete);
            unityErrorButton.onClick.AddListener(HandleError);
        }

        private void HandleGet()
        {
            StartCoroutine(GetRequest("https://httpbin.org/image/jpeg"));
        }

        private void HandlePost()
        {
            StartCoroutine(PostRequest("https://httpbin.org/post"));
        }

        private void HandlePut()
        {
            StartCoroutine(PutRequest("https://httpbin.org/put"));
        }

        private void HandleDelete()
        {
            StartCoroutine(DeleteRequest("https://httpbin.org/delete"));
        }

        private void HandleError()
        {
            // A non-existing page.
            StartCoroutine(GetRequest("https://error.html"));
        }
        
        private IEnumerator GetRequest(string url)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                yield return webRequest.SendWebRequest();
            }
        }
        
        private IEnumerator PostRequest(string url)
        {
            WWWForm form = new WWWForm();
            form.AddField("myField", "myData");
            using (UnityWebRequest webRequest = UnityWebRequest.Post(url, form))
            {
                yield return webRequest.SendWebRequest();
            }
        }
    
        private IEnumerator DeleteRequest(string url)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Delete(url))
            {
                yield return webRequest.SendWebRequest();
            }
        }
    
        private IEnumerator PutRequest(string url)
        {
            byte[] myData = System.Text.Encoding.UTF8.GetBytes("This is some test data");
            using (UnityWebRequest webRequest = UnityWebRequest.Put(url, myData))
            {
                yield return webRequest.SendWebRequest();
            }
        }
    }
}
