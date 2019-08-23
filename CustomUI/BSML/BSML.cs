﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CustomUI.BSML
{
    public static class BSML
    {
        public const string CoreNamespace = "bsml://beat-saber-markup-core";

        public static void RegisterCustomElement<T>() where T : IElement =>
            RegisterCustomElementImpl(typeof(T));

        public static void RegisterCustomElement(Type type)
        {
            if (!typeof(IElement).IsAssignableFrom(type))
                throw new ArgumentException($"Argument not derived from {nameof(IElement)}", nameof(type));
            RegisterCustomElementImpl(type);
        }

        internal static void RegisterCustomElementImpl(Type type)
        {

        }

        // Text elements have their own IElement type
        internal static IEnumerable<IElement> ReadTree(IEnumerable<XmlNode> elements, Type owningType)
        {
            return null;
        }

        internal static IEnumerable<Attribute> GetAttributes(XmlElement element, out Type childOwner, bool allowElementAttributes = true)
        {
            childOwner = null;
            return null;
        }
    }

    public class ElementNameAttribute : System.Attribute
    {
        public string Name;
        public ElementNameAttribute(string name)
        {
            Name = name;
        }
    }
}
