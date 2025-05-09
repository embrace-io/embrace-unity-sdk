using System;
using System.Collections;
using System.Threading;
using EmbraceSDK.EditorView;
using JetBrains.Annotations;
using NSubstitute;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EmbraceSDK.Tests
{
    /// <summary>
    /// Contains tests related to capturing Unity log messages originating from background threads.
    /// </summary>
    public class EmbraceExceptionLoggerTests
    {
        private bool _hasRecompiled;

        [OneTimeSetUp]
        public void Init()
        {
            _hasRecompiled = false;
        }

        [UnitySetUp]
        [UsedImplicitly]
        public IEnumerator RecompileBeforeTests()
        {
            const string SYMBOL = "EMBRACE_USE_THREADING";
            ScriptingDefineUtil defineUtil = new ScriptingDefineUtil();
            if (!_hasRecompiled && !defineUtil.CheckIfSettingIsEnabled(SYMBOL))
            {
                EmbraceLogger.Log($"Recompiling scripts with {SYMBOL} symbol defined for {GetType().Name}");

                defineUtil.ToggleSymbol("EMBRACE_USE_THREADING", true);
                defineUtil.ApplyModifiedProperties();

                AssetDatabaseUtil.ForceRecompileScripts();
                yield return new RecompileScripts();
                _hasRecompiled = true;
            }
        }

        [Test]
        public void ListenerReceivesLogMessagesFromBackgroundThreads()
        {
            const string message = "__Test threaded log message__";

            Embrace mockEmbrace = Substitute.For<Embrace>();
            mockEmbrace.StartSDK(null, false);

            Thread thread = new Thread(() => { Debug.Log(message); });
            thread.Start();
            thread.Join();

            mockEmbrace.Received().Embrace_Log_Handler(message, Environment.StackTrace, LogType.Log);

        }
    }
}