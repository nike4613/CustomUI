using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        private static T GetAttributeOrThrow<T>(MemberInfo info, string errorFormat) where T : System.Attribute
        {
            var attr = info.GetCustomAttribute<T>();
            if (attr == null)
                throw new ArgumentException(string.Format(errorFormat, typeof(T).Name));
            return attr;
        }

        internal static void RegisterCustomElementImpl(Type type)
        {
            const string NoAttrFormat = "Attempted to register element with no {0}!";
            var name = GetAttributeOrThrow<ElementNameAttribute>(type, NoAttrFormat).Name;
            var nameSpace = type.GetCustomAttribute<ElementNamespaceAttribute>()?.Namespace;
            if (nameSpace == null)
            {
                Logger.log.Warn($"No namespace specified for custom element {name}. This is not recommended as there may be name collisions.");
                nameSpace = ""; // this means the user must not have a default xmlns attribute and not specify namespace to use this
            }



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
    public class ElementNamespaceAttribute : System.Attribute
    {
        public string Namespace;
        public ElementNamespaceAttribute(string nameSpace)
        {
            Namespace = nameSpace;
        }
    }
}
