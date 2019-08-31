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
            TextElementType = default;
        }

        public static void RegisterCustomElement<T>(object state = null) where T : Element =>
            RegisterCustomElementImpl(typeof(T), state);

        public static void RegisterCustomElement(Type type, object state = null)
        {
            if (!typeof(Element).IsAssignableFrom(type))
                throw new ArgumentException($"Argument not derived from {nameof(Element)}", nameof(type));
            RegisterCustomElementImpl(type, state);
        }

        private static T GetAttributeOrThrow<T>(MemberInfo info, string errorFormat) where T : System.Attribute
        {
            var attr = info.GetCustomAttribute<T>();
            if (attr == null)
                throw new ArgumentException(string.Format(errorFormat, typeof(T).Name));
            return attr;
        }

        public struct ElementDefinition
        {
            public static ElementDefinition Empty = default;

            public Type Type;
            public object State;

            public override bool Equals(object obj)
            {
                return obj is ElementDefinition definition &&
                       EqualityComparer<Type>.Default.Equals(Type, definition.Type) &&
                       EqualityComparer<object>.Default.Equals(State, definition.State);
            }

            public override int GetHashCode()
            {
                var hashCode = -1743767797;
                hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(Type);
                hashCode = hashCode * -1521134295 + EqualityComparer<object>.Default.GetHashCode(State);
                return hashCode;
            }

            public static bool operator ==(ElementDefinition a, ElementDefinition b) =>
                a.Type == b.Type && a.State == b.State;
            public static bool operator !=(ElementDefinition a, ElementDefinition b) =>
                !(a == b);
        }

        // namespace, name
        private static Dictionary<Tuple<string, string>, ElementDefinition> customElementRegistrar = new Dictionary<Tuple<string, string>, ElementDefinition>();
        private static Dictionary<string, ElementDefinition> topLevelTypeRegistrar = new Dictionary<string, ElementDefinition>();

        internal static void RegisterCustomElementImpl(Type type, object state = null)
        {
            const string NoAttrFormat = "Attempted to register element with no {0}!";
            var name = GetAttributeOrThrow<ElementNameAttribute>(type, NoAttrFormat).Name;
            var nameSpace = type.GetCustomAttribute<ElementNamespaceAttribute>()?.Namespace;
            if (nameSpace == null)
            {
                Logger.log.Warn($"No namespace specified for custom element {name}. This is not recommended as there may be name collisions.");
                nameSpace = ""; // this means the user must not have a default xmlns attribute and not specify namespace to use this
            }

            RegisterCustomElementImpl(new ElementDefinition { Type = type, State = state }, nameSpace, name);
        }

        internal static void RegisterCustomElementImpl(ElementDefinition type, string nameSpace, string name)
        {
            customElementRegistrar.Add(Tuple.Create(nameSpace, name), type);
        }

        internal static void RegisterTopLevelElement<T>(object state = null) where T : Element
        {
            const string NoAttrFormat = "Attempted to register top level element with no {0}!";
            var name = GetAttributeOrThrow<ElementNameAttribute>(typeof(T), NoAttrFormat).Name;
            RegisterTopLevelElement(new ElementDefinition { Type = typeof(T), State = state }, name);
        }

        internal static void RegisterTopLevelElement(ElementDefinition type, string name)
        {
            topLevelTypeRegistrar.Add(name, type);
        }

        internal static ElementDefinition GetCustomElementDef(string nameSpace, string name)
        {
            if (customElementRegistrar.TryGetValue(Tuple.Create(nameSpace, name), out var val))
                return val;
            else
                return default;
        }

        internal static ElementDefinition GetTopLevelElementDef(string name)
        {
            if (topLevelTypeRegistrar.TryGetValue(name, out var val))
                return val;
            else
                return default;
        }

        internal static ElementDefinition TextElementType { get; private set; }

        internal static void SetTextElementType<T>(object state = null) where T : TextElement
            => TextElementType = new ElementDefinition { Type = typeof(T), State = state };

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
