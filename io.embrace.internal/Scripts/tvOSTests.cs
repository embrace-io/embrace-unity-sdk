using UnityEngine;
using UnityEngine.UI;

namespace EmbraceSDK.Tests 
{
    public class tvOSTests : MonoBehaviour
    {
        [SerializeField] private Text _text;

        private void Start()
        {
        
            _text.text = "Running tests...";

            Embrace_Tests tests = new Embrace_Tests();
            tests.RunTests();
            
            _text.text = "Tests complete";
        }
    }
}