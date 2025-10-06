using EmbraceSDK.EditorView;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using EmbraceSDK;
using UnityEditor;
using UnityEngine;
using YamlDotNet.Serialization;

namespace Embrace.Tools
{

    /// <summary>
    /// A tool that helps simplify the build process and helps prevent user error when publishing.
    /// </summary>
    internal class PublisherTool : ToolEditorWindow
    {
        public enum UpdateStatus
        {
            success,
            fail
        }

        private static PublisherData publisherData;
        private GUIStyle textAreaStyle;
        private int tabs;
        private UpdateStatus updateAwsCredentialsStatus;
        private UpdateStatus updateNexusStatus;
        private float AWScredentialStatusTimer;
        private float nexusStatusTimer;
        private float helpBoxRefreshTime = 2;
        private bool postToS3;
        private bool uploadToNexus;
        private bool updateChangelog;
        private bool needsDocPath;
        private bool isPublishing;
        private string progressMessage;
        private float progressPercent;
        private int buildOptionsIndex = 0;
        private string[] buildOptions = new string[] {"Local Build", "Publish"};
        private string[] tabsOptions = new string[] {"Info", "Build", "Settings"};

        private (string, float) SDK_PROGRESS = ("Updating SDK", 0.15f);
        private (string, float) DOCS_PROGRESS = ("Updating Docs", 0.3f);
        private (string, float) PACKAGE_PROGRESS = ("Updating Package", 0.45f);
        private (string, float) UNITYPACKAGE_PROGRESS = ("Exporting assets for unitypackage.", 0.6f);
        private (string, float) S3_PROGRESS = ("Uploading SDK to S3", 0.75f);
        private (string, float) NEXUS_PROGRESS = ("Uploading package to Nexus", 0.9f);
        private (string, float) COMPLETE_PROGRESS = ("SDK succesfully Published, Work Complete!", 1f);

        [MenuItem("Embrace/Publisher Tool")]
        public static void Init()
        {
            EmbraceEditorWindow.Setup();
            GetCredentials();
            TextAsset targetFile = Resources.Load<TextAsset>("PublisherData");
            publisherData = JsonUtility.FromJson<PublisherData>(targetFile.text);

            PublisherTool window = (PublisherTool)GetWindow(typeof(PublisherTool));
            window.minSize = new Vector2(410f, 550f);
            window.Show();
        }

        public override void Awake()
        {
            textAreaStyle = new GUIStyle("textArea");
            textAreaStyle.wordWrap = true;

            if (string.IsNullOrEmpty(publisherData.docRepoPath))
            {
                string docPath = Application.dataPath.Replace("embrace-unity-sdk/Assets", "embrace-docs");
                bool directoryExists = Directory.Exists(docPath);
                if (!directoryExists)
                {
                    needsDocPath = true;
                }
                else
                {
                    publisherData.docRepoPath = docPath;
                    System.IO.File.WriteAllText(Application.dataPath + "/Tools/Resources/PublisherData.json", JsonUtility.ToJson(publisherData));
                }

            }
        }

        public override void OnGUI()
        {
            base.OnGUI();
            GUILayout.BeginVertical(StaticStyleConfigs.DarkBoxStyle.guiStyle);
            GUILayout.Label("Publisher Tool", StaticStyleConfigs.LabelHeaderStyle.guiStyle);
            GUILayout.Label("version 1.4.0", StaticStyleConfigs.HeaderTextStyle.guiStyle);
            GUILayout.EndVertical();
            tabs = GUILayout.Toolbar(tabs, tabsOptions);

            switch (tabs)
            {
                case 0:
                    Info();
                    break;
                case 1:
                    BuildAndPublish();
                    break;
                case 2:
                    Settings();
                    break;
                default:
                    break;
            }

            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical(StaticStyleConfigs.DarkBoxStyle.guiStyle);
            GUILayout.Label("Release Notes", StaticStyleConfigs.LabelHeaderStyle.guiStyle);
            GUILayout.Space(StaticStyleConfigs.Space);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Unity Notes: ", StaticStyleConfigs.HeaderTextStyle.guiStyle);
            if (GUILayout.Button("Notion"))
            {
                Application.OpenURL("https://www.notion.so/embraceio/Releasing-Unity-SDK-5a92be518f8c431b8fc70fc04290c27a");
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();

            if (isPublishing)
                EditorUtility.DisplayProgressBar("Publishing SDK", progressMessage, progressPercent);
            else
                EditorUtility.ClearProgressBar();
        }

        private void Info()
        {
            GUILayout.BeginVertical(StaticStyleConfigs.LightBoxStyle.guiStyle);
            GUILayout.Label("SDK Info", StaticStyleConfigs.LabelHeaderStyle.guiStyle);
            GUILayout.Label("SDK Version");
            sdkInfo.version = EditorGUILayout.TextField(sdkInfo.version);
            GUILayout.EndVertical();


            GUILayout.BeginVertical(StaticStyleConfigs.LightBoxStyle.guiStyle);
            GUILayout.Label("Welcome Window", StaticStyleConfigs.LabelHeaderStyle.guiStyle);
            GUILayout.Space(10);
            GUILayout.Label("Announcement Title", StaticStyleConfigs.DefaultTextStyle.guiStyle);
            sdkInfo.wAnnouncementTitle = EditorGUILayout.TextArea(sdkInfo.wAnnouncementTitle);
            GUILayout.Space(10);
            GUILayout.Label("Announcement Message", StaticStyleConfigs.DefaultTextStyle.guiStyle);
            sdkInfo.wAnnouncementMessage = EditorGUILayout.TextArea(sdkInfo.wAnnouncementMessage, textAreaStyle);
            GUILayout.EndVertical();

            GUILayout.BeginVertical(StaticStyleConfigs.LightBoxStyle.guiStyle);
            GUILayout.Label("Docs", StaticStyleConfigs.LabelHeaderStyle.guiStyle);
            GUILayout.Label("Changelog");
            publisherData.changelogMessage = EditorGUILayout.TextArea(publisherData.changelogMessage, textAreaStyle);
            GUILayout.EndVertical();
        }

        private void BuildAndPublish()
        {
            GUILayout.BeginVertical(StaticStyleConfigs.LightBoxStyle.guiStyle);
            GUILayout.Label("Build SDK", StaticStyleConfigs.LabelHeaderStyle.guiStyle);
            buildOptionsIndex = EditorGUILayout.Popup("Build Type", buildOptionsIndex, buildOptions);
            if (buildOptionsIndex == 1)
            {
                postToS3 = EditorGUILayout.ToggleLeft("Post SDK to S3", postToS3);
                uploadToNexus = EditorGUILayout.ToggleLeft("Upload package to Nexus", uploadToNexus);
                updateChangelog = EditorGUILayout.ToggleLeft("Update changelog", updateChangelog);
                
                if (postToS3 && !credentials.HasCredentials())
                {
                    EmbraceLogger.LogWarning("You need your S3 credentials before you can publish.");
                    postToS3 = false;
                    tabs = 2;
                }

                if (uploadToNexus && !credentials.HasEndpoint())
                {
                    EmbraceLogger.LogWarning("You need your nexus credentials before you can publish.");
                    uploadToNexus = false;
                    tabs = 2;
                }
            }

            string buttonString = (buildOptionsIndex == 0) ? "Build" : "Build and Publish";
            if (GUILayout.Button(buttonString))
            {
                isPublishing = true;
                progressPercent = 0.0f;

                Export();
            }
            GUILayout.EndVertical();
        }

        private void Settings()
        {
            HandleAWSCredentials();

            HandleNexusEndpoint();

            if (needsDocPath)
            {
                EditorGUILayout.HelpBox("You need to give a path to your embrace-docs repo.", MessageType.Warning);
            }
            GUILayout.BeginVertical(StaticStyleConfigs.LightBoxStyle.guiStyle);
            GUILayout.Label("Docs repo path", StaticStyleConfigs.LabelHeaderStyle.guiStyle);
            GUILayout.Label("path");
            publisherData.docRepoPath = EditorGUILayout.TextArea(publisherData.docRepoPath);
            if (GUILayout.Button("Update path"))
            {
                System.IO.File.WriteAllText(Application.dataPath + "/Tools/Resources/PublisherData.json", JsonUtility.ToJson(publisherData));
            }
            GUILayout.EndVertical();
        }

        private void HandleAWSCredentials()
        {
            if (!credentials.HasCredentials())
            {
                EditorGUILayout.HelpBox("You need to add AWS credentials if you want to upload to s3.", MessageType.Warning);
            }

            if (updateAwsCredentialsStatus == UpdateStatus.success && AWScredentialStatusTimer > 0)
            {
                EditorGUILayout.HelpBox("success: credentials were successfully updated.", MessageType.Info);
                AWScredentialStatusTimer -= Time.deltaTime;
            }
            else if (updateAwsCredentialsStatus == UpdateStatus.fail && AWScredentialStatusTimer > 0)
            {
                EditorGUILayout.HelpBox("Fail: credentials were not updated.", MessageType.Warning);
                AWScredentialStatusTimer -= Time.deltaTime;
            }

            GUILayout.BeginVertical(StaticStyleConfigs.LightBoxStyle.guiStyle);
            GUILayout.Label("AWS Credentials", StaticStyleConfigs.LabelHeaderStyle.guiStyle);
            GUILayout.Label("AWS Access Key");
            credentials.awsAccessKey = EditorGUILayout.PasswordField(credentials.awsAccessKey);
            GUILayout.Label("AWS Secret Key");
            credentials.awsSecretKey = EditorGUILayout.PasswordField(credentials.awsSecretKey);
            if (GUILayout.Button("Update Credentials"))
            {
                string credentialsString = JsonUtility.ToJson(credentials);
                try
                {
                    AWScredentialStatusTimer = helpBoxRefreshTime;
                    updateAwsCredentialsStatus = UpdateStatus.success;
                    System.IO.File.WriteAllText(Application.persistentDataPath + "/Credentials.json", credentialsString);
                }
                catch (FileNotFoundException)
                {
                    updateAwsCredentialsStatus = UpdateStatus.fail;
                    Debug.LogError("FileNotFoundException: Unable to find " + Application.persistentDataPath + "/Credentials.json");
                }
            }
            GUILayout.EndVertical();
        }

        private void HandleNexusEndpoint()
        {
            if (!credentials.HasEndpoint())
            {
                EditorGUILayout.HelpBox("You need to add the Endpoint if you want to upload to Nexus.", MessageType.Warning);
            }

            if (updateNexusStatus == UpdateStatus.success && nexusStatusTimer > 0)
            {
                EditorGUILayout.HelpBox("success: Endpoint was successfully updated.", MessageType.Info);
                nexusStatusTimer -= Time.deltaTime;
            }
            else if (updateNexusStatus == UpdateStatus.fail && nexusStatusTimer > 0)
            {
                EditorGUILayout.HelpBox("Fail: Endpoint was not updated.", MessageType.Warning);
                nexusStatusTimer -= Time.deltaTime;
            }

            GUILayout.BeginVertical(StaticStyleConfigs.LightBoxStyle.guiStyle);
            GUILayout.Label("Nexus", StaticStyleConfigs.LabelHeaderStyle.guiStyle);
            GUILayout.Label("Endpoint for npm API");
            credentials.npmAPIEndpoint = EditorGUILayout.TextField(credentials.npmAPIEndpoint);
            if (GUILayout.Button("Update Endpoint"))
            {
                string endpointString = JsonUtility.ToJson(credentials);
                try
                {
                    nexusStatusTimer = helpBoxRefreshTime;
                    updateNexusStatus = UpdateStatus.success;
                    System.IO.File.WriteAllText(Application.persistentDataPath + "/Credentials.json", endpointString);
                }
                catch (FileNotFoundException)
                {
                    updateNexusStatus = UpdateStatus.fail;
                    Debug.LogError("FileNotFoundException: Unable to find " + Application.persistentDataPath + "/Credentials.json");
                }
            }
            GUILayout.EndVertical();
        }

        private void Export()
        {
            UpdateProgressGUI(SDK_PROGRESS.Item1, SDK_PROGRESS.Item2);
            // save SDK info
            sdkInfo.npmAPIEndpoint = credentials.npmAPIEndpoint;
            System.IO.File.WriteAllText(Application.dataPath.Replace("/Assets", "") + "/Packages/io.embrace.sdk/Resources/Info/EmbraceSdkInfo.json", JsonUtility.ToJson(sdkInfo));

            // update docs
            if (updateChangelog)
            {
                UpdateProgressGUI(DOCS_PROGRESS.Item1, DOCS_PROGRESS.Item2);
                UpdateDocs();
            }

            // update custom Unity package
            UpdateProgressGUI(PACKAGE_PROGRESS.Item1, PACKAGE_PROGRESS.Item2);
            UpdatePackage();

            // Cleanup
            androidConfiguration.SetDefaults();
            iOSConfiguration.SetDefaults();
            environments.Clear();

            // Export package
            UpdateProgressGUI(UNITYPACKAGE_PROGRESS.Item1, UNITYPACKAGE_PROGRESS.Item2);
            ExportToUnitypackage();

            // Upload package to S3
            UpdateProgressGUI(S3_PROGRESS.Item1, S3_PROGRESS.Item2);
            UploadPackageToS3();

            // Upload package to Nexus
            UpdateProgressGUI(NEXUS_PROGRESS.Item1, NEXUS_PROGRESS.Item2);
            UploadToNexus();

            // Wrapping this up
            UpdateProgressGUI(COMPLETE_PROGRESS.Item1, COMPLETE_PROGRESS.Item2);
            isPublishing = false;
            Debug.Log("Published: " + Application.dataPath + "/Builds/EmbraceSDK_ " + sdkInfo.version + ".unitypackage");
        }

        private void UploadPackageToS3()
        {
            if (credentials.HasCredentials() && postToS3)
            {
                AWSs3Uploader awsS3 = new AWSs3Uploader(credentials.awsAccessKey, credentials.awsSecretKey);
                awsS3.PostObject("EmbraceSDK_" + sdkInfo.version + ".unitypackage", Application.dataPath + "/Builds/EmbraceSDK_" + sdkInfo.version + ".unitypackage");
            }
            else
            {
                Debug.LogWarning("Did not upload to s3");
            }
        }

        private void ExportToUnitypackage()
        {
            List<string> exportedPackageAssetList = new List<string>();
            //Add Prefabs folder into the asset list
            exportedPackageAssetList.Add("Packages/io.embrace.sdk");

            //Export code and Prefabs with their dependencies into a .unitypackage
            if (!AssetDatabase.IsValidFolder("Assets/Builds"))
            {
                AssetDatabase.CreateFolder("Assets", "Builds");
            }
            AssetDatabase.ExportPackage(exportedPackageAssetList.ToArray(), Application.dataPath + "/Builds/EmbraceSDK_" + sdkInfo.version + ".unitypackage",
                ExportPackageOptions.Recurse);

            AssetDatabase.Refresh();
        }

        private void UpdateDocs()
        {
            ChangelogUpdate(publisherData.docRepoPath + "/content/unity/changelog.md");
            ChangelogUpdate(Application.dataPath.Replace("/Assets", "") + "/Packages/io.embrace.sdk/CHANGELOG.md");

            ConfigUpdate(publisherData.docRepoPath);
        }

        private void UpdatePackage()
        {
            string packageJson = File.ReadAllText(Application.dataPath.Replace("/Assets", "") + "/Packages/io.embrace.sdk/package.json");
            Package package = JsonConvert.DeserializeObject<Package>(packageJson);
            package.version = sdkInfo.version;
            string updatedJson = JsonConvert.SerializeObject(package, Formatting.Indented);
            File.WriteAllText(Application.dataPath.Replace("/Assets", "") + "/Packages/io.embrace.sdk/package.json", updatedJson);
        }

        private void ConfigUpdate(string path)
        {
            path += "/config.yaml";
            StringReader configString = new StringReader(System.IO.File.ReadAllText(path));

            var deserializer = new DeserializerBuilder().Build();
            //var obj = deserializer.Deserialize(configString);
            DocsConfig docsConfig = deserializer.Deserialize<DocsConfig>(configString);

            docsConfig._params.sdkVersion.unity = sdkInfo.version;

            var serializer = new SerializerBuilder().Build();
            string yaml = serializer.Serialize(docsConfig);
            File.WriteAllText(path, yaml);
        }

        private void ChangelogUpdate(string path)
        {
            string changelogString = System.IO.File.ReadAllText(path);
            changelogString = changelogString.Replace("# Unity SDK Changelog", String.Format("# Unity SDK Changelog{0}", ChangeLogEntry()));
            System.IO.File.WriteAllText(path, changelogString);
        }

        private string ChangeLogEntry()
        {
            string result = "";
            result += System.Environment.NewLine + System.Environment.NewLine + "## " + sdkInfo.version;
            result += System.Environment.NewLine + "*" + DateTime.Now.ToString("MMMM dd, yyyy") + "*";
            result += System.Environment.NewLine;
            result += System.Environment.NewLine + publisherData.changelogMessage;

            return result;
        }

        private void UploadToNexus()
        {
            if (uploadToNexus && credentials.HasEndpoint())
            {
#if UNITY_EDITOR_WIN
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    WorkingDirectory = Application.dataPath.Replace("/Assets", "") + "/Packages/io.embrace.sdk",
                    CreateNoWindow = true,
                    FileName = "cmd.exe",
                    Arguments = "/C npm publish --registry=" + credentials.npmAPIEndpoint,
                };

                System.Diagnostics.Process.Start(startInfo);

#elif UNITY_EDITOR_OSX
                string argument = "npm publish --registry=" + credentials.npmAPIEndpoint;
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    WorkingDirectory = Application.dataPath.Replace("/Assets", "") + "/Packages/io.embrace.sdk",
                    FileName = "/bin/bash",
                    Arguments = " -c \"" + argument + " \"",
                };

                System.Diagnostics.Process.Start(startInfo);
#endif
            }
            else
            {
                Debug.LogWarning("Did not upload to Nexus");
            }
        }

        private void UpdateProgressGUI(string message, float percent)
        {
            progressMessage = message;
            progressPercent = percent;
            OnGUI();
        }

    }
}
