using UnityEngine.UI;

namespace EmbraceSDK.Demo
{
    /// <summary>
    /// This demo demonstrates how to annotate sessions with user information. For more info please see our documentation.
    /// https://embrace.io/docs/unity/features/identify-users/
    /// </summary>
    public class UserDemo : DemoBase
    {
        public InputField userName;
        public InputField userEmail;
        public InputField userIdentifier;
        public InputField userPersona;
        public Button setAsPayerButton;
        public Button clearAsPayerButton;

        // set user data
        private void SetUserName()
        {
            Embrace.Instance.SetUsername(userName.text);
        }

        private void SetUserEmail()
        {
            Embrace.Instance.SetUserEmail(userEmail.text);
        }
        private void SetUserIdentifier()
        {
            Embrace.Instance.SetUserIdentifier(userIdentifier.text);
        }

        private void AddUserPersona()
        {
            Embrace.Instance.AddUserPersona(userPersona.text);
        }

        private void SetUserAsPayer()
        {
            Embrace.Instance.SetUserAsPayer();
        }

        // clear user data
        private void ClearUsername()
        {
            Embrace.Instance.ClearUsername();
        }

        private void ClearUserEmail()
        {
            Embrace.Instance.ClearUserEmail();
        }

        private void ClearUserIdentifier()
        {
            Embrace.Instance.ClearUserIdentifier();
        }

        private void ClearUserPersona()
        {
            Embrace.Instance.ClearUserPersona(null);
        }

        private void ClearUserAsPayer()
        {
            Embrace.Instance.ClearUserAsPayer();
        }

        #region HelperFunctions
        public void HandleClearUserName()
        {
            userName.text = "";
        }

        public void HandleClearUserEmail()
        {
            userEmail.text = "";
        }

        public void HandleClearUserIdentifier()
        {
            userIdentifier.text = "";
        }

        public void HandleClearUserPersona()
        {
            userPersona.text = "";
        }

        public void HandleSetAsPayer()
        {
            if(setAsPayerButton.gameObject.activeSelf)
            {
                SetUserAsPayer();
                setAsPayerButton.gameObject.SetActive(false);
                clearAsPayerButton.gameObject.SetActive(true);
            }
            else
            {
                ClearUserAsPayer();
                clearAsPayerButton.gameObject.SetActive(false);
                setAsPayerButton.gameObject.SetActive(true);
            }
        }

        public void HandleSubmitUserData()
        {
            if (!string.IsNullOrEmpty(userName.text))
                SetUserName();
            if (!string.IsNullOrEmpty(userEmail.text))
                SetUserEmail();
            if (!string.IsNullOrEmpty(userIdentifier.text))
                SetUserIdentifier();
            if (!string.IsNullOrEmpty(userPersona.text))
                AddUserPersona();

            Embrace.Instance.LogMessage("User Demo", EMBSeverity.Info);
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                HandleSubmitUserData();
            }
        }
        #endregion

    }

}
