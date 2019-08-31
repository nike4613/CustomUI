using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CustomUI.BSML
{
    internal class BSMLParser
    {
        private Assembly owner;
        private XmlDocument tree;
        private string StartingNamespace;

        internal XmlDocument Doc => tree;

        private BSMLParser(Assembly owner, string startingNamespace, XmlDocument tree)
        {
            this.tree = tree;
            this.owner = owner;
            StartingNamespace = startingNamespace;
        }

        public static BSMLParser LoadFrom(Assembly owner, string startingNamespace, TextReader reader)
        {
            var xml = new XmlDocument();
            xml.Load(reader);
            return new BSMLParser(owner, startingNamespace, xml);
        }


        internal struct ParseState
        {
            public Type Type;
            public ElementController Ref;
        }

        public Element Parse()
        { // seperate because there are only a handful of valid top-level elements that should be processed

            var root = tree.DocumentElement;

            var type = BSML.GetTopLevelElementDef(root.LocalName);

            var state = new ParseState { Ref = null, Type = null };

            return MakeElement(root, type, state);
        }

        private Type GetControllerType(Attribute controller, Type currentOwner)
        {
            if (!(controller.Type == AttributeType.Literal
               && controller.NameSpace == BSML.CoreNamespace
               && controller.Name == "controller")) return null;

            var value = controller.LiteralValue;
            if (value.StartsWith("."))
            { // subtype
                var ctrlType = currentOwner?.GetNestedType(value.Substring(1), BindingFlags.NonPublic | BindingFlags.Public);
                if (ctrlType == null) throw new TypeLoadException($"Could not find type {currentOwner?.FullName}/{value}");
                return ctrlType;
            }
            else
            { // non-subtype
                var dotIndex = value.IndexOf('.');

                if (dotIndex == -1)
                { // non-qualified name
                    // search same namespace in owning assembly, or starting namespace in owning assembly
                    var type = owner.GetTypes()
                                    .Where(t => t.Namespace == (currentOwner?.Namespace ?? StartingNamespace))
                                    .FirstOrDefault(t => t.Name == value);
                    if (type == null)
                        throw new TypeLoadException($"Could not find type {value}");

                    return type;
                }
                else
                { // its a fully-qualified type name
                    return owner.GetType(value, true, false);
                }
            }
        }

        internal IEnumerable<Attribute> GetAttributes(XmlElement element, ref ParseState state, out bool hasController, bool allowElementAttributes = true)
        {
            var attrs = new List<Attribute>();

            hasController = false;

            foreach (var attr in element.Attributes.Cast<XmlAttribute>())
            {
                var atr = new Attribute(this, attr, state.Type);

                var newT = GetControllerType(atr, state.Type);
                if (newT != null)
                {
                    if (hasController)
                        throw new InvalidProgramException("Cannot specify multiple controllers for one element");

                    hasController = true;
                    state.Type = newT;
                    state.Ref = Activator.CreateInstance(newT) as ElementController;
                }

                attrs.Add(atr);
            }

            if (allowElementAttributes)
            {
                foreach (var node in element.ChildNodes.Cast<XmlNode>())
                    if (node is XmlElement el && Attribute.IsElementAttribute(el))
                    {
                        var children = ReadTree(el.ChildNodes.Cast<XmlNode>(), state);

                        attrs.Add(new Attribute(this, el, state, children.ToArray()));
                    }
            }

            return attrs;
        }

        internal Element MakeElement(XmlElement elem, BSML.ElementDefinition def, ParseState state)
        {
            var attrs = GetAttributes(elem, ref state, out var hasController);

            var el = Activator.CreateInstance(def.Type) as Element;

            el.Controller = state.Ref;
            if (hasController) state.Ref.OwnedElement = el;

            if (el.InitializeInternal(attrs.ToList(), def.State))
            {
                var subElems = ReadTree(elem.ChildNodes.Cast<XmlNode>(), state);

                foreach (var e in subElems)
                    el.Add(e);
            }
            else el.AddChildXml(elem.ChildNodes.Cast<XmlNode>());

            return el;
        }

        // Text elements have their own IElement type
        internal IEnumerable<Element> ReadTree(IEnumerable<XmlNode> nodes, ParseState state)
        {
            foreach (var node in nodes)
            {
                if (node is XmlText text)
                {
                    var el = Activator.CreateInstance(BSML.TextElementType.Type) as TextElement;
                    el.Initialize(text, BSML.TextElementType.State);
                    yield return el;
                }
                else if (node is XmlElement elem)
                {
                    if (Attribute.IsElementAttribute(elem)) continue;

                    var ns = elem.NamespaceURI;
                    var name = elem.LocalName;

                    var type = BSML.GetCustomElementDef(ns, name);

                    if (type == default)
                    {
                        Logger.log.Warn($"Could not find element type {ns} {name}; ignoring");
                        continue;
                    }

                    yield return MakeElement(elem, type, state);
                }
                else
                {
                    Logger.log.Warn($"Unknown node type {node.GetType()}");
                }
            }
        }
    }
}
