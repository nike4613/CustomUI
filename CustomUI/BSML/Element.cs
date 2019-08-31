using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;

namespace CustomUI.BSML
{
    // interface for all custom elements, generated or not
    public abstract class Element : ICollection<Element> // is a collection of children
    {
        #region Collection
        private List<Element> children = new List<Element>();

        public int Count => children.Count;

        public bool IsReadOnly => false;

        public void Add(Element item) => children.Add(item);

        public void Clear() => children.Clear();

        public bool Contains(Element item) => children.Contains(item);

        public void CopyTo(Element[] array, int arrayIndex) => children.CopyTo(array, arrayIndex);

        public IEnumerator<Element> GetEnumerator() => children.GetEnumerator();

        public bool Remove(Element item) => children.Remove(item);

        IEnumerator IEnumerable.GetEnumerator() => (children as IEnumerable).GetEnumerator();

        public Element this[int i]
        {
            get => children[i];
            set => children[i] = value;
        }
        #endregion

        public ElementController Controller { get; internal set; }

        protected T ControllerAs<T>() where T : ElementController => Controller as T;

        public abstract void Initialize(Attribute[] attributes, object state);

        // returns whether or not children should be parsed and added, or the structure should be passed in
        internal virtual bool InitializeInternal(List<Attribute> attributes, object state)
        {
            Attribute attr = null;

            foreach (var a in attributes)
            {
                if (a.Type == AttributeType.SelfRef)
                {
                    if (attr != null) throw new InvalidProgramException("Cannot have 2 ref parameters on one element");
                    attr = a;
                }
            }

            if (attr != null)
                attr.BindingSetter(Controller, this);

            Initialize(attributes.ToArray(), state);

            return true;
        }

        internal virtual void AddChildXml(IEnumerable<XmlNode> nodes) { }

        /// <summary>
        /// Refreshes all bindings and updates the rendered structure appropriately.
        /// It is valid to call <see cref="Render(RectTransform)"/> from here.
        /// Should also call <see cref="Refresh"/> on all children.
        /// </summary>
        public abstract void Refresh();

        /// <summary>
        /// Renders this element into a Unity GameObject heirarchy. This is expected 
        /// to call <see cref="Render(RectTransform)"/> on all valid children. DO NOT
        /// destroy any game objects you do not create. Reparent your children.
        /// This should also correctly reparent the heirarchy if a new parent is given.
        /// </summary>
        /// <param name="parentTransform"></param>
        public abstract void Render(RectTransform parentTransform);
    }

    public abstract class TextElement : Element
    { // Initialize(Attribute[]) will not be called on ITextElement
        public abstract void Initialize(XmlText text, object state);
    }
}
