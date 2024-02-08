using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EmbraceSDK.EditorView
{
    /// <summary>
    /// Interface used by Settings Windows
    /// </summary>
    public interface ISettingsWindow
    {
        void OnGUI();
        void Initialize(MainSettingsEditor mainSettingsEditor);
        void OnDestroy();
    }
}