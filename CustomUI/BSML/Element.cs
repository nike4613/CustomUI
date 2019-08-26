using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CustomUI.BSML
{
    // interface for all custom elements, generated or not
    public abstract class Element : ICollection<Element> // is a collection of children
    {
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

        public abstract void Initialize(Attribute[] attributes);
    }

    public abstract class TextElement : Element
    { // Initialize(Attribute[]) will not be called on ITextElement
        public abstract void Initialize(XmlText text);
    }
}
