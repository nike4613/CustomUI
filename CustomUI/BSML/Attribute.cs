using FastExpressionCompiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

using static System.Linq.Expressions.Expression;

namespace CustomUI.BSML
{
    /// <summary>
    /// The type of an <see cref="Attribute"/>.
    /// </summary>
    public enum AttributeType
    {
        /// <summary>
        /// The attribute holds a literal string value.
        /// </summary>
        Literal = 0x1,
        /// <summary>
        /// The attribute is a binding of some kind. Never seen on its own.
        /// </summary>
        Binding = 0x2,
        /// <summary>
        /// The attribute is an input binding. It takes the result of the field/property
        /// specified as its value, which may be gotten repeatedly.
        /// </summary>
        InputBinding = Binding | 0x4,
        /// <summary>
        /// The attribute is an output binding. It puts some value in the field/property
        /// specified, which may be set repeatedly.
        /// </summary>
        OutputBinding = Binding | 0x8,
        /// <summary>
        /// The attribute is a function binding. This is used for events.
        /// </summary>
        FunctionBinding = Binding | 0x10,
        /// <summary>
        /// The attribute represents a self ref. This is an <see cref="OutputBinding"/> that 
        /// gets the value of the element the attribute is attached to.
        /// </summary>
        SelfRef = OutputBinding | 0x1,

        /// <summary>
        /// The attribute is an element-type attribute. It has child elements and attributes.
        /// This is used for more complex properties.
        /// </summary>
        ElementAttribute = 0x20
    }

    /// <summary>
    /// A BSML Attribute.
    /// </summary>
    public class Attribute
    {
        /// <summary>
        /// The XML namespace the attribute was defined with.
        /// </summary>
        public string NameSpace { get; private set; }
        /// <summary>
        /// The name of the attribute.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The type of attribute. Determines which fields are valid.
        /// </summary>
        public AttributeType Type { get; private set; }

        /// <summary>
        /// The literal value of the attribute. In the case of bindings, it will be the name of the
        /// field/property/method referenced. 
        /// </summary>
        public string LiteralValue { get; }

        /// <summary>
        /// A getter delegate type.
        /// </summary>
        /// <param name="target">the binding object source.</param>
        /// <returns>the binding value</returns>
        public delegate object GetBinding(object target);
        /// <summary>
        /// A setter delegate type.
        /// </summary>
        /// <param name="target">the binding object source.</param>
        /// <param name="value">the binding value</param>
        public delegate void SetBinding(object target, object value);

        /// <summary>
        /// The type that all bindings reference. Always valid.
        /// </summary>
        public Type LinkedType { get; private set; }
        /// <summary>
        /// The type of the binding. Only valid for bindings.
        /// </summary>
        public Type BindingType { get; private set; }

        /// <summary>
        /// The getter for an <see cref="AttributeType.OutputBinding"/>.
        /// </summary>
        public GetBinding BindingGetter { get; private set; }
        /// <summary>
        /// The setter for an <see cref="AttributeType.InputBinding"/>.
        /// </summary>
        public SetBinding BindingSetter { get; private set; }

        /// <summary>
        /// The <see cref="MethodInfo"/> for the function binding. Only valid on <see cref="AttributeType.FunctionBinding"/>
        /// </summary>
        public MethodInfo FunctionBinding { get; private set; }

        /// <summary>
        /// The attributes of this element attribute. Only valid on <see cref="AttributeType.ElementAttribute"/>.
        /// </summary>
        public Attribute[] ElementAttributes { get; private set; }

        /// <summary>
        /// The content of this element attribute. Only valid on <see cref="AttributeType.ElementAttribute"/>.
        /// </summary>
        public IElement[] ElementContent { get; private set; }

        internal Attribute(BSMLParser parser, XmlAttribute attr, Type connectedType)
        {
            Name = attr.LocalName;
            NameSpace = attr.NamespaceURI;
            LinkedType = connectedType;
            LiteralValue = attr.Value.Trim();

            if (LiteralValue[0] == '{' && LiteralValue[LiteralValue.Length - 1] == '}')
            {
                Type = AttributeType.Binding;
                var trimmed = LiteralValue.Substring(1, LiteralValue.Length - 2).Trim();

                if (trimmed[0] == '=') Type = AttributeType.InputBinding;
                if (trimmed[trimmed.Length - 1] == '=') Type = AttributeType.OutputBinding;

                if (Type == AttributeType.InputBinding || Type == AttributeType.OutputBinding)
                {
                    LiteralValue = trimmed.Trim('=').Trim();

                    // TODO: support multi-level accesses

                    if (Type == AttributeType.InputBinding)
                    {
                        var propExpr =
                            PropertyOrField(
                                Convert(
                                    Parameter(typeof(object), "self"),
                                    connectedType
                                ),
                                LiteralValue
                            );

                        BindingType = propExpr.Type;

                        BindingGetter = Lambda<GetBinding>(
                                Convert(propExpr, typeof(object)),
                                Parameter(typeof(object), "self")
                            ).CompileFast();
                    }

                    if (Type == AttributeType.OutputBinding)
                    {
                        var propExpr =
                            PropertyOrField(
                                Convert(
                                    Parameter(typeof(object), "self"),
                                    connectedType
                                ),
                                LiteralValue
                            );

                        BindingType = propExpr.Type;

                        BindingSetter = Lambda<SetBinding>(
                                Assign(
                                    Convert(propExpr, typeof(object)),
                                    Convert(
                                        Parameter(typeof(object), "value"),
                                        BindingType
                                    )
                                ),
                                Parameter(typeof(object), "self"),
                                Parameter(typeof(object), "value")
                            ).CompileFast();
                    }

                    if (Type == AttributeType.OutputBinding && NameSpace == BSML.CoreNamespace && Name == "ref")
                    {
                        Type = AttributeType.SelfRef;
                    }
                }
                else if (trimmed.EndsWith("()"))
                {
                    LiteralValue = trimmed;
                    Type = AttributeType.FunctionBinding;

                    trimmed = trimmed.TrimEnd('(', ')').Trim();

                    FunctionBinding = connectedType.GetMethod(trimmed, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                }
            }
            else
            {
                Type = AttributeType.Literal;
            }

            if (NameSpace == BSML.CoreNamespace && Name == "ref" && Type != AttributeType.SelfRef)
                throw new InvalidProgramException("'ref' parameter MUST be an OutputBinding");
        }

        internal Attribute(BSMLParser parser, XmlElement elem, Type connectedType)
        {
            if (!IsElementAttribute(elem))
                throw new ArgumentException("When an Attribute is constructed with an Element, is MUST be a valid Element attribute", nameof(elem));

            Name = elem.LocalName.Split('.').Last();
            NameSpace = elem.NamespaceURI;
            LinkedType = connectedType;

            ElementAttributes = parser.GetAttributes(elem, ref connectedType, false).ToArray();

            ElementContent = parser.ReadTree(elem.ChildNodes.Cast<XmlNode>(), connectedType).ToArray();
        }

        internal static bool IsElementAttribute(XmlElement elem)
        {
            var spl = elem.LocalName.Split('.');
            return spl.Length == 2 && spl[0] == elem.ParentNode.LocalName && elem.NamespaceURI == elem.ParentNode.NamespaceURI;
        }
    }
}
