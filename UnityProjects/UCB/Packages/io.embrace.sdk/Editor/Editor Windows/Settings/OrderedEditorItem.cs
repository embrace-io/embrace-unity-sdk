using System;

namespace EmbraceSDK.EditorView
{
    /// <summary>
    /// Attribute which specifies the name and ordering of the editor items.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class OrderedEditorItem : Attribute
    {
        private string name;
        private int index;
        public string Name { get { return name; } }
        public int Index { get { return index; } }
        public OrderedEditorItem(string name, int index) { this.name = name; this.index = index; }
    }
}
