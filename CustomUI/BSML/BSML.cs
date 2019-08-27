using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CustomUI.BSML
{
    public static class BSML
    {
        public const string CoreNamespace = "bsml://beat-saber-markup-core";

        internal static void ResetGlobalState()
        {
            sharedState = new ConditionalWeakTable<Type, object>();
            customElementRegistrar.Clear();
            topLevelTypeRegistrar.Clear();
            TextElementType = null;
        }

        public static void RegisterCustomElement<T>() where T : Element =>
            RegisterCustomElementImpl(typeof(T));

        public static void RegisterCustomElement(Type type)
        {
            if (!typeof(Element).IsAssignableFrom(type))
                throw new ArgumentException($"Argument not derived from {nameof(Element)}", nameof(type));
            RegisterCustomElementImpl(type);
        }

        private static T GetAttributeOrThrow<T>(MemberInfo info, string errorFormat) where T : System.Attribute
        {
            var attr = info.GetCustomAttribute<T>();
            if (attr == null)
                throw new ArgumentException(string.Format(errorFormat, typeof(T).Name));
            return attr;
        }

        // namespace, name
        private static Dictionary<Tuple<string, string>, Type> customElementRegistrar = new Dictionary<Tuple<string, string>, Type>();
        private static Dictionary<string, Type> topLevelTypeRegistrar = new Dictionary<string, Type>();

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

            RegisterCustomElementImpl(type, nameSpace, name);
        }

        internal static void RegisterCustomElementImpl(Type type, string nameSpace, string name)
        {
            customElementRegistrar.Add(Tuple.Create(nameSpace, name), type);
        }

        internal static void RegisterTopLevelElement<T>() where T : Element
        {
            const string NoAttrFormat = "Attempted to register top level element with no {0}!";
            var name = GetAttributeOrThrow<ElementNameAttribute>(typeof(T), NoAttrFormat).Name;
            RegisterTopLevelElement(typeof(T), name);
        }

        internal static void RegisterTopLevelElement(Type type, string name)
        {
            topLevelTypeRegistrar.Add(name, type);
        }

        internal static Type GetCustomElementType(string nameSpace, string name)
        {
            if (customElementRegistrar.TryGetValue(Tuple.Create(nameSpace, name), out var val))
                return val;
            else
                return null;
        }

        internal static Type GetTopLevelElementType(string name)
        {
            if (topLevelTypeRegistrar.TryGetValue(name, out var val))
                return val;
            else
                return null;
        }

        internal static Type TextElementType { get; private set; }

        internal static void SetTextElementType<T>() where T : TextElement
            => TextElementType = typeof(T);

        private static ConditionalWeakTable<Type, object> sharedState = new ConditionalWeakTable<Type, object>();

        /// <summary>
        /// Gets the shared state object of the type that it was called from, or <paramref name="skip"/> stack frames up.
        /// </summary>
        /// <note>
        /// If no object exists for that type, a new one is created as <typeparamref name="T"/> and returned. Otherwise, 
        /// if the type parameter is inconsistent, returns null.
        /// </note>
        /// <typeparam name="T">the type of the shared state</typeparam>
        /// <param name="skip">the number of stack frames to skip</param>
        /// <returns>the shared state object</returns>
        public static T GetSharedState<T>(int skip = 0) where T : class, new()
        {
            var type = new StackTrace(1).GetFrame(skip).GetMethod().DeclaringType;
            if (!sharedState.TryGetValue(type, out var val))
            {
                val = new T();
                sharedState.Add(type, val);
            }

            return val as T;
        }

        /// <summary>
        /// Clears the shared state object for the type calling, or <paramref name="skip"/> stack frames up.
        /// </summary>
        /// <param name="skip">the number of stack frames to skip</param>
        public static void ResetSharedState(int skip = 0)
        {
            var type = new StackTrace(1).GetFrame(skip).GetMethod().DeclaringType;
            sharedState.Remove(type);
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
