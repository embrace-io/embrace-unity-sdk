using System;
using System.Collections.Generic;
using UnityEngine;

namespace EmbraceSDK.EditorView
{
    public class GUIContentLibrary
    {
        public enum GUIContentIdentifier
        {
            GettingStartedLabelAppId,
            GettingStartedLabelAPIToken,
            GettingStartedErrorBaseProjectTemplateMissing,
            GettingStartedErrorLauncherTemplateMissing,
            GettingStartedErrorGradlePropertiesTemplateMissing,
            GettingStartedErrorBaseProjectTemplateMissingImport,
            GettingStartedErrorLauncherTemplateMissingPluginImport,
            GettingStartedErrorGradlePropertiesTemplateMissingAndroidX,
            GettingStartedErrorGradlePropertiesTemplateMissingJetifier,
            #if UNITY_2022_2_OR_NEWER
            GettingStartedErrorSettingsTemplateMissing,
            GettingStartedErrorSettingsTemplateMissingMavenCentral,
            #else
            GettingStartedErrorBaseProjectTemplateMissingMavenCentral,
            #endif
        }

        Dictionary<GUIContentIdentifier, (GUIContent content, GUIStyle style)> guiContentMap = new Dictionary<GUIContentIdentifier, (GUIContent, GUIStyle)>();

        // StyleConfigs provided to allow us to lazily create the necessary content
        public (GUIContent content, GUIStyle style) GetContentTuple(GUIContentIdentifier identifier, StyleConfigs styleConfigs = null)
        {
            if (guiContentMap.TryGetValue(identifier, out var tuple))
            {
                return tuple;
            }
            if (styleConfigs != null)
            {
                // Lazy Load and return
                LazyCreate(identifier, styleConfigs);
                if (guiContentMap.TryGetValue(identifier, out tuple))
                {
                    return tuple;
                }
            }
            throw new ArgumentException($"No mapping defined in GUIContentLibrary for {identifier}");
        }

        // It is unfortunately the responsibility of the caller to ensure that the styleConfigs are setup before using the library.
        // This is unavoidable because of the runtime styleConfigs requirement.
        private void LazyCreate(GUIContentIdentifier identifier, StyleConfigs styleConfigs)
        {
            switch (identifier)
            {
                case GUIContentIdentifier.GettingStartedLabelAppId:
                    guiContentMap[GUIContentIdentifier.GettingStartedLabelAppId] = 
                        (new GUIContent("App ID", EmbraceTooltips.AppId),
                            new GUIStyle(styleConfigs.defaultTextStyle.guiStyle) { fixedWidth = 160 });
                    break;
                case GUIContentIdentifier.GettingStartedLabelAPIToken:
                    guiContentMap[GUIContentIdentifier.GettingStartedLabelAPIToken] = 
                        (new GUIContent("Symbol Upload API Token", EmbraceTooltips.ApiToken),
                            new GUIStyle(styleConfigs.defaultTextStyle.guiStyle) { fixedWidth = 160 });
                    break;
                case GUIContentIdentifier.GettingStartedErrorBaseProjectTemplateMissing:
                    guiContentMap[GUIContentIdentifier.GettingStartedErrorBaseProjectTemplateMissing] =
                        (new GUIContent("Base Project Template Override Missing"),
                            new GUIStyle(styleConfigs.warningBoxStyle.guiStyle)
                            {
                                alignment = TextAnchor.MiddleCenter, normal =
                                    new GUIStyleState()
                                    {
                                        textColor = new Color(220f/ 255f, 93f/255f, 105f/255f, 1),
                                        background = styleConfigs.defaultTextStyle.guiStyle.normal.background,
                                    }
                            });
                    break;
                case GUIContentIdentifier.GettingStartedErrorLauncherTemplateMissing:
                    guiContentMap[GUIContentIdentifier.GettingStartedErrorLauncherTemplateMissing] = 
                        (new GUIContent("Launcher Template Override Missing"),
                            new GUIStyle(styleConfigs.warningBoxStyle.guiStyle)
                            {
                                alignment = TextAnchor.MiddleCenter, normal =
                                    new GUIStyleState()
                                    {
                                        textColor = new Color(220f/ 255f, 93f/255f, 105f/255f, 1),
                                        background = styleConfigs.defaultTextStyle.guiStyle.normal.background,
                                    }
                            });
                    break;
                case GUIContentIdentifier.GettingStartedErrorGradlePropertiesTemplateMissing:
                    guiContentMap[GUIContentIdentifier.GettingStartedErrorGradlePropertiesTemplateMissing] =
                        (new GUIContent("Gradle Properties Override Missing"), 
                            new GUIStyle(styleConfigs.warningBoxStyle.guiStyle)
                            {
                                alignment = TextAnchor.MiddleCenter, normal =
                                    new GUIStyleState()
                                    {
                                        textColor = new Color(220f/ 255f, 93f/255f, 105f/255f, 1),
                                        background = styleConfigs.defaultTextStyle.guiStyle.normal.background,
                                    }
                            });
                    break;
                case GUIContentIdentifier.GettingStartedErrorBaseProjectTemplateMissingImport:
                    guiContentMap[GUIContentIdentifier.GettingStartedErrorBaseProjectTemplateMissingImport] = 
                        (new GUIContent("Missing Import Statement in Base Project Template"), 
                            new GUIStyle(styleConfigs.warningBoxStyle.guiStyle)
                            {
                                alignment = TextAnchor.MiddleCenter, normal =
                                    new GUIStyleState()
                                    {
                                        textColor = new Color(220f/ 255f, 93f/255f, 105f/255f, 1),
                                        background = styleConfigs.defaultTextStyle.guiStyle.normal.background,
                                    }
                            });
                    break;
                case GUIContentIdentifier.GettingStartedErrorLauncherTemplateMissingPluginImport:
                    guiContentMap[GUIContentIdentifier.GettingStartedErrorLauncherTemplateMissingPluginImport] = 
                        (new GUIContent("Didn't find Embrace Plugin import in Launcher Template"), 
                            new GUIStyle(styleConfigs.warningBoxStyle.guiStyle)
                            {
                                alignment = TextAnchor.MiddleCenter, normal =
                                    new GUIStyleState()
                                    {
                                        textColor = new Color(220f/ 255f, 93f/255f, 105f/255f, 1),
                                        background = styleConfigs.defaultTextStyle.guiStyle.normal.background,
                                    }
                            });
                    break;
                case GUIContentIdentifier.GettingStartedErrorGradlePropertiesTemplateMissingAndroidX:
                    guiContentMap[GUIContentIdentifier.GettingStartedErrorGradlePropertiesTemplateMissingAndroidX] = 
                        (new GUIContent("Didn't find android.useAndroidX=true in gradleTemplate.properties"), 
                            new GUIStyle(styleConfigs.warningBoxStyle.guiStyle)
                            {
                                alignment = TextAnchor.MiddleCenter, normal =
                                    new GUIStyleState()
                                    {
                                        textColor = new Color(220f/ 255f, 93f/255f, 105f/255f, 1),
                                        background = styleConfigs.defaultTextStyle.guiStyle.normal.background,
                                    }
                            });
                    break;
                case GUIContentIdentifier.GettingStartedErrorGradlePropertiesTemplateMissingJetifier:
                    guiContentMap[GUIContentIdentifier.GettingStartedErrorGradlePropertiesTemplateMissingJetifier] = 
                        (new GUIContent("Didn't find android.enableJetifier=true in gradleTemplate.properties"), 
                            new GUIStyle(styleConfigs.warningBoxStyle.guiStyle)
                            {
                                alignment = TextAnchor.MiddleCenter, normal =
                                    new GUIStyleState()
                                    {
                                        textColor = new Color(220f/ 255f, 93f/255f, 105f/255f, 1),
                                        background = styleConfigs.defaultTextStyle.guiStyle.normal.background,
                                    }
                            });
                    break;
                #if UNITY_2022_2_OR_NEWER
                case GUIContentIdentifier.GettingStartedErrorSettingsTemplateMissing:
                    guiContentMap[GUIContentIdentifier.GettingStartedErrorSettingsTemplateMissing] = 
                        (new GUIContent("Settings Template Override Missing"), 
                            new GUIStyle(styleConfigs.warningBoxStyle.guiStyle)
                            {
                                alignment = TextAnchor.MiddleCenter, normal =
                                    new GUIStyleState()
                                    {
                                        textColor = new Color(220f/ 255f, 93f/255f, 105f/255f, 1),
                                        background = styleConfigs.defaultTextStyle.guiStyle.normal.background,
                                    }
                            });
                    break;
                case GUIContentIdentifier.GettingStartedErrorSettingsTemplateMissingMavenCentral:
                    guiContentMap[GUIContentIdentifier.GettingStartedErrorSettingsTemplateMissingMavenCentral] =
                        (new GUIContent("Missing Maven Central Repository Declaration in Settings Template"),
                            new GUIStyle(styleConfigs.warningBoxStyle.guiStyle)
                            {
                                alignment = TextAnchor.MiddleCenter, normal =
                                    new GUIStyleState()
                                    {
                                        textColor = new Color(220f/ 255f, 93f/255f, 105f/255f, 1),
                                        background = styleConfigs.defaultTextStyle.guiStyle.normal.background,
                                    }
                            });
                    break;
                #else
                case GUIContentIdentifier.GettingStartedErrorBaseProjectTemplateMissingMavenCentral:
                    guiContentMap[GUIContentIdentifier.GettingStartedErrorBaseProjectTemplateMissingMavenCentral] = 
                        (new GUIContent("Missing Maven Central Repository Declaration in Base Project Template"), 
                            new GUIStyle(styleConfigs.warningBoxStyle.guiStyle)
                            {
                                alignment = TextAnchor.MiddleCenter, normal =
                                    new GUIStyleState()
                                    {
                                        textColor = new Color(220f/ 255f, 93f/255f, 105f/255f, 1),
                                        background = styleConfigs.defaultTextStyle.guiStyle.normal.background,
                                    }
                            });
                    break;
                #endif
            }
        }
    }
}