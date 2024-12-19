using EmbraceSDK.EditorView;
using NSubstitute;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

using BuildTargetSpecifier = EmbraceSDK.EditorView.ScriptingDefineUtil.BuildTargetSpecifier;

namespace EmbraceSDK.Tests
{
    public class ScriptingDefineUtilTests
    {
        private string _iosSymbols;
        private string _androidSymbols;

        private ScriptingDefineUtil.IScriptingDefineSymbolSource _source;
        private ScriptingDefineUtil _utilInstance;

        private ScriptingDefineSettingsItem _itemA;
        private ScriptingDefineSettingsItem _itemB;
        private ScriptingDefineSettingsItem _undefinedItem;

        private const string PREDEFINED_SYMBOL_A = nameof(PREDEFINED_SYMBOL_A);
        private const string PREDEFINED_SYMBOL_B = nameof(PREDEFINED_SYMBOL_B);
        private const string UNDEFINED_SYMBOL = nameof(UNDEFINED_SYMBOL);

        [SetUp]
        public void SetUp()
        {
            _iosSymbols = $"{PREDEFINED_SYMBOL_A};{PREDEFINED_SYMBOL_B}";
            _androidSymbols = $"{PREDEFINED_SYMBOL_A};{PREDEFINED_SYMBOL_B}";

            _itemA = new ScriptingDefineSettingsItem()
            {
                symbol = PREDEFINED_SYMBOL_A,
                guiContent = new GUIContent("Predefined Symbol A"),
                defaultValue = true
            };
            _itemB = new ScriptingDefineSettingsItem()
            {
                symbol = PREDEFINED_SYMBOL_B,
                guiContent = new GUIContent("Predefined Symbol B"),
                defaultValue = true
            };
            _undefinedItem = new ScriptingDefineSettingsItem()
            {
                symbol = UNDEFINED_SYMBOL,
                guiContent = new GUIContent("Undefined Symbol"),
                defaultValue = false
            };


            _source = Substitute.For<ScriptingDefineUtil.IScriptingDefineSymbolSource>();
            _source.GetScriptingDefineSymbolsForGroup(BuildTargetSpecifier.iOS).Returns(_iosSymbols);
            _source.GetScriptingDefineSymbolsForGroup(BuildTargetSpecifier.Android).Returns(_androidSymbols);
            _source.SetScriptingDefineSymbolsForGroup(BuildTargetSpecifier.iOS, Arg.Do<string>(symbols => _iosSymbols = symbols));
            _source.SetScriptingDefineSymbolsForGroup(BuildTargetSpecifier.Android, Arg.Do<string>(symbols => _androidSymbols = symbols));

            _utilInstance = new ScriptingDefineUtil(_source);
        }

        [Test]
        public void CheckIfSettingIsEnabledReturnsTrueForDefinedSymbols()
        {
            Assert.IsTrue(_utilInstance.CheckIfSettingIsEnabled(_itemA));
        }

        [Test]
        public void CheckIfSettingIsEnabledReturnsFalseForUndefinedSymbols()
        {
            Assert.IsFalse(_utilInstance.CheckIfSettingIsEnabled(_undefinedItem));
        }

        [Test]
        public void ToggleSymbolDefinesASymbolThatWasUndefined()
        {
            _utilInstance.ToggleSymbol(_undefinedItem.symbol, true);
            _utilInstance.ApplyModifiedProperties();
            Assert.IsTrue(_utilInstance.CheckIfSettingIsEnabled(_undefinedItem));
        }

        [Test]
        public void ToggleSymbolUndefinesASymbolThatWasDefined()
        {
            _utilInstance.ToggleSymbol(_itemA.symbol, false);
            _utilInstance.ApplyModifiedProperties();
            Assert.IsFalse(_utilInstance.CheckIfSettingIsEnabled(_itemA));
        }

        [Test]
        public void DefiningASymbolDoesNotRemoveExistingSymbols()
        {
            _utilInstance.ToggleSymbol(_undefinedItem.symbol, true);
            _utilInstance.ApplyModifiedProperties();

            Assert.IsTrue(_iosSymbols.Contains(_itemA.symbol));
            Assert.IsTrue(_iosSymbols.Contains(_itemB.symbol));
            Assert.IsTrue(_iosSymbols.Contains(_undefinedItem.symbol));
            Assert.IsTrue(_androidSymbols.Contains(_itemA.symbol));
            Assert.IsTrue(_androidSymbols.Contains(_itemB.symbol));
            Assert.IsTrue(_androidSymbols.Contains(_undefinedItem.symbol));
        }

        [Test]
        public void UndefiningASymbolDoesNotRemoveExistingSymbols()
        {
            _utilInstance.ToggleSymbol(_itemA.symbol, false);
            _utilInstance.ApplyModifiedProperties();

            Assert.IsFalse(_iosSymbols.Contains(_itemA.symbol));
            Assert.IsTrue(_iosSymbols.Contains(_itemB.symbol));
            Assert.IsFalse(_androidSymbols.Contains(_itemA.symbol));
            Assert.IsTrue(_androidSymbols.Contains(_itemB.symbol));
        }

        [Test]
        public void ToggleSymbolDoesNotDuplicateSymbols()
        {
            string iosSymbolCache = _iosSymbols;
            string androidSymbolCache = _androidSymbols;

            _utilInstance.ToggleSymbol(_itemA.symbol, true);
            _utilInstance.ToggleSymbol(_undefinedItem.symbol, false);
            _utilInstance.ApplyModifiedProperties();

            Assert.AreEqual(iosSymbolCache, _iosSymbols);
            Assert.AreEqual(androidSymbolCache, _androidSymbols);
        }

        [Test]
        public void SymbolChangesAreNotAppliedUntilApplyModifiedPropertiesIsCalled()
        {
            _utilInstance.ToggleSymbol(_itemA.symbol, false);
            Assert.IsTrue(_iosSymbols.Contains(_itemA.symbol));
            Assert.IsTrue(_androidSymbols.Contains(_itemA.symbol));

            _utilInstance.ToggleSymbol(_undefinedItem.symbol, true);
            Assert.IsFalse(_iosSymbols.Contains(_undefinedItem.symbol));
            Assert.IsFalse(_androidSymbols.Contains(_undefinedItem.symbol));

            _utilInstance.ApplyModifiedProperties();

            Assert.IsFalse(_iosSymbols.Contains(_itemA.symbol));
            Assert.IsFalse(_androidSymbols.Contains(_itemA.symbol));
            Assert.IsTrue(_iosSymbols.Contains(_undefinedItem.symbol));
            Assert.IsTrue(_androidSymbols.Contains(_undefinedItem.symbol));
        }

        [Test]
        public void ApplyDefaultRemovesASymbolWhenDefaultIsFalse()
        {
            _itemA.defaultValue = false;
            _utilInstance.ApplyDefault(_itemA);
            _utilInstance.ApplyModifiedProperties();

            Assert.IsFalse(_utilInstance.CheckIfSettingIsEnabled(_itemA));
        }

        [Test]
        public void ApplyDefaultAddsASymbolWhenDefaultIsTrue()
        {
            _undefinedItem.defaultValue = true;
            _utilInstance.ApplyDefault(_undefinedItem);
            _utilInstance.ApplyModifiedProperties();

            Assert.IsTrue(_utilInstance.CheckIfSettingIsEnabled(_undefinedItem));
        }

        [Test]
        public void GetFlagNamesForSettingsItemsMatchesSettingsOrder()
        {
            ScriptingDefineSettingsItem[] items = new ScriptingDefineSettingsItem[]
            {
                _itemA, _itemB, _undefinedItem
            };

            string[] names = _utilInstance.GetFlagNamesForSettingsItems(items);

            Assert.AreEqual(items.Length, names.Length);

            for (int i = 0; i < items.Length; ++i)
            {
                Assert.AreEqual(items[i].guiContent.text, names[i]);
            }
        }
    }
}